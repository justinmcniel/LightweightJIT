using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class FunctionToken
    {
        private string? funcName = null;
        private Func<IEnumerable<object?>?, object?>? func;
        public static FunctionToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            FunctionToken res = new();
            int internalIndex = index;

            res.funcName = Utilities.NextTextToken(text, ref internalIndex);
            if (compiler.RuntimeFunctionRegistry.TryGetValue(res.funcName, out res.func))
            {
                if (res.func != null)
                {
                    index = internalIndex;
                    return res;
                }
            }

            if (compiler.ConditionalFunctionRegistry.TryGetValue(res.funcName, out var conditionalFunc))
            {
                if (conditionalFunc != null)
                {
                    res.func = (IEnumerable<object?>? args) => { return (object?)conditionalFunc(args); };
                    index = internalIndex;
                    return res;
                }
            }

            return null;
        }
        public string Decompile(string indentation = "") => funcName ?? "";
        public Func<IEnumerable<object?>?, object?> Compile(IInteractiveCompiler compiler)
        {
            if (func == null)
            { throw new CompilerException(); }

            return func;
        }
    }
}
