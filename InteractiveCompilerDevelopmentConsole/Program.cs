using InteractiveCompiler;

namespace InteractiveCompilerDevelopmentConsole
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string programBody = "";
            BaseCompiler compiler = new();
            compiler.RegisterProgram(programBody);
        }
    }
}