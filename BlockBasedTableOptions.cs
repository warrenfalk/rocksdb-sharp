using System;

namespace RocksDbSharp
{
#if ROCKSDB_BLOCK_BASED_TABLE_OPTIONS
    public class BlockBasedTableOptions : IDisposable, IRocksDbHandle
    {
        private IntPtr handle;

        public IntPtr Handle { get { return handle; } }

        public BlockBasedTableOptions()
        {
            this.handle = Native.Instance.rocksdb_block_based_options_create();
        }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                Native.Instance.rocksdb_block_based_options_destroy(handle);
                handle = IntPtr.Zero;
            }
        }

        public BlockBasedTableOptions SetBlockSize(ulong blockSize)
        {
            Native.Instance.rocksdb_block_based_options_set_block_size(handle, blockSize);
            return this;
        }

        public BlockBasedTableOptions SetBlockSizeDeviation(int blockSizeDeviation)
        {
            Native.Instance.rocksdb_block_based_options_set_block_size_deviation(handle, blockSizeDeviation);
            return this;
        }

        public BlockBasedTableOptions SetBlockRestartInterval(int blockRestartInterval)
        {
            Native.Instance.rocksdb_block_based_options_set_block_restart_interval(handle, blockRestartInterval);
            return this;
        }

        public BlockBasedTableOptions SetFilterPolicy(IntPtr filterPolicy)
        {
            Native.Instance.rocksdb_block_based_options_set_filter_policy(handle, filterPolicy);
            return this;
        }

        public BlockBasedTableOptions SetFilterPolicy(BloomFilterPolicy filterPolicy)
        {
            Native.Instance.rocksdb_block_based_options_set_filter_policy(handle, filterPolicy.Handle);
            return this;
        }

        public BlockBasedTableOptions SetNoBlockCache(bool noBlockCache)
        {
            Native.Instance.rocksdb_block_based_options_set_no_block_cache(handle, noBlockCache);
            return this;
        }

        public BlockBasedTableOptions SetBlockCache(IntPtr blockCache)
        {
            Native.Instance.rocksdb_block_based_options_set_block_cache(handle, blockCache);
            return this;
        }

        public BlockBasedTableOptions SetBlockCacheCompressed(IntPtr blockCacheCompressed)
        {
            Native.Instance.rocksdb_block_based_options_set_block_cache_compressed(handle, blockCacheCompressed);
            return this;
        }

        public BlockBasedTableOptions SetWholeKeyFiltering(bool wholeKeyFiltering)
        {
            Native.Instance.rocksdb_block_based_options_set_whole_key_filtering(handle, wholeKeyFiltering);
            return this;
        }

        public BlockBasedTableOptions SetFormatVersion(int formatVersion)
        {
            Native.Instance.rocksdb_block_based_options_set_format_version(handle, formatVersion);
            return this;
        }

        public BlockBasedTableOptions SetIndexType(BlockBasedTableIndexType indexType)
        {
            Native.Instance.rocksdb_block_based_options_set_index_type(handle, indexType);
            return this;
        }

        public BlockBasedTableOptions SetHashIndexAllowCollision(bool allowCollision)
        {
            Native.Instance.rocksdb_block_based_options_set_hash_index_allow_collision(handle, allowCollision);
            return this;
        }

        public BlockBasedTableOptions SetCacheIndexAndFilterBlocks(bool cacheIndexAndFilterBlocks)
        {
            Native.Instance.rocksdb_block_based_options_set_cache_index_and_filter_blocks(handle, cacheIndexAndFilterBlocks);
            return this;
        }

        public BlockBasedTableOptions SetSkipTableBuilderFlush(bool skipTableBuilderFlush)
        {
            Native.Instance.rocksdb_block_based_options_set_skip_table_builder_flush(handle, skipTableBuilderFlush);
            return this;
        }      
    }
#endif
}
