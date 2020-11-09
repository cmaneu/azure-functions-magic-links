using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Demos.MagicLinksAuth.Functions.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Azure.Demos.MagicLinksAuth.Functions
{
    public class ContentFunctions
    {
        private readonly IAccessTokenProvider _tokenProvider;
        
        public ContentFunctions(IAccessTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }
        
        [FunctionName(nameof(GetContentSummaryOptions))]
        public IActionResult GetContentSummaryOptions(
            [HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "content/{courseSlug}")]
            HttpRequest req,
            string courseSlug,
            ExecutionContext context,
            ILogger log)
        {
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", new StringValues("true"));
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", new StringValues("*"));
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", new StringValues("Content-Type, Set-Cookie"));
            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", new StringValues("GET, POST, OPTIONS"));

            return new OkObjectResult(null);
        }

        [FunctionName(nameof(GetContentSummary))]
        public IActionResult GetContentSummary(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "content/{courseSlug}")] HttpRequest req,
            string courseSlug,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("Requesting course summary");
            List<string> contentItems;

            // Check if someone is trying to do path transversal navigation
            if(courseSlug.Contains("."))
                return new BadRequestResult();

            string directoryPath = Path.Combine(context.FunctionDirectory, $"../Assets/courses/{courseSlug}");
            
            if (!Directory.Exists(directoryPath))
            {
                log.LogInformation($"Course {courseSlug} does not exists.");
                return new BadRequestResult();
            }

            var files = Directory.GetFiles(directoryPath, "*.md");
            contentItems = files.Select(Path.GetFileNameWithoutExtension).ToList();

            return new OkObjectResult(contentItems);
        }

        [FunctionName(nameof(GetContent))]
        public async Task<HttpResponseMessage> GetContent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "content/{courseSlug}/{contentSlug}")] HttpRequest req,
            string courseSlug,
            string contentSlug,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("Requesting specific chapter");

            var auth = _tokenProvider.ValidateToken(req);

            if (auth.Status != AccessTokenStatus.Valid)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            // Check if someone is trying to do path transversal navigation
            if (courseSlug.Contains(".") || contentSlug.Contains("."))
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            string filePath = Path.Combine(context.FunctionDirectory, $"../Assets/courses/{courseSlug}", contentSlug + ".md");

            if (!File.Exists(filePath))
            {
                log.LogInformation($"File {courseSlug}/{contentSlug} does not exists.");
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            string fileContents = await File.ReadAllTextAsync(filePath);
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            responseMessage.Content = new StringContent(fileContents);
            responseMessage.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = true,
                MaxAge = TimeSpan.FromSeconds(3600)
            };
            return responseMessage;
        }
    }
}
