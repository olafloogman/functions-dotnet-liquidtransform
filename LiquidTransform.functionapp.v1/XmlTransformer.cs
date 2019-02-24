using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using DotLiquid;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Xml;
using System.Web.Script.Serialization;
using System.Dynamic;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Linq;
using System;

namespace LiquidTransform.functionapp.v1
{
    public static class XmlTransformer
    {
        [FunctionName("XmlTransformer")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "xmltransformer/{liquidtransformfilename}")]HttpRequestMessage req,
            [Blob("liquid-transforms/{liquidtransformfilename}", FileAccess.Read)] Stream inputBlob,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (inputBlob == null)
            {
                throw new ArgumentNullException("Liquid transform not found");
            }

            // This indicates the response content type. If set to application/json it will perform additional formatting
            // Otherwise the Liquid transform is returned unprocessed.
            string responseContentType = req.Headers.Accept.FirstOrDefault().MediaType;

            // Load the Liquid transform in a string
            var sr = new StreamReader(inputBlob);
            var liquidTransform = sr.ReadToEnd();

            // Get request body
            var requestBody = await req.Content.ReadAsStringAsync();

            var xDoc = XDocument.Parse(requestBody);
            var json = JsonConvert.SerializeXNode(xDoc);

            // Convert the XML converted JSON to an object tree of primitive types
            var serializer = new JavaScriptSerializer();
            dynamic requestJson = serializer.Deserialize(json, typeof(object));

            // Wrap the JSON input in another content node to provide compatibility with Logic Apps Liquid transformations
            var transformInput = new Dictionary<string, object>();
            transformInput.Add("content", requestJson);

            var inputDictionary = Hash.FromDictionary(transformInput);

            // Execute the Liquid transform
            Template template = Template.Parse(liquidTransform);
            var output = template.Render(inputDictionary);

            if (responseContentType == "application/json")
            {
                // This will pretty print the JSON output, and also remove redundant comma characters after arrays as a result of Liquid for loops
                try
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JObject.Parse(output).ToString(), Encoding.UTF8, responseContentType)
                    };
                }
                catch (System.Exception ex)
                {
                    log.Error(ex.Message, ex);
                }

            }
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(output, Encoding.UTF8, responseContentType)
            };
        }

        private static Dictionary<string, object> GetXmlData(XElement xml)
        {
            var attr = xml.Attributes().ToDictionary(d => d.Name.LocalName, d => (object)d.Value);
            if (xml.HasElements) attr.Add("_value", xml.Elements().Select(e => GetXmlData(e)));
            else if (!xml.IsEmpty) attr.Add("_value", xml.Value);

            return new Dictionary<string, object> { { xml.Name.LocalName, attr } };
        }
    }
}
