using ConfigApi.Entities.Enums;
using ConfigApi.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ConfigApi.Mvc.Filters;

public class RequireTenantSelectionAttribute : TypeFilterAttribute
{
    public RequireTenantSelectionAttribute() : base(typeof(Filter)) { }

    private class Filter : IActionFilter
    {
        private readonly ITenantSelection _selection;

        public Filter(ITenantSelection selection) => _selection = selection;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.IsInRole(UserRole.SuperAdmin)) return;

            if (_selection.SelectedTenantId is null)
                context.Result = new RedirectToActionResult("Index", "Tenants", null);
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
