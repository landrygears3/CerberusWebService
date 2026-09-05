using CerberusClassLibrary.Model.Abac;
using System;
using System.Collections.Generic;
using System.Text;

namespace CerberusClassLibrary.Controller.Abac
{
    using System.Data;
    using System.Text.Json;
    using Microsoft.Data.SqlClient;

    public class AbacDestinatariosFunctions
    {
        private readonly string _connectionString;

        public AbacDestinatariosFunctions(
            string connectionString)
        {
            _connectionString =
                connectionString
                ?? throw new ArgumentNullException(
                    nameof(connectionString));
        }


        public async Task<List<ResolverDestinatariosAbacResponse>>
            ResolverDestinatariosAsync(
                ResolverDestinatariosAbacRequest request,
                CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));


            request.ActividadIds ??= new List<int>();
            request.DepartamentoIds ??= new List<int>();
            request.RolIds ??= new List<int>();


            var resultado =
                new List<ResolverDestinatariosAbacResponse>();


            var actividadJson =
                JsonSerializer.Serialize(
                    request.ActividadIds
                        .Distinct()
                        .ToList());


            var departamentoJson =
                JsonSerializer.Serialize(
                    request.DepartamentoIds
                        .Distinct()
                        .ToList());


            var rolJson =
                JsonSerializer.Serialize(
                    request.RolIds
                        .Distinct()
                        .ToList());


            await using var connection =
                new SqlConnection(_connectionString);


            await connection.OpenAsync(
                cancellationToken);


            await using var command =
                new SqlCommand(
                    "dbo.sp_ResolverDestinatariosAbac",
                    connection);


            command.CommandType =
                CommandType.StoredProcedure;


            command.Parameters.Add(
                new SqlParameter(
                    "@ActividadIds",
                    SqlDbType.NVarChar,
                    -1)
                {
                    Value = actividadJson
                });


            command.Parameters.Add(
                new SqlParameter(
                    "@DepartamentoIds",
                    SqlDbType.NVarChar,
                    -1)
                {
                    Value = departamentoJson
                });


            command.Parameters.Add(
                new SqlParameter(
                    "@RolIds",
                    SqlDbType.NVarChar,
                    -1)
                {
                    Value = rolJson
                });


            command.Parameters.Add(
                new SqlParameter(
                    "@MatchMode",
                    SqlDbType.VarChar,
                    10)
                {
                    Value = request.MatchMode
                });


            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);


            var numeroUsuarioOrdinal =
                reader.GetOrdinal("NumeroUsuario");

            var actividadesOrdinal =
                reader.GetOrdinal("ActividadIdsJson");

            var departamentosOrdinal =
                reader.GetOrdinal("DepartamentoIdsJson");

            var rolesOrdinal =
                reader.GetOrdinal("RolIdsJson");


            while (await reader.ReadAsync(cancellationToken))
            {
                var actividadesJson =
                    reader.IsDBNull(actividadesOrdinal)
                        ? "[]"
                        : reader.GetString(actividadesOrdinal);


                var departamentosJson =
                    reader.IsDBNull(departamentosOrdinal)
                        ? "[]"
                        : reader.GetString(departamentosOrdinal);


                var rolesJson =
                    reader.IsDBNull(rolesOrdinal)
                        ? "[]"
                        : reader.GetString(rolesOrdinal);


                var item =
                    new ResolverDestinatariosAbacResponse
                    {
                        NumeroUsuario =
                            reader.GetString(
                                numeroUsuarioOrdinal),

                        ActividadIds =
                            JsonSerializer
                                .Deserialize<List<int>>(
                                    actividadesJson)
                            ?? new List<int>(),

                        DepartamentoIds =
                            JsonSerializer
                                .Deserialize<List<int>>(
                                    departamentosJson)
                            ?? new List<int>(),

                        RolIds =
                            JsonSerializer
                                .Deserialize<List<int>>(
                                    rolesJson)
                            ?? new List<int>()
                    };


                resultado.Add(item);
            }


            return resultado;
        }
    }
}
