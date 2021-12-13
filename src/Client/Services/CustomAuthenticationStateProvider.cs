using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Vetrina.Client.Constants;

namespace Vetrina.Client.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient httpClient;
        private readonly ILocalStorageService localStorage;
        private readonly AuthenticationState anonymous;

        public CustomAuthenticationStateProvider(
            HttpClient httpClient,
            ILocalStorageService localStorage)
        {
            this.httpClient = httpClient;
            this.localStorage = localStorage;
            this.anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await localStorage.GetItemAsync<string>(AuthenticationConstants.LocalStorageAuthTokenKey);
            if (string.IsNullOrWhiteSpace(token))
            {
                return anonymous;
            }

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "bearer",
                    token);

            var authenticationState =
                new AuthenticationState(
                    new ClaimsPrincipal(
                        new ClaimsIdentity(
                            claims: JwtParser.ParseClaimsFromJwt(token),
                            authenticationType: "jwtAuthType",
                            nameType: ClaimTypes.Name,
                            roleType: ClaimTypes.Role)));

            var name = authenticationState.User.Claims.FirstOrDefault(x => string.Equals(x.Type, "name", StringComparison.OrdinalIgnoreCase));
            var claimValues = authenticationState.User.Claims.Select(x => x.Value).ToList();

            return authenticationState;
        }
            
        public void NotifyUserAuthenticated()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(anonymous));
        }
    }
}