using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveCompiler.Interpretation
{
    internal class BinToken
    {
        public int? Value { get; private set; }
        public static BinToken? TryParse(string text, ref int index, IInteractiveCompiler compiler)
        {
            BinToken res = new();
            int internalIndex = index;

            string val = "";
            try
            {
                while (text[internalIndex] == '1' || text[internalIndex] == '0')
                { val += text[internalIndex++]; }
            }
            catch (IndexOutOfRangeException)
            { return null; }

            if (!string.IsNullOrEmpty(val))
            {
                try
                { res.Value = Convert.ToInt32(val, 2); }
                catch { return null; }

                index = internalIndex;
                return res;
            }

            return null;
        }
    }
}
