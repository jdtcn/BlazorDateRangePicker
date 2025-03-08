using BlazorDateRangePicker;
using Demo.ClientSideApp;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("app");

builder.Services.AddScoped<Demo.Shared.IClipboard, Demo.Shared.BlazorClipboard>();

builder.Services.AddDateRangePicker(config =>
{
    config.Attributes = new Dictionary<string, object>
    {
        { "class", "form-control form-control-sm" }
    };
});

await builder.Build().RunAsync();
