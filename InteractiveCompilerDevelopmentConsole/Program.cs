using InteractiveCompiler;
using System.Reflection;

namespace InteractiveCompilerDevelopmentConsole
{
    public static class Program
    { /// TODO: Test Properties, test if elseif (no else)
        public static event EventHandler<object?>? MyEvent;
        public static event EventHandler<object?>? MyEvent2;
        public static event EventHandler<object?>? MyEvent3;
        public static event EventHandler<object?>? MyEvent4;
        static string Parse(object? o) => o?.ToString() ?? "null";

        public static object? MyFunc(IEnumerable<object?>? args)
        {
            if (args == null)
            {
                Console.WriteLine($"Args was null");
                return null;
            }
            if (args.Count() != 2)
            {
                Console.WriteLine($"Unexpected ArgCount: {args.Count()}");
                    return null;
            }

            object? sender = args.ElementAt(0);
            object? arg = args.ElementAt(1);

            Console.WriteLine($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered by {Parse(sender)} with an argument of {Parse(arg)}");
            Console.WriteLine();
            return true;
        }

        private static bool? returnObj1 = null;
        public static object? MyFunc2(IEnumerable<object?>? args)
        {
            Console.WriteLine($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            if (args != null)
            {
                foreach (var arg in args)
                { Console.WriteLine($"Argument: {Parse(arg)}"); }
            }
            var res = returnObj1;
            Console.WriteLine($"Return Value: {Parse(res)}");
            returnObj1 = true;
            Console.WriteLine();
            return res;
        }

        private static int myInt1 = 1;
        private const int myInt2 = 4;
        public static object? MyFunc3a(IEnumerable<object?>? args)
        {
            myInt1 += 2;
            Console.WriteLine($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myInt1}");
            Console.WriteLine();
            return myInt1;
        }

        public static object? MyFunc3b(IEnumerable<object?>? args)
        {
            Console.WriteLine($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myInt2}");
            Console.WriteLine();
            return myInt2;
        }

        public static object? MyFunc3c(IEnumerable<object?>? args)
        {
            Console.WriteLine($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            if (args != null)
            {
                foreach (var arg in args)
                { Console.WriteLine($"Argument: {Parse(arg)}"); }
            }
            Console.WriteLine();
            return null;
        }

        public static object? MyFunc3d(IEnumerable<object?>? args)
        {
            Console.WriteLine($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            Console.WriteLine();
            return null;
        }

        public static object? MyFunc4a(IEnumerable<object?>? args)
        {
            Console.WriteLine($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            Console.WriteLine();
            return null;
        }

        public static object? MyFunc4b(IEnumerable<object?>? args)
        {
            Console.WriteLine($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            Console.WriteLine();
            return null;
        }

        public static object? MyFunc4c(IEnumerable<object?>? args)
        {
            Console.WriteLine($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            Console.WriteLine();
            return null;
        }

        public static object? MyFunc4d(IEnumerable<object?>? args)
        {
            Console.WriteLine($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            Console.WriteLine();
            return null;
        }

        private static bool myBool1 = true;
        private static bool myBool2 = true;
        private static bool myBool3 = true;
        private static bool myBool4 = true;

        public static bool MyCondFunc4a(IEnumerable<object?>? args)
        {
            myBool1 = !myBool1;
            Console.WriteLine($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool1}");
            Console.WriteLine();
            return myBool1;
        }

        public static bool MyCondFunc4b(IEnumerable<object?>? args)
        {
            myBool2 = !myBool2;
            Console.WriteLine($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool2}");
            Console.WriteLine();
            return myBool2;
        }

        public static bool MyCondFunc4c(IEnumerable<object?>? args)
        {
            myBool3 = !myBool3;
            Console.WriteLine($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool3}");
            Console.WriteLine();
            return myBool3;
        }

        public static bool MyCondFunc4d(IEnumerable<object?>? args)
        {
            myBool4 = !myBool4;
            Console.WriteLine($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {myBool4}");
            Console.WriteLine();
            return myBool4;
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

        public static void Main(string[] args)
        {
            DirectoryInfo? codeBaseDirectory = new(Directory.GetCurrentDirectory());
            while (codeBaseDirectory != null && codeBaseDirectory?.Name != "InteractiveCompilerDevelopmentConsole")
            { codeBaseDirectory = codeBaseDirectory?.Parent; }

            var consoleBaseDir = codeBaseDirectory;
            codeBaseDirectory = codeBaseDirectory?.Parent;

            if (consoleBaseDir?.Name != "InteractiveCompilerDevelopmentConsole")
            { throw new Exception("Could not find base directory"); }
            
            string programBody = File.ReadAllText($"{consoleBaseDir.FullName}/compileTest.txt");
            BaseCompiler compiler = new();

            if (!compiler.RegisterTriggerEvent("MyTrigger", ref MyEvent)) { throw new Exception("Failed to register trigger"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc), MyFunc)) { throw new Exception("Failed to register function"); }

            if (!compiler.RegisterTriggerEvent("MyTrigger2", ref MyEvent2)) { throw new Exception("Failed to register trigger"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc2), MyFunc2)) { throw new Exception("Failed to register function"); }

            if (!compiler.RegisterTriggerEvent("MyTrigger3", ref MyEvent3)) { throw new Exception("Failed to register trigger"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc3a), MyFunc3a)) { throw new Exception("Failed to register function"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc3b), MyFunc3b)) { throw new Exception("Failed to register function"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc3c), MyFunc3c)) { throw new Exception("Failed to register function"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc3d), MyFunc3d)) { throw new Exception("Failed to register function"); }

            if (!compiler.RegisterTriggerEvent("MyTrigger4", ref MyEvent4)) { throw new Exception("Failed to register trigger"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc4a), MyFunc4a)) { throw new Exception("Failed to register function"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc4b), MyFunc4b)) { throw new Exception("Failed to register function"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc4c), MyFunc4c)) { throw new Exception("Failed to register function"); }
            if (!compiler.RegisterRuntimeFunction(nameof(MyFunc4d), MyFunc4d)) { throw new Exception("Failed to register function"); }
            if (!compiler.RegisterConditionalFunction(nameof(MyCondFunc4a), MyCondFunc4a)) { throw new Exception("Failed to register function"); }
            if (!compiler.RegisterConditionalFunction(nameof(MyCondFunc4b), MyCondFunc4b)) { throw new Exception("Failed to register function"); }
            if (!compiler.RegisterConditionalFunction(nameof(MyCondFunc4c), MyCondFunc4c)) { throw new Exception("Failed to register function"); }
            if (!compiler.RegisterConditionalFunction(nameof(MyCondFunc4d), MyCondFunc4d)) { throw new Exception("Failed to register function"); }

            var programID = compiler.RegisterProgram(programBody);

            if (programID == Guid.Empty)
            { throw new Exception("Failed to compile program"); }

            Console.WriteLine("Decompiling:");
            Console.WriteLine(compiler.DecompileProgram(programID));
            Console.WriteLine("Decompiling Complete.");
            Console.Write("Press Enter to Continue."); Console.ReadLine();

            TestTrigger1();
            Console.Write("Press Enter to Continue."); Console.ReadLine();
            TestTrigger2();
            Console.Write("Press Enter to Continue."); Console.ReadLine();
            TestTrigger3();
            Console.Write("Press Enter to Continue."); Console.ReadLine();
            TestTrigger4();

            return;
        }
    }
}