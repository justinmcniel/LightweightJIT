using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompilerTests
{
    public static class TestUtilities
    {
        public static DirectoryInfo FindBaseDirectory()
        {
            var codeBaseDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (codeBaseDirectory != null && codeBaseDirectory?.Name != "InteractiveCompiler")
            { codeBaseDirectory = codeBaseDirectory?.Parent; }

            if (codeBaseDirectory?.Name != "InteractiveCompiler" || codeBaseDirectory == null)
            { throw new Exception("Could not find base directory"); }

            return codeBaseDirectory;
        }

    }
}
