using System.Text.Json.Nodes;
using Infrastructure.Models.Enums;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Controller.Extensions;

public sealed class PollStatusExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.Name != "PollStatus" || operation.Responses == null)
            return;

        foreach (var response in operation.Responses.Values)
        {
            if (response.Content is not null &&
                response.Content.TryGetValue("application/json", out var mediaType) &&
                response.Description is "Accepted" or "Rejected" or "Pending")
            {
                mediaType.Example = JsonValue.Create(response.Description);
            }
        }
    }
}
