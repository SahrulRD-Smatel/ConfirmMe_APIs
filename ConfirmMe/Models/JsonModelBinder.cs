using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

public class JsonModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var request = bindingContext.HttpContext.Request;

        if (!request.HasFormContentType)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        var form = await request.ReadFormAsync();
        var key = bindingContext.ModelName;

        var json = form[key];
        if (string.IsNullOrWhiteSpace(json))
        {
            // Coba cari di Request.Form.Files dengan nama key
            var file = request.Form.Files.FirstOrDefault(f => f.Name == key);
            if (file != null)
            {
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                json = await reader.ReadToEndAsync();
            }
        }

        Console.WriteLine($"[JsonModelBinder] Key: {key}");
        Console.WriteLine($"[JsonModelBinder] Value: {(string.IsNullOrWhiteSpace(json) ? "(null or empty)" : json)}");

        if (string.IsNullOrWhiteSpace(json))
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        try
        {
            var result = JsonSerializer.Deserialize(json, bindingContext.ModelType, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            bindingContext.Result = ModelBindingResult.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JsonModelBinder] Error: {ex.Message}");
            bindingContext.Result = ModelBindingResult.Failed();
        }
    }

}
