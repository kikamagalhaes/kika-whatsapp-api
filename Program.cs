using System.Net;
using Kika.WhatsAppAutomation.Api.Options;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Loopback);

    var knownProxy = builder.Configuration["ReverseProxy:KnownProxy"];
    if (!string.IsNullOrWhiteSpace(knownProxy))
    {
        options.KnownProxies.Add(IPAddress.Parse(knownProxy));
    }
});

builder.Services
    .AddOptions<WhatsAppOptions>()
    .Bind(builder.Configuration.GetSection(WhatsAppOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.VerifyToken),
        $"{WhatsAppOptions.SectionName}:{nameof(WhatsAppOptions.VerifyToken)} must be configured.")
    .ValidateOnStart();

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
