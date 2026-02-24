using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ArgumentListToken
    {
        private List<ValueToken> Values { get; } = [];
        public static ArgumentListToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ArgumentListToken res = new();

            ValueToken? val;
            do
            {
                val = ValueToken.TryParse(text, ref index, compiler);
                if (val != null)
                { res.Values.Add(val); }

                if(!Utilities.NextTokenMatches(text, ref index, ","))
                { break; }
            } while (val != null);

            return res;
        }
        public string Decompile(string indentation = "") => String.Join(", ", Values);

        public Func<IEnumerable<object?>?> Compile(IInteractiveCompiler compiler)
        {
            Values[0].Compile(compiler);
            int len = Values.Count;

            Func<object?>[] Getters = new Func<object?>[len];
            for(int i = 0; i < len; i++)
            { Getters[i] = Values[i].Compile(compiler); }

            IEnumerable<object?>? GetArgs()
            {
                object?[] res = new object?[len];
                for (int i = 0;i < len; i++)
                { res[i] = Getters[i](); }
                return res;
            }

            return (len <= 0) ? (() => null) : GetArgs;
        }
    }
}
