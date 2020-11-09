using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Azure.Demos.MagicLinksAuth.Functions
{
    public static class AuthFunctions
    {
        [FunctionName(nameof(Login))]
        public static async Task<IActionResult> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = ".auth/login")] HttpRequest req,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            log.LogInformation("Authentication request");

            string email = await req.ReadAsStringAsync();

            // Create a request Id
            string requestId = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            // Create the actor
            var entityId = new EntityId(nameof(OneTimeAuthenticationActor), requestId);

            //await client.SignalEntityAsync<IOneTimeAuthenticationActor>(entityId, proxy => proxy.SetRequestId(requestId));
            await client.SignalEntityAsync<IOneTimeAuthenticationActor>(entityId, proxy => proxy.SetEmail(email));
            
            return new OkObjectResult(requestId);
        }


        [FunctionName(nameof(AuthCallback))]
        public static async Task<IActionResult> AuthCallback(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ".auth/callback/{requestId}")] HttpRequest req,
            string requestId,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            log.LogInformation("Callback request");

            var entityId = new EntityId(nameof(OneTimeAuthenticationActor), requestId);

            string jwtToken;
            string redirectUrl;

            // We ensure that nobody is using this entity while we're delete it.
            EntityStateResponse<OneTimeAuthenticationActor> stateResponse = await client.ReadEntityStateAsync<OneTimeAuthenticationActor>(entityId);
            if (!stateResponse.EntityExists)
            {
                return new BadRequestResult();
            }
            await client.SignalEntityAsync<IOneTimeAuthenticationActor>(entityId, proxy => proxy.Delete());
            log.LogInformation("Entity exists");

            jwtToken = stateResponse.EntityState.JwtToken;
            redirectUrl = stateResponse.EntityState.RedirectUri;

            // Set the JWT token in header
            var cookieOptions = new CookieOptions();
            cookieOptions.Expires = DateTime.Now.AddDays(20);
            cookieOptions.HttpOnly = true;
            cookieOptions.Secure = true;
            cookieOptions.SameSite = SameSiteMode.None;

            req.HttpContext.Response.Cookies.Append("auth.token", jwtToken, cookieOptions);
            
            return new OkObjectResult(jwtToken);

        }
    }
}
