// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.Security.UnitTests;

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.CFS.Approvals.Contracts;
using Microsoft.CFS.Approvals.Data.Azure.Storage.Interface;
using Microsoft.CFS.Approvals.DevTools.AppConfiguration;
using Microsoft.CFS.Approvals.SupportService.API.Controllers.api.v1;
using Microsoft.CFS.Approvals.SupportService.API.Filters;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

public class SupportAuthorizationTests
{
    [Theory]
    [InlineData("alice")]
    [InlineData("BOB")]
    public void AuthorizationFilter_AllowsExactCaseInsensitiveAdminMatch(string alias)
    {
        var filter = new AuthorizationFilter(CreateConfigurationHelper());
        var context = CreateAuthorizationContext("test", alias);

        filter.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Theory]
    [InlineData(null, "alice")]
    [InlineData("", "alice")]
    [InlineData("ali", null)]
    [InlineData("mallory", null)]
    public void AuthorizationFilter_RejectsMissingOrNonExactServerAlias(string loggedInAlias, string clientAlias)
    {
        var filter = new AuthorizationFilter(CreateConfigurationHelper());
        var context = CreateAuthorizationContext("test", loggedInAlias);
        if (clientAlias != null)
        {
            context.HttpContext.Request.Headers["useralias"] = clientAlias;
        }

        filter.OnAuthorization(context);

        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    [Fact]
    public void AuthorizationFilter_RejectsUnknownEnvironment()
    {
        var filter = new AuthorizationFilter(CreateConfigurationHelper());
        var context = CreateAuthorizationContext("unknown", "alice");

        filter.OnAuthorization(context);

        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    [Theory]
    [InlineData("alice", true)]
    [InlineData("bob", true)]
    [InlineData("ali", false)]
    [InlineData("", false)]
    public void CheckUserRole_UsesServerAliasAndExactMembership(string alias, bool expected)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["env"] = "test";
        if (!string.IsNullOrEmpty(alias))
        {
            httpContext.Request.Headers[Constants.LoggedInUserAlias] = alias;
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());
        var actionContextAccessor = new ActionContextAccessor { ActionContext = actionContext };
        var tableHelper = new Mock<ITableHelper>();
        var controller = new CommonController(
            _ => tableHelper.Object,
            CreateConfigurationHelper(),
            actionContextAccessor,
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Environmentlist"] = "test",
            }).Build())
        {
            ControllerContext = new ControllerContext(actionContext),
        };

        var result = Assert.IsType<OkObjectResult>(controller.CheckUserRole());

        Assert.Equal(expected, result.Value);
    }

    private static AuthorizationFilterContext CreateAuthorizationContext(string environment, string loggedInAlias)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["env"] = environment;
        if (loggedInAlias != null)
        {
            httpContext.Request.Headers[Constants.LoggedInUserAlias] = loggedInAlias;
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    private static ConfigurationHelper CreateConfigurationHelper()
    {
        var environmentConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["AdminUserList"] = "alice;Bob",
                ["StorageAccountName"] = "storage",
            })
            .Build();

        return new ConfigurationHelper(new Dictionary<string, IConfiguration>
        {
            ["test"] = environmentConfiguration,
        });
    }
}
