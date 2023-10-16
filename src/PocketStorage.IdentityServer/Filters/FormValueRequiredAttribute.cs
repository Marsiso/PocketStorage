using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace PocketStorage.IdentityServer.Filters;

public sealed class FormValueRequiredAttribute : ActionMethodSelectorAttribute
{
    private readonly string _name;

    public FormValueRequiredAttribute(string name) => _name = name;

    public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
    {
        if (string.Equals(routeContext.HttpContext.Request.Method, HttpMethods.Get, StringComparison.OrdinalIgnoreCase)
            || string.Equals(routeContext.HttpContext.Request.Method, HttpMethods.Head, StringComparison.OrdinalIgnoreCase)
            || string.Equals(routeContext.HttpContext.Request.Method, HttpMethods.Delete, StringComparison.OrdinalIgnoreCase)
            || string.Equals(routeContext.HttpContext.Request.Method, HttpMethods.Trace, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(routeContext.HttpContext.Request.ContentType)
            || !routeContext.HttpContext.Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !string.IsNullOrEmpty(routeContext.HttpContext.Request.Form[_name]);
    }
}
