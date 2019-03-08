using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;

namespace LiquidTransform.functionapp.v1
{
    public class CsvContentReader : IContentReader
    {
        public async Task<Hash> ParseRequestAsync(HttpContent content)
        {
            var stream = await content.ReadAsStreamAsync();

            throw new NotImplementedException();
        }
    }
}
