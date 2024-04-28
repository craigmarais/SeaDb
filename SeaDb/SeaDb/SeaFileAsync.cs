using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaDb
{
    public class SeaFileAsync
    {
        private readonly string _file;

        public long Position;
        public long Length => _stream.Length;
        
        private FileStream _stream;
        private FileStream _readStream;

        public SeaFileAsync(string fileName, string fileExtension)
        {

            _file = $"{fileName}.{fileExtension}";
            if (!File.Exists(_file))
            {
                _stream = File.Create(_file, 104_857_600, FileOptions.Asynchronous);
                //_readStream = File.OpenRead(_file);
            }
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
            _stream.WriteAsync(data.ToArray());
            Position += data.Length;
        }

        public int Read(byte[] buffer)
        {
            return _stream.ReadAsync(buffer).Result;
        }

        public int Read(byte[] buffer, int position)
        {
            return _stream.ReadAsync(buffer, position, (int)(Position-position)).Result;
        }

        public void Flush()
        {
            _stream.Flush(true);
        }

        public void FlushAsync()
        {
            _stream.FlushAsync();
        }

        public void Dispose()
        {
            Flush();
            _stream.Dispose();
        }
    }
}
