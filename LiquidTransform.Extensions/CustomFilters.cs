using DotLiquid;
using System;
using System.Collections.Generic;
using System.Text;

namespace LiquidTransform.Extensions
{
    public static class CustomFilters
    {
        public static string Padleft(Context context, string input, int totalWidth, string padChar = " ")
        {
            return input.PadLeft(totalWidth, padChar[0]);
        }

        public static string Padright(Context context, string input, int totalWidth, string padChar = " ")
        {
            return input.PadRight(totalWidth, padChar[0]);
        }
    }
}
