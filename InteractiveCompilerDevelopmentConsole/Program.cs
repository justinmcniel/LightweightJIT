using InteractiveCompiler;

namespace InteractiveCompilerDevelopmentConsole
{
    public static class Program
    {
        private static int x = 5;
        public static event EventHandler<IEnumerable<object?>?>? MyEvent;

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
        
        public static void Main(string[] args)
        {
            EventHandler<object> tmp = (object? sender, object arg) => 
                { Console.WriteLine(1); };
            tmp += (object? sender, object arg) => { Console.WriteLine(2); };
            tmp -= (object? sender, object arg) => { Console.WriteLine(2); };
            tmp?.Invoke(new(), 1);
            return;
            DirectoryInfo? codeBaseDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (codeBaseDirectory != null && codeBaseDirectory?.Name != "InteractiveCompilerDevelopmentConsole")
            { codeBaseDirectory = codeBaseDirectory?.Parent; }

            var consoleBaseDir = codeBaseDirectory;
            codeBaseDirectory = codeBaseDirectory?.Parent;

            if (consoleBaseDir?.Name != "InteractiveCompilerDevelopmentConsole")
            { throw new Exception("Could not find base directory"); }
            
            string programBody = File.ReadAllText($"{consoleBaseDir.FullName}/programTest.txt");
            BaseCompiler compiler = new();
            if (!compiler.RegisterTriggerEvent("MyTrigger", MyEvent))
            {
                throw new Exception("Failed to register trigger");
            }

            
            var programID = compiler.RegisterProgram(programBody);

            if (programID == Guid.Empty)
            {
                throw new Exception("Failed to compile program");
            }
            
            MyEvent?.Invoke(new object(), [1, 2, 3, 'a', "b", 'c']);

            return;
        }
    }
}