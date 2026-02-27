using Xunit;
using InteractiveCompiler;
using System.Reflection;
using InteractiveCompiler.Interpretation;
using System.Diagnostics;

namespace InteractiveCompilerTests
{
    [Collection("Static Dispatcher Test Collection")]
    [TestCaseOrderer(ordererTypeName: "InteractiveCompilerTests.PriorityOrderer", ordererAssemblyName: "InteractiveCompilerTests")]
    public class StaticDispatchCompilerTest
    {
        private static string CompileBody = "";
        private static Guid ProgramID = Guid.Empty;

        public static event EventHandler<object?>? MyEvent;
        public static event EventHandler<object?>? MyEvent2;
        public static event EventHandler<object?>? MyEvent3;
        public static event EventHandler<object?>? MyEvent4;
        public static event EventHandler<object?>? MyEvent5;
        public static event EventHandler<object?>? MyEvent6;
        public static event EventHandler<object?>? MyEvent7;

        [Fact, Priority(-2)]
        public static void TestInitialize()
        {
            CompileTestHelpers.BaseInitialize(out CompileBody,
                StaticDispatchCompiler.RegisterRuntimeFunction,
                StaticDispatchCompiler.RegisterConditionalFunction,
                StaticDispatchCompiler.RegisterProperty);

            Assert.True(StaticDispatchCompiler.RegisterTriggerEvent("MyTrigger", ref MyEvent));
            Assert.True(StaticDispatchCompiler.RegisterTriggerEvent("MyTrigger2", ref MyEvent2));
            Assert.True(StaticDispatchCompiler.RegisterTriggerEvent("MyTrigger3", ref MyEvent3));
            Assert.True(StaticDispatchCompiler.RegisterTriggerEvent("MyTrigger4", ref MyEvent4));
            Assert.True(StaticDispatchCompiler.RegisterTriggerEvent("MyTrigger5", ref MyEvent5));
            Assert.True(StaticDispatchCompiler.RegisterTriggerEvent("MyTrigger6", ref MyEvent6));
            Assert.True(StaticDispatchCompiler.RegisterTriggerEvent("MyTrigger7", ref MyEvent7));
        }

        public static readonly TimeSpan CompilationTimeLimit = new(0, 10, 0);

        [Fact, Priority(-1)]
        public static void TestCompile()
        {
            CompileTestHelpers.BaseCompile(() =>
            {
                var compilationTask = StaticDispatchCompiler.RegisterProgram(CompileBody, LoggingFunc: CompileTestHelpers.TextLog);
                var status = compilationTask.Status;
                Assert.True(status == TaskStatus.WaitingToRun ||
                            status == TaskStatus.Running ||
                            status == TaskStatus.WaitingForChildrenToComplete ||
                            status == TaskStatus.RanToCompletion);

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
                Assert.True(compilationTask.Wait(CompilationTimeLimit));
                Assert.True(compilationTask.IsCompletedSuccessfully);
                ProgramID = compilationTask.Result;
                Assert.NotEqual(Guid.Empty, ProgramID);
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
#pragma warning restore IDE0079 // Remove unnecessary suppression
            }, () =>
            {
                var emptyRegistration = StaticDispatchCompiler.RegisterProgram("", LoggingFunc: CompileTestHelpers.TextLog);
                var status = emptyRegistration.Status;
                Assert.True(status == TaskStatus.WaitingToRun ||
                            status == TaskStatus.Running ||
                            status == TaskStatus.WaitingForChildrenToComplete ||
                            status == TaskStatus.RanToCompletion);

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
                Assert.True(emptyRegistration.Wait(CompilationTimeLimit));
                Assert.True(emptyRegistration.IsCompletedSuccessfully);
                var tmpID = emptyRegistration.Result;
                Assert.Equal(Guid.Empty, tmpID);
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
#pragma warning restore IDE0079 // Remove unnecessary suppression
            });
        }

        [Fact]
        public static void TestTrigger1()
        {
            Assert.NotNull(MyEvent);
            CompileTestHelpers.TestTrigger1(MyEvent.Invoke);
        }

        [Fact]
        public static void TestTrigger2()
        {
            Assert.NotNull(MyEvent2);
            CompileTestHelpers.TestTrigger2(MyEvent2.Invoke);
        }

        [Fact]
        public static void TestTrigger3()
        {
            Assert.NotNull(MyEvent3);
            CompileTestHelpers.TestTrigger3(MyEvent3.Invoke);
        }

        [Fact]
        public static void TestTrigger4()
        {
            Assert.NotNull(MyEvent4);
            CompileTestHelpers.TestTrigger4(MyEvent4.Invoke);
        }

        [Fact]
        public static void TestTrigger5()
        {
            Assert.NotNull(MyEvent5);
            CompileTestHelpers.TestTrigger5(MyEvent5.Invoke);
        }

        [Fact]
        public static void TestTrigger6()
        {
            Assert.NotNull(MyEvent6);
            CompileTestHelpers.TestTrigger6(MyEvent6.Invoke);
        }

        [Fact]
        public static void TestTrigger7()
        {
            Assert.NotNull(MyEvent7);
            CompileTestHelpers.TestTrigger7(MyEvent7.Invoke);
        }

        [Fact, Priority(int.MaxValue)]
        public static void TestShutdown()
        {
            StaticDispatchCompiler.Shutdown(millisTimeout: 100);
            Assert.False(StaticDispatchCompiler.IsRunning());
        }
    }
}