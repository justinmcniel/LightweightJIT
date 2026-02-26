using InteractiveCompiler.Interpretation;

namespace InteractiveCompiler
{
    public static class StaticDispatchCompiler
    { /* Ideally we would order the tests to make the initializations come first, 
       *     and verify that the dispatcher stops, setting stopping it as a test that goes last
       * But I don't feel like implementing ITestCaseOrderer
       *    (which would probably just be copying tutorial code tbh)
       *    and I don't feel like forcing the tests to be in alphabetical order.
       */
        private static BaseCompiler Backer { get; } = new();
        private static CancellationTokenSource Canceller { get; } = new();
        private static Thread DispatchThread { get; } = new Thread(() =>DispatchThreadFunc(Canceller.Token));
        private static SemaphoreSlim DispatchSem { get; } = new(0, int.MaxValue);
        private static Queue<Action> DispatchQueue { get; } = [];

        static StaticDispatchCompiler() => DispatchThread.Start();

        public static bool IsRunning() => DispatchThread.IsAlive;
        
        static void DispatchThreadFunc(CancellationToken cancelToken)
        {
            Action? request = null;
            while (!cancelToken.IsCancellationRequested)
            {
                try
                { DispatchSem.Wait(cancelToken); }
                catch (OperationCanceledException)
                { break; }
                if (cancelToken.IsCancellationRequested)
                { break; }
                
                lock (DispatchQueue)
                { request = DispatchQueue.Dequeue(); }

                lock(Backer) //Because we were unable to push the Trigger Registration to the Dispatch Thread
                { request?.Invoke(); }
            }

            DispatchSem.Dispose();
        }

        public static void Shutdown(int millisTimeout = 0)
        {
            Canceller.Cancel();
            if (millisTimeout > 0)
            { DispatchThread.Join(millisTimeout); }
        }

        private static void RequestDispatch(Action request, bool waitForCompletion = true)
        {
            Semaphore waiter = new(0, 1);
            lock (DispatchQueue)
            { DispatchQueue.Enqueue( () =>
            {
                request();
                waiter.Release();
            }); }
            
            DispatchSem.Release();
            if (waitForCompletion)
            { waiter.WaitOne(); }
        }
        
        public static Task<Guid> RegisterProgram(string programBody, object? invokingObject = null, Action<string?>? LoggingFunc = null)
        {
            /*
            Guid? res = null;
            RequestDispatch(() => { res = Backer.RegisterProgram(programBody, invokingObject, LoggingFunc); });
            return res ?? Guid.Empty;
            */
            Task<Guid> compilationTask = new(() =>
            {
                lock (Backer)
                { Backer.Log = LoggingFunc ?? Backer.Log; }

                int index = 0;
                ProgramToken? program;
                try
                {
                    program = ProgramToken.TryParse(programBody, ref index, Backer);
                }
                catch
                {
                    Backer.CompilationCompleteSignal();
                    return Guid.Empty;
                }

                if (index == 0 || program == null)
                {
                    Backer.CompilationCompleteSignal();
                    return Guid.Empty;
                }

                var eventsList = program.Compile(Backer);

                Guid? res = null;
                RequestDispatch(() => res = Backer.RegisterProgramHelper(invokingObject, program, eventsList));
                return res ?? Guid.Empty;
            });

            compilationTask.Start();
            return compilationTask;
        }

        public static string DecompileProgram(Guid programID)
        {
            string? res = null;
            RequestDispatch(() => { res = Backer.DecompileProgram(programID); });
            return res ?? "";
        }

        public static bool RemoveProgram(Guid programID)
        {
            bool? res = null;
            RequestDispatch(() => { res = Backer.RemoveProgram(programID); });
            return res ?? false;
        }

        public static void ClearPrograms(bool waitForClear = true) =>
            RequestDispatch(() => { Backer.ClearPrograms(); }, waitForClear);

        
        public static bool RegisterTriggerEvent(string eventName, ref EventHandler<object?>? eventHandler)
        {
            lock(Backer) // Cannot use ref in a lambda, so I can't pass it to the dispatcher, and if I don't use a ref, then it won't add it to the original eventHandler
            { return Backer.RegisterTriggerEvent(eventName, ref eventHandler); }
        }

        public static bool RemoveTriggerEvent(string eventName)
        {
            bool? res = null;
            RequestDispatch(() => { res = Backer.RemoveTriggerEvent(eventName); });
            return res ?? false;
        }

        public static void ClearTriggerEvents(bool waitForClear = true) =>
            RequestDispatch(() => { Backer.ClearTriggerEvents(); }, waitForClear);

        public static bool RegisterRuntimeFunction(string functionName, Func<IEnumerable<object?>?, object?> function)
        {
            bool? res = null;
            RequestDispatch(() => { res = Backer.RegisterRuntimeFunction(functionName, function); });
            return res ?? false;
        }

        public static bool RemoveRuntimeFunction(string functionName)
        {
            bool? res = null;
            RequestDispatch(() => { res = Backer.RemoveRuntimeFunction(functionName); });
            return res ?? false;
        }

        public static void ClearRuntimeFunctions(bool waitForClear = true) =>
            RequestDispatch(() => { Backer.ClearRuntimeFunctions(); }, waitForClear);

        public static bool RegisterConditionalFunction(string functionName, Func<IEnumerable<object?>?, bool> function)
        {
            bool? res = null;
            RequestDispatch(() => { res = Backer.RegisterConditionalFunction(functionName, function); });
            return res ?? false;
        }

        public static bool RemoveConditionalFunction(string functionName)
        {
            bool? res = null;
            RequestDispatch(() => { res = Backer.RemoveConditionalFunction(functionName); });
            return res ?? false;
        }

        public static void ClearConditionalFunctions(bool waitForClear = true) =>
            RequestDispatch(() => { Backer.ClearConditionalFunctions(); }, waitForClear);

        public static bool RegisterProperty(string propertyName, Func<object?> Getter, Action<object?> Setter)
        {
            bool? res = null;
            RequestDispatch(() => { res = Backer.RegisterProperty(propertyName, Getter, Setter); });
            return res ?? false;
        }

        public static bool RemoveProperty(string propertyName)
        {
            bool? res = null;
            RequestDispatch(() => { res = Backer.RemoveProperty(propertyName); });
            return res ?? false;
        }

        public static void ClearProperties(bool waitForClear = true) =>
            RequestDispatch(() => { Backer.ClearPrograms(); }, waitForClear);
    }
}