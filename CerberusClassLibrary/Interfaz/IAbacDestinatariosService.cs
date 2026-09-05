using CerberusClassLibrary.Model.Abac;


namespace CerberusClassLibrary.Interfaz
{
    public interface IAbacDestinatariosService
    {
        Task<IReadOnlyCollection<ResolverDestinatariosAbacResponse>>
            ResolverDestinatariosAsync(
                ResolverDestinatariosAbacRequest request,
                CancellationToken cancellationToken = default);
    }
}
