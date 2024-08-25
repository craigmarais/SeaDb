using SeaDb.Utilities;
using System.IO.MemoryMappedFiles;

namespace SeaDb
{
    public class SeaMappedFile
    {
        private readonly string _filePath;
        private readonly string _file;
        private bool _canFlush;
        private bool _flushing;
        public long Position;
        public long Length;

        private MemoryMappedFile _mmf;
        private MemoryMappedViewStream _stream;
        private MemoryMappedViewAccessor _accessor;

        public SeaMappedFile(string fileName, string fileExtension)
        {
            _filePath = $"{fileName}.{fileExtension}";
            _file = _filePath.Substring(_filePath.LastIndexOf('/') + 1);
            InitializeFile(Constants.MAX_FILE_SIZE);
        }

        public bool IsOpen()
        {
            return _stream.CanWrite;
        }

        public void Write(Span<byte> data)
        {
            if (data.Length + Position > Length)
            {
                GrowFile();
            }

            _stream.Write(data);
            Position += data.Length;
            _accessor.Write(0, Position);
        }

        private void GrowFile()
        {
            Flush();
            _canFlush = false;

            while (_flushing)
            { }

            _stream.Dispose();
            _mmf.Dispose();

            _mmf = MemoryMappedFile.CreateFromFile(_filePath, FileMode.OpenOrCreate, _file, Length + Constants.MAX_FILE_SIZE, MemoryMappedFileAccess.ReadWrite);
            _stream = _mmf.CreateViewStream(8, 0);
            _accessor = _mmf.CreateViewAccessor(0, 8);
            Length = _stream.Length;
            _canFlush = true;
        }

        private void InitializeFile(long capacity)
        {
            _mmf = MemoryMappedFile.CreateFromFile(_filePath, FileMode.OpenOrCreate, _file, capacity, MemoryMappedFileAccess.ReadWrite);
            _stream = _mmf.CreateViewStream(8, 0);
            _accessor = _mmf.CreateViewAccessor(0, 8);
            Length = _stream.Length;
            _canFlush = true;
        }

        public int Read(byte[] buffer)
        {
            return _stream.Read(buffer);
        }

        public int Read(byte[] buffer, int position)
        {
            if (Position <= position)
                return 0;

            using (var stream = _mmf.CreateViewAccessor(position, 0))
            {
                return stream.ReadArray(0, buffer, 0, (int)(Position - position));
            }
        }

        public void Flush()
        {
            if (_canFlush)
            {
                _flushing = true;
                _stream.Flush();
                _flushing = false;
            }
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
