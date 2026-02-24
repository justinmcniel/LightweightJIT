using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class ComparisonOperatorToken
    {
        public enum OperatorType
        {
            EQUALS,
            NOT_EQUALS,
            LESS_THAN,
            LESS_THAN_OR_EQUAL,
            GREATER_THAN,
            GREATER_THAN_OR_EQUAL,
        }

        public OperatorType? Value { get; private set; }

        public static ComparisonOperatorToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            ComparisonOperatorToken res = new();
            int internalIndex = index;

            if (Utilities.NextTokenMatches(text, ref internalIndex, "=="))
            { res.Value = OperatorType.EQUALS; }
            else if (Utilities.NextTokenMatches(text, ref internalIndex, "!="))
            { res.Value = OperatorType.NOT_EQUALS; }
            else if (Utilities.NextTokenMatches(text, ref internalIndex, "<="))
            { res.Value = OperatorType.LESS_THAN_OR_EQUAL; }
            else if (Utilities.NextTokenMatches(text, ref internalIndex, "<"))
            { res.Value = OperatorType.LESS_THAN; }
            else if (Utilities.NextTokenMatches(text, ref internalIndex, ">="))
            { res.Value = OperatorType.GREATER_THAN_OR_EQUAL; }
            else if (Utilities.NextTokenMatches(text, ref internalIndex, ">"))
            { res.Value = OperatorType.GREATER_THAN; }

            if (res.Value == null)
            { return null; }

            index = internalIndex;
            return res;
        }
        public string Decompile(string indentation = "") => Value switch
        {
            OperatorType.EQUALS => "==",
            OperatorType.NOT_EQUALS => "!=",
            OperatorType.LESS_THAN => "<",
            OperatorType.LESS_THAN_OR_EQUAL => "<=",
            OperatorType.GREATER_THAN => ">",
            OperatorType.GREATER_THAN_OR_EQUAL => ">=",
            _ => "",
        };
    }
}
