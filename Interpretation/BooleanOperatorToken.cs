using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class BooleanOperatorToken
    {
        public enum OperatorType
        {
            AND,
            OR,
        }

        public OperatorType? Value { get; private set; }

        public static BooleanOperatorToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            BooleanOperatorToken res = new();
            int internalIndex = index;

            if (Utilities.NextTokenMatches(text, ref internalIndex, "and"))
            { res.Value = OperatorType.AND; }
            else if (Utilities.NextTokenMatches(text, ref internalIndex, "or"))
            { res.Value = OperatorType.OR; }

            if (res.Value == null)
            { return null; }

            index = internalIndex;
            return res;
        }
    }
}
