using InteractiveCompiler;
using System.Diagnostics;
using System.Reflection;
using InteractiveCompilerTests;
using InteractiveCompiler.Interpretation;

namespace InteractiveCompilerDevelopmentConsole
{
    public static class Program
    { /// TODO: Static Dispatcher
        public static event EventHandler<object?>? MyEvent;
        public static event EventHandler<object?>? MyEvent2;
        public static event EventHandler<object?>? MyEvent3;
        public static event EventHandler<object?>? MyEvent4;
        public static event EventHandler<object?>? MyEvent5;
        private static bool myBool5a = true;

        public static void Main(string[] args)
        {
            var baseDirectory = TestUtilities.FindBaseDirectory();
            string compileTestBody = File.ReadAllText($"{baseDirectory.FullName}/compileTest.txt");
            string parseTestBody = File.ReadAllText($"{baseDirectory.FullName}/parseTest.txt");
            BaseCompiler compiler = new();

            if (!compiler.RegisterTriggerEvent("MyTrigger", ref MyEvent)) { throw new Exception("Failed to register trigger 1"); }
            if (!compiler.RegisterRuntimeFunction("MyFunc", CompileTestHelpers.MyFunc)) { throw new Exception("Failed to register function 1"); }

            if (!compiler.RegisterTriggerEvent("MyTrigger2", ref MyEvent2)) { throw new Exception("Failed to register trigger 2"); }
            if (!compiler.RegisterRuntimeFunction("MyFunc2", CompileTestHelpers.MyFunc2)) { throw new Exception("Failed to register function 2"); }

            if (!compiler.RegisterTriggerEvent("MyTrigger3", ref MyEvent3)) { throw new Exception("Failed to register trigger 3"); }
            if (!compiler.RegisterRuntimeFunction("MyFunc3a", CompileTestHelpers.MyFunc3a)) { throw new Exception("Failed to register function 3a"); }
            if (!compiler.RegisterRuntimeFunction("MyFunc3b", CompileTestHelpers.MyFunc3b)) { throw new Exception("Failed to register function 3b"); }
            if (!compiler.RegisterRuntimeFunction("MyFunc3c", CompileTestHelpers.MyFunc3c)) { throw new Exception("Failed to register function 3c"); }
            if (!compiler.RegisterRuntimeFunction("MyFunc3d", CompileTestHelpers.MyFunc3d)) { throw new Exception("Failed to register function 3d"); }
            
            if (!compiler.RegisterTriggerEvent("MyTrigger4", ref MyEvent4)) { throw new Exception("Failed to register trigger 4"); }
            if (!compiler.RegisterRuntimeFunction("MyFunc4a", CompileTestHelpers.MyFunc4a)) { throw new Exception("Failed to register function 4a"); }
            if (!compiler.RegisterRuntimeFunction("MyFunc4b", CompileTestHelpers.MyFunc4b)) { throw new Exception("Failed to register function 4b"); }
            if (!compiler.RegisterRuntimeFunction("MyFunc4c", CompileTestHelpers.MyFunc4c)) { throw new Exception("Failed to register function 4c"); }
            if (!compiler.RegisterRuntimeFunction("MyFunc4d", CompileTestHelpers.MyFunc4d)) { throw new Exception("Failed to register function 4d"); }
            if (!compiler.RegisterConditionalFunction("MyCondFunc4a", CompileTestHelpers.MyCondFunc4a)) { throw new Exception("Failed to conditional register function 4a"); }
            if (!compiler.RegisterConditionalFunction("MyCondFunc4b", CompileTestHelpers.MyCondFunc4b)) { throw new Exception("Failed to conditional register function 4b"); }
            if (!compiler.RegisterConditionalFunction("MyCondFunc4c", CompileTestHelpers.MyCondFunc4c)) { throw new Exception("Failed to conditional register function 4c"); }
            if (!compiler.RegisterConditionalFunction("MyCondFunc4d", CompileTestHelpers.MyCondFunc4d)) { throw new Exception("Failed to conditional register function 4d"); }

            if (!compiler.RegisterTriggerEvent("MyTrigger5", ref MyEvent5)) { throw new Exception("Failed to register trigger 5"); }
            if (!compiler.RegisterRuntimeFunction("MyFunc5", CompileTestHelpers.MyFunc5)) { throw new Exception("Failed to Register Function 5"); }
            if (!compiler.RegisterConditionalFunction("MyCondFunc5a", CompileTestHelpers.MyCondFunc5a)) { throw new Exception("Failed to register conditional function 5a"); }
            if (!compiler.RegisterConditionalFunction("MyCondFunc5b", CompileTestHelpers.MyCondFunc5b)) { throw new Exception("Failed to register conditional function 5b"); }
            if (!compiler.RegisterProperty("MyVar5", () => myBool5a, (arg) =>
            {
                if (arg == null) { throw new Exception("Arg was null"); }
                if (arg is bool b) { myBool5a = b; }
                else { throw new Exception($"Expected bool, but got {arg.GetType().Name}"); }
            })) { throw new Exception("Failed to register property 5"); }

            Stopwatch sw = Stopwatch.StartNew();
            var compileProgramID = compiler.RegisterProgram(compileTestBody);
            sw.Stop();
            Console.WriteLine($"Compiled the compile test vector in {sw.ElapsedMilliseconds}ms");

            if (compileProgramID == Guid.Empty)
            { throw new Exception("Failed to compile program"); }

            Console.WriteLine($"Decompiling:");
            sw.Restart();
            Console.WriteLine(compiler.DecompileProgram(compileProgramID));
            sw.Stop();
            Console.WriteLine($"Decompiling Complete in {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            BaseCompilerRuntimeUnitTest.TestTrigger1();
            BaseCompilerRuntimeUnitTest.TestTrigger2();
            BaseCompilerRuntimeUnitTest.TestTrigger3();
            BaseCompilerRuntimeUnitTest.TestTrigger4();
            BaseCompilerRuntimeUnitTest.TestTrigger5();
            sw.Stop();
            Console.WriteLine($"Running Complete in {sw.ElapsedMilliseconds}ms");

            sw.Restart();
            var parseProgramID = compiler.RegisterProgram(parseTestBody);
            sw.Stop();
            Console.WriteLine($"Compiled the parse test vector in {sw.ElapsedMilliseconds}ms");


            return;
        }
    }
}