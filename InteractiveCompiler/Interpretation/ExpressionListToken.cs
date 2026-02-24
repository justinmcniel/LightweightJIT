using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ExpressionListToken
    {
        private List<ExpressionToken> Expressions { get; } = [];
        public static ExpressionListToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ExpressionListToken res = new();

            ExpressionToken? tmp;
            int internalIndex = index;

            while(true)
            {
                try
                {
                    tmp = ExpressionToken.TryParse(text, ref internalIndex, compiler);
                    Utilities.SkipWhitespace(text, ref internalIndex);

                    if (tmp == null)
                    { break; }

                    res.Expressions.Add(tmp);
                }
                catch (IndexOutOfRangeException)
                { break; }
            }

            if ( res.Expressions.Count == 0)
            { return null; }

            index = internalIndex;
            return res;
        }

        public string Decompile(string indentation = "")
        {
            string tmp = "";
            string res = "";

            foreach(var expression in Expressions)
            {
                res += tmp + expression.Decompile(indentation + "\t");
                tmp = $"\r\n{indentation}\t";
            }

            return res;
        }

        public Action<object?, IEnumerable<object?>?> Compile(IInteractiveCompiler compiler)
        {
            Action[] expressionFuncs = new Action[Expressions.Count];
            for (int i = 0; i < Expressions.Count; i++)
            { expressionFuncs[i] = Expressions[i].Compile(compiler); }

            void Reaction(object? sender, IEnumerable<object?>? environment)
            {
                Guid programID = Guid.Empty;
                if (environment != null)
                {
                    foreach (var envar in environment)
                    {
                        if (envar is Guid ID)
                        { programID = ID; }
                    }
                }

                foreach (var expressionFunc in expressionFuncs)
                { expressionFunc(); }
            }

            return Reaction;
        }
    }
}
