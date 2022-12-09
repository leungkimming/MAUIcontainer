using Microsoft.Identity.Client;

namespace MAUIcontainer {
    public interface IAuthService {
        public Task<string> GetAccessToken(CancellationToken cancellationToken);
        public Task<AuthenticationResult> LoginAsync(CancellationToken cancellationToken);
        public Task<AuthenticationResult> LoginStatus();
        public void LogoutAsync();
    }
}
