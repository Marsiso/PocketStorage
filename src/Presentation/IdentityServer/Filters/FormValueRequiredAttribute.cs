﻿using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace IdentityServer.Filters;

public sealed class FormValueRequiredAttribute : ActionMethodSelectorAttribute
{
    #region Private Fields

    private readonly string _name;

    #endregion Private Fields

    #region Public Constructors

    public FormValueRequiredAttribute(string name)
    {
        _name = name;
    }

    #endregion Public Constructors

    #region Public Methods

    public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
    {
        if (string.Equals(routeContext.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase)
            || string.Equals(routeContext.HttpContext.Request.Method, "HEAD", StringComparison.OrdinalIgnoreCase)
            || string.Equals(routeContext.HttpContext.Request.Method, "DELETE", StringComparison.OrdinalIgnoreCase)
            || string.Equals(routeContext.HttpContext.Request.Method, "TRACE", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(routeContext.HttpContext.Request.ContentType)
            || !routeContext.HttpContext.Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !string.IsNullOrEmpty(routeContext.HttpContext.Request.Form[_name]);
    }

    #endregion Public Methods
}