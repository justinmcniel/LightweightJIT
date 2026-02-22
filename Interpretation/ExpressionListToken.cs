using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ExpressionListToken
    {
        private List<ExpressionToken> Expressions { get; } = [];
        public static ExpressionListToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ExpressionListToken res = new();

            ExpressionToken? tmp;
            int internalIndex = index;

            while(true)
            {
                try
                {
                    int tmpIndex = internalIndex;
                    tmp = ExpressionToken.TryParse(text, ref tmpIndex, compiler);
                    if (!Utilities.NextTokenMatches(text, ref tmpIndex, ";"))
                    { break; }
                    internalIndex = tmpIndex;

                    Utilities.SkipWhitespace(text, ref internalIndex);

                    if (tmp == null)
                    { break; }
                }
                catch (IndexOutOfRangeException)
                { break; }
            }

            if ( res.Expressions.Count == 0)
            { return null; }

            index = internalIndex;
            return res;
        }
    }
}
