using InteractiveCompiler;
using System;
using System.Diagnostics;
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
        public static event EventHandler<object?>? MyEvent6;
        public static event EventHandler<object?>? MyEvent7;

        [Fact, Priority(-2)]
        public static void TestInitialize()
        {
            Compiler = new();
            Assert.NotNull(Compiler);
            Assert.IsType<BaseCompiler>(Compiler);

            CompileTestHelpers.BaseInitialize(out CompileBody,
                Compiler.RegisterRuntimeFunction,
                Compiler.RegisterConditionalFunction,
                Compiler.RegisterProperty);

            Assert.True(Compiler.RegisterTriggerEvent("MyTrigger", ref MyEvent));
            Assert.True(Compiler.RegisterTriggerEvent("MyTrigger2", ref MyEvent2));
            Assert.True(Compiler.RegisterTriggerEvent("MyTrigger3", ref MyEvent3));
            Assert.True(Compiler.RegisterTriggerEvent("MyTrigger4", ref MyEvent4));
            Assert.True(Compiler.RegisterTriggerEvent("MyTrigger5", ref MyEvent5));
            Assert.True(Compiler.RegisterTriggerEvent("MyTrigger6", ref MyEvent6));
            Assert.True(Compiler.RegisterTriggerEvent("MyTrigger7", ref MyEvent7));
        }

        [Fact, Priority(-1)]
        public static void TestCompile()
        {
            CompileTestHelpers.BaseCompile(() =>
            {
                ProgramID = Compiler!.RegisterProgram(CompileBody, LoggingFunc: CompileTestHelpers.TextLog);
                Assert.NotEqual(Guid.Empty, ProgramID);
            }, () =>
            {
                var tmpID = Compiler!.RegisterProgram("", LoggingFunc: CompileTestHelpers.TextLog);
                Assert.Equal(Guid.Empty, tmpID);
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
    }
}