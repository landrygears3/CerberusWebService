using CerberusClassLibrary.Model;
using CerberusClassLibrary.Model.Abac;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CerberusClassLibrary.Controller.Abac
{
    public class AbacCommitService
    {
        private readonly string _connectionString;

        public AbacCommitService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Commit general (Depto + PersonaRol + PersonaActividad).
        /// Valida que el usuario del token tenga los 3 permisos:
        /// MODULO.RHH.ROLES.ALTA, BAJA, MODIFICACION.
        /// </summary>
        public async Task<ResponseModel<bool>> CommitAccesosAsync(
            string aspNetUserIdFromToken,
            SavePersonaAccessRequest request,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(aspNetUserIdFromToken))
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Code = 401,
                    Message = "No autorizado",
                    Desc = "No se recibió el AspNetUserId desde el token.",
                    Data = false
                };
            }

            if (request == null || string.IsNullOrWhiteSpace(request.UserNumber))
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Solicitud inválida",
                    Desc = "UserNumber requerido (ej: CER00010).",
                    Data = false
                };
            }

            // 1) Validar permisos (commit general => los 3 siempre)
            var requiredPermissions = new[]
            {
                "MODULO.RHH.ROLES.ALTA",
                "MODULO.RHH.ROLES.BAJA",
                "MODULO.RHH.ROLES.MODIFICACION"
            };

            foreach (var perm in requiredPermissions)
            {
                var ok = await IsAllowedAsync(aspNetUserIdFromToken, perm, ct);
                if (!ok)
                {
                    return new ResponseModel<bool>
                    {
                        IsSuccess = false,
                        Code = 403,
                        Message = "Sin permisos",
                        Desc = $"No cuenta con el permiso requerido: {perm}",
                        Data = false
                    };
                }
            }

            // 2) Ejecutar commit en BD (SP transaccional)
            var committed = await ExecuteCommitAsync(request, ct);

            return new ResponseModel<bool>
            {
                IsSuccess = committed.Data,
                Code = committed.Data ? 200 : 500,
                Message = committed.Message,
                Desc = committed.Desc,
                Data = committed.Data
            };
        }

        /// <summary>
        /// Valida si un usuario (AspNetUserId) tiene permiso para una ActivityKey.
        /// Usa: dbo.sp_IsAllowedActivity
        /// </summary>
        public async Task<ResponseModel<bool>> CheckPermissionAsync(
            string aspNetUserIdFromToken,
            string activityKey,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(aspNetUserIdFromToken))
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Code = 401,
                    Message = "No autorizado",
                    Desc = "No se recibió el AspNetUserId desde el token.",
                    Data = false
                };
            }

            if (string.IsNullOrWhiteSpace(activityKey))
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Solicitud inválida",
                    Desc = "activityKey requerido.",
                    Data = false
                };
            }

            var allowed = await IsAllowedAsync(aspNetUserIdFromToken, activityKey.Trim(), ct);

            return new ResponseModel<bool>
            {
                IsSuccess = true,
                Code = 200,
                Message = "OK",
                Desc = "Permiso evaluado correctamente.",
                Data = allowed
            };
        }

        // ============================
        // Internos (ADO.NET)
        // ============================

        private async Task<bool> IsAllowedAsync(string aspNetUserId, string activityKey, CancellationToken ct)
        {
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand("dbo.sp_IsAllowedActivity", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new SqlParameter("@AspNetUserId", SqlDbType.NVarChar, 450) { Value = aspNetUserId });
            cmd.Parameters.Add(new SqlParameter("@ActivityKey", SqlDbType.NVarChar, 150) { Value = activityKey });

            // SP devuelve: SELECT CAST(0/1 AS BIT) AS Allowed;
            // ExecuteScalar regresa el primer campo de la primera fila => BIT
            var scalar = await cmd.ExecuteScalarAsync(ct);
            return scalar != null && Convert.ToBoolean(scalar);
        }

        private async Task<ResponseModel<bool>> ExecuteCommitAsync(
     SavePersonaAccessRequest req,
     CancellationToken ct = default)
        {
            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync(ct);

                await using var cmd = new SqlCommand("dbo.sp_SavePersonaAccess", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // ==========================
                // Parámetro NumeroUsuario
                // ==========================
                cmd.Parameters.Add(new SqlParameter("@NumeroUsuario", SqlDbType.NVarChar, 50)
                {
                    Value = req.UserNumber?.Trim()
                });

                // ==========================
                // Departamento
                // ==========================
                cmd.Parameters.Add(new SqlParameter("@DepartamentoId", SqlDbType.Int)
                {
                    Value = (object?)req.DepartamentoId ?? DBNull.Value
                });

                // ==========================
                // TVP Roles
                // ==========================
                var rolesTable = new DataTable();
                rolesTable.Columns.Add("Id", typeof(int));

                if (req.RoleIds != null)
                {
                    foreach (var id in req.RoleIds.Distinct())
                        rolesTable.Rows.Add(id);
                }

                var pRoles = cmd.Parameters.AddWithValue("@RoleIds", rolesTable);
                pRoles.SqlDbType = SqlDbType.Structured;
                pRoles.TypeName = "dbo.IntList";

                // ==========================
                // TVP Reglas Persona
                // ==========================
                var rulesTable = new DataTable();
                rulesTable.Columns.Add("ActividadId", typeof(int));
                rulesTable.Columns.Add("IsAllowed", typeof(bool));

                if (req.PersonaRules != null)
                {
                    foreach (var rule in req.PersonaRules
                                 .GroupBy(x => x.ActividadId)
                                 .Select(g => g.First()))
                    {
                        rulesTable.Rows.Add(rule.ActividadId, rule.IsAllowed);
                    }
                }

                var pRules = cmd.Parameters.AddWithValue("@PersonaRules", rulesTable);
                pRules.SqlDbType = SqlDbType.Structured;
                pRules.TypeName = "dbo.PersonaRuleList";

                // ==========================
                // OUTPUT Error
                // ==========================
                var pError = new SqlParameter("@ErrorDesc", SqlDbType.NVarChar, 4000)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(pError);

                // ==========================
                // Ejecutar
                // ==========================
                var scalar = await cmd.ExecuteScalarAsync(ct);
                var success = scalar != null && Convert.ToBoolean(scalar);

                var errorMessage = pError.Value == DBNull.Value
                    ? null
                    : pError.Value?.ToString();

                return new ResponseModel<bool>
                {
                    IsSuccess = success,
                    Code = success ? 200 : 500,
                    Message = success ? "OK" : "Error",
                    Desc = success
                        ? "Commit aplicado correctamente."
                        : errorMessage ?? "No fue posible aplicar el commit.",
                    Data = success
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = "Exception",
                    Desc = ex.Message,
                    Data = false
                };
            }
        }

        public async Task<ResponseModel<PersonaAccessResponse>> GetPersonaAccessAsync(
            string aspNetUserIdFromToken,
            string numeroUsuarioConsultado,
            CancellationToken ct = default)
        {
            // 0) Token requerido
            if (string.IsNullOrWhiteSpace(aspNetUserIdFromToken))
            {
                return new ResponseModel<PersonaAccessResponse>
                {
                    IsSuccess = false,
                    Code = 401,
                    Message = "No autorizado",
                    Desc = "No se recibió el AspNetUserId desde el token.",
                    Data = null!
                };
            }

            // 1) Validar permiso del operador (quien consume el endpoint)
            var canView = await IsAllowedAsync(aspNetUserIdFromToken, "MODULO.RHH.ROLES.VER", ct);
            if (!canView)
            {
                return new ResponseModel<PersonaAccessResponse>
                {
                    IsSuccess = false,
                    Code = 403,
                    Message = "Sin permisos",
                    Desc = "No cuenta con el permiso requerido: MODULO.RHH.ROLES.VER",
                    Data = null!
                };
            }

            // 2) Validación de entrada
            if (string.IsNullOrWhiteSpace(numeroUsuarioConsultado))
            {
                return new ResponseModel<PersonaAccessResponse>
                {
                    IsSuccess = false,
                    Code = 400,
                    Message = "Solicitud inválida",
                    Desc = "NumeroUsuario requerido.",
                    Data = null!
                };
            }

            // 3) Ejecutar SP que regresa: depto + roles + reglas persona
            try
            {
                var result = new PersonaAccessResponse();

                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync(ct);

                await using var cmd = new SqlCommand("dbo.sp_GetPersonaAccessByNumeroUsuario", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add(new SqlParameter("@NumeroUsuario", SqlDbType.NVarChar, 50)
                {
                    Value = numeroUsuarioConsultado.Trim()
                });

                await using var reader = await cmd.ExecuteReaderAsync(ct);

                // Result set 1: PersonaId + DepartamentoId
                if (await reader.ReadAsync(ct))
                {
                    result.DepartamentoId = reader["DepartamentoId"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(reader["DepartamentoId"]);
                }
                else
                {
                    return new ResponseModel<PersonaAccessResponse>
                    {
                        IsSuccess = false,
                        Code = 404,
                        Message = "No encontrado",
                        Desc = "No se encontró el usuario/persona activa para el NumeroUsuario enviado.",
                        Data = null!
                    };
                }

                // Result set 2: Roles
                if (await reader.NextResultAsync(ct))
                {
                    while (await reader.ReadAsync(ct))
                        result.RoleIds.Add(Convert.ToInt32(reader["RolId"]));
                }

                // Result set 3: Reglas PersonaActividad (Allows/Denys -> IsAllowed)
                if (await reader.NextResultAsync(ct))
                {
                    while (await reader.ReadAsync(ct))
                    {
                        var isAllowedObj = reader["IsAllowed"];
                        if (isAllowedObj == DBNull.Value) continue;

                        result.PersonaRules.Add(new PersonaRuleResponseItem
                        {
                            ActividadId = Convert.ToInt32(reader["ActividadId"]),
                            IsAllowed = Convert.ToBoolean(isAllowedObj)
                        });
                    }
                }

                return new ResponseModel<PersonaAccessResponse>
                {
                    IsSuccess = true,
                    Code = 200,
                    Message = "OK",
                    Desc = "Configuración recuperada correctamente.",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<PersonaAccessResponse>
                {
                    IsSuccess = false,
                    Code = 500,
                    Message = "Error",
                    Desc = ex.Message,
                    Data = null!
                };
            }
        }


    }

}