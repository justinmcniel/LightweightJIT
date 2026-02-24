using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ConditionalFunctionCallToken
    {
        private Func<IEnumerable<object?>?, bool>? func = null;
        private ArgumentListToken? argListToken = null;
        public static ConditionalFunctionCallToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ConditionalFunctionCallToken res = new();
            int internalIndex = index;

            string funcName = Utilities.NextTextToken(text, ref internalIndex);

            if (compiler.ConditionalFunctionRegistry.TryGetValue(funcName, out res.func) && res.func != null)
            {
                if (Utilities.NextTokenMatches(text, ref internalIndex, "("))
                {
                    res.argListToken = ArgumentListToken.TryParse(text, ref internalIndex, compiler);
                    if (res.argListToken != null)
                    {
                        if (Utilities.NextTokenMatches(text, ref internalIndex, ")"))
                        {
                            index = internalIndex;
                            return res;
                        }
                    }
                }
            }

            return null;
        }
    }
}
