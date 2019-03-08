using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using DotLiquid;
using System.Text;
using System;
using LiquidTransform.Extensions;

namespace LiquidTransform.functionapp.v1
{
    public static class LiquidTransformer
    {
        /// <summary>
        /// Converts Json to XML using a Liquid mapping. The filename of the liquid map needs to be provided in the path. 
        /// The tranformation is executed with the HTTP request body as input.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="inputBlob"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("LiquidTransformer")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "liquidtransformer/{liquidtransformfilename}")] HttpRequestMessage req,
            [Blob("liquid-transforms/{liquidtransformfilename}", FileAccess.Read)] Stream inputBlob,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (inputBlob == null)
            {
                log.Error("inputBlog null");
                return req.CreateErrorResponse(HttpStatusCode.NotFound, "Liquid transform not found");
            }

            // This indicates the response content type. If set to application/json it will perform additional formatting
            // Otherwise the Liquid transform is returned unprocessed.
            string requestContentType = req.Content.Headers.ContentType.MediaType;
            string responseContentType = req.Headers.Accept.FirstOrDefault().MediaType;

            // Load the Liquid transform in a string
            var sr = new StreamReader(inputBlob);
            var liquidTransform = sr.ReadToEnd();

            var contentReader = ContentFactory.GetContentReader(requestContentType);
            var contentWriter = ContentFactory.GetContentWriter(responseContentType);

            Hash inputHash;

            try
            {
                inputHash = await contentReader.ParseRequestAsync(req.Content);

            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error parsing request body", ex);
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
                log.Error(ex.Message, ex);
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error parsing Liquid template", ex);
            }

            string output = string.Empty;

            try
            {
                output = template.Render(inputHash);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error rendering Liquid template", ex);
            }

            if (template.Errors != null && template.Errors.Count > 0)
            {
                if (template.Errors[0].InnerException != null)
                {
                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error rendering Liquid template: {template.Errors[0].Message}", template.Errors[0].InnerException);
                }
                else
                {
                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error rendering Liquid template: {template.Errors[0].Message}");
                }
            }

            try
            {
                var content = contentWriter.CreateResponse(output);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = content
                };
            }
            catch (Exception ex)
            {
                // Just log the error, and return the Liquid output without parsing
                log.Error(ex.Message, ex);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(output, Encoding.UTF8, responseContentType)
                };
            }
        }
    }
}
