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
        private Dictionary<Guid, ProgramToken> ProgramTokenLookupTable { get; } = [];
        private Dictionary<string, (Func<object?> Getter, Action<object?> Setter)> BoundProperties { get; } = [];

        private event EventHandler<object?>? OnCompilationComplete;
        private event EventHandler<object?>? DoNotInvoke;

        public BaseCompiler()
        {
            VariableRegistry[Guid.Empty] = [];
            RegisterTriggerEvent("OnAnyCompilationComplete", ref OnCompilationComplete);
            RegisterTriggerEvent("Immediately", ref DoNotInvoke);
            RegisterRuntimeFunction("Log", (args) => { if (args != null) { foreach (var arg in args) { Console.WriteLine(arg); } } return null; });
        }

        public Guid RegisterProgram(string programBody, object? invokingObject = null)
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

            ProgramTokenLookupTable[program.ID] = program;

            var eventsList = program.Compile(this);

            CompilationThreadProgramLookupTable.Remove(Environment.CurrentManagedThreadId);

            foreach(var (Trigger, Reaction) in eventsList)
            {
                if (Trigger == "Immediately")
                { 
                    Reaction(invokingObject, [program.ID]); 
                }
                else
                {
                    if (!TriggerEventsRegistry.TryGetValue(Trigger, out var reactionList))
                    {
                        reactionList = [];
                        TriggerEventsRegistry[Trigger] = reactionList;
                    }
                    reactionList.Add((Reaction, program.ID));
                }
            }

            OnCompilationComplete?.Invoke(this, null); //will retrigger whenever a new program is registered

            return program.ID;
        }

        public string DecompileProgram(Guid programID)
        {
            if(!ProgramTokenLookupTable.TryGetValue(programID, out var program))
            { return ""; }
            return program!.Decompile();
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
                    VariableRegistry[ProgramID]["triggerer"] = sender;
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
            BoundProperties[propertyName] = (Getter, Setter);
            return true;
        }

        public Func<object?> PropertyGetter(string propertyName) => BoundProperties[propertyName].Getter;

        public Action<object?> PropertySetter(string propertyName) => BoundProperties[propertyName].Setter;

        public bool RemoveProperty(string propertyName) => BoundProperties.Remove(propertyName);

        public void ClearProperties() => BoundProperties.Clear();

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
            if (BoundProperties.ContainsKey(name))
            { return true; }
            name = name.Trim();
            if (!VariableRegistry.TryGetValue(GetThreadsProgramID(), out var variableRegistry))
            { throw new CompilerException(); }
            return variableRegistry.ContainsKey(name);
        }
        public Func<object?> VariableGetter(string variableName, Guid? guid = null)
        {
            if (BoundProperties.TryGetValue(variableName, out var property))
            { return property.Getter; }

            if (guid == null)
            { guid = GetThreadsProgramID(); }

            if (!VariableRegistry.TryGetValue((Guid)guid, out var variableRegistry))
            { throw new CompilerException(); }
            return () => { return variableRegistry[variableName]; };
        }

        public Action<object?> VariableSetter(string variableName, Guid? guid = null)
        {
            if (BoundProperties.TryGetValue(variableName, out var property))
            { return property.Setter; }

            if (guid == null)
            { guid = GetThreadsProgramID(); }

            if (!VariableRegistry.TryGetValue((Guid)guid, out var variableRegistry))
            { throw new CompilerException(); }
            return (object? value) => { variableRegistry[variableName] = value; };
        }

        public void CreateVariableRegistry() => VariableRegistry[GetThreadsProgramID()] = [];
    }
}
