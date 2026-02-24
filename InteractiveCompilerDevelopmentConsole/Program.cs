using InteractiveCompiler;

namespace InteractiveCompilerDevelopmentConsole
{
    public static class Program
    {
        private static int x = 5;
        public static event EventHandler<object?>? MyEvent;// = (sender, args) => 
        //{ Console.WriteLine($"{nameof(MyEvent)} triggered by {(sender ?? "UNKNOWN")} with the args of {args?.ToString()}"); };

        public static object? MyFunc(IEnumerable<object?>? args)
        {
            if (args == null)
            { throw new Exception("Got No Args"); }

            foreach (var arg in args)
            {
                if (arg is int argInt)
                {
                    x += argInt;
                }
            }

            return null;
        }

        public static object? MyFunc2(IEnumerable<object?>? args)
        {
            if (args == null)
            { throw new Exception("Got No Args"); }

            foreach (var arg in args)
            {
                Console.WriteLine((arg ?? ((object)"EmptyArg")).ToString());
            }

            return null;
        }

        public static bool MyCondFunc(IEnumerable<object?>? args)
        {
            if (args == null)
            { return false; }
            if (args.Count() < 2)
            { return false; }
            bool res = false;
            foreach (var arg in args)
            {
                if (arg is bool  argBool)
                { res |= argBool; }
                else
                { res |= arg != null; }
            }
            return res;
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
            
            string programBody = File.ReadAllText($"{consoleBaseDir.FullName}/programTest.txt");
            Console.WriteLine(programBody.IndexOf("On MyTrigger: if(\tmyVar2 != myVar3 or"));
            BaseCompiler compiler = new();

            if (!compiler.RegisterTriggerEvent("MyTrigger", ref MyEvent))
            {
                throw new Exception("Failed to register trigger");
            }

            if(!compiler.RegisterRuntimeFunction("MyFunc", MyFunc))
            {
                throw new Exception("Failed to register function");
            }

            if(!compiler.RegisterRuntimeFunction("MyFunc2", MyFunc2))
            {
                throw new Exception("Failed to register function");
            }

            if(!compiler.RegisterConditionalFunction("MyCondFunc", MyCondFunc))
            {
                throw new Exception("Failed to register function");
            }

            var programID = compiler.RegisterProgram(programBody);

            if (programID == Guid.Empty)
            {
                throw new Exception("Failed to compile program");
            }

            Console.WriteLine("Decompiling:");
            Console.WriteLine(compiler.DecompileProgram(programID));
            Console.WriteLine("Decompiling Complete.");

            MyEvent?.Invoke("Main", 68);

            return;
        }
    }
}