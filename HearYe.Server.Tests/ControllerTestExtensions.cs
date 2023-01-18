using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HearYe.Server.Tests
{
    public static class ControllerTestExtensions
    {
        public static T WithAnonymousIdentity<T>(this T controller) where T : ControllerBase
        {
            if (controller.ControllerContext is null || controller.ControllerContext.HttpContext is null)
            {
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
            }

            var principal = new ClaimsPrincipal(new ClaimsIdentity());

            controller.ControllerContext.HttpContext.User = principal;

            return controller;
        }

        public static T WithAuthenticatedIdentity<T>(this T controller, string dbId) where T : ControllerBase
        {
            if (controller.ControllerContext is null || controller.ControllerContext.HttpContext is null)
            {
                controller.ControllerContext = new ControllerContext();
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
            }

            var principal = new ClaimsPrincipal
            (
                new ClaimsIdentity
                (
                    new Claim[]
                    {
                        new Claim("extension_DatabaseId", dbId, "http://www.w3.org/2001/XMLSchema#string")
                    }, "AuthUserTest1"
                )
            );

            controller.ControllerContext.HttpContext.User = principal;

            return controller;
        }
    }
}
