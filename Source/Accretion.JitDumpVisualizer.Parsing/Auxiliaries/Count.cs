namespace Accretion.JitDumpVisualizer.Parsing.Auxiliaries
{
    public static class Count
    {
        public static unsafe int OfLeading(char* start, char character)
        {
            int count = 0;
            while (start[count] == character)
            {
                count++;
            }

            Assert.True(start[count] != character);
            return count;
        }
    }
}
