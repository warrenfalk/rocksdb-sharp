using System;

namespace RocksDbSharp
{
    /*
    Configure options for a RocksDb store.

    Note on SetXXX() syntax:
       Why not syntax like new Options { XXX = ... } instead?  Two reasons
       1. The rocksdb C API does not support reading the options and so a class with properties is not an appropriate representation
       2. The API functions are named as imperatives and don't always begin with "set" so one like "OptimizeLevelStyleCompaction" wouldn't work right
    */
    public class Options : IDisposable, IRocksDbHandle
    {
        private IntPtr handle;

        public IntPtr Handle { get { return handle; } }

#if ROCKSDB_BLOCK_BASED_TABLE_OPTIONS
        public Options SetBlockBasedTableFactory(BlockBasedTableOptions table_options)
        {
            // Args: table_options
            Native.Instance.rocksdb_options_set_block_based_table_factory(handle, table_options.Handle);
            return this;
        }
#endif

#if ROCKSDB_CUCKOO_TABLE_OPTIONS
        public Options set_cuckoo_table_factory(rocksdb_cuckoo_table_options_t* table_options)
        {
            // Args: table_options
            Native.Instance.rocksdb_options_set_cuckoo_table_factory(handle, table_options);
            return this;
        }
#endif
        public Options()
        {
            this.handle = Native.Instance.rocksdb_options_create();
        }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                Native.Instance.rocksdb_options_destroy(handle);
                handle = IntPtr.Zero;
            }
        }

        public Options IncreaseParallelism(int totalThreads)
        {
            Native.Instance.rocksdb_options_increase_parallelism(handle, totalThreads);
            return this;
        }

        public Options OptimizeForPointLookup(ulong blockCacheSizeMb)
        {
            Native.Instance.rocksdb_options_optimize_for_point_lookup(handle, blockCacheSizeMb);
            return this;
        }

        public Options OptimizeLevelStyleCompaction(ulong memtableMemoryBudget)
        {
            Native.Instance.rocksdb_options_optimize_level_style_compaction(handle, memtableMemoryBudget);
            return this;
        }

        public Options OptimizeUniversalStyleCompaction(ulong memtableMemoryBudget)
        {
            Native.Instance.rocksdb_options_optimize_universal_style_compaction(handle, memtableMemoryBudget);
            return this;
        }

        public Options SetCompactionFilter(IntPtr compactionFilter)
        {
            Native.Instance.rocksdb_options_set_compaction_filter(handle, compactionFilter);
            return this;
        }

        public Options SetCompactionFilterFactory(IntPtr compactionFilterFactory)
        {
            Native.Instance.rocksdb_options_set_compaction_filter_factory(handle, compactionFilterFactory);
            return this;
        }

        public Options SetComparator(IntPtr comparator)
        {
            Native.Instance.rocksdb_options_set_comparator(handle, comparator);
            return this;
        }

        public Options SetMergeOperator(IntPtr mergeOperator)
        {
            Native.Instance.rocksdb_options_set_merge_operator(handle, mergeOperator);
            return this;
        }

        public Options SetUint64addMergeOperator()
        {
            Native.Instance.rocksdb_options_set_uint64add_merge_operator(handle);
            return this;
        }

        public Options SetCompressionPerLevel(int[] levelValues, ulong numLevels)
        {
            Native.Instance.rocksdb_options_set_compression_per_level(handle, levelValues, numLevels);
            return this;
        }

        public Options SetCreateIfMissing(bool value)
        {
            Native.Instance.rocksdb_options_set_create_if_missing(handle, value);
            return this;
        }

        public Options SetCreateMissingColumnFamilies(bool value)
        {
            Native.Instance.rocksdb_options_set_create_missing_column_families(handle, value);
            return this;
        }

        public Options SetErrorIfExists(bool value)
        {
            Native.Instance.rocksdb_options_set_error_if_exists(handle, value);
            return this;
        }

        public Options SetParanoidChecks(bool value)
        {
            Native.Instance.rocksdb_options_set_paranoid_checks(handle, value);
            return this;
        }

        public Options SetEnv(IntPtr env)
        {
            Native.Instance.rocksdb_options_set_env(handle, env);
            return this;
        }

        public Options SetInfoLog(IntPtr logger)
        {
            Native.Instance.rocksdb_options_set_info_log(handle, logger);
            return this;
        }

        public Options SetInfoLogLevel(int value)
        {
            Native.Instance.rocksdb_options_set_info_log_level(handle, value);
            return this;
        }

        public Options SetWriteBufferSize(ulong value)
        {
            Native.Instance.rocksdb_options_set_write_buffer_size(handle, value);
            return this;
        }

        public Options SetMaxOpenFiles(int value)
        {
            Native.Instance.rocksdb_options_set_max_open_files(handle, value);
            return this;
        }

        public Options SetMaxTotalWalSize(ulong n)
        {
            Native.Instance.rocksdb_options_set_max_total_wal_size(handle, n);
            return this;
        }

        public Options SetCompressionOptions(int p1, int p2, int p3)
        {
            Native.Instance.rocksdb_options_set_compression_options(handle, p1, p2, p3);
            return this;
        }

        public Options SetPrefixExtractor(IntPtr sliceTransform)
        {
            Native.Instance.rocksdb_options_set_prefix_extractor(handle, sliceTransform);
            return this;
        }

        public Options SetPrefixExtractor(SliceTransform sliceTransform)
        {
            Native.Instance.rocksdb_options_set_prefix_extractor(handle, sliceTransform.Handle);
            return this;
        }

        public Options SetNumLevels(int value)
        {
            Native.Instance.rocksdb_options_set_num_levels(handle, value);
            return this;
        }

        public Options SetLevel0FileNumCompactionTrigger(int value)
        {
            Native.Instance.rocksdb_options_set_level0_file_num_compaction_trigger(handle, value);
            return this;
        }

        public Options SetLevel0SlowdownWritesTrigger(int value)
        {
            Native.Instance.rocksdb_options_set_level0_slowdown_writes_trigger(handle, value);
            return this;
        }

        public Options SetLevel0StopWritesTrigger(int value)
        {
            Native.Instance.rocksdb_options_set_level0_stop_writes_trigger(handle, value);
            return this;
        }

        public Options SetMaxMemCompactionLevel(int value)
        {
            Native.Instance.rocksdb_options_set_max_mem_compaction_level(handle, value);
            return this;
        }

        public Options SetTargetFileSizeBase(ulong value)
        {
            Native.Instance.rocksdb_options_set_target_file_size_base(handle, value);
            return this;
        }

        public Options SetTargetFileSizeMultiplier(int value)
        {
            Native.Instance.rocksdb_options_set_target_file_size_multiplier(handle, value);
            return this;
        }

        public Options SetMaxBytesForLevelBase(ulong value)
        {
            Native.Instance.rocksdb_options_set_max_bytes_for_level_base(handle, value);
            return this;
        }

        public Options SetMaxBytesForLevelMultiplier(int value)
        {
            Native.Instance.rocksdb_options_set_max_bytes_for_level_multiplier(handle, value);
            return this;
        }

        public Options SetExpandedCompactionFactor(int value)
        {
            Native.Instance.rocksdb_options_set_expanded_compaction_factor(handle, value);
            return this;
        }

        public Options SetMaxGrandparentOverlapFactor(int value)
        {
            Native.Instance.rocksdb_options_set_max_grandparent_overlap_factor(handle, value);
            return this;
        }

        public Options SetMaxBytesForLevelMultiplierAdditional(int[] levelValues, ulong numLevels)
        {
            Native.Instance.rocksdb_options_set_max_bytes_for_level_multiplier_additional(handle, levelValues, numLevels);
            return this;
        }

        public Options EnableStatistics()
        {
            Native.Instance.rocksdb_options_enable_statistics(handle);
            return this;
        }

        public Options StatisticsGetString()
        {
            Native.Instance.rocksdb_options_statistics_get_string(handle);
            return this;
        }

        public Options SetMaxWriteBufferNumber(int value)
        {
            Native.Instance.rocksdb_options_set_max_write_buffer_number(handle, value);
            return this;
        }

        public Options SetMinWriteBufferNumberToMerge(int value)
        {
            Native.Instance.rocksdb_options_set_min_write_buffer_number_to_merge(handle, value);
            return this;
        }

        public Options SetMaxWriteBufferNumberToMaintain(int value)
        {
            Native.Instance.rocksdb_options_set_max_write_buffer_number_to_maintain(handle, value);
            return this;
        }

        public Options SetMaxBackgroundCompactions(int value)
        {
            Native.Instance.rocksdb_options_set_max_background_compactions(handle, value);
            return this;
        }

        public Options SetMaxBackgroundFlushes(int value)
        {
            Native.Instance.rocksdb_options_set_max_background_flushes(handle, value);
            return this;
        }

        public Options SetMaxLogFileSize(ulong value)
        {
            Native.Instance.rocksdb_options_set_max_log_file_size(handle, value);
            return this;
        }

        public Options SetLogFileTimeToRoll(ulong value)
        {
            Native.Instance.rocksdb_options_set_log_file_time_to_roll(handle, value);
            return this;
        }

        public Options SetKeepLogFileNum(ulong value)
        {
            Native.Instance.rocksdb_options_set_keep_log_file_num(handle, value);
            return this;
        }

        public Options SetRecycleLogFileNum(ulong value)
        {
            Native.Instance.rocksdb_options_set_recycle_log_file_num(handle, value);
            return this;
        }

        public Options SetSoftRateLimit(double value)
        {
            Native.Instance.rocksdb_options_set_soft_rate_limit(handle, value);
            return this;
        }

        public Options SetHardRateLimit(double value)
        {
            Native.Instance.rocksdb_options_set_hard_rate_limit(handle, value);
            return this;
        }

        public Options SetRateLimitDelayMaxMilliseconds(uint value)
        {
            Native.Instance.rocksdb_options_set_rate_limit_delay_max_milliseconds(handle, value);
            return this;
        }

        public Options SetMaxManifestFileSize(ulong value)
        {
            Native.Instance.rocksdb_options_set_max_manifest_file_size(handle, value);
            return this;
        }

        public Options SetTableCacheNumshardbits(int value)
        {
            Native.Instance.rocksdb_options_set_table_cache_numshardbits(handle, value);
            return this;
        }

        public Options SetTableCacheRemoveScanCountLimit(int value)
        {
            Native.Instance.rocksdb_options_set_table_cache_remove_scan_count_limit(handle, value);
            return this;
        }

        public Options SetArenaBlockSize(ulong value)
        {
            Native.Instance.rocksdb_options_set_arena_block_size(handle, value);
            return this;
        }

        public Options SetUseFsync(int value)
        {
            Native.Instance.rocksdb_options_set_use_fsync(handle, value);
            return this;
        }

        public Options SetDbLogDir(string value)
        {
            Native.Instance.rocksdb_options_set_db_log_dir(handle, value);
            return this;
        }

        public Options SetWalDir(string value)
        {
            Native.Instance.rocksdb_options_set_wal_dir(handle, value);
            return this;
        }

        public Options SetWALTtlSeconds(ulong value)
        {
            Native.Instance.rocksdb_options_set_WAL_ttl_seconds(handle, value);
            return this;
        }

        public Options SetWALSizeLimitMB(ulong value)
        {
            Native.Instance.rocksdb_options_set_WAL_size_limit_MB(handle, value);
            return this;
        }

        public Options SetManifestPreallocationSize(ulong value)
        {
            Native.Instance.rocksdb_options_set_manifest_preallocation_size(handle, value);
            return this;
        }

        public Options SetPurgeRedundantKvsWhileFlush(bool value)
        {
            Native.Instance.rocksdb_options_set_purge_redundant_kvs_while_flush(handle, value);
            return this;
        }

        public Options SetAllowOsBuffer(bool value)
        {
            Native.Instance.rocksdb_options_set_allow_os_buffer(handle, value);
            return this;
        }

        public Options SetAllowMmapReads(bool value)
        {
            Native.Instance.rocksdb_options_set_allow_mmap_reads(handle, value);
            return this;
        }

        public Options SetAllowMmapWrites(bool value)
        {
            Native.Instance.rocksdb_options_set_allow_mmap_writes(handle, value);
            return this;
        }

        public Options SetIsFdCloseOnExec(bool value)
        {
            Native.Instance.rocksdb_options_set_is_fd_close_on_exec(handle, value);
            return this;
        }

        public Options SetSkipLogErrorOnRecovery(bool value)
        {
            Native.Instance.rocksdb_options_set_skip_log_error_on_recovery(handle, value);
            return this;
        }

        public Options SetStatsDumpPeriodSec(uint value)
        {
            Native.Instance.rocksdb_options_set_stats_dump_period_sec(handle, value);
            return this;
        }

        public Options SetAdviseRandomOnOpen(bool value)
        {
            Native.Instance.rocksdb_options_set_advise_random_on_open(handle, value);
            return this;
        }

        public Options SetAccessHintOnCompactionStart(int value)
        {
            Native.Instance.rocksdb_options_set_access_hint_on_compaction_start(handle, value);
            return this;
        }

        public Options SetUseAdaptiveMutex(bool value)
        {
            Native.Instance.rocksdb_options_set_use_adaptive_mutex(handle, value);
            return this;
        }

        public Options SetBytesPerSync(ulong value)
        {
            Native.Instance.rocksdb_options_set_bytes_per_sync(handle, value);
            return this;
        }

        public Options SetVerifyChecksumsInCompaction(bool value)
        {
            Native.Instance.rocksdb_options_set_verify_checksums_in_compaction(handle, value);
            return this;
        }

        public Options SetFilterDeletes(bool value)
        {
            Native.Instance.rocksdb_options_set_filter_deletes(handle, value);
            return this;
        }

        public Options SetMaxSequentialSkipInIterations(ulong value)
        {
            Native.Instance.rocksdb_options_set_max_sequential_skip_in_iterations(handle, value);
            return this;
        }

        public Options SetDisableDataSync(int value)
        {
            Native.Instance.rocksdb_options_set_disable_data_sync(handle, value);
            return this;
        }

        public Options SetDisableAutoCompactions(int value)
        {
            Native.Instance.rocksdb_options_set_disable_auto_compactions(handle, value);
            return this;
        }

        public Options SetDeleteObsoleteFilesPeriodMicros(ulong value)
        {
            Native.Instance.rocksdb_options_set_delete_obsolete_files_period_micros(handle, value);
            return this;
        }

        public Options SetSourceCompactionFactor(int value)
        {
            Native.Instance.rocksdb_options_set_source_compaction_factor(handle, value);
            return this;
        }

        public Options PrepareForBulkLoad()
        {
            Native.Instance.rocksdb_options_prepare_for_bulk_load(handle);
            return this;
        }

        public Options SetMemtableVectorRep()
        {
            Native.Instance.rocksdb_options_set_memtable_vector_rep(handle);
            return this;
        }

        public Options SetHashSkipListRep(ulong p1, int p2, int p3)
        {
            Native.Instance.rocksdb_options_set_hash_skip_list_rep(handle, p1, p2, p3);
            return this;
        }

        public Options SetHashLinkListRep(ulong value)
        {
            Native.Instance.rocksdb_options_set_hash_link_list_rep(handle, value);
            return this;
        }

        public Options SetPlainTableFactory(ulong p1, int p2, double p3, ulong p4)
        {
            Native.Instance.rocksdb_options_set_plain_table_factory(handle, p1, p2, p3, p4);
            return this;
        }

        public Options SetMinLevelToCompress(int level)
        {
            Native.Instance.rocksdb_options_set_min_level_to_compress(handle, level);
            return this;
        }

        public Options SetMemtablePrefixBloomBits(uint value)
        {
            Native.Instance.rocksdb_options_set_memtable_prefix_bloom_bits(handle, value);
            return this;
        }

        public Options SetMemtablePrefixBloomProbes(int value)
        {
            Native.Instance.rocksdb_options_set_memtable_prefix_bloom_probes(handle, value);
            return this;
        }

        public Options SetMaxSuccessiveMerges(ulong value)
        {
            Native.Instance.rocksdb_options_set_max_successive_merges(handle, value);
            return this;
        }

        public Options SetMinPartialMergeOperands(uint value)
        {
            Native.Instance.rocksdb_options_set_min_partial_merge_operands(handle, value);
            return this;
        }

        public Options SetBloomLocality(uint value)
        {
            Native.Instance.rocksdb_options_set_bloom_locality(handle, value);
            return this;
        }

        public Options SetInplaceUpdateSupport(bool value)
        {
            Native.Instance.rocksdb_options_set_inplace_update_support(handle, value);
            return this;
        }

        public Options SetInplaceUpdateNumLocks(ulong value)
        {
            Native.Instance.rocksdb_options_set_inplace_update_num_locks(handle, value);
            return this;
        }

        public Options SetCompression(CompressionTypeEnum value)
        {
            Native.Instance.rocksdb_options_set_compression(handle, value);
            return this;
        }

        public Options SetCompactionStyle(CompactionStyleEnum value)
        {
            Native.Instance.rocksdb_options_set_compaction_style(handle, value);
            return this;
        }

        public Options SetUniversalCompactionOptions(IntPtr universalCompactionOptions)
        {
            Native.Instance.rocksdb_options_set_universal_compaction_options(handle, universalCompactionOptions);
            return this;
        }

        public Options SetFifoCompactionOptions(IntPtr fifoCompactionOptions)
        {
            Native.Instance.rocksdb_options_set_fifo_compaction_options(handle, fifoCompactionOptions);
            return this;
        }

        public Options SetMemtablePrefixBloomHugePageTlbSize(ulong size)
        {
            Native.Instance.rocksdb_options_set_memtable_prefix_bloom_huge_page_tlb_size(handle, size);
            return this;
        }
    }
}
