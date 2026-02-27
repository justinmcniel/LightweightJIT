using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ElseIfExpressionToken
    {
        private List<(ConditionalToken Conditional, ExpressionListToken Expressions)> CodeBlocks { get; } = [];

        private static bool? BeginningMatches(string text, ref int index, IInteractiveCompiler compiler)
        {
            bool else_present = Utilities.NextTokenMatches(text, ref index, "else");
            var tmpIndex = index;
            Utilities.SkipWhitespace(text, ref index);
            bool whiteSpace_present = tmpIndex != index;
            bool if_present = Utilities.NextTokenMatches(text, ref index, "if");
            bool oParen_present = Utilities.NextTokenMatches(text, ref index, "(");
            if (!oParen_present && else_present && whiteSpace_present && if_present)
            {
                compiler.LogError($"ERROR: {Utilities.GetPosition(text, index)} " +
                    $"Was expecting {Utilities.ReadableSymbol("(")}, " +
                    $"but got {Utilities.NextTokenReadable(text, index)} instead");
                return null;
            }
            return else_present && whiteSpace_present && if_present && oParen_present;
        }

        public static ElseIfExpressionToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ElseIfExpressionToken res = new();
            int internalIndex = index;

            bool errorlessBeginning = true;
            int elseifIndex = internalIndex;
            while (BeginningMatches(text, ref elseifIndex, compiler) ?? (errorlessBeginning = false))
            {
                ConditionalToken? cond = ConditionalToken.TryParse(text, ref elseifIndex, compiler);
                if(cond != null && Utilities.NextTokenMatches(text, ref elseifIndex, ")"))
                {
                    if (Utilities.NextTokenMatches(text, ref elseifIndex, "{"))
                    {
                        ExpressionListToken? expressions = ExpressionListToken.TryParse(text, ref elseifIndex, compiler);
                        if (expressions != null && Utilities.NextTokenMatches(text, ref elseifIndex, "}"))
                        {
                            res.CodeBlocks.Add((cond, expressions));
                            internalIndex = elseifIndex;
                        }
                        else if (expressions != null)
                        {
                            compiler.LogError($"ERROR: {Utilities.GetPosition(text, internalIndex)} " +
                                $"Was expecting {Utilities.ReadableSymbol("}")}, " +
                                $"but got {Utilities.NextTokenReadable(text, internalIndex)} instead");
                            return null;
                        }
                        else
                        { return null; }
                    }
                    else
                    {
                        compiler.LogError($"ERROR: {Utilities.GetPosition(text, internalIndex)} " +
                            $"Was expecting {Utilities.ReadableSymbol("{")}, " +
                            $"but got {Utilities.NextTokenReadable(text, internalIndex)} instead");
                        return null;
                    }
                }
                else
                {
                    compiler.LogError($"ERROR: {Utilities.GetPosition(text, internalIndex)} " +
                        $"Was expecting {Utilities.ReadableSymbol(")")}, " +
                        $"but got {Utilities.NextTokenReadable(text, internalIndex)} instead");
                    return null;
                }
            }

            if(!errorlessBeginning)
            { return null; }

            if (res.CodeBlocks.Count > 0)
            {
                index = internalIndex;
                return res;
            }

            return null;
        }
        public string Decompile(string indentation = "")
        {
            string res = "";
            foreach(var (Conditional, Expressions) in CodeBlocks)
            {
                res += $"else if ({Conditional.Decompile(indentation)})\r\n{indentation}";
                res += $"{{\r\n{indentation}\t";
                res += Expressions.Decompile(indentation + "\t").TrimEnd() + $"\r\n{indentation}";
                res += $"}}\r\n{indentation}";
            }
            return res;
        }

        public Func<bool>? Compile(IInteractiveCompiler compiler)
        {
            int len = CodeBlocks.Count;
            if (len <= 0)
            { return null; }

            var Conditionals = new Func<bool>[len];
            var Expressions = new Action[len];

            for (int i = 0; i < len; i++)
            {
                var (Conditional, Expression) = CodeBlocks[i];
                Conditionals[i] = Conditional.Compile(compiler);
                var exprFunc = Expression.Compile(compiler);
                Expressions[i] = () => { exprFunc(null, null); };
            }

            bool Evaluate()
            {
                for (int i = 0; i < len; i++)
                {
                    if (Conditionals[i]())
                    {
                        Expressions[i]();
                        return true;
                    }
                }

                return false;
            }
            return Evaluate;
        }
    }
}
