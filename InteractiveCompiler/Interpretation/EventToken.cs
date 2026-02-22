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

        private EventHandler? Trigger { get; set;  }
        private ExpressionListToken? ExpressionList { get; set; }
        public static EventToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            int internalIndex = index;

            if (!Utilities.NextTokenMatches(text, ref internalIndex, "On"))
            { return null; }

            int tmpIndex = internalIndex;
            string triggerName = Utilities.NextTextToken(text, ref tmpIndex);
            if (tmpIndex == internalIndex || triggerName == "") { return null; }

            if (!compiler.TriggerEvents.TryGetValue(triggerName, out var triggerHandler))
            { throw new CompilerException($"Failed To find Trigger called {triggerName}"); }

            EventToken res = new()
            {
                Trigger = triggerHandler,
                ExpressionList = ExpressionListToken.TryParse(text, ref internalIndex, compiler)
            };

            if(res.ExpressionList == null)
            { return null; }

            index = internalIndex;
            return res;
        }
    }
}
