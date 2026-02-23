using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ProgramToken
    {
        private EventListToken EventList { get; set; } = new();

        public Guid ID { get; private set; }
        public static ProgramToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ProgramToken res = new()
            { ID = Guid.NewGuid() };

            compiler.CompilationThreadProgramLookupTable[Environment.CurrentManagedThreadId] = res.ID;

            compiler.VariableRegistry[res.ID] = [];
            compiler.VariableRegistry[res.ID]["triggerArgument"] = null;

            EventListToken? tmp = EventListToken.TryParse(text, ref index, compiler);

            compiler.CompilationThreadProgramLookupTable.Remove(Environment.CurrentManagedThreadId);
            if (tmp == null)
            { return null; }

            res.EventList = tmp;
            return res;
        }
    }
}
