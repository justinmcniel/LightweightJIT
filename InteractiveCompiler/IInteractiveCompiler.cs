using System;

namespace InteractiveCompiler
{
    public interface IInteractiveCompiler
    {
        public Dictionary<string, List<(Action<object?, IEnumerable<object?>?> Reaction, Guid ProgramID)>> TriggerEventsRegistry { get; }
        public Dictionary<string, Func<IEnumerable<object?>?, object?>> RuntimeFunctionRegistry { get; }
        public Dictionary<string, Func<IEnumerable<object?>?, bool>> ConditionalFunctionRegistry { get; }
        protected Dictionary<Guid, Dictionary<string, object?>> VariableRegistry { get; }

        public Guid RegisterProgram(string programBody);
        public string DecompileProgram(Guid programID);
        public bool RemoveProgram(Guid programID);
        public void ClearPrograms();

        public bool RegisterTriggerEvent(string eventName, ref EventHandler<object?>? eventHandler);
        public bool RemoveTriggerEvent(string eventName); //does not remove existing functions from the EventHandlers
        public void ClearTriggerEvents();

        public bool RegisterRuntimeFunction(string functionName, Func<IEnumerable<object?>?, object?> function);
        public bool RemoveRuntimeFunction(string functionName);
        public void ClearRuntimeFunctions();

        public bool RegisterConditionalFunction(string functionName, Func<IEnumerable<object?>?, bool> function);
        public bool RemoveConditionalFunction(string functionName);
        public void ClearConditionalFunctions();

        public bool RegisterProperty(string propertyName, Func<object?> Getter, Action<object?> Setter);
        public bool RemoveProperty(string propertyName);
        public void ClearProperties();

        internal Dictionary<int, Guid> CompilationThreadProgramLookupTable { get; }
        internal Guid GetThreadsProgramID();
        internal void NewVariable(string name);
        internal bool VariableExists(string name);
        internal void CreateVariableRegistry();
    }
}
