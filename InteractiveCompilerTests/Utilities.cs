using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        static string Parse(object? o) => o?.ToString() ?? "null";

        public static readonly Queue<string?> TextLogEvents = [];
        public static bool LogToConsole { get; set; } = false;
        public static void TextLog(string? s = "")
        {
            TextLogEvents.Enqueue(s);
            TestLog(s);
        }
        public static void TestLog(string? s = "")
        {
            if (LogToConsole)
            { Console.WriteLine(s); }
        }

        public enum CallType
        {
            RUNTIME_FUNCTION,
            CONDITIONAL_FUNCTION,
        }

        public static readonly Queue<(Func<IEnumerable<object?>?, object?> Func, IEnumerable<object?>? Args, object? ret)> RuntimeCallLog = [];
        public static readonly Queue<(Func<IEnumerable<object?>?, bool> Func, IEnumerable<object?>? Args, bool ret)> CondCallLog = [];
        public static readonly Queue<CallType> CallOrderLog = [];

        public static void LogCall(Func<IEnumerable<object?>?, object?> func, IEnumerable<object?>? args, object? ret)
        {
            RuntimeCallLog.Enqueue((func, args, ret));
            CallOrderLog.Enqueue(CallType.RUNTIME_FUNCTION);
        }
        public static void LogCall(Func<IEnumerable<object?>?, bool> func, IEnumerable<object?>? args, bool ret)
        {
            CondCallLog.Enqueue((func, args, ret));
            CallOrderLog.Enqueue(CallType.CONDITIONAL_FUNCTION);
        }

        public static void ClearLogs()
        {
            TextLogEvents.Clear();
            RuntimeCallLog.Clear();
            CondCallLog.Clear();
            CallOrderLog.Clear();
        }

        public static void ResetTests()
        {
            ClearLogs();
            ReturnObj1 = null;
            MyInt1 = 1;
            MyBool1 = true;
            MyBool2 = true;
            MyBool3 = true;
            MyBool4 = true;
            MyBool5a = true;
            MyBool5b = true;
        }

        public static object? MyFunc(IEnumerable<object?>? args)
        {
            if (args == null)
            {
                TestLog($"Args was null");
                LogCall(MyFunc, args, null);
                return null;
            }
            if (args.Count() != 2)
            {
                TestLog($"Unexpected ArgCount: {args.Count()}");
                LogCall(MyFunc, args, null);
                return null;
            }

            object? sender = args.ElementAt(0);
            object? arg = args.ElementAt(1);

            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered by {Parse(sender)} with an argument of {Parse(arg)}");
            TestLog();
            LogCall(MyFunc, args, true);
            return true;
        }

        public static bool? ReturnObj1 { get; set; } = null;
        public static object? MyFunc2(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            if (args != null)
            {
                foreach (var arg in args)
                { TestLog($"Argument: {Parse(arg)}"); }
            }
            var res = ReturnObj1;
            TestLog($"Return Value: {Parse(res)}");
            ReturnObj1 = true;
            TestLog();
            LogCall(MyFunc2, args, res);
            return res;
        }

        public static int MyInt1 { get; set; } = 1;
        public const int MY_INT_2 = 4;
        public static object? MyFunc3a(IEnumerable<object?>? args)
        {
            MyInt1 += 2;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MyInt1}");
            TestLog();
            LogCall(MyFunc3a, args, MyInt1);
            return MyInt1;
        }

        public static object? MyFunc3b(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MY_INT_2}");
            TestLog();
            LogCall(MyFunc3b, args, MY_INT_2);
            return MY_INT_2;
        }

        public static object? MyFunc3c(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            if (args != null)
            {
                foreach (var arg in args)
                { TestLog($"Argument: {Parse(arg)}"); }
            }
            TestLog();
            LogCall(MyFunc3c, args, null);
            return null;
        }

        public static object? MyFunc3d(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            TestLog();
            LogCall(MyFunc3d, args, null);
            return null;
        }

        public static object? MyFunc4a(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            TestLog();
            LogCall(MyFunc4a, args, null);
            return null;
        }

        public static object? MyFunc4b(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            TestLog();
            LogCall(MyFunc4b, args, null);
            return null;
        }

        public static object? MyFunc4c(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            TestLog();
            LogCall(MyFunc4c, args, null);
            return null;
        }

        public static object? MyFunc4d(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            TestLog();
            LogCall(MyFunc4d, args, null);
            return null;
        }

        public static bool MyBool1 { get; set; } = true;
        public static bool MyBool2 { get; set; } = true;
        public static bool MyBool3 { get; set; } = true;
        public static bool MyBool4 { get; set; } = true;

        public static bool MyCondFunc4a(IEnumerable<object?>? args)
        {
            MyBool1 = !MyBool1;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MyBool1}");
            TestLog();
            LogCall(MyCondFunc4a, args, MyBool1);
            return MyBool1;
        }

        public static bool MyCondFunc4b(IEnumerable<object?>? args)
        {
            MyBool2 = !MyBool2;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MyBool2}");
            TestLog();
            LogCall(MyCondFunc4b, args, MyBool2);
            return MyBool2;
        }

        public static bool MyCondFunc4c(IEnumerable<object?>? args)
        {
            MyBool3 = !MyBool3;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MyBool3}");
            TestLog();
            LogCall(MyCondFunc4c, args, MyBool3);
            return MyBool3;
        }

        public static bool MyCondFunc4d(IEnumerable<object?>? args)
        {
            MyBool4 = !MyBool4;
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MyBool4}");
            TestLog();
            LogCall(MyCondFunc4d, args, MyBool4);
            return MyBool4;
        }

        public static bool MyBool5a { get; set; } = true;
        public static bool MyBool5b { get; set; } = true;

        public static object? MyFunc5(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered");
            MyBool5b = false;
            TestLog($"Set {nameof(MyBool5b)} to {MyBool5b}");
            TestLog();
            LogCall(MyFunc5, args, null);
            return null;
        }
        public static bool MyCondFunc5a(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MyBool5a}");
            TestLog();
            LogCall(MyCondFunc5a, args, MyBool5a);
            return MyBool5a;
        }

        public static bool MyCondFunc5b(IEnumerable<object?>? args)
        {
            TestLog($"{Parse(MethodBase.GetCurrentMethod()?.Name)} Triggered and returned {MyBool5b}");
            TestLog();
            TestLog();
            LogCall(MyCondFunc5b, args, MyBool5b);
            return MyBool5b;
        }
    }
}
