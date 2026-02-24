using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class EventListToken
    {
        private List<EventToken> Events { get; } = [];
        public static EventListToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            EventListToken res = new();

            EventToken? tmp;
            int internalIndex = index;
            while(true)
            {
                try
                {
                    tmp = EventToken.TryParse(text, ref internalIndex, compiler);

                    Utilities.SkipWhitespace(text, ref internalIndex);

                    if (tmp == null)
                    { //if one EventToken fails to compile, we can still compile the next
                        int nextEventStart = Utilities.NextInstanceOf(text, internalIndex + 1, "On");
                        if (nextEventStart >= text.Length)
                        { break; }

                        internalIndex = nextEventStart;
                    }
                    else
                    {
                        res.Events.Add(tmp);
                    }
                }
                catch (IndexOutOfRangeException) { break; }
                catch (CompilerException) { internalIndex++; } //ignore, and try to find the next Event
            }

            if (res.Events.Count == 0)
            { return null; }

            index = internalIndex;
            return res;
        }

        public string Decompile(string indentation = "")
        {
            string res = "";
            foreach (var eventToken in Events)
            { res += eventToken.Decompile(indentation)  + "\r\n\r\n"; }
            return res;
        }

        public List<(string Trigger, Action<object?, IEnumerable<object?>?> Reaction)> Compile(IInteractiveCompiler compiler)
        {
            List<(string Trigger, Action<object?, IEnumerable<object?>?> Reaction)> res = [];
            foreach(var eventToken in Events)
            { res.Add(eventToken.Compile(compiler)); }
            return res;
        }
    }
}
