using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Vetrina.Autogen.API.Client;
using Vetrina.Autogen.API.Client.Contracts;
using Vetrina.Client.Services;

namespace Vetrina.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => 
                new HttpClient
                {
                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
                });

            builder.Services.AddMudServices();
            builder.Services.AddMudBlazorDialog();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddSingleton<ApplicationState>();
            builder.Services.AddScoped<IClipboardService, ClipboardService>();
            builder.Services.AddScoped<IAuthenticationClient, AuthenticationClient>();
            builder.Services.AddScoped<IUsersClient, UsersClient>();
            builder.Services.AddScoped<ISearchPromotionsClient, SearchPromotionsClient>();

            // Register Auth services.
            builder.Services.AddOptions();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddAuthenticationCore();

            await builder.Build().RunAsync();
        }
    }
}
