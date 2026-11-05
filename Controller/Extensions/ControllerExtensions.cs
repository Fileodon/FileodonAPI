using Microsoft.AspNetCore.Mvc;

namespace Controller.Extensions;

public static class ControllerExtensions
{
    public static ActionResult SeeOther(this ControllerBase controller, string location, string value)
    {
        if (string.IsNullOrEmpty(location))
        {
            throw new ArgumentException("Location cannot be null or empty.", nameof(location));
        }
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        }

        controller.Response.Headers.Location = location;
        controller.Response.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(value));
        return new StatusCodeResult(303); // 303 See Other
    }
}
