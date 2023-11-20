using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;

namespace PocketStorage.Application.Filters;

public sealed class FormValueRequiredAttribute(string name) : ActionMethodSelectorAttribute
{
    public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
    {
        if (string.Equals(routeContext.HttpContext.Request.Method, HttpMethods.Get)
            || string.Equals(routeContext.HttpContext.Request.Method, HttpMethods.Head)
            || string.Equals(routeContext.HttpContext.Request.Method, HttpMethods.Delete)
            || string.Equals(routeContext.HttpContext.Request.Method, HttpMethods.Trace)
            || string.IsNullOrEmpty(routeContext.HttpContext.Request.ContentType)
            || !routeContext.HttpContext.Request.ContentType.StartsWith("application/x-www-form-urlencoded"))
        {
            return false;
        }

        return !string.IsNullOrEmpty(routeContext.HttpContext.Request.Form[name]);
    }
}
