using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LiquidTransform.functionapp.v1
{
    public class JsonContentWriter : IContentWriter
    {
        string _contentType;

        public JsonContentWriter(string contentType)
        {
            _contentType = contentType;
        }

        public StringContent CreateResponse(string output)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.NullValueHandling = NullValueHandling.Include;
            jsonSerializerSettings.StringEscapeHandling = StringEscapeHandling.Default;

            var jsonObject = JsonConvert.DeserializeObject(output, jsonSerializerSettings);
            var jsonString = JsonConvert.SerializeObject(jsonObject, jsonSerializerSettings);

            return new StringContent(jsonString, Encoding.UTF8, _contentType);
        }
    }
}
