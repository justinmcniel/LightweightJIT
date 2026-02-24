using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ExpressionToken
    {
        private ValueToken? valueToken = null;
        private string? AssignedVariableName = null;
        private FunctionCallToken? funcCallToken = null;
        private ConditionalExpressionToken? condExpToken = null;
        public static ExpressionToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            int internalIndex = index;
            ExpressionToken res = new();

            if (Utilities.NextTokenMatches(text, ref internalIndex, "let"))
            { // Variable Assignment
                string varName = Utilities.NextTextToken(text, ref internalIndex);

                if (!String.IsNullOrEmpty(varName) && Utilities.NextTokenMatches(text, ref internalIndex, "="))
                {
                    res.valueToken = ValueToken.TryParse(text, ref internalIndex, compiler);
                    if (res.valueToken != null)
                    {
                        if (Utilities.NextTokenMatches(text, ref internalIndex, ";"))
                        {
                            compiler.NewVariable(varName);
                            res.AssignedVariableName = varName;
                            index = internalIndex;
                            return res;
                        }
                    }
                }
            }
            res.valueToken = null;
            internalIndex = index;

            res.funcCallToken = FunctionCallToken.TryParse(text, ref internalIndex, compiler);
            if (res.funcCallToken != null)
            {
                if (Utilities.NextTokenMatches(text, ref internalIndex, ";"))
                {
                    index = internalIndex;
                    return res;
                }
            }
            res.funcCallToken = null;
            internalIndex = index;

            res.condExpToken = ConditionalExpressionToken.TryParse(text, ref internalIndex, compiler);
            if (res.condExpToken != null)
            {
                index = internalIndex;
                return res;
            }
            //res.condExpToken = null; //implicit and irrelevant
            //internalIndex = tmpIndex; //implicit and irrelevant

            return null;
        }
    }
}
