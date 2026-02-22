using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ValueToken
    {
        private string? variableName = null;
        private ImmediateToken? immediateToken = null;
        private FunctionCallToken? funcCallToken = null;
        public static ValueToken? TryParse(string text, ref int index, IInterractiveCompiler compiler)
        {
            int internalIndex = index;
            ValueToken res = new();

            int tmpIndex = internalIndex;
            res.variableName = Utilities.NextTextToken(text, ref internalIndex);
            if (!compiler.VariableRegistry.TryGetValue(Utilities.GetCurrentCompilationThreadProgramId(compiler), out var variableRegistry))
            { throw new CompilerException(); }

            if (!String.IsNullOrEmpty(res.variableName) && variableRegistry.ContainsKey(res.variableName))
            {
                index = internalIndex;
                return res;
            }
            res.variableName = null;
            internalIndex = tmpIndex;

            res.immediateToken = ImmediateToken.TryParse(text, ref internalIndex, compiler);
            if (res.immediateToken != null)
            {
                index = internalIndex;
                return res;
            }
            //res.immediateToken = null; //implicit
            internalIndex = tmpIndex;

            res.funcCallToken = FunctionCallToken.TryParse(text, ref internalIndex, compiler);
            if (res.funcCallToken != null)
            {
                index = internalIndex;
                return res;
            }
            //res.funcCallToken = null; //implicit and irrelevant
            //internalIndex = tmpIndex; //implicit and irrelevant

            return null;
        }
    }
}
