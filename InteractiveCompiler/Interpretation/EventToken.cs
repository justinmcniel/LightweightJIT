using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class EventToken
    {

        private string? Trigger { get; set;  }
        private ExpressionListToken? ExpressionList { get; set; }
        public static EventToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            int internalIndex = index;

            if (!Utilities.NextTokenMatches(text, ref internalIndex, "On"))
            { return null; }

            int tmpIndex = internalIndex;
            string triggerName = Utilities.NextTextToken(text, ref internalIndex);
            if (tmpIndex == internalIndex || triggerName == "") { return null; }

            if (!compiler.TriggerEventsRegistry.ContainsKey(triggerName))
            { throw new CompilerException($"Failed To find Trigger called {triggerName}"); }

            if (!Utilities.NextTokenMatches(text, ref internalIndex, ":"))
            { return null; }

            EventToken res = new()
            {
                Trigger = triggerName,
                ExpressionList = ExpressionListToken.TryParse(text, ref internalIndex, compiler)
            };

            if (res.ExpressionList == null)
            { return null; }

            index = internalIndex;
            return res;
        }

        public string Decompile(string indentation = "")
        {
            string res = "";
            res += indentation + "On " + Trigger + ": \r\n\t" + ExpressionList?.Decompile(indentation);
            return res;
        }

        public (string Trigger, Action<object?, IEnumerable<object?>?> Reaction) Compile(IInteractiveCompiler compiler)
        {
            if (Trigger == null || ExpressionList == null)
            { throw new CompilerException(); }
            return (Trigger, ExpressionList.Compile(compiler));
        }
    }
}
