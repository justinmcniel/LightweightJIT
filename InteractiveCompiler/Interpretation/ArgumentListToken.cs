using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ArgumentListToken
    {
        private List<ValueToken> Values { get; } = [];
        public static ArgumentListToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ArgumentListToken res = new();

            ValueToken? val;
            do
            {
                val = ValueToken.TryParse(text, ref index, compiler);
                if (val != null)
                { res.Values.Add(val); }

                if(!Utilities.NextTokenMatches(text, ref index, ","))
                { break; }
            } while (val != null);

            return res;
        }
    }
}
