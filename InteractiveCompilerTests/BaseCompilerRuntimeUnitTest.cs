using InteractiveCompiler;
using System;
using System.Reflection;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace InteractiveCompilerTests
{
    [Collection("Base Compiler Test Collection")]
    [TestCaseOrderer(ordererTypeName: "InteractiveCompilerTests.PriorityOrderer", ordererAssemblyName: "InteractiveCompilerTests")]
    public class BaseCompilerRuntimeUnitTest
    {
        private static string CompileBody = "";
        private static BaseCompiler? Compiler = null;
        private static Guid ProgramID = Guid.Empty;

        public static event EventHandler<object?>? MyEvent;
        public static event EventHandler<object?>? MyEvent2;
        public static event EventHandler<object?>? MyEvent3;
        public static event EventHandler<object?>? MyEvent4;
        public static event EventHandler<object?>? MyEvent5;

        private static void TextLog(string? s = "") => TestUtilities.TextLog(s);
        public static Queue<string?> TextLogEvents { get => TestUtilities.TextLogEvents; }
        public static Queue<(Func<IEnumerable<object?>?, object?> Func, IEnumerable<object?>? Args, object? ret)> RuntimeCallLog { get => TestUtilities.RuntimeCallLog; }
        public static Queue<(Func<IEnumerable<object?>?, bool> Func, IEnumerable<object?>? Args, bool ret)> CondCallLog { get => TestUtilities.CondCallLog; }
        public static Queue<TestUtilities.CallType> CallOrderLog { get => TestUtilities.CallOrderLog; }

        private static void ClearLogs() => TestUtilities.ClearLogs();

        [Fact, Priority(-2)]
        public static void TestInitialize()
        {
            TestUtilities.ResetTests();
            var baseDirectory = TestUtilities.FindBaseDirectory();
            CompileBody = File.ReadAllText($"{baseDirectory.FullName}/compileTest.txt");

            Compiler = new BaseCompiler();

            Assert.NotNull(Compiler);
            Assert.IsType<BaseCompiler>(Compiler);
            Assert.False(String.IsNullOrEmpty(CompileBody), "Failed to load the program to test");

            Assert.True(Compiler.RegisterTriggerEvent("MyTrigger", ref MyEvent));
            Assert.True(Compiler.RegisterRuntimeFunction("MyFunc", TestUtilities.MyFunc));

            Assert.True(Compiler.RegisterTriggerEvent("MyTrigger2", ref MyEvent2));
            Assert.True(Compiler.RegisterRuntimeFunction("MyFunc2", TestUtilities.MyFunc2));

            Assert.True(Compiler.RegisterTriggerEvent("MyTrigger3", ref MyEvent3));
            Assert.True(Compiler.RegisterRuntimeFunction("MyFunc3a", TestUtilities.MyFunc3a));
            Assert.True(Compiler.RegisterRuntimeFunction("MyFunc3b", TestUtilities.MyFunc3b));
            Assert.True(Compiler.RegisterRuntimeFunction("MyFunc3c", TestUtilities.MyFunc3c));
            Assert.True(Compiler.RegisterRuntimeFunction("MyFunc3d", TestUtilities.MyFunc3d));

            Assert.True(Compiler.RegisterTriggerEvent("MyTrigger4", ref MyEvent4));
            Assert.True(Compiler.RegisterRuntimeFunction("MyFunc4a", TestUtilities.MyFunc4a));
            Assert.True(Compiler.RegisterRuntimeFunction("MyFunc4b", TestUtilities.MyFunc4b));
            Assert.True(Compiler.RegisterRuntimeFunction("MyFunc4c", TestUtilities.MyFunc4c));
            Assert.True(Compiler.RegisterRuntimeFunction("MyFunc4d", TestUtilities.MyFunc4d));
            Assert.True(Compiler.RegisterConditionalFunction("MyCondFunc4a", TestUtilities.MyCondFunc4a));
            Assert.True(Compiler.RegisterConditionalFunction("MyCondFunc4b", TestUtilities.MyCondFunc4b));
            Assert.True(Compiler.RegisterConditionalFunction("MyCondFunc4c", TestUtilities.MyCondFunc4c));
            Assert.True(Compiler.RegisterConditionalFunction("MyCondFunc4d", TestUtilities.MyCondFunc4d));

            Assert.True(Compiler.RegisterTriggerEvent("MyTrigger5", ref MyEvent5));
            Assert.True(Compiler.RegisterRuntimeFunction("MyFunc5", TestUtilities.MyFunc5));
            Assert.True(Compiler.RegisterConditionalFunction("MyCondFunc5a", TestUtilities.MyCondFunc5a));
            Assert.True(Compiler.RegisterConditionalFunction("MyCondFunc5b", TestUtilities.MyCondFunc5b));
            Assert.True(Compiler.RegisterProperty("MyVar5", () => TestUtilities.MyBool5a, (arg) =>
            {
                TestUtilities.MyBool5a = arg switch
                {
                    null => throw new Exception("Arg was null"),
                    bool b => b,
                    _ => throw new Exception($"Expected bool, but got {arg.GetType().Name}")
                };
            }));
        }

        [Fact, Priority(-1)]
        public static void TestCompile()
        {
            ClearLogs();
            ProgramID = Compiler!.RegisterProgram(CompileBody, LoggingFunc: TextLog);

            Assert.NotEqual(Guid.Empty, ProgramID);

            Assert.Equal(2, TextLogEvents.Count);
            Assert.Equal("This Compilation Complete", TextLogEvents.Dequeue());
            Assert.Equal("Any compilation complete.", TextLogEvents.Dequeue());

            var tmpID = Compiler!.RegisterProgram("", LoggingFunc: TextLog);

            Assert.Single(TextLogEvents);
            Assert.Equal("Any compilation complete.", TextLogEvents.Dequeue());
        }

        [Fact]
        public static void TestTrigger1()
        {
            ClearLogs();
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

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            var (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc, func);
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

        [Fact]
        public static void TestTrigger2()
        {
            ClearLogs();
            /*
             * MyFunc2-null, MyFunc2(123)-true
             * MyFunc2-true
             */
            MyEvent2?.Invoke(null, null);
            Assert.Equal(2, CallOrderLog.Count);
            Assert.Empty(TextLogEvents);
            Assert.Empty(CondCallLog);
            Assert.Equal(2, RuntimeCallLog.Count);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            var (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc2, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc2, func);
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

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc2, func);
            Assert.True(args == null || !args.Any());
            Assert.IsType<bool>(ret);
            Assert.Equal(true, ret);
        }

        [Fact]
        public static void TestTrigger3()
        {
            ClearLogs();
            /*
             * MyFunc3a-3, MyFunc3b-4, MyFunc3c("test string")
             * MyFunc3a-5, MyFunc3b-4, MyFunc3d
             */
            MyEvent3?.Invoke(null, null);
            Assert.Equal(3, CallOrderLog.Count);
            Assert.Empty(TextLogEvents);
            Assert.Empty(CondCallLog);
            Assert.Equal(3, RuntimeCallLog.Count);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            var (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc3a, func);
            Assert.True(args == null || !args.Any());
            Assert.IsType<int>(ret);
            Assert.Equal(3, ret);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc3b, func);
            Assert.True(args == null || !args.Any());
            Assert.IsType<int>(ret);
            Assert.Equal(4, ret);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc3c, func);
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

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc3a, func);
            Assert.True(args == null || !args.Any());
            Assert.IsType<int>(ret);
            Assert.Equal(5, ret);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc3b, func);
            Assert.True(args == null || !args.Any());
            Assert.IsType<int>(ret);
            Assert.Equal(4, ret);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc3d, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);
        }

        [Fact]
        public static void TestTrigger4()
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

            MyEvent4?.Invoke(null, null);
            Assert.Equal(4, CallOrderLog.Count);
            Assert.Equal(3, CondCallLog.Count);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            var (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4b, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4c, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (var func, args, var ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc4d, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            MyEvent4?.Invoke(null, null);
            Assert.Equal(2, CallOrderLog.Count);
            Assert.Single(CondCallLog);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc4a, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            MyEvent4?.Invoke(null, null);
            Assert.Equal(3, CallOrderLog.Count);
            Assert.Equal(2, CondCallLog.Count);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4b, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc4b, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            MyEvent4?.Invoke(null, null);
            Assert.Equal(2, CallOrderLog.Count);
            Assert.Single(CondCallLog);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc4a, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            MyEvent4?.Invoke(null, null);
            Assert.Equal(5, CallOrderLog.Count);
            Assert.Equal(4, CondCallLog.Count);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4b, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4c, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4d, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc4d, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);


            TestUtilities.MyBool3 = false;
            MyEvent4?.Invoke(null, null);
            Assert.Equal(2, CallOrderLog.Count);
            Assert.Single(CondCallLog);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc4a, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            MyEvent4?.Invoke(null, null);
            Assert.Equal(3, CallOrderLog.Count);
            Assert.Equal(2, CondCallLog.Count);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4b, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc4b, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            MyEvent4?.Invoke(null, null);
            Assert.Equal(2, CallOrderLog.Count);
            Assert.Single(CondCallLog);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc4a, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);

            MyEvent4?.Invoke(null, null);
            Assert.Equal(5, CallOrderLog.Count);
            Assert.Equal(4, CondCallLog.Count);
            Assert.Single(RuntimeCallLog);
            Assert.Empty(TextLogEvents);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4b, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4c, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc4d, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (func, args, ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc4c, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);
        }

        [Fact]
        public static void TestTrigger5()
        {
            ClearLogs();
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

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            var (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc5a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            MyEvent5?.Invoke(null, null);
            Assert.Equal(3, CallOrderLog.Count);
            Assert.Empty(TextLogEvents);
            Assert.Single(RuntimeCallLog);
            Assert.Equal(2, CondCallLog.Count);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc5a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc5b, funcB);
            Assert.True(args == null || !args.Any());
            Assert.True(retB);

            Assert.Equal(TestUtilities.CallType.RUNTIME_FUNCTION, CallOrderLog.Dequeue());
            (var func, args, var ret) = RuntimeCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyFunc5, func);
            Assert.True(args == null || !args.Any());
            Assert.Null(ret);


            MyEvent5?.Invoke(null, null);
            Assert.Equal(2, CallOrderLog.Count);
            Assert.Empty(TextLogEvents);
            Assert.Empty(RuntimeCallLog);
            Assert.Equal(2, CondCallLog.Count);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc5a, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);

            Assert.Equal(TestUtilities.CallType.CONDITIONAL_FUNCTION, CallOrderLog.Dequeue());
            (funcB, args, retB) = CondCallLog.Dequeue();
            Assert.Equal(TestUtilities.MyCondFunc5b, funcB);
            Assert.True(args == null || !args.Any());
            Assert.False(retB);
        }
    }
}