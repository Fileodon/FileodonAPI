using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Controller.Middleware;

public sealed class JsonModelBinder<T> : IModelBinder
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (result == ValueProviderResult.None)
            return Task.CompletedTask;

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);

        var json = result.FirstValue;
        if (string.IsNullOrWhiteSpace(json))
            return Task.CompletedTask;

        try
        {
            var value = JsonSerializer.Deserialize<T>(json, Options);
            bindingContext.Result = ModelBindingResult.Success(value);
        }
        catch (JsonException ex)
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, ex.Message);
        }

        return Task.CompletedTask;
    }
}
