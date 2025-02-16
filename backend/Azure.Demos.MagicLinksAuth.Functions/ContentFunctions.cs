using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Azure.Demos.MagicLinksAuth.Functions
{
    public static class ContentFunctions
    {
        [FunctionName(nameof(GetContentSummary))]
        public static async Task<IActionResult> GetContentSummary(
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
        public static async Task<HttpResponseMessage> GetContent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "content/{courseSlug}/{contentSlug}")] HttpRequest req,
            string courseSlug,
            string contentSlug,
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("Requesting specific chapter");

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
