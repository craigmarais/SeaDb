using System.Collections.Concurrent;

namespace SeaDb
{
    public class GPS
    {
        private readonly ConcurrentDictionary
        <
            ulong /*The starting key of each group*/,
            ulong /*The group key*/
        > _groupKeyMap = new();
        
        public ulong GroupKeyOfMessage(ulong messageKey)
        {
            ulong lastKey = 0;
            foreach (var (firstKey, groupKey) in _groupKeyMap)
            {
                if (firstKey > messageKey)
                    return lastKey;
                lastKey = groupKey;
            }

            return 0;
        }

        public void PlotCordinate(ulong startKey, ulong groupKey)
        {
            _groupKeyMap[startKey] = groupKey;
        }
    }
}
