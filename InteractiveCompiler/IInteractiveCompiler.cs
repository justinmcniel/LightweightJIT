using System;

namespace InteractiveCompiler
{
    public interface IInteractiveCompiler
    {
        public Dictionary<string, EventHandler> TriggerEvents { get; }
        public Dictionary<string, Func<IEnumerable<object?>?, object?>> RuntimeFunctionRegistry { get; }
        public Dictionary<string, Func<IEnumerable<object?>?, bool>> ConditionalFunctionRegistry { get; }
        public Dictionary<Guid, Dictionary<string, object?>> VariableRegistry { get; }
        public Dictionary<Guid, IEnumerable<(EventHandler Handler, Action<object?, EventArgs> function)>> EventTokensRegistry { get; }

        public Guid RegisterProgram(string programBody);
        public bool RemoveProgram(Guid programID);
        public void ClearPrograms();

        public bool RegisterTriggerEvent(string eventName, EventHandler<IEnumerable<object?>?>? eventHandler);
        public bool RemoveTriggerEvent(string eventName);
        public void ClearTriggerEvents();

        public bool RegisterRuntimeFunction(string functionName, Func<IEnumerable<object?>?, object?> function);
        public bool RemoveRuntimeFunction(string functionName);
        public void ClearRuntimeFunctions();

        public bool RegisterConditionalFunction(string functionName, Func<IEnumerable<object?>?, bool> function);
        public bool RemoveConditionalFunction(string functionName);
        public void ClearConditionalFunctions();

        public bool RegisterProperty(string propertyName, Func<object?> getter, Action<object?> setter);
        public bool RemoveProperty(string propertyName);
        public void ClearProperties();

        internal Dictionary<int, Guid> CompilationThreadProgramLookupTable { get; }
    }
}
