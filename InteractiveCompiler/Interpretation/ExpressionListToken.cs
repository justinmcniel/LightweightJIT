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
                    tmp = ExpressionToken.TryParse(text, ref internalIndex, compiler);
                    Utilities.SkipWhitespace(text, ref internalIndex);

                    if (tmp == null)
                    { break; }

                    res.Expressions.Add(tmp);
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
