using Microsoft.AspNetCore.Builder;

namespace DotNET5API.Services
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseTime(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestResponseTimeMiddleware>();
        }
        public static IApplicationBuilder UseTokenChecker(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CheckHeaderMiddleware>();
        }
    }
}
