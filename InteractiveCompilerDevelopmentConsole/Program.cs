using InteractiveCompiler;
using System.Diagnostics;
using System.Reflection;

namespace InteractiveCompilerDevelopmentConsole
{
    public static class Program
    {
        public static event EventHandler<object?>? MyEvent;
        public static event EventHandler<object?>? MyEvent2;
        public static event EventHandler<object?>? MyEvent3;
        public static event EventHandler<object?>? MyEvent4;
        public static event EventHandler<object?>? MyEvent5;
        static string Parse(object? o) => o?.ToString() ?? "null";

        private static void InterpreterLog(string s = "")
        {
            //Console.WriteLine(s);
        }

        public static object? MyFunc(IEnumerable<object?>? args)
        {
            if (args == null)
            {
                InterpreterLog($"Args was null");
                return null;
            }
            if (args.Count() != 2)
            {
                InterpreterLog($"Unexpected ArgCount: {args.Count()}");
                    return null;
            }

            object? sender = args.ElementAt(0);
            object? arg = args.ElementAt(1);

            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered by {Parse(sender)} with an argument of {Parse(arg)}");
            InterpreterLog();
            return true;
        }

        private static bool? returnObj1 = null;
        public static object? MyFunc2(IEnumerable<object?>? args)
        {
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            if (args != null)
            {
                foreach (var arg in args)
                { InterpreterLog($"Argument: {Parse(arg)}"); }
            }
            var res = returnObj1;
            InterpreterLog($"Return Value: {Parse(res)}");
            returnObj1 = true;
            InterpreterLog();
            return res;
        }

        private static int myInt1 = 1;
        private const int myInt2 = 4;
        public static object? MyFunc3a(IEnumerable<object?>? args)
        {
            myInt1 += 2;
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myInt1}");
            InterpreterLog();
            return myInt1;
        }

        public static object? MyFunc3b(IEnumerable<object?>? args)
        {
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myInt2}");
            InterpreterLog();
            return myInt2;
        }

        public static object? MyFunc3c(IEnumerable<object?>? args)
        {
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            if (args != null)
            {
                foreach (var arg in args)
                { InterpreterLog($"Argument: {Parse(arg)}"); }
            }
            InterpreterLog();
            return null;
        }

        public static object? MyFunc3d(IEnumerable<object?>? args)
        {
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            InterpreterLog();
            return null;
        }

        public static object? MyFunc4a(IEnumerable<object?>? args)
        {
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            InterpreterLog();
            return null;
        }

        public static object? MyFunc4b(IEnumerable<object?>? args)
        {
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            InterpreterLog();
            return null;
        }

        public static object? MyFunc4c(IEnumerable<object?>? args)
        {
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            InterpreterLog();
            return null;
        }

        public static object? MyFunc4d(IEnumerable<object?>? args)
        {
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            InterpreterLog();
            return null;
        }

        private static bool myBool1 = true;
        private static bool myBool2 = true;
        private static bool myBool3 = true;
        private static bool myBool4 = true;

        public static bool MyCondFunc4a(IEnumerable<object?>? args)
        {
            myBool1 = !myBool1;
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool1}");
            InterpreterLog();
            return myBool1;
        }

        public static bool MyCondFunc4b(IEnumerable<object?>? args)
        {
            myBool2 = !myBool2;
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool2}");
            InterpreterLog();
            return myBool2;
        }

        public static bool MyCondFunc4c(IEnumerable<object?>? args)
        {
            myBool3 = !myBool3;
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool3}");
            InterpreterLog();
            return myBool3;
        }

        public static bool MyCondFunc4d(IEnumerable<object?>? args)
        {
            myBool4 = !myBool4;
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool4}");
            InterpreterLog();
            return myBool4;
        }

        private static bool myBool5a = true;
        private static bool myBool5b = true;

        private static object? MyFunc5(IEnumerable<object?>? args)
        {
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            myBool5b = false;
            InterpreterLog($"Set {nameof(myBool5b)} to {myBool5b}");
            InterpreterLog();
            return null;
        }
        private static bool MyCondFunc5a(IEnumerable<object?>? args)
        {
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool5a}");
            InterpreterLog();
            return myBool5a;
        }

        private static bool MyCondFunc5b(IEnumerable<object?>? args)
        {
            InterpreterLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool5b}");
            InterpreterLog();
            InterpreterLog();
            return myBool5b;
        }

        private static void TestTrigger1()
        {
            /*
             * MyFunc(sender, triggerArgument)
             */
            MyEvent?.Invoke("TestTrigger1", 0b1010);
        }

        private static void TestTrigger2()
        {
            /*
             * 2-null, 2(123)-true
             * 2-true
             */
            MyEvent2?.Invoke(null, null);
            MyEvent2?.Invoke(null, null);
        }

        private static void TestTrigger3()
        {
            /*
             * 3a-3, 3b-4, 3c("test string")
             * 3a-5, 3b-4, 3d
             */
            MyEvent3?.Invoke(null, null);
            MyEvent3?.Invoke(null, null);
        }

        private static void TestTrigger4()
        {
            /* expect 
             * b4a-f, b4b-f, b4c-f, 4d, 
             * b4a-t, 4a, 
             * b4a-f, b4b-t, 4b, 
             * b4a-t, 4a, 
             * b4a-f, b4b-f, b4c-t, b4d-f, 4d
             * 
             * ** SET MY BOOL 3 TO FALSE
             * b4a-t, 4a, 
             * b4a-f, b4b-t, 4b, 
             * b4a-t, 4a, 
             * b4a-f, b4b-f, b4c-t, b4d-t, 4c
             */
            MyEvent4?.Invoke(null, null);
            MyEvent4?.Invoke(null, null);
            MyEvent4?.Invoke(null, null);
            MyEvent4?.Invoke(null, null);
            MyEvent4?.Invoke(null, null);

            myBool3 = false;
            MyEvent4?.Invoke(null, null);
            MyEvent4?.Invoke(null, null);
            MyEvent4?.Invoke(null, null);
            MyEvent4?.Invoke(null, null);
        }

        private static void TestTrigger5()
        {
            /*
             * 5a-t
             * 5a-f, 5b-t, 5
             * 5a-f, 5b-f
             */
            MyEvent5?.Invoke(null, null);
            MyEvent5?.Invoke(null, null);
            MyEvent5?.Invoke(null, null);
        }

        public static void Main(string[] args)
        {
            DirectoryInfo? codeBaseDirectory = new(Directory.GetCurrentDirectory());
            while (codeBaseDirectory != null && codeBaseDirectory?.Name != "InteractiveCompilerDevelopmentConsole")
            { codeBaseDirectory = codeBaseDirectory?.Parent; }

            var consoleBaseDir = codeBaseDirectory;
            codeBaseDirectory = codeBaseDirectory?.Parent;

            if (consoleBaseDir?.Name != "InteractiveCompilerDevelopmentConsole")
            { throw new Exception("Could not find base directory"); }
            
            string compileTestBody = File.ReadAllText($"{consoleBaseDir.FullName}/compileTest.txt");
            string parseTestBody = File.ReadAllText($"{consoleBaseDir.FullName}/parseTest.txt");
            BaseCompiler compiler = new();

            if (!compiler.RegisterTriggerEvent("MyTrigger", ref MyEvent)) { throw new Exception("Failed to register trigger 1"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc), MyFunc)) { throw new Exception("Failed to register function 1"); }

            if (!compiler.RegisterTriggerEvent("MyTrigger2", ref MyEvent2)) { throw new Exception("Failed to register trigger 2"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc2), MyFunc2)) { throw new Exception("Failed to register function 2"); }

            if (!compiler.RegisterTriggerEvent("MyTrigger3", ref MyEvent3)) { throw new Exception("Failed to register trigger 3"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc3a), MyFunc3a)) { throw new Exception("Failed to register function 3a"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc3b), MyFunc3b)) { throw new Exception("Failed to register function 3b"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc3c), MyFunc3c)) { throw new Exception("Failed to register function 3c"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc3d), MyFunc3d)) { throw new Exception("Failed to register function 3d"); }

            if (!compiler.RegisterTriggerEvent("MyTrigger4", ref MyEvent4)) { throw new Exception("Failed to register trigger 4"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc4a), MyFunc4a)) { throw new Exception("Failed to register function 4a"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc4b), MyFunc4b)) { throw new Exception("Failed to register function 4b"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc4c), MyFunc4c)) { throw new Exception("Failed to register function 4c"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc4d), MyFunc4d)) { throw new Exception("Failed to register function 4d"); }
            if (!compiler.RegisterConditionalFunction(nameof(MyCondFunc4a), MyCondFunc4a)) { throw new Exception("Failed to conditional register function 4a"); }
            if (!compiler.RegisterConditionalFunction(nameof(MyCondFunc4b), MyCondFunc4b)) { throw new Exception("Failed to conditional register function 4b"); }
            if (!compiler.RegisterConditionalFunction(nameof(MyCondFunc4c), MyCondFunc4c)) { throw new Exception("Failed to conditional register function 4c"); }
            if (!compiler.RegisterConditionalFunction(nameof(MyCondFunc4d), MyCondFunc4d)) { throw new Exception("Failed to conditional register function 4d"); }

            if (!compiler.RegisterTriggerEvent("MyTrigger5", ref MyEvent5)) { throw new Exception("Failed to register trigger 5"); }
            if (!compiler.RegisterRuntimeFunction("MyFunc5", MyFunc5)) { throw new Exception("Failed to Register Function 5"); }
            if (!compiler.RegisterConditionalFunction(nameof(MyCondFunc5a), MyCondFunc5a)) { throw new Exception("Failed to register conditional function 5a"); }
            if (!compiler.RegisterConditionalFunction(nameof(MyCondFunc5b), MyCondFunc5b)) { throw new Exception("Failed to register conditional function 5b"); }
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
            TestTrigger1();
            TestTrigger2();
            TestTrigger3();
            TestTrigger4();
            TestTrigger5();
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