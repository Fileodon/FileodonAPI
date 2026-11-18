using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Controller.Extensions;

public sealed class PollStatusExamplesOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var methodName =
            (context.Description.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo.Name
            ?? context.Description.ActionDescriptor.EndpointMetadata
                .OfType<MethodInfo>().FirstOrDefault()?.Name;

        if (methodName != "PollStatus" || operation.Responses is null)
            return Task.CompletedTask;

        foreach (var response in operation.Responses.Values)
        {
            if (response.Content is not null &&
                response.Content.TryGetValue("application/json", out var mediaType) &&
                response.Description is "Accepted" or "Rejected" or "Pending")
            {
                mediaType.Example = JsonValue.Create(response.Description);
            }
        }

        return Task.CompletedTask;
    }
}
