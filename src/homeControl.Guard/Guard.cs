using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace homeControl
{
    public static class Guard
    {
        private sealed class AssertFailedException : Exception
        {
            public AssertFailedException(string message,
                string memberName,
                string filePath,
                int lineNumber) : base(FormatErrorMessage(message, memberName, filePath, lineNumber))
            {
            }

            private static string FormatErrorMessage(string assertionDescription, string memberName, string filePath, int lineNumber)
            {
                return $@"Assertion is false: {assertionDescription}.
at {memberName},
{filePath}, line {lineNumber}.";
            }
        }

        [Conditional("DEBUG")]
        [AssertionMethod]
        public static void DebugAssertArgument(
            [AssertionCondition(AssertionConditionType.IS_TRUE)]bool assertion,
            [InvokerParameterName]string argName,
            [CallerMemberName]string memberName = null,
            [CallerFilePath]string filePath = null,
            [CallerLineNumber]int lineNumber = 0)
        {
            if (!assertion)
            {
                throw new AssertFailedException($"argument {argName} does not satisfy precondition", memberName, filePath, lineNumber);
            }
        }

        [Conditional("DEBUG")]
        [AssertionMethod]
        public static void DebugAssertArgumentNotNull<T>(
            [NoEnumeration]T argument,
            [InvokerParameterName]string argName,
            [CallerMemberName] string memberName = null,
            [CallerFilePath] string filePath = null,
            [CallerLineNumber] int lineNumber = 0)
            where T: class
        {
            if (argument == null)
            {
                throw new AssertFailedException($"argument {argName} should not be null", memberName, filePath, lineNumber);
            }
        }

        [Conditional("DEBUG")]
        [AssertionMethod]
        public static void DebugAssertArgumentNotDefault<T>(
            T argument,
            [InvokerParameterName]string argName,
            [CallerMemberName] string memberName = null,
            [CallerFilePath] string filePath = null,
            [CallerLineNumber] int lineNumber = 0)
            where T : struct
        {
            if (default(T).Equals(argument))
            {
                throw new AssertFailedException($"argument {argName} should not have default value", memberName, filePath, lineNumber);
            }
        }

        [Conditional("DEBUG")]
        [AssertionMethod]
        public static void DebugAssert(
            [AssertionCondition(AssertionConditionType.IS_TRUE)]bool assertion,
            [NotNull]string message,
            [CallerMemberName]string memberName = null,
            [CallerFilePath]string filePath = null,
            [CallerLineNumber]int lineNumber = 0)
        {
            DebugAssertArgument(!string.IsNullOrEmpty(message), nameof(message));

            if (!assertion)
            {
                throw new AssertFailedException(message, memberName, filePath, lineNumber);
            }
        }
    }
}
