using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AuthFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var username = context.HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(username))
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
        }
    }
}