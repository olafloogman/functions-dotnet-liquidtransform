using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;

namespace LiquidTransform.functionapp.v1
{
    public class XmlContentReader : IContentReader
    {
        public XmlContentReader(string contentType)
        {

        }

        public Hash ParseRequest(string content)
        {
            throw new NotImplementedException();
        }
    }
}
