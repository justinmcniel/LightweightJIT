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

        public string Decompile(string indentation = "")
        {
            string res = "";
            if (!String.IsNullOrEmpty(AssignedVariableName) && valueToken != null)
            {
                res += $"let {AssignedVariableName} = {valueToken.Decompile(indentation)};" + $"\r\n{indentation}";
            }
            else if (funcCallToken != null)
            {
                res += $"{funcCallToken.Decompile(indentation)};" + $"\r\n{indentation}";
            }
            else if (condExpToken != null)
            {
                res += condExpToken.Decompile(indentation) + $"\r\n{indentation}";
            }
            return res;
        }

        public Action Compile(IInteractiveCompiler compiler)
        {
            if (!String.IsNullOrEmpty(AssignedVariableName) && valueToken != null)
            {
                var VariableSetter = compiler.VariableSetter(AssignedVariableName);
                var ValueGetter = valueToken.Compile(compiler);
                return () => { VariableSetter(ValueGetter()); };
            }
            else if (funcCallToken != null)
            {
                return () => funcCallToken.Compile(compiler);
            }
            else if (condExpToken != null)
            {
                return condExpToken.Compile(compiler);
            }
            throw new CompilerException();
        }
    }
}
