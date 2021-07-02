using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNET5API.Services
{
    public class CheckHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        public CheckHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //Testing a header key-value, customize as needed.
            if (context.Request.Headers.ContainsKey("Host") && context.Request.Headers["Host"].ToString().Length > 3)
            {
                await _next.Invoke(context);
            }
            else
            {
                //if a request header is missing in the request, end the middleware pipeline
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Token missing for request: " + context.Request.Path);
            }
        }

    }
}
