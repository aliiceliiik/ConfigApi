using ConfigApi.Business.Security;
using ConfigApi.Context.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ConfigApi.Api.Filters;

public class DomainCheckAttribute : TypeFilterAttribute
{
    public DomainCheckAttribute() : base(typeof(DomainCheckFilter)) { }

    private class DomainCheckFilter : IAsyncActionFilter
    {
        private readonly ICompanyRepository _companyRepository;

        public DomainCheckFilter(ICompanyRepository companyRepository)
            => _companyRepository = companyRepository;

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var companyIdClaim = context.HttpContext.User.FindFirst("companyId")?.Value;

            if (!int.TryParse(companyIdClaim, out var companyId))
            {
                context.Result = new UnauthorizedObjectResult(
                    new { message = "Token geçersiz." });
                return;
            }

            var request = context.HttpContext.Request;
            var origin = request.Headers.Origin.FirstOrDefault()
                         ?? request.Headers.Referer.FirstOrDefault();

            var domain = DomainHelper.Extract(origin);
            var allowedDomains = await _companyRepository.GetAllowedDomainsAsync(companyId);

            if (allowedDomains is null || !DomainHelper.IsAllowed(domain, allowedDomains))
            {
                context.Result = new ObjectResult(
                    new { message = "Bu domainden erişim yetkiniz yok." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            await next();
        }
    }
}