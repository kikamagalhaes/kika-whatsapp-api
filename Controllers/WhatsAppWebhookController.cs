using System.Text.Json;
using Kika.WhatsAppAutomation.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Kika.WhatsAppAutomation.Api.Controllers;

[ApiController]
[Route("webhooks/whatsapp")]
public sealed class WhatsAppWebhookController : ControllerBase
{
    private readonly ILogger<WhatsAppWebhookController> _logger;
    private readonly WhatsAppOptions _options;

    public WhatsAppWebhookController(
        ILogger<WhatsAppWebhookController> logger,
        IOptions<WhatsAppOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    [HttpGet]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Verify(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        var isValidRequest = string.Equals(mode, "subscribe", StringComparison.Ordinal)
            && string.Equals(verifyToken, _options.VerifyToken, StringComparison.Ordinal);

        if (!isValidRequest)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        return Content(challenge ?? string.Empty, "text/plain");
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Receive([FromBody] JsonElement payload)
    {
        _logger.LogInformation(
            "WhatsApp webhook received with JSON value kind {ValueKind}.",
            payload.ValueKind);

        return Ok();
    }
}
