using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ConditionalToken
    {
        private ConditionalPrimeToken? value1 = null;
        private BooleanOperatorToken? operatorToken = null;
        private ConditionalToken? value2 = null;

        public static ConditionalToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        { //780
            ConditionalToken res = new();
            int internalIndex = index;

            res.value1 = ConditionalPrimeToken.TryParse(text, ref internalIndex, compiler);
            if (res.value1 == null)
            { return null; }
            index = internalIndex;

            res.operatorToken = BooleanOperatorToken.TryParse(text, ref internalIndex, compiler);
            if (res.operatorToken != null)
            {
                res.value2 = ConditionalToken.TryParse(text, ref internalIndex, compiler);
                if(res.value2 != null)
                { index = internalIndex; }
                else
                { res.operatorToken = null; }
            }

            return res;
        }
        public string Decompile(string indentation = "")
        {
            string res = value1?.Decompile(indentation) ?? "";
            if(operatorToken != null && value2 != null)
            { res += $" {operatorToken.Decompile(indentation)} {value2.Decompile(indentation)}"; }
            return res;
        }
    }
}
