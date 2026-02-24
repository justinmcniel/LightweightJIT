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
        public Dictionary<string, List<(Action<object?, IEnumerable<object?>?> Reaction, Guid ProgramID)>> TriggerEventsRegistry { get; } = [];
        public Dictionary<string, Func<IEnumerable<object?>?, object?>> RuntimeFunctionRegistry { get; } = [];
        public Dictionary<string, Func<IEnumerable<object?>?, bool>> ConditionalFunctionRegistry {  get; } = [];
        public Dictionary<Guid, Dictionary<string, object?>> VariableRegistry { get; } = [];
        public Dictionary<int, Guid> CompilationThreadProgramLookupTable { get; } = [];

        public BaseCompiler()
        {
            //
        }

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

            List<(string Trigger, Action<object?, IEnumerable<object?>?> Reaction)> eventsList = [];
            throw new NotImplementedException();

            foreach(var (Trigger, Reaction) in eventsList)
            {
                if (!TriggerEventsRegistry.TryGetValue(Trigger, out var reactionList))
                {
                    reactionList = [];
                    TriggerEventsRegistry[Trigger] = reactionList;
                }
                reactionList.Add((Reaction, program.ID));
            }

            return program.ID;
        }
        public bool RemoveProgram(Guid programID)
        {
            bool res = false;
            foreach (var kvp in TriggerEventsRegistry)
            { TriggerEventsRegistry[kvp.Key] = kvp.Value.Where(set =>
            {
                return set.ProgramID != programID;
            }).ToList() ?? []; }

            res |= VariableRegistry.Remove(programID);
            return res;
        }
        public void ClearPrograms()
        {
            foreach (var trigger in TriggerEventsRegistry.Keys)
            { TriggerEventsRegistry[trigger].Clear(); }
            
            foreach (var programID in VariableRegistry.Keys)
            { _ = RemoveProgram(programID); }
        }

        public bool RegisterTriggerEvent(string eventName, ref EventHandler<object?>? eventHandler)
        {
            void Invoker(object? sender, object? triggerArgument)
            {
                foreach(var (Reaction, ProgramID) in TriggerEventsRegistry[eventName])
                {
                    VariableRegistry[ProgramID]["triggerArgument"] = triggerArgument;
                    Reaction.Invoke(sender, [ProgramID]);
                }
            };
            eventHandler += Invoker;
            TriggerEventsRegistry[eventName] = []; // Log this?

            return true;
        }
        
        public bool RemoveTriggerEvent(string eventName) => TriggerEventsRegistry.Remove(eventName);

        public void ClearTriggerEvents() => TriggerEventsRegistry.Clear();

        public bool RegisterRuntimeFunction(string functionName, Func<IEnumerable<object?>?, object?> function)
        {
            RuntimeFunctionRegistry[functionName] = function;
            return true;
        }

        public bool RemoveRuntimeFunction(string functionName) => RuntimeFunctionRegistry.Remove(functionName);

        public void ClearRuntimeFunctions() => RuntimeFunctionRegistry.Clear();

        public bool RegisterConditionalFunction(string functionName, Func<IEnumerable<object?>?, bool> function)
        {
            ConditionalFunctionRegistry[functionName] = function;
            return true;
        }

        public bool RemoveConditionalFunction(string functionName) => ConditionalFunctionRegistry.Remove(functionName);

        public void ClearConditionalFunctions() => ConditionalFunctionRegistry.Clear();

        public bool RegisterProperty(string propertyName, Func<object?> Getter, Action<object?> Setter)
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

        public Guid GetThreadsProgramID()
        {
            if (!CompilationThreadProgramLookupTable.TryGetValue(Environment.CurrentManagedThreadId, out var res))
            { return Guid.Empty; }
            return res;
        }

        public void NewVariable(string name)
        {
            name = name.Trim();
            if (!VariableRegistry.TryGetValue(GetThreadsProgramID(), out var variableRegistry))
            { throw new CompilerException(); }
            variableRegistry[name] = null;
        }

        public bool VariableExists(string name)
        {
            name = name.Trim();
            if (!VariableRegistry.TryGetValue(GetThreadsProgramID(), out var variableRegistry))
            { throw new CompilerException(); }
            return variableRegistry.ContainsKey(name);
        }

        public void CreateVariableRegistry() => VariableRegistry[GetThreadsProgramID()] = [];
    }
}
