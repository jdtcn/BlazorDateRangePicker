using BlazorDateRangePicker;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Demo.ClientSideApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDateRangePicker(config =>
            {
                config.Attributes = new Dictionary<string, object>
                {
                    { "class", "form-control form-control-sm" }
                };
            });
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
