﻿using SeaDb.Utilities;

namespace SeaDb
{
    public class SeaFileGroup : IDisposable
    {
        private int _fileSize;
        private readonly SeaFile _dataFile;
        private readonly SeaFile _indexFile;
        private readonly Thread _flushThread;
        private bool _keelhauled;
        public SeaFileGroup(string directory, ulong groupKey)
        {
            var fileKey = $"{directory}/{groupKey}";
            _dataFile = new SeaFile(fileKey, "cdat");
            _indexFile = new SeaFile(fileKey, "cind");
            OpenWrite();
            _fileSize = (int)_dataFile.Length;

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
            OpenRead();
            var dataIndex = DataIndexOfKey(key);
            var bufferSize = _dataFile.Length - dataIndex;
            var buffer = new byte[bufferSize];
            var length = _dataFile.Read(buffer, (int)dataIndex);
            OpenWrite();

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

        private void OpenRead()
        {
            _indexFile.OpenRead();
            _dataFile.OpenRead();
        }
        
        private void OpenWrite()
        {
            _indexFile.OpenWrite();
            _dataFile.OpenWrite();
        }

        private unsafe long DataIndexOfKey(ulong key)
        {
            byte[] buffer = new byte[(int)_indexFile.Length];
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
