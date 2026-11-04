using Microsoft.AspNetCore.Mvc;

namespace Controller.Extensions;

public static class ControllerExtensions
{
    public static ActionResult SeeOther(this ControllerBase controller, string location)
    {
        if (string.IsNullOrEmpty(location))
        {
            throw new ArgumentException("Location cannot be null or empty.", nameof(location));
        }

        controller.Response.Headers.Location = location;
        return new StatusCodeResult(303); // 303 See Other
    }
}
