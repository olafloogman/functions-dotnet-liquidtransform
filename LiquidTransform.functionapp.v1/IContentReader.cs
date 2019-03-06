using DotLiquid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LiquidTransform.functionapp.v1
{
    public interface IContentReader
    {
        Hash ParseRequest(string content);
    }
}
