﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DotLiquid.Extensible.AzFunc.v4
{
    public interface IContentWriter
    {
        StringContent CreateResponse(string output);
    }
}