using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace InteractiveCompilerTests
{ // this code is largly borrowed from https://learn.microsoft.com/en-us/dotnet/core/testing/order-unit-tests?pivots=xunit
    [AttributeUsage(AttributeTargets.All)]
    public class PriorityAttribute(int prio) : Attribute { public int Prio { get; } = prio; }

    public class PriorityOrderer : ITestCaseOrderer
    {
        public static readonly string assemblyName = typeof(PriorityAttribute).AssemblyQualifiedName!;
        public const int defaultPriority = 0;

        private static int GetPrio<TTest>(TTest testCase) where TTest : ITestCase
        {
            return testCase.TestMethod.Method.GetCustomAttributes(assemblyName).FirstOrDefault()?
                .GetNamedArgument<int>(nameof(PriorityAttribute.Prio)) ?? 0;
        }

        public IEnumerable<TTest> OrderTestCases<TTest>(IEnumerable<TTest> testCases) where TTest : ITestCase
        {
            var sortedTests = new SortedDictionary<int, List<TTest>>();

            foreach(var testCase in testCases)
            {
                int prio = GetPrio(testCase);
                if(!sortedTests.TryGetValue(prio, out var testList))
                { 
                    testList = [];
                    sortedTests[prio] = testList;
                }

                testList.Add(testCase);
            }

            Random rng = new();
            foreach (TTest testCase in sortedTests.Keys.SelectMany(
                prio => sortedTests[prio].OrderBy(testCase => rng.Next())))
            {
                yield return testCase;
            }
        }
    }
}
