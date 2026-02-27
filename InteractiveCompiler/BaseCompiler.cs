using InteractiveCompiler.Interpretation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public event EventHandler<object?>? OnCompilationComplete;
        private event EventHandler<object?>? DoNotInvoke;
        public Action<string?> InternalLog = (s) => Console.WriteLine(s);
        public Action<string?> ErrorLog = (s) => Debug.WriteLine(s);

        public void Log(object? s) => InternalLog(s?.ToString() ?? "null");
        public void LogError(object? s) => ErrorLog(s?.ToString() ?? "null");

        public BaseCompiler()
        {
            VariableRegistry[Guid.Empty] = [];
            RegisterTriggerEvent("OnAnyCompilationComplete", ref OnCompilationComplete);
            RegisterTriggerEvent("Immediately", ref DoNotInvoke);
            RegisterRuntimeFunction("Log", (args) => { if (args != null) { foreach (var arg in args) { Log(arg); } } return null; });
        }

        public Guid RegisterProgram(string programBody, object? invokingObject = null,
            Action<string?>? LoggingFunc = null, Action<string?> ? ErrorFunc = null)
        {
            lock (this)
            { 
                InternalLog = LoggingFunc ?? InternalLog;
                ErrorLog = ErrorFunc ?? ErrorLog; 
            }

            int index = 0;
            ProgramToken? program;
            try
            {
                program = ProgramToken.TryParse(programBody, ref index, this);
            }
            catch
            {
                OnCompilationComplete?.Invoke(this, null);
                return Guid.Empty;
            }

            if (index == 0 || program == null)
            {
                OnCompilationComplete?.Invoke(this, null);
                return Guid.Empty;
            }

            var eventsList = program.Compile(this);

            if (eventsList == null || eventsList.Count == 0)
            {
                OnCompilationComplete?.Invoke(this, null);
                return Guid.Empty;
            }

            return RegisterProgramHelper(invokingObject, program, eventsList);
        }

        internal void CompilationCompleteSignal() => OnCompilationComplete?.Invoke(this, null);
        internal Guid RegisterProgramHelper(object? invokingObject, ProgramToken program, List<(string Trigger, Action<object?, IEnumerable<object?>?> Reaction)> eventsList)
        {
            lock (this)
            {
                ProgramTokenLookupTable[program.ID] = program;

                CompilationThreadProgramLookupTable.Remove(Environment.CurrentManagedThreadId);

                foreach (var (Trigger, Reaction) in eventsList)
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

                OnCompilationComplete?.Invoke(this, null);

                return program.ID;
            }
        }

        public string DecompileProgram(Guid programID)
        {
            lock (this)
            {
                if (!ProgramTokenLookupTable.TryGetValue(programID, out var program))
                { return ""; }
                return program!.Decompile();
            }
        }

        public bool RemoveProgram(Guid programID)
        {
            lock (this)
            {
                bool res = false;
                foreach (var kvp in TriggerEventsRegistry)
                {
                    TriggerEventsRegistry[kvp.Key] = kvp.Value.Where(set =>
                {
                    return set.ProgramID != programID;
                }).ToList() ?? [];
                }

                res |= VariableRegistry.Remove(programID);
                return res;
            }
        }
        public void ClearPrograms()
        {
            lock (this)
            {
                foreach (var trigger in TriggerEventsRegistry.Keys)
                { TriggerEventsRegistry[trigger].Clear(); }

                foreach (var programID in VariableRegistry.Keys)
                { _ = RemoveProgram(programID); }
            }
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

            lock (this)
            {
                eventHandler += Invoker;
                TriggerEventsRegistry[eventName] = []; // Log this?
            }

            return true;
        }

        public bool RemoveTriggerEvent(string eventName)
        {
            lock (this)
            {
                return TriggerEventsRegistry.Remove(eventName);
            }
        }

        public void ClearTriggerEvents()
        {
            lock (this)
            {
                TriggerEventsRegistry.Clear();
            }
        }

        public bool RegisterRuntimeFunction(string functionName, Func<IEnumerable<object?>?, object?> function)
        {
            lock (this)
            {
                RuntimeFunctionRegistry[functionName] = function;
                return true;
            }
        }

        public bool RemoveRuntimeFunction(string functionName)
        {
            lock (this)
            {
                return RuntimeFunctionRegistry.Remove(functionName);
            }
        }

        public void ClearRuntimeFunctions()
        {

            lock (this)
            {
                RuntimeFunctionRegistry.Clear();
            }
        }

        public bool RegisterConditionalFunction(string functionName, Func<IEnumerable<object?>?, bool> function)
        {
            lock (this)
            {
                ConditionalFunctionRegistry[functionName] = function;
                return true;
            }
        }

        public bool RemoveConditionalFunction(string functionName)
        {
            lock (this)
            {
                return ConditionalFunctionRegistry.Remove(functionName);
            }
        }

        public void ClearConditionalFunctions()
        {
            lock (this)
            {
                ConditionalFunctionRegistry.Clear();
            }
        }

        public bool RegisterProperty(string propertyName, Func<object?> Getter, Action<object?> Setter)
        {
            lock (this)
            {
                BoundProperties[propertyName] = (Getter, Setter);
                return true;
            }
        }

        public Func<object?> PropertyGetter(string propertyName)
        {
            lock (this)
            {
                return BoundProperties[propertyName].Getter;
            }
        }

        public Action<object?> PropertySetter(string propertyName)
        {
            lock (this)
            {
                return BoundProperties[propertyName].Setter;
            }
        }

        public bool RemoveProperty(string propertyName)
        {
            lock (this)
            {
                return BoundProperties.Remove(propertyName);
            }
        }

        public void ClearProperties()
        {
            lock (this)
            {
                BoundProperties.Clear();
            }
        }

        public Guid GetThreadsProgramID()
        {
            lock (this)
            {
                if (!CompilationThreadProgramLookupTable.TryGetValue(Environment.CurrentManagedThreadId, out var res))
                { return Guid.Empty; }
                return res;
            }
        }

        public void NewVariable(string name)
        {
            if (String.IsNullOrEmpty(name))
            { throw new CompilerException(); }
            lock (this)
            {
                name = name.Trim();
                if (!VariableRegistry.TryGetValue(GetThreadsProgramID(), out var variableRegistry))
                { throw new CompilerException(); }
                variableRegistry[name] = null;
            }
        }

        public bool VariableExists(string name)
        {
            lock (this)
            {
                if (BoundProperties.ContainsKey(name))
                { return true; }
                name = name.Trim();
                if (!VariableRegistry.TryGetValue(GetThreadsProgramID(), out var variableRegistry))
                { throw new CompilerException(); }
                return variableRegistry.ContainsKey(name);
            }
        }
        public Func<object?> VariableGetter(string variableName, Guid? guid = null)
        {
            lock (this)
            {
                if (BoundProperties.TryGetValue(variableName, out var property))
                { return property.Getter; }

                if (guid == null)
                { guid = GetThreadsProgramID(); }

                if (!VariableRegistry.TryGetValue((Guid)guid, out var variableRegistry))
                { throw new CompilerException(); }
                return () => { return variableRegistry[variableName]; };
            }
        }

        public Action<object?> VariableSetter(string variableName, Guid? guid = null)
        {
            lock (this)
            {
                if (BoundProperties.TryGetValue(variableName, out var property))
                { return property.Setter; }

                if (guid == null)
                { guid = GetThreadsProgramID(); }

                if (!VariableRegistry.TryGetValue((Guid)guid, out var variableRegistry))
                { throw new CompilerException(); }
                return (object? value) => { variableRegistry[variableName] = value; };
            }
        }

        public void CreateVariableRegistry() 
        { lock(this) { VariableRegistry[GetThreadsProgramID()] = []; } }
    }
}
