using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ExportModelStateAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        // Only export if ModelState is invalid and we are performing a redirect
        if (!context.ModelState.IsValid &&
            (context.Result is RedirectResult || context.Result is RedirectToActionResult || context.Result is RedirectToRouteResult))
        {
            var controller = context.Controller as Controller;
            if (controller != null)
            {
                var errors = context.ModelState
                    .Where(ms => ms.Value!.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                // FIX: Serialize the dictionary to a string before storing it
                controller.TempData["DeserializedModelStateErrors"] = JsonSerializer.Serialize(errors);
            }
        }

        base.OnActionExecuted(context);
    }
}