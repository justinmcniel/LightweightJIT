using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler
{
    public class CompilerException : Exception
    {
        public CompilerException() : base() { }
        public CompilerException(string? message) : base(message) { }
    }
}
