namespace Accretion.JitDumpVisualizer.Parsing.Tokens.Lexing
{
    internal static unsafe partial class Lexer
    {
        // Parsing methods in this file save on machine code size in switches by packing the information
        // This is in the following general format (measured to save about up to ~15% in code size as compared to the naive approach):
        // ulong result = [...[Width][Enum]]
        // Tight packing ensures nothing is wasted on encoding the constants in assembly
        // Further savings could be achieved by returning "result" directly
        // Instead of using "out int width"
        // I am not prepared to go that far yet
    }
}
