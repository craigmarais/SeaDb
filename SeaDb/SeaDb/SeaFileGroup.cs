using SeaDb.Utilities;

namespace SeaDb
{
    public class SeaFileGroup : IDisposable
    {
        private long _fileSize;
        private readonly SeaMappedFile _dataFile;
        private readonly SeaMappedFile _indexFile;
        private readonly Thread _flushThread;
        private bool _keelhauled;
        public SeaFileGroup(string directory, ulong groupKey)
        {
            var fileKey = $"{directory}/{groupKey}";
            _dataFile = new SeaMappedFile(fileKey, "cdat");
            _indexFile = new SeaMappedFile(fileKey, "cind");

            _flushThread = new Thread(RunFlush);
            _flushThread.Start();
        }

        public unsafe void Write(ulong key, Span<byte> data)
        {
            var index = new Structures.Index(key, (uint)_dataFile.Position);
            var span = new Span<byte>(&index, sizeof(Structures.Index));
            _indexFile.Write(span);
            _dataFile.Write(data);
            _fileSize += data.Length;
        }

        public Memory<byte> ReadFrom(ulong key)
        {
            var dataIndex = DataIndexOfKey(key);
            var bufferSize = _dataFile.Length - dataIndex;
            var buffer = new byte[bufferSize];
            var length = _dataFile.Read(buffer, (int)dataIndex);

            return buffer.AsMemory()[..length];
        }

        public bool CanFit(int length)
        {
            return _fileSize + length < Constants.MAX_FILE_SIZE;
        }
        
        public void Flush()
        {
            if (_indexFile.IsOpen())
                _indexFile.Flush();
            if (_dataFile.IsOpen())
                _dataFile.Flush();
        }

        private unsafe long DataIndexOfKey(ulong key)
        {
            byte[] buffer = new byte[_indexFile.Length];
            var length = _indexFile.Read(buffer);

            var indexSpan = buffer.AsSpan();
            while (length > 0)
            {
                var index = new Structures.Index(indexSpan[^length..sizeof(Structures.Index)]);

                if (key == index.Key)
                    return index.Position;

                length -= sizeof(Structures.Index);
            }

            return -1;
        }

        private void RunFlush()
        {
            while (!_keelhauled)
            {
                Flush();
            }
        }

        public void Dispose()
        {
            _keelhauled = true;
            if (_flushThread.IsAlive)
                _flushThread.Join();
            
            _dataFile.Dispose();
            _indexFile.Dispose();
        }
    }
}
