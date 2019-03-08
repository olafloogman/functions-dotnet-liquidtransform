using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using DotLiquid;

namespace LiquidTransform.functionapp.v1
{
    public class JsonContentReader : IContentReader
    {
        public JsonContentReader()
        {

        }

        public async Task<Hash> ParseRequestAsync(HttpContent content)
        {
            string requestBody = await content.ReadAsStringAsync();

            var transformInput = new Dictionary<string, object>();
            var serializer = new JavaScriptSerializer();
            // Let's not shy away from some big JSON files
            serializer.MaxJsonLength = Int32.MaxValue;
            dynamic requestJson = serializer.Deserialize(requestBody, typeof(object));

            // Wrap the JSON input in another content node to provide compatibility with Logic Apps Liquid transformations
            transformInput.Add("content", requestJson);

            return Hash.FromDictionary(transformInput);
        }
    }
}
