using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ConditionalExpressionToken
    {
        ConditionalToken? ifConditionalToken = null;
        ExpressionListToken? ifExpressionListToken = null;
        ElseIfExpressionToken? elseIfExpressionToken = null;
        ExpressionListToken? elseExpressionList = null;
        public static ConditionalExpressionToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ConditionalExpressionToken res = new();
            int internalIndex = index;

            if (!Utilities.NextTokenMatches(text, ref internalIndex, "if")) { return null; }

            if (!Utilities.NextTokenMatches(text, ref internalIndex, "(")) { return null; }

            res.ifConditionalToken = ConditionalToken.TryParse(text, ref internalIndex, compiler);
            if (res.ifConditionalToken == null) { return null; }

            if (!Utilities.NextTokenMatches(text, ref internalIndex, ")")) { return null; }

            if (!Utilities.NextTokenMatches(text, ref internalIndex, "{")) { return null; }

            res.ifExpressionListToken = ExpressionListToken.TryParse(text, ref internalIndex, compiler);
            if(res.ifExpressionListToken == null) { return null; }

            if (!Utilities.NextTokenMatches(text, ref internalIndex, "}")) { return null; }

            res.elseIfExpressionToken = ElseIfExpressionToken.TryParse(text, ref internalIndex, compiler);

            int elseIndex = internalIndex;
            if(Utilities.NextTokenMatches(text, ref elseIndex, "else"))
            {
                if(Utilities.NextTokenMatches(text, ref elseIndex, "{"))
                {
                    res.elseExpressionList = ExpressionListToken.TryParse(text, ref elseIndex, compiler);
                    if (res.elseExpressionList != null)
                    {
                        if(Utilities.NextTokenMatches(text, ref elseIndex, "}"))
                        {
                            internalIndex = elseIndex;
                        }
                        else
                        {
                            res.elseExpressionList = null;
                        }
                    }
                }
            }

            index = internalIndex;
            return res;
        }
    }
}
