using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TMS201.Filters
{
    public class RoleFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("Role");

            if (role != "SuperAdmin" && role != "Admin") // allow both
            {
                context.Result = new RedirectToActionResult("Index", "Ticket", null);
            }
        }
    }
}