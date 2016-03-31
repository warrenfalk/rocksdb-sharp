using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocksDbSharp
{
    public class ColumnFamilyOptions : OptionsHandle {

#if ROCKSDB_BLOCK_BASED_TABLE_OPTIONS
        public ColumnFamilyOptions SetBlockBasedTableFactory(BlockBasedTableOptions table_options)
        {
            // Args: table_options
            Native.Instance.rocksdb_options_set_block_based_table_factory(Handle, table_options.Handle);
            return this;
        }
#endif

#if ROCKSDB_CUCKOO_TABLE_OPTIONS
        public ColumnFamilyOptions set_cuckoo_table_factory(rocksdb_cuckoo_table_options_t* table_options)
        {
            // Args: table_options
            Native.Instance.rocksdb_options_set_cuckoo_table_factory(Handle, table_options);
            return this;
        }
#endif

        // Use this if you don't need to keep the data sorted, i.e. you'll never use
        // an iterator, only Put() and Get() API calls
        //
        // Not supported in ROCKSDB_LITE
        public ColumnFamilyOptions OptimizeForPointLookup(ulong blockCacheSizeMb)
        {
            Native.Instance.rocksdb_options_optimize_for_point_lookup(Handle, blockCacheSizeMb);
            return this;
        }

        // Default values for some parameters in ColumnFamilyOptions are not
        // optimized for heavy workloads and big datasets, which means you might
        // observe write stalls under some conditions. As a starting point for tuning
        // RocksDB options, use the following two functions:
        // * OptimizeLevelStyleCompaction -- optimizes level style compaction
        // * OptimizeUniversalStyleCompaction -- optimizes universal style compaction
        // Universal style compaction is focused on reducing Write Amplification
        // Factor for big data sets, but increases Space Amplification. You can learn
        // more about the different styles here:
        // https://github.com/facebook/rocksdb/wiki/Rocksdb-Architecture-Guide
        // Make sure to also call IncreaseParallelism(), which will provide the
        // biggest performance gains.
        // Note: we might use more memory than memtable_memory_budget during high
        // write rate period
        //
        // OptimizeUniversalStyleCompaction is not supported in ROCKSDB_LITE
        public ColumnFamilyOptions OptimizeLevelStyleCompaction(ulong memtableMemoryBudget)
        {
            Native.Instance.rocksdb_options_optimize_level_style_compaction(Handle, memtableMemoryBudget);
            return this;
        }

        // Default values for some parameters in ColumnFamilyOptions are not
        // optimized for heavy workloads and big datasets, which means you might
        // observe write stalls under some conditions. As a starting point for tuning
        // RocksDB options, use the following two functions:
        // * OptimizeLevelStyleCompaction -- optimizes level style compaction
        // * OptimizeUniversalStyleCompaction -- optimizes universal style compaction
        // Universal style compaction is focused on reducing Write Amplification
        // Factor for big data sets, but increases Space Amplification. You can learn
        // more about the different styles here:
        // https://github.com/facebook/rocksdb/wiki/Rocksdb-Architecture-Guide
        // Make sure to also call IncreaseParallelism(), which will provide the
        // biggest performance gains.
        // Note: we might use more memory than memtable_memory_budget during high
        // write rate period
        //
        // OptimizeUniversalStyleCompaction is not supported in ROCKSDB_LITE
        public ColumnFamilyOptions OptimizeUniversalStyleCompaction(ulong memtableMemoryBudget)
        {
            Native.Instance.rocksdb_options_optimize_universal_style_compaction(Handle, memtableMemoryBudget);
            return this;
        }

        // A single CompactionFilter instance to call into during compaction.
        // Allows an application to modify/delete a key-value during background
        // compaction.
        //
        // If the client requires a new compaction filter to be used for different
        // compaction runs, it can specify compaction_filter_factory instead of this
        // option.  The client should specify only one of the two.
        // compaction_filter takes precedence over compaction_filter_factory if
        // client specifies both.
        //
        // If multithreaded compaction is being used, the supplied CompactionFilter
        // instance may be used from different threads concurrently and so should be
        // thread-safe.
        //
        // Default: nullptr
        public ColumnFamilyOptions SetCompactionFilter(IntPtr compactionFilter)
        {
            Native.Instance.rocksdb_options_set_compaction_filter(Handle, compactionFilter);
            return this;
        }

        // This is a factory that provides compaction filter objects which allow
        // an application to modify/delete a key-value during background compaction.
        //
        // A new filter will be created on each compaction run.  If multithreaded
        // compaction is being used, each created CompactionFilter will only be used
        // from a single thread and so does not need to be thread-safe.
        //
        // Default: nullptr
        public ColumnFamilyOptions SetCompactionFilterFactory(IntPtr compactionFilterFactory)
        {
            Native.Instance.rocksdb_options_set_compaction_filter_factory(Handle, compactionFilterFactory);
            return this;
        }

        // Comparator used to define the order of keys in the table.
        // Default: a comparator that uses lexicographic byte-wise ordering
        //
        // REQUIRES: The client must ensure that the comparator supplied
        // here has the same name and orders keys *exactly* the same as the
        // comparator provided to previous open calls on the same DB.
        public ColumnFamilyOptions SetComparator(IntPtr comparator)
        {
            Native.Instance.rocksdb_options_set_comparator(Handle, comparator);
            return this;
        }

        // REQUIRES: The client must provide a merge operator if Merge operation
        // needs to be accessed. Calling Merge on a DB without a merge operator
        // would result in Status::NotSupported. The client must ensure that the
        // merge operator supplied here has the same name and *exactly* the same
        // semantics as the merge operator provided to previous open calls on
        // the same DB. The only exception is reserved for upgrade, where a DB
        // previously without a merge operator is introduced to Merge operation
        // for the first time. It's necessary to specify a merge operator when
        // openning the DB in this case.
        // Default: nullptr
        public ColumnFamilyOptions SetMergeOperator(IntPtr mergeOperator)
        {
            Native.Instance.rocksdb_options_set_merge_operator(Handle, mergeOperator);
            return this;
        }

        public ColumnFamilyOptions SetUint64addMergeOperator()
        {
            Native.Instance.rocksdb_options_set_uint64add_merge_operator(Handle);
            return this;
        }

        // Different levels can have different compression policies. There
        // are cases where most lower levels would like to use quick compression
        // algorithms while the higher levels (which have more data) use
        // compression algorithms that have better compression but could
        // be slower. This array, if non-empty, should have an entry for
        // each level of the database; these override the value specified in
        // the previous field 'compression'.
        //
        // NOTICE if level_compaction_dynamic_level_bytes=true,
        // compression_per_level[0] still determines L0, but other elements
        // of the array are based on base level (the level L0 files are merged
        // to), and may not match the level users see from info log for metadata.
        // If L0 files are merged to level-n, then, for i>0, compression_per_level[i]
        // determines compaction type for level n+i-1.
        // For example, if we have three 5 levels, and we determine to merge L0
        // data to L4 (which means L1..L3 will be empty), then the new files go to
        // L4 uses compression type compression_per_level[1].
        // If now L0 is merged to L2. Data goes to L2 will be compressed
        // according to compression_per_level[1], L3 using compression_per_level[2]
        // and L4 using compression_per_level[3]. Compaction for each level can
        // change when data grows.
        public ColumnFamilyOptions SetCompressionPerLevel(int[] levelValues, ulong numLevels)
        {
            Native.Instance.rocksdb_options_set_compression_per_level(Handle, levelValues, numLevels);
            return this;
        }

        public ColumnFamilyOptions SetInfoLogLevel(int value)
        {
            Native.Instance.rocksdb_options_set_info_log_level(Handle, value);
            return this;
        }

        // Amount of data to build up in memory (backed by an unsorted log
        // on disk) before converting to a sorted on-disk file.
        //
        // Larger values increase performance, especially during bulk loads.
        // Up to max_write_buffer_number write buffers may be held in memory
        // at the same time,
        // so you may wish to adjust this parameter to control memory usage.
        // Also, a larger write buffer will result in a longer recovery time
        // the next time the database is opened.
        //
        // Note that write_buffer_size is enforced per column family.
        // See db_write_buffer_size for sharing memory across column families.
        //
        // Default: 4MB
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetWriteBufferSize(ulong value)
        {
            Native.Instance.rocksdb_options_set_write_buffer_size(Handle, value);
            return this;
        }

        // different options for compression algorithms
        public ColumnFamilyOptions SetCompressionOptions(int p1, int p2, int p3)
        {
            Native.Instance.rocksdb_options_set_compression_options(Handle, p1, p2, p3);
            return this;
        }

        // If non-nullptr, use the specified function to determine the
        // prefixes for keys.  These prefixes will be placed in the filter.
        // Depending on the workload, this can reduce the number of read-IOP
        // cost for scans when a prefix is passed via ReadOptions to
        // db.NewIterator().  For prefix filtering to work properly,
        // "prefix_extractor" and "comparator" must be such that the following
        // properties hold:
        //
        // 1) key.starts_with(prefix(key))
        // 2) Compare(prefix(key), key) <= 0.
        // 3) If Compare(k1, k2) <= 0, then Compare(prefix(k1), prefix(k2)) <= 0
        // 4) prefix(prefix(key)) == prefix(key)
        //
        // Default: nullptr
        public ColumnFamilyOptions SetPrefixExtractor(IntPtr sliceTransform)
        {
            Native.Instance.rocksdb_options_set_prefix_extractor(Handle, sliceTransform);
            return this;
        }

        // If non-nullptr, use the specified function to determine the
        // prefixes for keys.  These prefixes will be placed in the filter.
        // Depending on the workload, this can reduce the number of read-IOP
        // cost for scans when a prefix is passed via ReadOptions to
        // db.NewIterator().  For prefix filtering to work properly,
        // "prefix_extractor" and "comparator" must be such that the following
        // properties hold:
        //
        // 1) key.starts_with(prefix(key))
        // 2) Compare(prefix(key), key) <= 0.
        // 3) If Compare(k1, k2) <= 0, then Compare(prefix(k1), prefix(k2)) <= 0
        // 4) prefix(prefix(key)) == prefix(key)
        //
        // Default: nullptr
        public ColumnFamilyOptions SetPrefixExtractor(SliceTransform sliceTransform)
        {
            Native.Instance.rocksdb_options_set_prefix_extractor(Handle, sliceTransform.Handle);
            return this;
        }

        // Number of levels for this database
        public ColumnFamilyOptions SetNumLevels(int value)
        {
            Native.Instance.rocksdb_options_set_num_levels(Handle, value);
            return this;
        }

        // Number of files to trigger level-0 compaction. A value <0 means that
        // level-0 compaction will not be triggered by number of files at all.
        //
        // Default: 4
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetLevel0FileNumCompactionTrigger(int value)
        {
            Native.Instance.rocksdb_options_set_level0_file_num_compaction_trigger(Handle, value);
            return this;
        }

        // Soft limit on number of level-0 files. We start slowing down writes at this
        // point. A value <0 means that no writing slow down will be triggered by
        // number of files in level-0.
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetLevel0SlowdownWritesTrigger(int value)
        {
            Native.Instance.rocksdb_options_set_level0_slowdown_writes_trigger(Handle, value);
            return this;
        }

        // Maximum number of level-0 files.  We stop writes at this point.
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetLevel0StopWritesTrigger(int value)
        {
            Native.Instance.rocksdb_options_set_level0_stop_writes_trigger(Handle, value);
            return this;
        }

        // This does not do anything anymore. Deprecated.
        [Obsolete("Thid does not do anything anymore")]
        public ColumnFamilyOptions SetMaxMemCompactionLevel(int value)
        {
            Native.Instance.rocksdb_options_set_max_mem_compaction_level(Handle, value);
            return this;
        }

        // Target file size for compaction.
        // target_file_size_base is per-file size for level-1.
        // Target file size for level L can be calculated by
        // target_file_size_base * (target_file_size_multiplier ^ (L-1))
        // For example, if target_file_size_base is 2MB and
        // target_file_size_multiplier is 10, then each file on level-1 will
        // be 2MB, and each file on level 2 will be 20MB,
        // and each file on level-3 will be 200MB.
        //
        // Default: 2MB.
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetTargetFileSizeBase(ulong value)
        {
            Native.Instance.rocksdb_options_set_target_file_size_base(Handle, value);
            return this;
        }

        // By default target_file_size_multiplier is 1, which means
        // by default files in different levels will have similar size.
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetTargetFileSizeMultiplier(int value)
        {
            Native.Instance.rocksdb_options_set_target_file_size_multiplier(Handle, value);
            return this;
        }

        // Control maximum total data size for a level.
        // max_bytes_for_level_base is the max total for level-1.
        // Maximum number of bytes for level L can be calculated as
        // (max_bytes_for_level_base) * (max_bytes_for_level_multiplier ^ (L-1))
        // For example, if max_bytes_for_level_base is 20MB, and if
        // max_bytes_for_level_multiplier is 10, total data size for level-1
        // will be 20MB, total file size for level-2 will be 200MB,
        // and total file size for level-3 will be 2GB.
        //
        // Default: 10MB.
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetMaxBytesForLevelBase(ulong value)
        {
            Native.Instance.rocksdb_options_set_max_bytes_for_level_base(Handle, value);
            return this;
        }

        // Default: 10.
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetMaxBytesForLevelMultiplier(int value)
        {
            Native.Instance.rocksdb_options_set_max_bytes_for_level_multiplier(Handle, value);
            return this;
        }

        // Maximum number of bytes in all compacted files.  We avoid expanding
        // the lower level file set of a compaction if it would make the
        // total compaction cover more than
        // (expanded_compaction_factor * targetFileSizeLevel()) many bytes.
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetExpandedCompactionFactor(int value)
        {
            Native.Instance.rocksdb_options_set_expanded_compaction_factor(Handle, value);
            return this;
        }

        // Control maximum bytes of overlaps in grandparent (i.e., level+2) before we
        // stop building a single file in a level->level+1 compaction.
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetMaxGrandparentOverlapFactor(int value)
        {
            Native.Instance.rocksdb_options_set_max_grandparent_overlap_factor(Handle, value);
            return this;
        }

        // Different max-size multipliers for different levels.
        // These are multiplied by max_bytes_for_level_multiplier to arrive
        // at the max-size of each level.
        //
        // Default: 1
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetMaxBytesForLevelMultiplierAdditional(int[] levelValues, ulong numLevels)
        {
            Native.Instance.rocksdb_options_set_max_bytes_for_level_multiplier_additional(Handle, levelValues, numLevels);
            return this;
        }

        // The maximum number of write buffers that are built up in memory.
        // The default and the minimum number is 2, so that when 1 write buffer
        // is being flushed to storage, new writes can continue to the other
        // write buffer.
        // If max_write_buffer_number > 3, writing will be slowed down to
        // options.delayed_write_rate if we are writing to the last write buffer
        // allowed.
        //
        // Default: 2
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetMaxWriteBufferNumber(int value)
        {
            Native.Instance.rocksdb_options_set_max_write_buffer_number(Handle, value);
            return this;
        }

        // The minimum number of write buffers that will be merged together
        // before writing to storage.  If set to 1, then
        // all write buffers are fushed to L0 as individual files and this increases
        // read amplification because a get request has to check in all of these
        // files. Also, an in-memory merge may result in writing lesser
        // data to storage if there are duplicate records in each of these
        // individual write buffers.  Default: 1
        public ColumnFamilyOptions SetMinWriteBufferNumberToMerge(int value)
        {
            Native.Instance.rocksdb_options_set_min_write_buffer_number_to_merge(Handle, value);
            return this;
        }

        // The total maximum number of write buffers to maintain in memory including
        // copies of buffers that have already been flushed.  Unlike
        // max_write_buffer_number, this parameter does not affect flushing.
        // This controls the minimum amount of write history that will be available
        // in memory for conflict checking when Transactions are used.
        //
        // When using an OptimisticTransactionDB:
        // If this value is too low, some transactions may fail at commit time due
        // to not being able to determine whether there were any write conflicts.
        //
        // When using a TransactionDB:
        // If Transaction::SetSnapshot is used, TransactionDB will read either
        // in-memory write buffers or SST files to do write-conflict checking.
        // Increasing this value can reduce the number of reads to SST files
        // done for conflict detection.
        //
        // Setting this value to 0 will cause write buffers to be freed immediately
        // after they are flushed.
        // If this value is set to -1, 'max_write_buffer_number' will be used.
        //
        // Default:
        // If using a TransactionDB/OptimisticTransactionDB, the default value will
        // be set to the value of 'max_write_buffer_number' if it is not explicitly
        // set by the user.  Otherwise, the default is 0.
        public ColumnFamilyOptions SetMaxWriteBufferNumberToMaintain(int value)
        {
            Native.Instance.rocksdb_options_set_max_write_buffer_number_to_maintain(Handle, value);
            return this;
        }

        // DEPRECATED -- this options is no longer used
        // Puts are delayed to options.delayed_write_rate when any level has a
        // compaction score that exceeds soft_rate_limit. This is ignored when == 0.0.
        //
        // Default: 0 (disabled)
        //
        // Dynamically changeable through SetOptions() API
        [Obsolete("this option is no longer used")]
        public ColumnFamilyOptions SetSoftRateLimit(double value)
        {
            Native.Instance.rocksdb_options_set_soft_rate_limit(Handle, value);
            return this;
        }

        // DEPRECATED -- this options is no longer used
        [Obsolete("this option is no longer used")]
        public ColumnFamilyOptions SetHardRateLimit(double value)
        {
            Native.Instance.rocksdb_options_set_hard_rate_limit(Handle, value);
            return this;
        }

        // DEPRECATED -- this options is no longer used
        [Obsolete("this option is no longer used")]
        public ColumnFamilyOptions SetRateLimitDelayMaxMilliseconds(uint value)
        {
            Native.Instance.rocksdb_options_set_rate_limit_delay_max_milliseconds(Handle, value);
            return this;
        }

        // size of one block in arena memory allocation.
        // If <= 0, a proper value is automatically calculated (usually 1/8 of
        // writer_buffer_size, rounded up to a multiple of 4KB).
        //
        // There are two additional restriction of the The specified size:
        // (1) size should be in the range of [4096, 2 << 30] and
        // (2) be the multiple of the CPU word (which helps with the memory
        // alignment).
        //
        // We'll automatically check and adjust the size number to make sure it
        // conforms to the restrictions.
        //
        // Default: 0
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetArenaBlockSize(ulong value)
        {
            Native.Instance.rocksdb_options_set_arena_block_size(Handle, value);
            return this;
        }

        // DEPREACTED
        // Does not have any effect.
        [Obsolete("Does not have any effect")]
        public ColumnFamilyOptions SetPurgeRedundantKvsWhileFlush(bool value)
        {
            Native.Instance.rocksdb_options_set_purge_redundant_kvs_while_flush(Handle, value);
            return this;
        }

        // DEPRECATED -- this options is no longer used
        [Obsolete("this option is no longer used")]
        public ColumnFamilyOptions SetSkipLogErrorOnRecovery(bool value)
        {
            Native.Instance.rocksdb_options_set_skip_log_error_on_recovery(Handle, value);
            return this;
        }

        // If true, compaction will verify checksum on every read that happens
        // as part of compaction
        //
        // Default: true
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetVerifyChecksumsInCompaction(bool value)
        {
            Native.Instance.rocksdb_options_set_verify_checksums_in_compaction(Handle, value);
            return this;
        }

        // Use KeyMayExist API to filter deletes when this is true.
        // If KeyMayExist returns false, i.e. the key definitely does not exist, then
        // the delete is a noop. KeyMayExist only incurs in-memory look up.
        // This optimization avoids writing the delete to storage when appropriate.
        //
        // Default: false
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetFilterDeletes(bool value)
        {
            Native.Instance.rocksdb_options_set_filter_deletes(Handle, value);
            return this;
        }

        // An iteration->Next() sequentially skips over keys with the same
        // user-key unless this option is set. This number specifies the number
        // of keys (with the same userkey) that will be sequentially
        // skipped before a reseek is issued.
        //
        // Default: 8
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetMaxSequentialSkipInIterations(ulong value)
        {
            Native.Instance.rocksdb_options_set_max_sequential_skip_in_iterations(Handle, value);
            return this;
        }

        // Disable automatic compactions. Manual compactions can still
        // be issued on this column family
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetDisableAutoCompactions(int value)
        {
            Native.Instance.rocksdb_options_set_disable_auto_compactions(Handle, value);
            return this;
        }

        // Maximum number of bytes in all source files to be compacted in a
        // single compaction run. We avoid picking too many files in the
        // source level so that we do not exceed the total source bytes
        // for compaction to exceed
        // (source_compaction_factor * targetFileSizeLevel()) many bytes.
        // Default:1, i.e. pick maxfilesize amount of data as the source of
        // a compaction.
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetSourceCompactionFactor(int value)
        {
            Native.Instance.rocksdb_options_set_source_compaction_factor(Handle, value);
            return this;
        }

        public ColumnFamilyOptions SetMemtableVectorRep()
        {
            Native.Instance.rocksdb_options_set_memtable_vector_rep(Handle);
            return this;
        }

        public ColumnFamilyOptions SetHashSkipListRep(ulong p1, int p2, int p3)
        {
            Native.Instance.rocksdb_options_set_hash_skip_list_rep(Handle, p1, p2, p3);
            return this;
        }

        public ColumnFamilyOptions SetHashLinkListRep(ulong value)
        {
            Native.Instance.rocksdb_options_set_hash_link_list_rep(Handle, value);
            return this;
        }

        public ColumnFamilyOptions SetPlainTableFactory(ulong p1, int p2, double p3, ulong p4)
        {
            Native.Instance.rocksdb_options_set_plain_table_factory(Handle, p1, p2, p3, p4);
            return this;
        }

        // Different levels can have different compression policies. There
        // are cases where most lower levels would like to use quick compression
        // algorithms while the higher levels (which have more data) use
        // compression algorithms that have better compression but could
        // be slower. This array, if non-empty, should have an entry for
        // each level of the database; these override the value specified in
        // the previous field 'compression'.
        //
        // NOTICE if level_compaction_dynamic_level_bytes=true,
        // compression_per_level[0] still determines L0, but other elements
        // of the array are based on base level (the level L0 files are merged
        // to), and may not match the level users see from info log for metadata.
        // If L0 files are merged to level-n, then, for i>0, compression_per_level[i]
        // determines compaction type for level n+i-1.
        // For example, if we have three 5 levels, and we determine to merge L0
        // data to L4 (which means L1..L3 will be empty), then the new files go to
        // L4 uses compression type compression_per_level[1].
        // If now L0 is merged to L2. Data goes to L2 will be compressed
        // according to compression_per_level[1], L3 using compression_per_level[2]
        // and L4 using compression_per_level[3]. Compaction for each level can
        // change when data grows.
        public ColumnFamilyOptions SetMinLevelToCompress(int level)
        {
            Native.Instance.rocksdb_options_set_min_level_to_compress(Handle, level);
            return this;
        }

        // if prefix_extractor is set and bloom_bits is not 0, create prefix bloom
        // for memtable
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetMemtablePrefixBloomBits(uint value)
        {
            Native.Instance.rocksdb_options_set_memtable_prefix_bloom_bits(Handle, value);
            return this;
        }

        // number of hash probes per key
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetMemtablePrefixBloomProbes(int value)
        {
            Native.Instance.rocksdb_options_set_memtable_prefix_bloom_probes(Handle, value);
            return this;
        }

        // Maximum number of successive merge operations on a key in the memtable.
        //
        // When a merge operation is added to the memtable and the maximum number of
        // successive merges is reached, the value of the key will be calculated and
        // inserted into the memtable instead of the merge operation. This will
        // ensure that there are never more than max_successive_merges merge
        // operations in the memtable.
        //
        // Default: 0 (disabled)
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetMaxSuccessiveMerges(ulong value)
        {
            Native.Instance.rocksdb_options_set_max_successive_merges(Handle, value);
            return this;
        }

        // The number of partial merge operands to accumulate before partial
        // merge will be performed. Partial merge will not be called
        // if the list of values to merge is less than min_partial_merge_operands.
        //
        // If min_partial_merge_operands < 2, then it will be treated as 2.
        //
        // Default: 2
        public ColumnFamilyOptions SetMinPartialMergeOperands(uint value)
        {
            Native.Instance.rocksdb_options_set_min_partial_merge_operands(Handle, value);
            return this;
        }

        // Control locality of bloom filter probes to improve cache miss rate.
        // This option only applies to memtable prefix bloom and plaintable
        // prefix bloom. It essentially limits every bloom checking to one cache line.
        // This optimization is turned off when set to 0, and positive number to turn
        // it on.
        // Default: 0
        public ColumnFamilyOptions SetBloomLocality(uint value)
        {
            Native.Instance.rocksdb_options_set_bloom_locality(Handle, value);
            return this;
        }

        // Allows thread-safe inplace updates. If this is true, there is no way to
        // achieve point-in-time consistency using snapshot or iterator (assuming
        // concurrent updates). Hence iterator and multi-get will return results
        // which are not consistent as of any point-in-time.
        // If inplace_callback function is not set,
        //   Put(key, new_value) will update inplace the existing_value iff
        //   * key exists in current memtable
        //   * new sizeof(new_value) <= sizeof(existing_value)
        //   * existing_value for that key is a put i.e. kTypeValue
        // If inplace_callback function is set, check doc for inplace_callback.
        // Default: false.
        public ColumnFamilyOptions SetInplaceUpdateSupport(bool value)
        {
            Native.Instance.rocksdb_options_set_inplace_update_support(Handle, value);
            return this;
        }

        // Number of locks used for inplace update
        // Default: 10000, if inplace_update_support = true, else 0.
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetInplaceUpdateNumLocks(ulong value)
        {
            Native.Instance.rocksdb_options_set_inplace_update_num_locks(Handle, value);
            return this;
        }

        // Compress blocks using the specified compression algorithm.  This
        // parameter can be changed dynamically.
        //
        // Default: kSnappyCompression, if it's supported. If snappy is not linked
        // with the library, the default is kNoCompression.
        //
        // Typical speeds of kSnappyCompression on an Intel(R) Core(TM)2 2.4GHz:
        //    ~200-500MB/s compression
        //    ~400-800MB/s decompression
        // Note that these speeds are significantly faster than most
        // persistent storage speeds, and therefore it is typically never
        // worth switching to kNoCompression.  Even if the input data is
        // incompressible, the kSnappyCompression implementation will
        // efficiently detect that and will switch to uncompressed mode.
        public ColumnFamilyOptions SetCompression(CompressionTypeEnum value)
        {
            Native.Instance.rocksdb_options_set_compression(Handle, value);
            return this;
        }

        // The compaction style. Default: kCompactionStyleLevel
        public ColumnFamilyOptions SetCompactionStyle(CompactionStyleEnum value)
        {
            Native.Instance.rocksdb_options_set_compaction_style(Handle, value);
            return this;
        }

        // The options needed to support Universal Style compactions
        public ColumnFamilyOptions SetUniversalCompactionOptions(IntPtr universalCompactionOptions)
        {
            Native.Instance.rocksdb_options_set_universal_compaction_options(Handle, universalCompactionOptions);
            return this;
        }

        // The options for FIFO compaction style
        public ColumnFamilyOptions SetFifoCompactionOptions(IntPtr fifoCompactionOptions)
        {
            Native.Instance.rocksdb_options_set_fifo_compaction_options(Handle, fifoCompactionOptions);
            return this;
        }

        // Page size for huge page TLB for bloom in memtable. If <=0, not allocate
        // from huge page TLB but from malloc.
        // Need to reserve huge pages for it to be allocated. For example:
        //      sysctl -w vm.nr_hugepages=20
        // See linux doc Documentation/vm/hugetlbpage.txt
        //
        // Dynamically changeable through SetOptions() API
        public ColumnFamilyOptions SetMemtablePrefixBloomHugePageTlbSize(ulong size)
        {
            Native.Instance.rocksdb_options_set_memtable_prefix_bloom_huge_page_tlb_size(Handle, size);
            return this;
        }

    };
}
