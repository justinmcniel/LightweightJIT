using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ElseIfExpressionToken
    {
        private List<(ConditionalToken Conditional, ExpressionListToken Expressions)> CodeBlocks { get; } = [];

        private static bool BeginningMatches(string text, ref int index)
        {
            bool else_present = Utilities.NextTokenMatches(text, ref index, "else");
            var tmpIndex = index;
            Utilities.SkipWhitespace(text, ref index);
            bool whiteSpace_present = tmpIndex != index;
            bool if_present = Utilities.NextTokenMatches(text, ref index, "if");
            bool oParen_present = Utilities.NextTokenMatches(text, ref index, "(");
            return else_present && whiteSpace_present && if_present && oParen_present;
        }

        public static ElseIfExpressionToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ElseIfExpressionToken res = new();
            int internalIndex = index;

            int elseifIndex = internalIndex;
            while (BeginningMatches(text, ref elseifIndex))
            {
                ConditionalToken? cond = ConditionalToken.TryParse(text, ref elseifIndex, compiler);
                if(cond != null && Utilities.NextTokenMatches(text, ref elseifIndex, ")"))
                {
                    if (Utilities.NextTokenMatches(text, ref elseifIndex, "{"))
                    {
                        ExpressionListToken? expressions = ExpressionListToken.TryParse(text, ref elseifIndex, compiler);
                        if (expressions != null && Utilities.NextTokenMatches(text, ref elseifIndex, "}"))
                        {
                            res.CodeBlocks.Add((cond, expressions));
                        }
                    }
                }
            }

            if (res.CodeBlocks.Count > 0)
            {
                index = internalIndex;
                return res;
            }

            return null;
        }
    }
}
