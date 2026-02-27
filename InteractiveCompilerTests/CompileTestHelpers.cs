using InteractiveCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompilerTests
{
    public static class CompileTestHelpers
    {
        public static void BaseInitialize(out string CompileBody,
            Func<string, Func<IEnumerable<object?>?, object?>, bool> RuntimeRegisterer,
            Func<string, Func<IEnumerable<object?>?, bool>, bool> ConditionalRegisterer,
            Func<string, Func<object?>, Action<object?>, bool> PropertyRegisterer)
        {
            ResetTests();

            var baseDirectory = TestUtilities.FindBaseDirectory();
            CompileBody = File.ReadAllText($"{baseDirectory.FullName}/compileTest.txt");

            Assert.False(String.IsNullOrEmpty(CompileBody), "Failed to load the program to test");
            Assert.True(RuntimeRegisterer("MyFunc", MyFunc));

            Assert.True(RuntimeRegisterer("MyFunc2", MyFunc2));

            Assert.True(RuntimeRegisterer("MyFunc3a", MyFunc3a));
            Assert.True(RuntimeRegisterer("MyFunc3b", MyFunc3b));
            Assert.True(RuntimeRegisterer("MyFunc3c", MyFunc3c));
            Assert.True(RuntimeRegisterer("MyFunc3d", MyFunc3d));

            Assert.True(RuntimeRegisterer("MyFunc4a", MyFunc4a));
            Assert.True(RuntimeRegisterer("MyFunc4b", MyFunc4b));
            Assert.True(RuntimeRegisterer("MyFunc4c", MyFunc4c));
            Assert.True(RuntimeRegisterer("MyFunc4d", MyFunc4d));
            Assert.True(ConditionalRegisterer("MyCondFunc4a", MyCondFunc4a));
            Assert.True(ConditionalRegisterer("MyCondFunc4b", MyCondFunc4b));
            Assert.True(ConditionalRegisterer("MyCondFunc4c", MyCondFunc4c));
            Assert.True(ConditionalRegisterer("MyCondFunc4d", MyCondFunc4d));

            Assert.True(RuntimeRegisterer("MyFunc5", MyFunc5));
            Assert.True(ConditionalRegisterer("MyCondFunc5a", MyCondFunc5a));
            Assert.True(ConditionalRegisterer("MyCondFunc5b", MyCondFunc5b));
            Assert.True(PropertyRegisterer("MyVar5", () => MyBool5a, (arg) =>
            {
                MyBool5a = arg switch
                {
                    null => throw new Exception("Arg was null"),
                    bool b => b,
                    _ => throw new Exception($"Expected bool, but got {arg.GetType().Name}")
                };
            }));

            Assert.True(RuntimeRegisterer("MyFunc6", MyFunc6));
            Assert.True(ConditionalRegisterer("MyCondFunc6", MyCondFunc6));
        }

        public static void BaseCompile(Action Compile, Action EmptyRegisterer)
        {
            ClearLogs();

            Compile();
            var tmp = TextLogEvents;
            Assert.Equal(2, TextLogEvents.Count);
            Assert.Equal("This Compilation Complete", TextLogEvents.Dequeue());
            Assert.Equal("Any compilation complete.", TextLogEvents.Dequeue());

            EmptyRegisterer();

            Assert.Single(TextLogEvents);
            Assert.Equal("Any compilation complete.", TextLogEvents.Dequeue());
        }

        static string Parse(object? o) => o?.ToString() ?? "null";

        public static readonly Queue<string?> TextLogEvents = [];
        public static bool LogToConsole { get; set; } = false;
        public static void TextLog(string? s = "")
        {
            TextLogEvents.Enqueue(s);
            TestLog(s);
        }
        public static void TestLog(string? s = "")
        {
            if (LogToConsole)
            { Console.WriteLine(s); }
        }

        public enum CallType
        {
            RUNTIME_FUNCTION,
            CONDITIONAL_FUNCTION,
        }

        public static readonly Queue<(Func<IEnumerable<object?>?, object?> Func, IEnumerable<object?>? Args, object? ret)> RuntimeCallLog = [];
        public static readonly Queue<(Func<IEnumerable<object?>?, bool> Func, IEnumerable<object?>? Args, bool ret)> CondCallLog = [];
        public static readonly Queue<CallType> CallOrderLog = [];

        public static void LogCall(Func<IEnumerable<object?>?, object?> func, IEnumerable<object?>? args, object? ret)
        {
            RuntimeCallLog.Enqueue((func, args, ret));
            CallOrderLog.Enqueue(CallType.RUNTIME_FUNCTION);
        }
        public static void LogCall(Func<IEnumerable<object?>?, bool> func, IEnumerable<object?>? args, bool ret)
        {
            CondCallLog.Enqueue((func, args, ret));
            CallOrderLog.Enqueue(CallType.CONDITIONAL_FUNCTION);
        }

        public static void ClearLogs()
        {
            TextLogEvents.Clear();
            RuntimeCallLog.Clear();
            CondCallLog.Clear();
            CallOrderLog.Clear();
        }

        public static void ResetTests()
        {
            ClearLogs();
            ReturnObj1 = null;
            MyInt1 = 1;
            MyBool1 = true;
            MyBool2 = true;
            MyBool3 = true;
            MyBool4 = true;
            MyBool5a = true;
            MyBool5b = true;
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

        public static void TestTrigger1(Action<object?, object?> event1)
        {
            ClearLogs();
            /*
             * MyFunc(sender, triggerArgument)
             */
            var sender = "TestTrigger1";
            var triggerArg = 0b1010;
            event1?.Invoke(sender, triggerArg);

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
        }

        public static bool? ReturnObj1 { get; set; } = null;
        public static object? MyFunc2(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            if (args != null)
            {
                foreach (var arg in args)
                { TestLog($"Argument: {Parse(arg)}"); }
            }
            var res = ReturnObj1;
            TestLog($"Return Value: {Parse(res)}");
            ReturnObj1 = true;
            TestLog();
            LogCall(MyFunc2, args, res);
            return res;
        }

        public static void TestTrigger2(Action<object?, object?> event2)
        {
            ClearLogs();
            /*
             * MyFunc2-null, MyFunc2(123)-true
             * MyFunc2-true
             */
            event2?.Invoke(null, null);
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

            event2?.Invoke(null, null);
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
        }

        public static int MyInt1 { get; set; } = 1;
        public const int MY_INT_2 = 4;
        public static object? MyFunc3a(IEnumerable<object?>? args)
        {
            MyInt1 += 2;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MyInt1}");
            TestLog();
            LogCall(MyFunc3a, args, MyInt1);
            return MyInt1;
        }

        public static object? MyFunc3b(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MY_INT_2}");
            TestLog();
            LogCall(MyFunc3b, args, MY_INT_2);
            return MY_INT_2;
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

        public static void TestTrigger3(Action<object?, object?> event3)
        {
            ClearLogs();
            /*
             * MyFunc3a-3, MyFunc3b-4, MyFunc3c("test string")
             * MyFunc3a-5, MyFunc3b-4, MyFunc3d
             */
            event3?.Invoke(null, null);
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


            event3?.Invoke(null, null);
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

        public static bool MyBool1 { get; set; } = true;
        public static bool MyBool2 { get; set; } = true;
        public static bool MyBool3 { get; set; } = true;
        public static bool MyBool4 { get; set; } = true;

        public static bool MyCondFunc4a(IEnumerable<object?>? args)
        {
            MyBool1 = !MyBool1;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MyBool1}");
            TestLog();
            LogCall(MyCondFunc4a, args, MyBool1);
            return MyBool1;
        }

        public static bool MyCondFunc4b(IEnumerable<object?>? args)
        {
            MyBool2 = !MyBool2;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MyBool2}");
            TestLog();
            LogCall(MyCondFunc4b, args, MyBool2);
            return MyBool2;
        }

        public static bool MyCondFunc4c(IEnumerable<object?>? args)
        {
            MyBool3 = !MyBool3;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MyBool3}");
            TestLog();
            LogCall(MyCondFunc4c, args, MyBool3);
            return MyBool3;
        }

        public static bool MyCondFunc4d(IEnumerable<object?>? args)
        {
            MyBool4 = !MyBool4;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MyBool4}");
            TestLog();
            LogCall(MyCondFunc4d, args, MyBool4);
            return MyBool4;
        }

        public static void TestTrigger4(Action<object?, object?> event4)
        {
            ClearLogs();
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

            event4?.Invoke(null, null);
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

            event4?.Invoke(null, null);
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

            event4?.Invoke(null, null);
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

            event4?.Invoke(null, null);
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

            event4?.Invoke(null, null);
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


            MyBool3 = false;
            event4?.Invoke(null, null);
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

            event4?.Invoke(null, null);
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

            event4?.Invoke(null, null);
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

            event4?.Invoke(null, null);
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
        }

        public static bool MyBool5a { get; set; } = true;
        public static bool MyBool5b { get; set; } = true;

        public static object? MyFunc5(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            MyBool5b = false;
            TestLog($"Set {nameof(MyBool5b)} to {MyBool5b}");
            TestLog();
            LogCall(MyFunc5, args, null);
            return null;
        }
        public static bool MyCondFunc5a(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MyBool5a}");
            TestLog();
            LogCall(MyCondFunc5a, args, MyBool5a);
            return MyBool5a;
        }

        public static bool MyCondFunc5b(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MyBool5b}");
            TestLog();
            TestLog();
            LogCall(MyCondFunc5b, args, MyBool5b);
            return MyBool5b;
        }

        public static void TestTrigger5(Action<object?, object?> event5)
        {
            ClearLogs();
            /*
             * MyCondFunc5a-t
             * MyCondFunc5a-f, MyCondFunc5b-t, MyFunc5
             * MyCondFunc5a-f, MyCondFunc5b-f
             */

            event5?.Invoke(null, null);
            Assert.Single(CallOrderLog);
            Assert.Empty(RuntimeCallLog);
            Assert.Empty(TextLogEvents);
            Assert.Single(CondCallLog);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            var (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc5a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            event5?.Invoke(null, null);
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


            event5?.Invoke(null, null);
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
        }

        public static bool MyBool6 { get; set; } = true;
        public static bool MyCondFunc6(IEnumerable<object?>? args)
        {
            MyBool6 = !MyBool6;
            LogCall(MyCondFunc6, args, MyBool6);
            return MyBool6;
        }

        public static object? MyFunc6(IEnumerable<object?>? args)
        {
            LogCall(MyFunc6, args, null);
            return null;
        }

        public static void TestTrigger6(Action<object?, object?> event6)
        {
            ClearLogs();
            /*
             * MyCondFunc6-f, MyFunc6(true), MyFunc6(true)
             * MyCondFunc6-t, MyFunc6(true)
             * MyCondFunc6-f, MyFunc6(true), MyFunc6(true)
             * MyCondFunc6-t, MyFunc6(true)
             */
            event6?.Invoke(null, null);
            Assert.Equal(3, CallOrderLog.Count);
            Assert.Equal(2, RuntimeCallLog.Count);
            Assert.Empty(TextLogEvents);
            Assert.Single(CondCallLog);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            var (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc6, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (var func, args, var ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc6, func);
            Assert.NotNull(args);
            Assert.Single(args);
            Assert.IsType<bool>(args.ElementAt(0));
            Assert.Equal(true, args.ElementAt(0));
            Assert.Null(ret);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc6, func);
            Assert.NotNull(args);
            Assert.Single(args);
            Assert.IsType<bool>(args.ElementAt(0));
            Assert.Equal(true, args.ElementAt(0));
            Assert.Null(ret);

            event6?.Invoke(null, null);
            Assert.Equal(2, CallOrderLog.Count);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);
            Assert.Single(CondCallLog);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc6, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc6, func);
            Assert.NotNull(args);
            Assert.Single(args);
            Assert.IsType<bool>(args.ElementAt(0));
            Assert.Equal(true, args.ElementAt(0));
            Assert.Null(ret);

            event6?.Invoke(null, null);
            Assert.Equal(3, CallOrderLog.Count);
            Assert.Equal(2, RuntimeCallLog.Count);
            Assert.Empty(TextLogEvents);
            Assert.Single(CondCallLog);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc6, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc6, func);
            Assert.NotNull(args);
            Assert.Single(args);
            Assert.IsType<bool>(args.ElementAt(0));
            Assert.Equal(true, args.ElementAt(0));
            Assert.Null(ret);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc6, func);
            Assert.NotNull(args);
            Assert.Single(args);
            Assert.IsType<bool>(args.ElementAt(0));
            Assert.Equal(true, args.ElementAt(0));
            Assert.Null(ret);

            event6?.Invoke(null, null);
            Assert.Equal(2, CallOrderLog.Count);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);
            Assert.Single(CondCallLog);

            Assert.Equal(CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(MyCondFunc6, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(MyFunc6, func);
            Assert.NotNull(args);
            Assert.Single(args);
            Assert.IsType<bool>(args.ElementAt(0));
            Assert.Equal(true, args.ElementAt(0));
            Assert.Null(ret);
        }
    }
}
