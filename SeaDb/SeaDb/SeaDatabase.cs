using SeaDb.Structures;
using SeaDb.Utilities;

namespace SeaDb
{
    public class SeaDatabase : IDisposable
    {
        // the index set is used to identify the file group index ranges, this way we can narrow the index to a smaller subset of files.
        private readonly SeaFile _indexSet;
        private SeaFileGroup _workingGroup;
        private readonly GPS _gps;
        private string _dbDirectory;

        public SeaDatabase(string databaseName)
        {
            _dbDirectory = Path.Join(AppContext.BaseDirectory, databaseName);
            if (!Directory.Exists(_dbDirectory))
                Directory.CreateDirectory(_dbDirectory);

            _gps = new GPS();
            //_indexSet = new SeaFile("index_set", "cist");
            //InitializeIndexCache();
            _workingGroup = new SeaFileGroup(_dbDirectory, Sequences.GetNextGroupKey());
        }


        public void Write(ulong key, Span<byte> data)
        {
            if (!_workingGroup.CanFit(data.Length))
            {
                _workingGroup.Dispose();
                _workingGroup = new SeaFileGroup(_dbDirectory, Sequences.GetNextGroupKey());
            }

            _workingGroup.Write(key, data);
        }

        public Memory<byte> ReadFrom(ulong key)
        {
            if (key == 0)
                key = 1;
            return _workingGroup.ReadFrom(key);
        }

        private unsafe void InitializeIndexCache()
        {
            byte[] indexSetBytes = new byte[1000];
            var length = _indexSet.Read(indexSetBytes);

            while (length > 0)
            {
                var index = new IndexSetElement(indexSetBytes[^length..]);
                _gps.PlotCordinate(index.FirstKey, index.GroupKey);
                length -= sizeof(IndexSetElement);
                
            }
        }

        public void Flush()
        {
            _workingGroup.Flush();
        }

        public void Dispose()
        {
            //_indexSet.Dispose();
            _workingGroup.Dispose();
        }
    }
}
