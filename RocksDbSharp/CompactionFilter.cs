using System;

namespace RocksDbSharp
{
    public class CompactionFilter
    {
        public IntPtr Handle;
        private readonly NameDelegate getNameDelegate;
        private readonly FilterDelegate filterDelegate;
        private readonly DestructorDelegate destroyDelegate;

        public CompactionFilter(NameDelegate nameDelegate, 
                                FilterDelegate filterDelegate, 
                                DestructorDelegate destroyDelegate, 
                                IntPtr state)
        {
            this.getNameDelegate = nameDelegate;
            this.filterDelegate = filterDelegate;
            this.destroyDelegate = destroyDelegate;
            Handle = Native.Instance.rocksdb_compactionfilter_create(state, destroyDelegate, filterDelegate, getNameDelegate);
        }        
    }
}
