using Microsoft.AspNetCore.Mvc;

namespace Controller.Extensions;

public static class ControllerExtensions
{
    public static ActionResult SeeOther(this ControllerBase controller, string location, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(location, nameof(location));
        ArgumentException.ThrowIfNullOrEmpty(value, nameof(value));

        controller.Response.Headers.Location = location;
        controller.Response.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(value));
        return new StatusCodeResult(303); // 303 See Other
    }
}
