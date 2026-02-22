using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class DecimalToken
    {
        public int? Value { get; private set; }
        public static DecimalToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            DecimalToken res = new();
            int internalIndex = index;

            string val = "";
            try
            {
                while (Char.IsDigit(text[internalIndex]))
                { val += text[internalIndex++]; }
            }
            catch (IndexOutOfRangeException)
            { return null; }

            if (!string.IsNullOrEmpty(val))
            {
                try
                { res.Value = Convert.ToInt32(val, 10); }
                catch { return null; }

                index = internalIndex;
                return res;
            }

            return null;
        }
    }
}
