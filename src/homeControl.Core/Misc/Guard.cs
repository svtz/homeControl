using System;
using System.Diagnostics;

namespace homeControl.Core.Misc
{
    public static class Guard
    {
        [Conditional("DEBUG")]
        public static void DebugAssertArgument(bool assertion, string argName)
        {
            if (!assertion)
            {
                throw new ArgumentException(argName);
            }
        }
    }
}
