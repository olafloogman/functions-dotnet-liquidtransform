using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;

namespace DotLiquid.Extensible.AzFunc.v4
{
    public class CsvContentReader : IContentReader
    {
        public async Task<Hash> ParseRequestAsync(Stream content)
        {
            var transformInput = new Dictionary<string, object>();

            List<object[]> csv = new List<object[]>();

            StreamReader sr = new StreamReader(content);
            while (!sr.EndOfStream)
            {
                var line = await sr.ReadLineAsync();

                csv.Add(line.Split(','));
            }

            transformInput.Add("content", csv.ToArray<object>());

            return Hash.FromDictionary(transformInput);
        }
    }
}
