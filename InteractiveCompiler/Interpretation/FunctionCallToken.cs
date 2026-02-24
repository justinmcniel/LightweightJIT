using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class FunctionCallToken
    {
        private FunctionToken? funcToken = null;
        private ConditionalFunctionCallToken? condFuncCall = null;
        private ArgumentListToken? argListToken = null;
        public static FunctionCallToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            FunctionCallToken res = new();
            int internalIndex = index;

            res.funcToken = FunctionToken.TryParse(text, ref internalIndex, compiler);
            if (res.funcToken != null)
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

            internalIndex = index;
            res.condFuncCall = ConditionalFunctionCallToken.TryParse(text, ref internalIndex, compiler);
            if (res.condFuncCall != null)
            {
                index = internalIndex;
                return res;
            }

            return null;
        }
        public string Decompile(string indentation = "")
        {
            if (condFuncCall != null)
            { return condFuncCall.Decompile(indentation); }

            return $"{funcToken?.Decompile(indentation)}({argListToken?.Decompile(indentation)})";
        }
    }
}
