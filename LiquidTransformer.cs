using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotLiquid;
using System.Text;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DotLiquid.Extensible.AzFunc.v4
{
    public class LiquidTransformer
    {
        private readonly ILogger<LiquidTransformer> _logger;

        public LiquidTransformer(ILogger<LiquidTransformer> logger)
        {
            _logger = logger;
        }

        [Function("LiquidTransformer")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "liquidtransformer/{liquidtransformfilename}")] HttpRequest req,
            [BlobInput("liquid-transforms/{liquidtransformfilename}")] Stream inputBlob, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            if (inputBlob == null)
            {
                log.LogError("inputBlog null");
                return new NotFoundObjectResult("Liquid transform not found");
            }

            // This indicates the response content type. If set to application/json it will perform additional formatting
            // Otherwise the Liquid transform is returned unprocessed.
            string requestContentType = req.Headers.ContentType.FirstOrDefault() ?? "application/json";
            string responseContentType = req.Headers.Accept.FirstOrDefault() ?? "application/json";

            // Load the Liquid transform in a string
            var sr = new StreamReader(inputBlob);
            var liquidTransform = sr.ReadToEnd();

            var contentReader = ContentFactory.GetContentReader(requestContentType);
            var contentWriter = ContentFactory.GetContentWriter(responseContentType);

            Hash inputHash;

            try
            {
                inputHash = await contentReader.ParseRequestAsync(req.Body);

            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
                return new BadRequestObjectResult("Error parsing request body");
            }

            // Register the Liquid custom filter extensions
            Template.RegisterFilter(typeof(CustomFilters));

            // Execute the Liquid transform
            Template template;

            try
            {
                template = Template.Parse(liquidTransform);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);

                return new BadRequestObjectResult("Error parsing Liquid template");
            }

            string output = string.Empty;

            try
            {
                output = template.Render(inputHash);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
                return new BadRequestObjectResult("Error rendering Liquid template");
            }

            if (template.Errors != null && template.Errors.Count > 0)
            {
                if (template.Errors[0].InnerException != null)
                {
                    return new BadRequestObjectResult($"Error rendering Liquid template: {template.Errors[0].Message} {template.Errors[0].InnerException!.Message}");
                }
                else
                {
                    return new BadRequestObjectResult($"Error rendering Liquid template: {template.Errors[0].Message}");
                }
            }

            try
            {
                var content = contentWriter.CreateResponse(output);

                return new OkObjectResult(content);
            }
            catch (Exception ex)
            {
                // Just log the error, and return the Liquid output without parsing
                log.LogError(ex.Message, ex);

                return new OkObjectResult(new StringContent(output, Encoding.UTF8, responseContentType));
            }
        }
    }
}
