using Xunit;
using InteractiveCompiler;
using System.Reflection;

namespace InteractiveCompilerTests
{
    public class StaticDispatchCompilerTest
    {
        private static string CompileBody = "";
        private static Guid ProgramID = Guid.Empty;

        private static bool _testedInitialize = false;
        [Fact]
        public static void TestInitialize()
        {
            if (!String.IsNullOrEmpty(CompileBody)) { return; } // already initialize

            var baseDirectory = TestUtilities.FindBaseDirectory();
            CompileBody = File.ReadAllText($"{baseDirectory.FullName}/compileTest.txt");

            Assert.False(String.IsNullOrEmpty(CompileBody), "Failed to load the program to test");

            Assert.True(StaticDispatchCompiler.RegisterTriggerEvent("MyTrigger", ref MyEvent));
            Assert.True(StaticDispatchCompiler.RegisterRuntimeFunction("MyFunc", MyFunc));
            
            Assert.True(StaticDispatchCompiler.RegisterTriggerEvent("MyTrigger2", ref MyEvent2));
            Assert.True(StaticDispatchCompiler.RegisterRuntimeFunction("MyFunc2", MyFunc2));

            Assert.True(StaticDispatchCompiler.RegisterTriggerEvent("MyTrigger3", ref MyEvent3));
            Assert.True(StaticDispatchCompiler.RegisterRuntimeFunction("MyFunc3a", MyFunc3a));
            Assert.True(StaticDispatchCompiler.RegisterRuntimeFunction("MyFunc3b", MyFunc3b));
            Assert.True(StaticDispatchCompiler.RegisterRuntimeFunction("MyFunc3c", MyFunc3c));
            Assert.True(StaticDispatchCompiler.RegisterRuntimeFunction("MyFunc3d", MyFunc3d));

            Assert.True(StaticDispatchCompiler.RegisterTriggerEvent("MyTrigger4", ref MyEvent4));
            Assert.True(StaticDispatchCompiler.RegisterRuntimeFunction("MyFunc4a", MyFunc4a));
            Assert.True(StaticDispatchCompiler.RegisterRuntimeFunction("MyFunc4b", MyFunc4b));
            Assert.True(StaticDispatchCompiler.RegisterRuntimeFunction("MyFunc4c", MyFunc4c));
            Assert.True(StaticDispatchCompiler.RegisterRuntimeFunction("MyFunc4d", MyFunc4d));
            Assert.True(StaticDispatchCompiler.RegisterConditionalFunction("MyCondFunc4a", MyCondFunc4a));
            Assert.True(StaticDispatchCompiler.RegisterConditionalFunction("MyCondFunc4b", MyCondFunc4b));
            Assert.True(StaticDispatchCompiler.RegisterConditionalFunction("MyCondFunc4c", MyCondFunc4c));
            Assert.True(StaticDispatchCompiler.RegisterConditionalFunction("MyCondFunc4d", MyCondFunc4d));

            Assert.True(StaticDispatchCompiler.RegisterTriggerEvent("MyTrigger5", ref MyEvent5));
            Assert.True(StaticDispatchCompiler.RegisterRuntimeFunction("MyFunc5", MyFunc5));
            Assert.True(StaticDispatchCompiler.RegisterConditionalFunction("MyCondFunc5a", MyCondFunc5a));
            Assert.True(StaticDispatchCompiler.RegisterConditionalFunction("MyCondFunc5b", MyCondFunc5b));
            Assert.True(StaticDispatchCompiler.RegisterProperty("MyVar5", () => myBool5a, (arg) =>
            {
                myBool5a = arg switch
                {
                    null => throw new Exception("Arg was null"),
                    bool b => b,
                    _ => throw new Exception($"Expected bool, but got {arg.GetType().Name}")
                };
            }));
            
            _testedInitialize = true;
            HandleDispatcherShutdown();
        }

        private static bool _testedCompile = false;
        [Fact]
        public static void TestCompile()
        {
            if (ProgramID != Guid.Empty) { return; } //already compiled
            CheckInitialize(); ClearLogs();
            ProgramID = StaticDispatchCompiler.RegisterProgram(CompileBody, LoggingFunc: TextLog);

            Assert.NotEqual(Guid.Empty, ProgramID);

            Assert.Equal(2, TextLogEvents.Count);
            Assert.Equal("This Compilation Complete", TextLogEvents.Dequeue());
            Assert.Equal("Any compilation complete.", TextLogEvents.Dequeue());

            var tmpID = StaticDispatchCompiler.RegisterProgram("", LoggingFunc: TextLog);

            Assert.Single(TextLogEvents);
            Assert.Equal("Any compilation complete.", TextLogEvents.Dequeue());
            
            _testedCompile = true;
            HandleDispatcherShutdown();
        }

        private static void CheckInitialize()
        {
            if (String.IsNullOrEmpty(CompileBody))
            { TestInitialize(); }
        }

        private static void CheckSetup()
        {
            CheckInitialize();
            if (ProgramID == Guid.Empty)
            { TestCompile(); }
        }

        public static event EventHandler<object?>? MyEvent;
        public static event EventHandler<object?>? MyEvent2;
        public static event EventHandler<object?>? MyEvent3;
        public static event EventHandler<object?>? MyEvent4;
        public static event EventHandler<object?>? MyEvent5;
        static string Parse(object? o) => o?.ToString() ?? "null";

        private static readonly Queue<string?> TextLogEvents = [];
        public static bool LogToConsole { get; set; } = false;
        private static void TextLog(string? s = "")
        {
            TextLogEvents.Enqueue(s);
            TestLog(s);
        }
        private static void TestLog(string? s = "")
        {
            if (LogToConsole)
            { Console.WriteLine(s); }
        }

        enum CallType
        {
            RUNTIME_FUNCTION,
            CONDITIONAL_FUNCTION,
        }

        private static readonly Queue<(Func<IEnumerable<object?>?, object?> Func, IEnumerable<object?>? Args, object? ret)> RuntimeCallLog = [];
        private static readonly Queue<(Func<IEnumerable<object?>?, bool> Func, IEnumerable<object?>? Args, bool ret)> CondCallLog = [];
        private static readonly Queue<CallType> CallOrderLog = [];

        private static void LogCall(Func<IEnumerable<object?>?, object?> func, IEnumerable<object?>? args, object? ret)
        {
            RuntimeCallLog.Enqueue((func, args, ret));
            CallOrderLog.Enqueue(CallType.RUNTIME_FUNCTION);
        }
        private static void LogCall(Func<IEnumerable<object?>?, bool> func, IEnumerable<object?>? args, bool ret)
        {
            CondCallLog.Enqueue((func, args, ret));
            CallOrderLog.Enqueue(CallType.CONDITIONAL_FUNCTION);
        }

        private static void ClearLogs()
        {
            TextLogEvents.Clear();
            RuntimeCallLog.Clear();
            CondCallLog.Clear();
            CallOrderLog.Clear();
        }

        public static object? MyFunc(IEnumerable<object?>? args)
        {
            if (args == null)
            {
                TestLog($"Args was null");
                LogCall(MyFunc, args, null);
                return null;
            }
            if (args.Count() != 2)
            {
                TestLog($"Unexpected ArgCount: {args.Count()}");
                LogCall(MyFunc, args, null);
                return null;
            }

            object? sender = args.ElementAt(0);
            object? arg = args.ElementAt(1);

            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered by {Parse(sender)} with an argument of {Parse(arg)}");
            TestLog();
            LogCall(MyFunc, args, true);
            return true;
        }

        private static bool? returnObj1 = null;
        public static object? MyFunc2(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            if (args != null)
            {
                foreach (var arg in args)
                { TestLog($"Argument: {Parse(arg)}"); }
            }
            var res = returnObj1;
            TestLog($"Return Value: {Parse(res)}");
            returnObj1 = true;
            TestLog();
            LogCall(MyFunc2, args, res);
            return res;
        }

        private static int myInt1 = 1;
        private const int myInt2 = 4;
        public static object? MyFunc3a(IEnumerable<object?>? args)
        {
            myInt1 += 2;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myInt1}");
            TestLog();
            LogCall(MyFunc3a, args, myInt1);
            return myInt1;
        }

        public static object? MyFunc3b(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myInt2}");
            TestLog();
            LogCall(MyFunc3b, args, myInt2);
            return myInt2;
        }

        public static object? MyFunc3c(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            if (args != null)
            {
                foreach (var arg in args)
                { TestLog($"Argument: {Parse(arg)}"); }
            }
            TestLog();
            LogCall(MyFunc3c, args, null);
            return null;
        }

        public static object? MyFunc3d(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            TestLog();
            LogCall(MyFunc3d, args, null);
            return null;
        }

        public static object? MyFunc4a(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            TestLog();
            LogCall(MyFunc4a, args, null);
            return null;
        }

        public static object? MyFunc4b(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            TestLog();
            LogCall(MyFunc4b, args, null);
            return null;
        }

        public static object? MyFunc4c(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            TestLog();
            LogCall(MyFunc4c, args, null);
            return null;
        }

        public static object? MyFunc4d(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            TestLog();
            LogCall(MyFunc4d, args, null);
            return null;
        }

        private static bool myBool1 = true;
        private static bool myBool2 = true;
        private static bool myBool3 = true;
        private static bool myBool4 = true;

        public static bool MyCondFunc4a(IEnumerable<object?>? args)
        {
            myBool1 = !myBool1;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool1}");
            TestLog();
            LogCall(MyCondFunc4a, args, myBool1);
            return myBool1;
        }

        public static bool MyCondFunc4b(IEnumerable<object?>? args)
        {
            myBool2 = !myBool2;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool2}");
            TestLog();
            LogCall(MyCondFunc4b, args, myBool2);
            return myBool2;
        }

        public static bool MyCondFunc4c(IEnumerable<object?>? args)
        {
            myBool3 = !myBool3;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool3}");
            TestLog();
            LogCall(MyCondFunc4c, args, myBool3);
            return myBool3;
        }

        public static bool MyCondFunc4d(IEnumerable<object?>? args)
        {
            myBool4 = !myBool4;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool4}");
            TestLog();
            LogCall(MyCondFunc4d, args, myBool4);
            return myBool4;
        }

        private static bool myBool5a = true;
        private static bool myBool5b = true;

        public static object? MyFunc5(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            myBool5b = false;
            TestLog($"Set {nameof(myBool5b)} to {myBool5b}");
            TestLog();
            LogCall(MyFunc5, args, null);
            return null;
        }
        public static bool MyCondFunc5a(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool5a}");
            TestLog();
            LogCall(MyCondFunc5a, args, myBool5a);
            return myBool5a;
        }

        public static bool MyCondFunc5b(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool5b}");
            TestLog();
            TestLog();
            LogCall(MyCondFunc5b, args, myBool5b);
            return myBool5b;
        }





























        private static bool _tested1 = false;
        [Fact]
        public static void TestTrigger1()
        {
            CheckSetup(); ClearLogs();
            /*
             * MyFunc(sender, triggerArgument)
             */
            var sender = "TestTrigger1";
            var triggerArg = 0b1010;
            MyEvent?.Invoke(sender, triggerArg);

            Assert.Single(CallOrderLog);
            Assert.Empty(CondCallLog);
            Assert.Empty(TextLogEvents);
            Assert.Single(RuntimeCallLog);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            var (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc, func);
            Assert.NotNull(args);
            Assert.Equal(2, args.Count());

            var arg0 = args.ElementAt(0);
            Assert.IsType(sender.GetType(), arg0);
            Assert.Equal(sender, arg0);

            var arg1 = args.ElementAt(1);
            Assert.IsType(triggerArg.GetType(), arg1);
            Assert.Equal(triggerArg, arg1);

            Assert.NotNull(ret);
            Assert.IsType<bool>(ret);
            Assert.Equal(true, ret);
            
            _tested1 = true;
            HandleDispatcherShutdown();
        }

        private static bool _tested2 = false;
        [Fact]
        public static void TestTrigger2()
        {
            CheckSetup(); ClearLogs();
            /*
             * MyFunc2-null, MyFunc2(123)-true
             * MyFunc2-true
             */
            MyEvent2?.Invoke(null, null);
            Assert.Equal(2, CallOrderLog.Count);
            Assert.Empty(TextLogEvents);
            Assert.Empty(CondCallLog);
            Assert.Equal(2, RuntimeCallLog.Count);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            var (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc2, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc2, func);
            Assert.NotNull(args);
            Assert.Single(args);
            var arg0 = args.ElementAt(0);
            Assert.NotNull(arg0);
            Assert.IsType<int>(arg0);
            Assert.Equal(123, arg0);
            Assert.IsType<bool>(ret);
            Assert.Equal(true, ret);

            MyEvent2?.Invoke(null, null);
            Assert.Single(CallOrderLog);
            Assert.Empty(CondCallLog);
            Assert.Empty(TextLogEvents);
            Assert.Single(RuntimeCallLog);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc2, func);
            Assert.True(args == null || !args.Any());
            Assert.IsType<bool>(ret);
            Assert.Equal(true, ret);
            
            _tested2 = true;
            HandleDispatcherShutdown();
        }

        private static bool _tested3 = false;
        [Fact]
        public static void TestTrigger3()
        {
            CheckSetup(); ClearLogs();
            /*
             * MyFunc3a-3, MyFunc3b-4, MyFunc3c("test string")
             * MyFunc3a-5, MyFunc3b-4, MyFunc3d
             */
            MyEvent3?.Invoke(null, null);
            Assert.Equal(3, CallOrderLog.Count);
            Assert.Empty(TextLogEvents);
            Assert.Empty(CondCallLog);
            Assert.Equal(3, RuntimeCallLog.Count);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            var (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc3a, func);
            Assert.True(args == null || !args.Any());
            Assert.IsType<int>(ret);
            Assert.Equal(3, ret);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc3b, func);
            Assert.True(args == null || !args.Any());
            Assert.IsType<int>(ret);
            Assert.Equal(4, ret);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc3c, func);
            Assert.NotNull(args);
            Assert.Single(args);
            var arg0 = args.ElementAt(0);
            Assert.NotNull(arg0);
            Assert.IsType<string>(arg0);
            Assert.Equal("test string", arg0);
            Assert.Null(ret);


            MyEvent3?.Invoke(null, null);
            Assert.Equal(3, CallOrderLog.Count);
            Assert.Empty(TextLogEvents);
            Assert.Empty(CondCallLog);
            Assert.Equal(3, RuntimeCallLog.Count);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc3a, func);
            Assert.True(args == null || !args.Any());
            Assert.IsType<int>(ret);
            Assert.Equal(5, ret);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc3b, func);
            Assert.True(args == null || !args.Any());
            Assert.IsType<int>(ret);
            Assert.Equal(4, ret);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc3d, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);
            
            _tested3 = true;
            HandleDispatcherShutdown();
        }

        private static bool _tested4 = false;
        [Fact]
        public static void TestTrigger4()
        {
            CheckSetup(); ClearLogs();
            /* expect 
             * MyCondFunc4a-f, MyCondFunc4b-f, MyCondFunc4c-f, MyFunc4d, 
             * MyCondFunc4a-t, MyFunc4a, 
             * MyCondFunc4a-f, MyCondFunc4b-t, MyFunc4b, 
             * MyCondFunc4a-t, MyFunc4a, 
             * MyCondFunc4a-f, MyCondFuncb-f, MyCondFunc4c-t, MyCondFunc4d-f, MyFunc4d
             * 
             * ** SET MY BOOL 3 TO FALSE
             * MyCondFunc4a-t, MyFunc4a, 
             * MyCondFunc4a-f, MyCondFunc4b-t, MyFunc4b, 
             * MyCondFunc4a-t, MyFunc4a, 
             * MyCondFunc4a-f, MyCondFunc4b-f, MyCondFunc4c-t, MyCondFunc4d-t, MyFunc4c
             */

            MyEvent4?.Invoke(null, null);
            Assert.Equal(4, CallOrderLog.Count);
            Assert.Equal(3, CondCallLog.Count);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            var (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4b, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4c, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (var func, args, var ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc4d, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            MyEvent4?.Invoke(null, null);
            Assert.Equal(2, CallOrderLog.Count);
            Assert.Single(CondCallLog);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc4a, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            MyEvent4?.Invoke(null, null);
            Assert.Equal(3, CallOrderLog.Count);
            Assert.Equal(2, CondCallLog.Count);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4b, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc4b, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            MyEvent4?.Invoke(null, null);
            Assert.Equal(2, CallOrderLog.Count);
            Assert.Single(CondCallLog);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc4a, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            MyEvent4?.Invoke(null, null);
            Assert.Equal(5, CallOrderLog.Count);
            Assert.Equal(4, CondCallLog.Count);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4b, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4c, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4d, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc4d, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);


            myBool3 = false;
            MyEvent4?.Invoke(null, null);
            Assert.Equal(2, CallOrderLog.Count);
            Assert.Single(CondCallLog);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc4a, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            MyEvent4?.Invoke(null, null);
            Assert.Equal(3, CallOrderLog.Count);
            Assert.Equal(2, CondCallLog.Count);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4b, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc4b, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            MyEvent4?.Invoke(null, null);
            Assert.Equal(2, CallOrderLog.Count);
            Assert.Single(CondCallLog);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc4a, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            MyEvent4?.Invoke(null, null);
            Assert.Equal(5, CallOrderLog.Count);
            Assert.Equal(4, CondCallLog.Count);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4b, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4c, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc4d, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc4c, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);
            
            _tested4 = true;
            HandleDispatcherShutdown();
        }

        private static bool _tested5 = false;
        [Fact]
        public static void TestTrigger5()
        {
            CheckSetup(); ClearLogs();
            /*
             * MyCondFunc5a-t
             * MyCondFunc5a-f, MyCondFunc5b-t, MyFunc5
             * MyCondFunc5a-f, MyCondFunc5b-f
             */

            MyEvent5?.Invoke(null, null);
            Assert.Single(CallOrderLog);
            Assert.Empty(RuntimeCallLog);
            Assert.Empty(TextLogEvents);
            Assert.Single(CondCallLog);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            var (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc5a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            MyEvent5?.Invoke(null, null);
            Assert.Equal(3, CallOrderLog.Count);
            Assert.Empty(TextLogEvents);
            Assert.Single(RuntimeCallLog);
            Assert.Equal(2, CondCallLog.Count);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc5a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc5b, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (var func, args, var ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc5, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);


            MyEvent5?.Invoke(null, null);
            Assert.Equal(2, CallOrderLog.Count);
            Assert.Empty(TextLogEvents);
            Assert.Empty(RuntimeCallLog);
            Assert.Equal(2, CondCallLog.Count);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc5a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc5b, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);
            
            _tested5 = true;
            HandleDispatcherShutdown();
        }

        private static void HandleDispatcherShutdown()
        {
            if (_testedInitialize &&
                _testedCompile &&
                _tested1 &&
                _tested2 &&
                _tested3 &&
                _tested4 &&
                _tested5)
            {
                StaticDispatchCompiler.Shutdown();
            }
        }
    }
}