using System;
using System.Collections.Generic;
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
                }
                catch (IndexOutOfRangeException)
                { break; }
            }

            if (res.Events.Count == 0)
            { return null; }

            index = internalIndex;
            return res;
        }
    }
}
