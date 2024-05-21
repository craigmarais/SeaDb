using SeaDb.Utilities;
using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaDb
{
    public class SeaMappedFile
    {
        private readonly string _file;

        public long Position;
        public long Length;

        private MemoryMappedFile _mmf;
        private MemoryMappedViewStream _stream;

        public SeaMappedFile(string fileName, string fileExtension)
        {
            _file = $"{fileName}.{fileExtension}";
            _mmf = MemoryMappedFile.CreateFromFile(_file, FileMode.Create, null, Constants.MAX_FILE_SIZE, MemoryMappedFileAccess.ReadWrite);
            _stream = _mmf.CreateViewStream();
            Length = _stream.Length;
        }

        public bool IsOpen()
        {
            return _stream.CanWrite;
        }

        public void Write(Span<byte> data)
        {
            _stream.Write(data);
            Position += data.Length;
        }

        public int Read(byte[] buffer)
        {
            return _stream.Read(buffer);
        }

        public int Read(byte[] buffer, int position)
        {
            using (var stream = _mmf.CreateViewAccessor(position, 0))
            {
                return stream.ReadArray(0, buffer, 0, (int)(Position - position));
            }
        }

        public void Flush()
        {
            _stream.Flush();
        }

        public void Dispose()
        {
            Flush();
            _stream.Close();
            _stream.Dispose();
            _mmf.SafeMemoryMappedFileHandle.Close();
            _mmf.Dispose();
        }
    }
}
