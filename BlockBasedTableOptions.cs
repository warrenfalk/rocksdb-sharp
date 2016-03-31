using System;

namespace RocksDbSharp
{
#if ROCKSDB_BLOCK_BASED_TABLE_OPTIONS
    public class BlockBasedTableOptions
    {
        public IntPtr Handle { get; protected set; }

        public BlockBasedTableOptions()
        {
            this.Handle = Native.Instance.rocksdb_block_based_options_create();
        }

        ~BlockBasedTableOptions()
        {
            if (Handle != IntPtr.Zero)
            {
                Native.Instance.rocksdb_block_based_options_destroy(Handle);
                Handle = IntPtr.Zero;
            }
        }

        public BlockBasedTableOptions SetBlockSize(ulong blockSize)
        {
            Native.Instance.rocksdb_block_based_options_set_block_size(Handle, blockSize);
            return this;
        }

        public BlockBasedTableOptions SetBlockSizeDeviation(int blockSizeDeviation)
        {
            Native.Instance.rocksdb_block_based_options_set_block_size_deviation(Handle, blockSizeDeviation);
            return this;
        }

        public BlockBasedTableOptions SetBlockRestartInterval(int blockRestartInterval)
        {
            Native.Instance.rocksdb_block_based_options_set_block_restart_interval(Handle, blockRestartInterval);
            return this;
        }

        public BlockBasedTableOptions SetFilterPolicy(IntPtr filterPolicy)
        {
            Native.Instance.rocksdb_block_based_options_set_filter_policy(Handle, filterPolicy);
            return this;
        }

        public BlockBasedTableOptions SetFilterPolicy(BloomFilterPolicy filterPolicy)
        {
            Native.Instance.rocksdb_block_based_options_set_filter_policy(Handle, filterPolicy.Handle);
            return this;
        }

        public BlockBasedTableOptions SetNoBlockCache(bool noBlockCache)
        {
            Native.Instance.rocksdb_block_based_options_set_no_block_cache(Handle, noBlockCache);
            return this;
        }

        public BlockBasedTableOptions SetBlockCache(IntPtr blockCache)
        {
            Native.Instance.rocksdb_block_based_options_set_block_cache(Handle, blockCache);
            return this;
        }

        public BlockBasedTableOptions SetBlockCacheCompressed(IntPtr blockCacheCompressed)
        {
            Native.Instance.rocksdb_block_based_options_set_block_cache_compressed(Handle, blockCacheCompressed);
            return this;
        }

        public BlockBasedTableOptions SetWholeKeyFiltering(bool wholeKeyFiltering)
        {
            Native.Instance.rocksdb_block_based_options_set_whole_key_filtering(Handle, wholeKeyFiltering);
            return this;
        }

        public BlockBasedTableOptions SetFormatVersion(int formatVersion)
        {
            Native.Instance.rocksdb_block_based_options_set_format_version(Handle, formatVersion);
            return this;
        }

        public BlockBasedTableOptions SetIndexType(BlockBasedTableIndexType indexType)
        {
            Native.Instance.rocksdb_block_based_options_set_index_type(Handle, indexType);
            return this;
        }

        public BlockBasedTableOptions SetHashIndexAllowCollision(bool allowCollision)
        {
            Native.Instance.rocksdb_block_based_options_set_hash_index_allow_collision(Handle, allowCollision);
            return this;
        }

        public BlockBasedTableOptions SetCacheIndexAndFilterBlocks(bool cacheIndexAndFilterBlocks)
        {
            Native.Instance.rocksdb_block_based_options_set_cache_index_and_filter_blocks(Handle, cacheIndexAndFilterBlocks);
            return this;
        }

        public BlockBasedTableOptions SetSkipTableBuilderFlush(bool skipTableBuilderFlush)
        {
            Native.Instance.rocksdb_block_based_options_set_skip_table_builder_flush(Handle, skipTableBuilderFlush);
            return this;
        }      
    }
#endif
}
