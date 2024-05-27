using SeaDb.Utilities;
using System.IO.MemoryMappedFiles;

namespace SeaDb
{
    public class SeaMappedFile
    {
        private readonly string _file;
        private bool _canFlush;
        private bool _flushing;
        public long Position;
        public long Length;

        private MemoryMappedFile _mmf;
        private MemoryMappedViewStream _stream;

        public SeaMappedFile(string fileName, string fileExtension)
        {
            _file = $"{fileName}.{fileExtension}";
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
        }

        private void GrowFile()
        {
            Flush();
            _canFlush = false;

            while (_flushing)
            { }

            _stream.Dispose();
            _mmf.Dispose();

            _mmf = MemoryMappedFile.CreateFromFile(_file, FileMode.OpenOrCreate, _file[^5..], Length + Constants.MAX_FILE_SIZE, MemoryMappedFileAccess.ReadWrite);
            _stream = _mmf.CreateViewStream();
            Length = _stream.Length;
            _canFlush = true;
        }

        private void InitializeFile(long capacity)
        {
            _mmf = MemoryMappedFile.CreateFromFile(_file, FileMode.OpenOrCreate, _file[^5..], capacity, MemoryMappedFileAccess.ReadWrite);
            _stream = _mmf.CreateViewStream();
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
