using InteractiveCompiler.Interpretation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler
{
    public class BaseCompiler : IInteractiveCompiler
    {
        public Dictionary<string, EventHandler<IEnumerable<object?>?>> TriggerEvents { get; } = [];
        public Dictionary<string, Func<IEnumerable<object?>?, object?>> RuntimeFunctionRegistry { get; } = [];
        public Dictionary<string, Func<IEnumerable<object?>?, bool>> ConditionalFunctionRegistry {  get; } = [];
        public Dictionary<Guid, Dictionary<string, object?>> VariableRegistry { get; } = [];
        public Dictionary<Guid, IEnumerable<(EventHandler<IEnumerable<object?>?> Handler, Action<object?, IEnumerable<object?>?> Reaction)>> EventTokensRegistry {  get; } = [];

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

            CompilationThreadProgramLookupTable.Remove(Environment.CurrentManagedThreadId);

            List<(EventHandler Handler, Action<object?, EventArgs> Reaction)> eventLists = [];
            throw new NotImplementedException();

            if(EventTokensRegistry.ContainsKey(program.ID))
            { RemoveProgram(program.ID); }

            //EventTokensRegistry[program.ID] = eventLists;

            return program.ID;
        }
        public bool RemoveProgram(Guid programID)
        {
            bool res = false;
            foreach (var reaction in EventTokensRegistry[programID])
            {
                var handler = reaction.Handler;
                //handler -= reaction.Reaction;

                void tmp123(object? a, IEnumerable<object> b)
                { }
                handler = handler +  tmp123;

                var tmp234 = tmp123;
                handler += tmp234.Invoke;

                var tmp345 = tmp234.Invoke;
                //handler += tmp345;
                
                throw new NotImplementedException();
            }
            res |= EventTokensRegistry.Remove(programID);
            res |= VariableRegistry.Remove(programID);
            return res;
        }
        public void ClearPrograms()
        {
            foreach (var programID in EventTokensRegistry.Keys)
            { _ = RemoveProgram(programID); }
            
            foreach (var programID in VariableRegistry.Keys)
            { _ = RemoveProgram(programID); }
        }

        public bool RegisterTriggerEvent(string eventName, EventHandler<IEnumerable<object?>?>? eventHandler)
        {
            if (eventHandler != null)
            {
                TriggerEvents[eventName] = eventHandler;
            }
            else
            {
                TriggerEvents[eventName] = void (object? sender, IEnumerable<object?>? args) => { };
            }

            return true;
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
