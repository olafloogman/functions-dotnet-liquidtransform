using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using DotLiquid;
using Newtonsoft.Json;

namespace LiquidTransform.functionapp.v1
{
    public class XmlContentReader : IContentReader
    {
        public XmlContentReader()
        {

        }

        public async Task<Hash> ParseRequestAsync(HttpContent content)
        {
            string requestBody = await content.ReadAsStringAsync();
            
            var transformInput = new Dictionary<string, object>();

            var xDoc = XDocument.Parse(requestBody);
            var json = JsonConvert.SerializeXNode(xDoc);

            // Convert the XML converted JSON to an object tree of primitive types
            var serializer = new JavaScriptSerializer();
            dynamic requestJson = serializer.Deserialize(json, typeof(object));

            // Wrap the JSON input in another content node to provide compatibility with Logic Apps Liquid transformations
            transformInput.Add("content", requestJson);

            return Hash.FromDictionary(transformInput);
        }
    }
}
