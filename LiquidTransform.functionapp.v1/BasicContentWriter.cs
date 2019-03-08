using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LiquidTransform.functionapp.v1
{
    public class BasicContentWriter : IContentWriter
    {
        string _contentType;

        public BasicContentWriter(string contentType)
        {
            _contentType = contentType;
        }

        public StringContent CreateResponse(string output)
        {
            return new StringContent(output, Encoding.UTF8, _contentType);
        }
    }
}
