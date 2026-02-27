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
                        else
                        {
                            compiler.LogError($"ERROR: {Utilities.GetPosition(text, internalIndex)} " +
                                $"Was expecting {Utilities.ReadableSymbol(")")}, " +
                                $"but got {Utilities.NextTokenReadable(text, internalIndex)} instead");
                            return null;
                        }
                    }
                }
                else if (!compiler.VariableExists(res.funcToken.funcName ?? ""))
                {
                    compiler.LogError($"ERROR: {Utilities.GetPosition(text, internalIndex)} " +
                        $"Was expecting {Utilities.ReadableSymbol("(")}, " +
                        $"but got {Utilities.NextTokenReadable(text, internalIndex)} instead");
                    return null;
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

        public Func<object?> Compile(IInteractiveCompiler compiler)
        {
            if (condFuncCall != null)
            {
                var condFunc = condFuncCall.Compile(compiler);
                return () => condFunc(); 
            }

            if(funcToken == null)
            { throw new CompilerException(); }
            var func = funcToken.Compile(compiler);
            var argumentGetter = (argListToken?.Compile(compiler)) ?? (() => null);
            return () => func(argumentGetter());
        }
    }
}
