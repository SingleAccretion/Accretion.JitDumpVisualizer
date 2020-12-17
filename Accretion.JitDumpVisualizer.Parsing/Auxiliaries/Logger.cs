using System;
using System.Globalization;
using System.IO;

namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
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

        public static void Log(string message) => _logWriter?.WriteLine(message);
    }
}
