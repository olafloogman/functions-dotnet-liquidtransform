using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using DotLiquid;
using Newtonsoft.Json;

namespace DotLiquid.Extensible.AzFunc.v4
{
    public class JsonContentReader : IContentReader
    {
        public JsonContentReader()
        {

        }

        public async Task<Hash> ParseRequestAsync(Stream content)
        {
            using (var sr = new StreamReader(content))
            {
                string requestBody = await sr.ReadToEndAsync();

                var transformInput = new Dictionary<string, object>();

                var requestJson = JsonConvert.DeserializeObject<IDictionary<string, object>>(requestBody, new DictionaryConverter());

                // Wrap the JSON input in another content node to provide compatibility with Logic Apps Liquid transformations
                transformInput.Add("content", requestJson);

                return Hash.FromDictionary(transformInput);
            }
        }
    }
}
