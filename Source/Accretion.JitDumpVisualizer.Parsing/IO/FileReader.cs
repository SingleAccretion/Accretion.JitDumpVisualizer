using System;
using System.IO;

namespace Accretion.JitDumpVisualizer.Parsing.IO
{
    internal struct FileReader
    {
        private readonly StreamReader _fileStream;

        public FileReader(Stream fileStream)
        {
            _fileStream = new StreamReader(fileStream);
            // We expect the dump to be almost exclusively ASCII
            Length = fileStream.Length;
        }

        public int ReadBlock(Span<char> buffer)
        {
            int totalBytes = 0;

            while (buffer.Length > 0)
            {
                int bytesRead = _fileStream.Read(buffer);
                totalBytes += bytesRead;

                if (bytesRead == 0)
                    break;

                buffer = buffer.Slice(bytesRead);
            }

            return totalBytes;
        }

        public long Length { get; }

        public void Dispose() => _fileStream.Dispose();
    }
}
