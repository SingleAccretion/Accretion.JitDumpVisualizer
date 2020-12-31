using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries.Logging
{
    internal static class Logger
    {
        public const string EnableLoggingEnvironmentVariable = "JitDumpVisualizerEnableLogging";

        public static bool Enabled { get; } = Environment.GetEnvironmentVariable(EnableLoggingEnvironmentVariable)?.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase) ?? false;
        public static bool OverwriteOldLogs { get; } = true;

        private static readonly StreamWriter? _logWriter;

        static Logger()
        {
            if (Enabled)
            {
                var uniquifier = OverwriteOldLogs ? "" : $"-{DateTime.Now.ToString(CultureInfo.InvariantCulture)}";
                _logWriter = new StreamWriter($"JitDumpVisualizerLog{uniquifier}");
            }
        }

        [Conditional(Assert.DebugMode)]
        public static void Log(LoggedEvent loggedEvent) => _logWriter?.WriteLine(loggedEvent.ToString());
        
        [Conditional(Assert.DebugMode)]
        public static void Log<T>(LoggedEvent loggedEvent, T value) => _logWriter?.WriteLine($"{loggedEvent}: {value}");
    }
}
