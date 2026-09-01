using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UI.Filters
{
    public class ImportModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.Controller as Controller;
            if (controller != null && controller.TempData.ContainsKey("DeserializedModelStateErrors"))
            {
                var rawJson = controller.TempData["DeserializedModelStateErrors"] as string;

                if (!string.IsNullOrEmpty(rawJson))
                {
                    // FIX: Deserialize the raw JSON string back into the dictionary
                    var errors = JsonSerializer.Deserialize<Dictionary<string, string[]>>(rawJson);

                    if (errors != null)
                    {
                        foreach (var error in errors)
                        {
                            foreach (var errorMessage in error.Value)
                            {
                                context.ModelState.AddModelError(error.Key, errorMessage);
                            }
                        }
                    }
                }
            }

            base.OnActionExecuting(context);
        }
    }
}