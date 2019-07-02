using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LiquidTransform.functionapp.v2
{
    public interface IContentWriter
    {
        StringContent CreateResponse(string output);
    }
}
