using System;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using GiganticEmu.Shared.Backend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GiganticEmu.Web;

[AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
public class RequireApiKeyAttribute : Attribute, IAsyncActionFilter
{
    private const string HEADER_NAME = "X-API-KEY";
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {

        if (!context.HttpContext.Request.Headers.TryGetValue(HEADER_NAME, out var extractedApiKey))
        {
            context.Result = new ObjectResult(new ResponseBase(RequestResult.Unauthorized));
            return;
        }

        var config = context.HttpContext.RequestServices.GetRequiredService<IOptions<BackendConfiguration>>();

        if (!(config.Value.ApiKey?.Equals(extractedApiKey) ?? false))
        {
            context.Result = new ObjectResult(new ResponseBase(RequestResult.Unauthorized));
            return;
        }

        await next();
    }
}
