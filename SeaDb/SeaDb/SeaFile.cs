namespace SeaDb
{
    public class SeaFile : IDisposable
    {
        private readonly string _file;

        public long Position;
        public long Length => _stream.Length;

        private FileStream _stream;
        
        public SeaFile(string fileName, string fileExtension)
        {
            
            _file = $"{fileName}.{fileExtension}";
            if (!File.Exists(_file))
                _stream = File.Create(_file , 104_857_600);
        }

        public void OpenRead()
        {
            _stream ??= File.OpenRead(_file);

            if (!_stream.CanRead)
            {
                Dispose();
                _stream = File.OpenRead(_file);
            }

            Position = _stream.Length;
        }

        public void OpenWrite()
        {
            _stream ??= File.OpenWrite(_file);

            if (!_stream.CanWrite)
            {
                Dispose();
                _stream = File.OpenWrite(_file);
            }

            Position = _stream.Length;
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
            _stream.Position = 0;
            return _stream.Read(buffer, 0, (int)_stream.Length);
        }
        
        public int Read(byte[] buffer, int position)
        {
            _stream.Position = position;
            return _stream.Read(buffer, position, (int)(Length-position));
        }

        public void Flush()
        {
            _stream.Flush(true);
        }

        public void Dispose()
        {
            Flush();
            _stream.Dispose();
        }
    }
}
