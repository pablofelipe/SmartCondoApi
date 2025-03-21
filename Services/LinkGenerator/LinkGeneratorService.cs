namespace SmartCondoApi.Services.LinkGenerator
{
    public class LinkGeneratorService : ILinkGeneratorService
    {
        private readonly Microsoft.AspNetCore.Routing.LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LinkGeneratorService(Microsoft.AspNetCore.Routing.LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor)
        {
            _linkGenerator = linkGenerator;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GenerateConfirmationLink(string action, string controller, object values)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext não está disponível.");
            }

            return _linkGenerator.GetUriByAction(
                httpContext,
                action: action,
                controller: controller,
                values: values,
                scheme: httpContext.Request.Scheme
            );
        }
    }
}
