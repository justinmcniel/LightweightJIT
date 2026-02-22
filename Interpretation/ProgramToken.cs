using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ProgramToken
    {
        private EventListToken EventList { get; } = new();

        public Guid ID { get; private set; }
        public static ProgramToken? TryParse(string text, ref int index, IInterractiveCompiler compiler)
        {
            ProgramToken res = new()
            { ID = Guid.NewGuid() };

            compiler.CompilationThreadProgramLookupTable[Environment.CurrentManagedThreadId] = res.ID;
            EventListToken? tmp = EventListToken.TryParse(text, ref index, compiler);
            compiler.CompilationThreadProgramLookupTable.Remove(Environment.CurrentManagedThreadId);
            if (tmp == null)
            { return null; }

            return res;
        }
    }
}
