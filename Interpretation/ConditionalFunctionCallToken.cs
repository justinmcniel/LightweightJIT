using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ConditionalFunctionCallToken
    {
        private Func<IEnumerable<object?>?, bool>? func;
        public static ConditionalFunctionCallToken? TryParse(string text, ref int index, IInterractiveCompiler compiler)
        {
            ConditionalFunctionCallToken res = new();
            int internalIndex = index;

            string funcName = Utilities.NextTextToken(text, ref internalIndex);

            if (compiler.ConditionalFunctionRegistry.TryGetValue(funcName, out res.func))
            {
                if (res.func != null)
                {
                    index = internalIndex;
                    return res;
                }
            }

            return null;
        }
    }
}
