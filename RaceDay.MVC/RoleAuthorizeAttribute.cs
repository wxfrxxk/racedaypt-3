using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace RaceDay.MVC.Filters
{
    // Protects MVC pages: the user must be logged in (session) and have one of the allowed roles.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            int? userId = session.GetInt32("UserID");
            string? role = session.GetString("Role");

            if (userId == null || role == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account",
                    new { returnUrl = context.HttpContext.Request.Path.ToString() });
                return;
            }

            if (_roles.Length > 0 && !_roles.Contains(role))
            {
                if (context.Controller is Controller controller)
                {
                    controller.TempData["Error"] = "You do not have access to that page.";
                }

                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }
}
