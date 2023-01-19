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

        public static T WithAuthenticatedIdentity<T>(this T controller, string dbId, string aadOid = "f09cc0b1-f05d-40e0-9684-c4a945d4e7e0") 
            where T : ControllerBase
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
                        new Claim("extension_DatabaseId", dbId, "http://www.w3.org/2001/XMLSchema#string"),
                        new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", aadOid, "http://www.w3.org/2001/XMLSchema#string")
                    }, "AuthUserTest1"
                )
            );

            controller.ControllerContext.HttpContext.User = principal;

            return controller;
        }
    }
}
