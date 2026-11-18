using Microsoft.AspNetCore.Authorization;

namespace Controller.Middleware.Attribute;

/// <summary>
/// An attribute that can be applied to controllers or actions to require the user's email to be verified.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireVerifiedEmailAttribute : AuthorizeAttribute
{
    public RequireVerifiedEmailAttribute() : base("EmailVerified") { }
}

/// <summary>
/// An attribute that can be applied to controllers or actions to allow access without requiring the user's email to be verified.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowUnverifiedEmailAttribute : System.Attribute { }