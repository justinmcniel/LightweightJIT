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

            compiler.CreateVariableRegistry();
            compiler.NewVariable("triggerArgument");
            compiler.NewVariable("triggerer");

            EventListToken? tmp = EventListToken.TryParse(text, ref index, compiler);

            if (tmp == null)
            { return null; }

            res.EventList = tmp;
            return res;
        }

        public string Decompile(string indentation = "") => EventList.Decompile(indentation);
        public List<(string Trigger, Action<object?, IEnumerable<object?>?> Reaction)> Compile(IInteractiveCompiler compiler) => EventList.Compile(compiler);
    }
}
