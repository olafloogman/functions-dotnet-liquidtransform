using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiquidTransform.functionapp.v1
{
    public static class ContentFactory
    {
        public static IContentReader GetContentReader(string contentType)
        {
            switch (contentType)
            {
                default:
                    return new JsonContentReader(contentType);
            }
        }

        public static IContentWriter GetContentWriter(string acceptHeader)
        {
            return new BasicContentWriter();
        }
    }
}
