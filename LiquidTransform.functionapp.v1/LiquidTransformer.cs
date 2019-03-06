using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using DotLiquid;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Text;
using Newtonsoft.Json;
using System;
using System.Xml.Linq;
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
            string responseContentType = req.Headers.Accept.FirstOrDefault().MediaType;
            string requestContentType = req.Content.Headers.ContentType.MediaType;

            // Load the Liquid transform in a string
            var sr = new StreamReader(inputBlob);
            var liquidTransform = sr.ReadToEnd();

            string requestBody = await req.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Please provide a request body");
            }

            //IContentReader contentReader = DependencyInjection.Resolve(typeof(IContentReader), requestContentType, context.FunctionName, context.InvocationId);
            

            Hash inputHash;

            try
            {
                inputHash = ParseRequest(requestBody, requestContentType);

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

            if (responseContentType == "application/json")
            {
                // This will pretty print the JSON output, and also remove redundant comma characters after arrays as a result of for loop 
                // constructs in Liquid, escape strings, and nullify empty numbers
                try
                {
                    JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
                    jsonSerializerSettings.NullValueHandling = NullValueHandling.Include;
                    jsonSerializerSettings.StringEscapeHandling = StringEscapeHandling.Default;

                    var jsonObject = JsonConvert.DeserializeObject(output, jsonSerializerSettings);
                    var jsonString = JsonConvert.SerializeObject(jsonObject, jsonSerializerSettings);

                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(jsonString, Encoding.UTF8, responseContentType)
                    };
                }
                catch (Exception ex)
                {
                    // Just log the error, and return the Liquid output without JSON parsing
                    log.Error(ex.Message, ex);
                }

            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(output, Encoding.UTF8, responseContentType)
            };
        }

        private static Hash ParseRequest(string requestBody, string contentType)
        {
            Hash inputDictionary;
            var transformInput = new Dictionary<string, object>();

            if (contentType.EndsWith("json"))
            {
                // Convert the JSON input to an object tree of primitive types
                var serializer = new JavaScriptSerializer();
                // Let's not shy away from some big JSON files
                serializer.MaxJsonLength = Int32.MaxValue;
                dynamic requestJson = serializer.Deserialize(requestBody, typeof(object));

                // Wrap the JSON input in another content node to provide compatibility with Logic Apps Liquid transformations
                transformInput.Add("content", requestJson);

                inputDictionary = Hash.FromDictionary(transformInput);
            }
            else if (contentType.EndsWith("xml"))
            {
                var xDoc = XDocument.Parse(requestBody);
                var json = JsonConvert.SerializeXNode(xDoc);

                // Convert the XML converted JSON to an object tree of primitive types
                var serializer = new JavaScriptSerializer();
                dynamic requestJson = serializer.Deserialize(json, typeof(object));

                // Wrap the JSON input in another content node to provide compatibility with Logic Apps Liquid transformations
                transformInput.Add("content", requestJson);

                inputDictionary = Hash.FromDictionary(transformInput);
            }
            else
            {
                // Unsupported content type
                throw new NotSupportedException($"Media type {contentType} not supported");
            }

            return inputDictionary;
        }
    }
}
