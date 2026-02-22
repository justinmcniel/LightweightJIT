using InteractiveCompiler.Interpretation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler
{
    public class BaseCompiler : IInterractiveCompiler
    {
        public Dictionary<string, EventHandler> TriggerEvents { get; } = [];
        public Dictionary<string, Func<IEnumerable<object?>?, object?>> RuntimeFunctionRegistry { get; } = [];
        public Dictionary<string, Func<IEnumerable<object?>?, bool>> ConditionalFunctionRegistry {  get; } = [];
        public Dictionary<Guid, Dictionary<string, object?>> VariableRegistry { get; } = [];
        public Dictionary<Guid, IEnumerable<(EventHandler Handler, Action<object?, EventArgs> function)>> EventTokensRegistry {  get; } = [];

        public Dictionary<int, Guid> CompilationThreadProgramLookupTable { get; } = [];

        public Guid RegisterProgram(string programBody)
        {
            int index = 0;
            ProgramToken? program;
            try
            {
                program = ProgramToken.TryParse(programBody, ref index, this);
            }
            catch { return Guid.Empty; }

            if(index == 0 ||  program == null)
            { return Guid.Empty; }

            if (CompilationThreadProgramLookupTable.ContainsKey(Environment.CurrentManagedThreadId))
            { CompilationThreadProgramLookupTable.Remove(Environment.CurrentManagedThreadId); }

            List<(EventHandler Handler, Action<object?, EventArgs> function)> eventLists = [];
            throw new NotImplementedException();

            if(EventTokensRegistry.ContainsKey(program.ID))
            { RemoveProgram(program.ID); }

            EventTokensRegistry[program.ID] = eventLists;

            return program.ID;
        }
        public bool RemoveProgram(Guid programID)
        {
            throw new NotImplementedException();
        }
        public void ClearPrograms()
        {
            throw new NotImplementedException();
        }

        public bool RegisterTriggerEvent(string eventName, EventHandler<IEnumerable<object?>?> eventHandler)
        {
            throw new NotImplementedException();
        }
        public bool RemoveTriggerEvent(string eventName)
        {
            throw new NotImplementedException();
        }
        public void ClearTriggerEvents()
        {
            throw new NotImplementedException();
        }

        public bool RegisterRuntimeFunction(string functionName, Func<IEnumerable<object?>?, object?> function)
        {
            throw new NotImplementedException();
        }
        public bool RemoveRuntimeFunction(string functionName)
        {
            throw new NotImplementedException();
        }
        public void ClearRuntimeFunctions()
        {
            throw new NotImplementedException();
        }

        public bool RegisterConditionalFunction(string functionName, Func<IEnumerable<object?>?, bool> function)
        {
            throw new NotImplementedException();
        }
        public bool RemoveConditionalFunction(string functionName)
        {
            throw new NotImplementedException();
        }
        public void ClearConditionalFunctions()
        {
            throw new NotImplementedException();
        }

        public bool RegisterProperty(string propertyName, Func<object?> getter, Action<object?> setter)
        {
            throw new NotImplementedException();
        }
        public bool RemoveProperty(string propertyName)
        {
            throw new NotImplementedException();
        }
        public void ClearProperties()
        {
            throw new NotImplementedException();
        }
    }
}
