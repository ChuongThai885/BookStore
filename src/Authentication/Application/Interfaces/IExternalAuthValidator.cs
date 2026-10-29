using Authentication.Application.DTOs;

namespace Authentication.Application.Interfaces
{
    public interface IExternalAuthValidator
    {
        string ProviderName { get; }
        Task<ExternalUserPayload?> ValidateAsync(string token, CancellationToken cancellationToken = default);
    }
}
