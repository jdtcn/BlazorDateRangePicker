using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BlazorDateRangePicker;
using TextCopy;

namespace Demo.ClientSideApp
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable")]
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.InjectClipboard();
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