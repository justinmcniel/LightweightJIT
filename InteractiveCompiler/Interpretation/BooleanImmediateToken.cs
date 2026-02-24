using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class BooleanImmediateToken
    {
        public bool? Value { get; private set; }
        public static BooleanImmediateToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            BooleanImmediateToken res = new();
            int internalIndex = index;

            if (Utilities.NextTokenMatches(text, ref internalIndex, "true")) 
            { res.Value = true; }
            else if (Utilities.NextTokenMatches(text, ref internalIndex, "false")) 
            { res.Value = false; }

            if(res.Value == null)
            { return null; }

            index = internalIndex;
            return res;
        }
        public string Decompile(string indentation = "")
        {
            if (Value == null)
            { return ""; }
            return (bool)Value ? "true" : "false";
        }

        public Func<bool> Compile(IInteractiveCompiler compiler)
        {
            if (Value == null)
            { throw new CompilerException(); }

            return ((bool)Value) ? (() => true) : (() => false);
        }
    }
}
