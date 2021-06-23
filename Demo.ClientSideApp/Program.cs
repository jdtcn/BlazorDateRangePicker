using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BlazorDateRangePicker;

namespace Demo.ClientSideApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddSingleton<Shared.IClipboard, Shared.BlazorClipboard>();

            builder.Services.AddDateRangePicker(config =>
            {
                config.Attributes = new Dictionary<string, object>
                {
                    { "class", "form-control form-control-sm" }
                };
            });

            await builder.Build().RunAsync();
        }
    }
}