using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Transitional;

namespace RocksDbSharp
{
    public class ColumnFamilyOptions : OptionsHandle
    {
        ComparatorReferences ComparatorRef { get; set; }
        MergeOperatorReferences MergeOperatorRef { get; set; }

        public ColumnFamilyOptions SetBlockBasedTableFactory(BlockBasedTableOptions table_options)
        {
            References.BlockBasedTableFactory = table_options;
            // Args: table_options
            Native.Instance.rocksdb_options_set_block_based_table_factory(Handle, table_options.Handle);
            return this;
        }

#if ROCKSDB_CUCKOO_TABLE_OPTIONS
        public ColumnFamilyOptions set_cuckoo_table_factory(rocksdb_cuckoo_table_options_t* table_options)
        {
            // Args: table_options
            Native.Instance.rocksdb_options_set_cuckoo_table_factory(Handle, table_options);
            return this;
        }
#endif

        /// <summary>
        /// Use this if you don't need to keep the data sorted, i.e. you'll never use
        /// an iterator, only Put() and Get() API calls
        ///
        /// Not supported in ROCKSDB_LITE
        /// </summary>
        public ColumnFamilyOptions OptimizeForPointLookup(ulong blockCacheSizeMb)
        {
            Native.Instance.rocksdb_options_optimize_for_point_lookup(Handle, blockCacheSizeMb);
            return this;
        }

        /// <summary>
        /// Default values for some parameters in ColumnFamilyOptions are not
        /// optimized for heavy workloads and big datasets, which means you might
        /// observe write stalls under some conditions. As a starting point for tuning
        /// RocksDB options, use the following two functions:
        /// * OptimizeLevelStyleCompaction -- optimizes level style compaction
        /// * OptimizeUniversalStyleCompaction -- optimizes universal style compaction
        /// Universal style compaction is focused on reducing Write Amplification
        /// Factor for big data sets, but increases Space Amplification. You can learn
        /// more about the different styles here:
        /// https://github.com/facebook/rocksdb/wiki/Rocksdb-Architecture-Guide
        /// Make sure to also call IncreaseParallelism(), which will provide the
        /// biggest performance gains.
        /// Note: we might use more memory than memtable_memory_budget during high
        /// write rate period
        ///
        /// OptimizeUniversalStyleCompaction is not supported in ROCKSDB_LITE
        /// </summary>
        public ColumnFamilyOptions OptimizeLevelStyleCompaction(ulong memtableMemoryBudget)
        {
            Native.Instance.rocksdb_options_optimize_level_style_compaction(Handle, memtableMemoryBudget);
            return this;
        }

        /// <summary>
        /// Default values for some parameters in ColumnFamilyOptions are not
        /// optimized for heavy workloads and big datasets, which means you might
        /// observe write stalls under some conditions. As a starting point for tuning
        /// RocksDB options, use the following two functions:
        /// * OptimizeLevelStyleCompaction -- optimizes level style compaction
        /// * OptimizeUniversalStyleCompaction -- optimizes universal style compaction
        /// Universal style compaction is focused on reducing Write Amplification
        /// Factor for big data sets, but increases Space Amplification. You can learn
        /// more about the different styles here:
        /// https://github.com/facebook/rocksdb/wiki/Rocksdb-Architecture-Guide
        /// Make sure to also call IncreaseParallelism(), which will provide the
        /// biggest performance gains.
        /// Note: we might use more memory than memtable_memory_budget during high
        /// write rate period
        ///
        /// OptimizeUniversalStyleCompaction is not supported in ROCKSDB_LITE
        /// </summary>
        public ColumnFamilyOptions OptimizeUniversalStyleCompaction(ulong memtableMemoryBudget)
        {
            Native.Instance.rocksdb_options_optimize_universal_style_compaction(Handle, memtableMemoryBudget);
            return this;
        }

        /// <summary>
        /// A single CompactionFilter instance to call into during compaction.
        /// Allows an application to modify/delete a key-value during background
        /// compaction.
        ///
        /// If the client requires a new compaction filter to be used for different
        /// compaction runs, it can specify compaction_filter_factory instead of this
        /// option.  The client should specify only one of the two.
        /// compaction_filter takes precedence over compaction_filter_factory if
        /// client specifies both.
        ///
        /// If multithreaded compaction is being used, the supplied CompactionFilter
        /// instance may be used from different threads concurrently and so should be
        /// thread-safe.
        ///
        /// Default: nullptr
        /// </summary>
        public ColumnFamilyOptions SetCompactionFilter(IntPtr compactionFilter)
        {
            Native.Instance.rocksdb_options_set_compaction_filter(Handle, compactionFilter);
            return this;
        }

        /// <summary>
        /// This is a factory that provides compaction filter objects which allow
        /// an application to modify/delete a key-value during background compaction.
        ///
        /// A new filter will be created on each compaction run.  If multithreaded
        /// compaction is being used, each created CompactionFilter will only be used
        /// from a single thread and so does not need to be thread-safe.
        ///
        /// Default: nullptr
        /// </summary>
        public ColumnFamilyOptions SetCompactionFilterFactory(IntPtr compactionFilterFactory)
        {
            Native.Instance.rocksdb_options_set_compaction_filter_factory(Handle, compactionFilterFactory);
            return this;
        }

        /// <summary>
        /// If non-zero, we perform bigger reads when doing compaction. If you're
        /// running RocksDB on spinning disks, you should set this to at least 2MB.
        /// That way RocksDB's compaction is doing sequential instead of random reads.
        ///
        /// When non-zero, we also force new_table_reader_for_compaction_inputs to
        /// true.
        ///
        /// Default: 0
        //// </summary>
        public ColumnFamilyOptions SetCompactionReadaheadSize(ulong size)
        {
            Native.Instance.rocksdb_options_compaction_readahead_size(Handle, (UIntPtr)size);
            return this;
        }

        /// <summary>
        /// Comparator used to define the order of keys in the table.
        /// Default: a comparator that uses lexicographic byte-wise ordering
        ///
        /// REQUIRES: The client must ensure that the comparator supplied
        /// here has the same name and orders keys *exactly* the same as the
        /// comparator provided to previous open calls on the same DB.
        /// </summary>
        public ColumnFamilyOptions SetComparator(IntPtr comparator)
        {
            Native.Instance.rocksdb_options_set_comparator(Handle, comparator);
            return this;
        }

        /// <summary>
        /// Comparator used to define the order of keys in the table.
        /// Default: a comparator that uses lexicographic byte-wise ordering
        ///
        /// REQUIRES: The client must ensure that the comparator supplied
        /// here has the same name and orders keys *exactly* the same as the
        /// comparator provided to previous open calls on the same DB.
        /// </summary>
        public ColumnFamilyOptions SetComparator(Comparator comparator)
        {
            // Allocate some memory for the name bytes
            var name = comparator.Name ?? comparator.GetType().FullName;
            var nameBytes = Encoding.UTF8.GetBytes(name + "\0");
            var namePtr = Marshal.AllocHGlobal(nameBytes.Length);
            Marshal.Copy(nameBytes, 0, namePtr, nameBytes.Length);

            // Hold onto a reference to everything that needs to stay alive
            ComparatorRef = new ComparatorReferences
            {
                GetComparator = () => comparator,
                CompareDelegate = Comparator_Compare,
                DestructorDelegate = Comparator_Destroy,
                NameDelegate = Comparator_GetNamePtr,
            };

            // Allocate the state
            var state = new ComparatorState
            {
                NamePtr = namePtr,
                GetComparatorPtr = CurrentFramework.GetFunctionPointerForDelegate<GetComparator>(ComparatorRef.GetComparator)
            };
            var statePtr = Marshal.AllocHGlobal(Marshal.SizeOf(state));
            Marshal.StructureToPtr(state, statePtr, false);

            // Create the comparator
            IntPtr handle = Native.Instance.rocksdb_comparator_create(
                state: statePtr,
                destructor: ComparatorRef.DestructorDelegate,
                compare: ComparatorRef.CompareDelegate,
                name: ComparatorRef.NameDelegate
            );

            return SetComparator(handle);
        }

        delegate Comparator GetComparator();
        private class ComparatorReferences
        {
            public GetComparator GetComparator { get; set; }
            public DestructorDelegate DestructorDelegate { get; set; }
            public CompareDelegate CompareDelegate { get; set; }
            public NameDelegate NameDelegate { get; set; }
        }

        private unsafe int Comparator_Compare(IntPtr state, IntPtr a, UIntPtr alen, IntPtr b, UIntPtr blen)
        {
            var getComparatorPtr = (*((ComparatorState*)state)).GetComparatorPtr;
            var getComparator = CurrentFramework.GetDelegateForFunctionPointer<GetComparator>(getComparatorPtr);
            var comparator = getComparator();
            return comparator.Compare(a, alen, b, blen);
        }

        private unsafe static void Comparator_Destroy(IntPtr state)
        {
            var namePtr = (*((ComparatorState*)state)).NamePtr;
            Marshal.FreeHGlobal(namePtr);
            Marshal.FreeHGlobal(state);
        }

        private unsafe static IntPtr Comparator_GetNamePtr(IntPtr state)
            => (*((ComparatorState*)state)).NamePtr;

        [StructLayout(LayoutKind.Sequential)]
        private struct ComparatorState
        {
            public IntPtr GetComparatorPtr { get; set; }
            public IntPtr NamePtr { get; set; }
        }

        /// <summary>
        /// REQUIRES: The client must provide a merge operator if Merge operation
        /// needs to be accessed. Calling Merge on a DB without a merge operator
        /// would result in Status::NotSupported. The client must ensure that the
        /// merge operator supplied here has the same name and *exactly* the same
        /// semantics as the merge operator provided to previous open calls on
        /// the same DB. The only exception is reserved for upgrade, where a DB
        /// previously without a merge operator is introduced to Merge operation
        /// for the first time. It's necessary to specify a merge operator when
        /// openning the DB in this case.
        /// Default: nullptr
        /// </summary>
        public ColumnFamilyOptions SetMergeOperator(MergeOperator mergeOperator)
        {
            // Allocate some memory for the name bytes
            var name = mergeOperator.Name ?? mergeOperator.GetType().FullName;
            var nameBytes = Encoding.UTF8.GetBytes(name + "\0");
            var namePtr = Marshal.AllocHGlobal(nameBytes.Length);
            Marshal.Copy(nameBytes, 0, namePtr, nameBytes.Length);

            // Hold onto a reference to everything that needs to stay alive
            MergeOperatorRef = new MergeOperatorReferences
            {
                GetMergeOperator = () => mergeOperator,
                DestructorDelegate = MergeOperator_Destroy,
                NameDelegate = MergeOperator_GetNamePtr,
                DeleteValueDelegate = MergeOperator_DeleteValue,
                FullMergeDelegate = MergeOperator_FullMerge,
                PartialMergeDelegate = MergeOperator_PartialMerge,
            };

            // Allocate the state
            var state = new MergeOperatorState
            {
                NamePtr = namePtr,
                GetMergeOperatorPtr = CurrentFramework.GetFunctionPointerForDelegate<GetMergeOperator>(MergeOperatorRef.GetMergeOperator)
            };
            var statePtr = Marshal.AllocHGlobal(Marshal.SizeOf(state));
            Marshal.StructureToPtr(state, statePtr, false);

            // Create the merge operator
            IntPtr handle = Native.Instance.rocksdb_mergeoperator_create(
                state: statePtr,
                destructor: MergeOperatorRef.DestructorDelegate,
                delete_value: MergeOperatorRef.DeleteValueDelegate,
                full_merge: MergeOperatorRef.FullMergeDelegate,
                partial_merge: MergeOperatorRef.PartialMergeDelegate,
                name: MergeOperatorRef.NameDelegate
            );

            return SetMergeOperator(handle);
        }

        private static MergeOperator GetMergeOperatorFromPtr(IntPtr getMergeOperatorPtr)
        {
            var getMergeOperator = CurrentFramework.GetDelegateForFunctionPointer<GetMergeOperator>(getMergeOperatorPtr);
            return getMergeOperator();
        }

        private unsafe static IntPtr MergeOperator_PartialMerge(IntPtr state, IntPtr key, UIntPtr keyLength, IntPtr operandsList, IntPtr operandsListLength, int numOperands, IntPtr success, IntPtr newValueLength)
        {
            var mergeOperator = GetMergeOperatorFromPtr((*((MergeOperatorState*)state)).GetMergeOperatorPtr);
            return mergeOperator.PartialMerge(key, keyLength, operandsList, operandsListLength, numOperands, success, newValueLength);
        }

        private unsafe static IntPtr MergeOperator_FullMerge(IntPtr state, IntPtr key, UIntPtr keyLength, IntPtr existingValue, UIntPtr existingValueLength, IntPtr operandsList, IntPtr operandsListLength, int numOperands, IntPtr success, IntPtr newValueLength)
        {
            var mergeOperator = GetMergeOperatorFromPtr((*((MergeOperatorState*)state)).GetMergeOperatorPtr);
            return mergeOperator.FullMerge(key, keyLength, existingValue, existingValueLength, operandsList, operandsListLength, numOperands, success, newValueLength);
        }

        private unsafe static void MergeOperator_DeleteValue(IntPtr state, IntPtr value, UIntPtr valueLength)
        {
            var mergeOperator = GetMergeOperatorFromPtr((*((MergeOperatorState*)state)).GetMergeOperatorPtr);
            mergeOperator.DeleteValue(value, valueLength);
        }

        delegate MergeOperator GetMergeOperator();
        private class MergeOperatorReferences
        {
            public GetMergeOperator GetMergeOperator { get; set; }
            public DestructorDelegate DestructorDelegate { get; set; }
            public NameDelegate NameDelegate { get; set; }
            public DeleteValueDelegate DeleteValueDelegate { get; set; }
            public FullMergeDelegate FullMergeDelegate { get; set; }
            public PartialMergeDelegate PartialMergeDelegate { get; set; }
        }

        private unsafe static void MergeOperator_Destroy(IntPtr state)
        {
            var namePtr = (*((MergeOperatorState*)state)).NamePtr;
            Marshal.FreeHGlobal(namePtr);
            Marshal.FreeHGlobal(state);
        }

        private unsafe static IntPtr MergeOperator_GetNamePtr(IntPtr state)
            => (*((MergeOperatorState*)state)).NamePtr;

        [StructLayout(LayoutKind.Sequential)]
        private struct MergeOperatorState
        {
            public IntPtr GetMergeOperatorPtr { get; set; }
            public IntPtr NamePtr { get; set; }
        }

        /// <summary>
        /// REQUIRES: The client must provide a merge operator if Merge operation
        /// needs to be accessed. Calling Merge on a DB without a merge operator
        /// would result in Status::NotSupported. The client must ensure that the
        /// merge operator supplied here has the same name and *exactly* the same
        /// semantics as the merge operator provided to previous open calls on
        /// the same DB. The only exception is reserved for upgrade, where a DB
        /// previously without a merge operator is introduced to Merge operation
        /// for the first time. It's necessary to specify a merge operator when
        /// openning the DB in this case.
        /// Default: nullptr
        /// </summary>
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

        /// <summary>
        /// Different levels can have different compression policies. There
        /// are cases where most lower levels would like to use quick compression
        /// algorithms while the higher levels (which have more data) use
        /// compression algorithms that have better compression but could
        /// be slower. This array, if non-empty, should have an entry for
        /// each level of the database; these override the value specified in
        /// the previous field 'compression'.
        ///
        /// NOTICE if level_compaction_dynamic_level_bytes=true,
        /// compression_per_level[0] still determines L0, but other elements
        /// of the array are based on base level (the level L0 files are merged
        /// to), and may not match the level users see from info log for metadata.
        /// If L0 files are merged to level-n, then, for i>0, compression_per_level[i]
        /// determines compaction type for level n+i-1.
        /// For example, if we have three 5 levels, and we determine to merge L0
        /// data to L4 (which means L1..L3 will be empty), then the new files go to
        /// L4 uses compression type compression_per_level[1].
        /// If now L0 is merged to L2. Data goes to L2 will be compressed
        /// according to compression_per_level[1], L3 using compression_per_level[2]
        /// and L4 using compression_per_level[3]. Compaction for each level can
        /// change when data grows.
        /// </summary>
        public ColumnFamilyOptions SetCompressionPerLevel(Compression[] levelValues, ulong numLevels)
        {
            var values = levelValues.Select(x => (int)x).ToArray();
            Native.Instance.rocksdb_options_set_compression_per_level(Handle, values, (UIntPtr)numLevels);
            return this;
        }

        /// <summary>
        /// Different levels can have different compression policies. There
        /// are cases where most lower levels would like to use quick compression
        /// algorithms while the higher levels (which have more data) use
        /// compression algorithms that have better compression but could
        /// be slower. This array, if non-empty, should have an entry for
        /// each level of the database; these override the value specified in
        /// the previous field 'compression'.
        ///
        /// NOTICE if level_compaction_dynamic_level_bytes=true,
        /// compression_per_level[0] still determines L0, but other elements
        /// of the array are based on base level (the level L0 files are merged
        /// to), and may not match the level users see from info log for metadata.
        /// If L0 files are merged to level-n, then, for i>0, compression_per_level[i]
        /// determines compaction type for level n+i-1.
        /// For example, if we have three 5 levels, and we determine to merge L0
        /// data to L4 (which means L1..L3 will be empty), then the new files go to
        /// L4 uses compression type compression_per_level[1].
        /// If now L0 is merged to L2. Data goes to L2 will be compressed
        /// according to compression_per_level[1], L3 using compression_per_level[2]
        /// and L4 using compression_per_level[3]. Compaction for each level can
        /// change when data grows.
        /// </summary>
        [Obsolete("Use Compression enum")]
        public ColumnFamilyOptions SetCompressionPerLevel(Compression[] levelValues, UIntPtr numLevels)
        {
            Native.Instance.rocksdb_options_set_compression_per_level(Handle, levelValues, numLevels);
            return this;
        }

        public ColumnFamilyOptions SetInfoLogLevel(int value)
        {
            Native.Instance.rocksdb_options_set_info_log_level(Handle, value);
            return this;
        }

        /// <summary>
        /// Amount of data to build up in memory (backed by an unsorted log
        /// on disk) before converting to a sorted on-disk file.
        ///
        /// Larger values increase performance, especially during bulk loads.
        /// Up to max_write_buffer_number write buffers may be held in memory
        /// at the same time,
        /// so you may wish to adjust this parameter to control memory usage.
        /// Also, a larger write buffer will result in a longer recovery time
        /// the next time the database is opened.
        ///
        /// Note that write_buffer_size is enforced per column family.
        /// See db_write_buffer_size for sharing memory across column families.
        ///
        /// Default: 4MB
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetWriteBufferSize(ulong value)
        {
            Native.Instance.rocksdb_options_set_write_buffer_size(Handle, (UIntPtr)value);
            return this;
        }

        /// <summary>
        /// different options for compression algorithms
        /// </summary>
        public ColumnFamilyOptions SetCompressionOptions(int p1, int p2, int p3, int p4)
        {
            Native.Instance.rocksdb_options_set_compression_options(Handle, p1, p2, p3, p4);
            return this;
        }

        /// <summary>
        /// If non-nullptr, use the specified function to determine the
        /// prefixes for keys.  These prefixes will be placed in the filter.
        /// Depending on the workload, this can reduce the number of read-IOP
        /// cost for scans when a prefix is passed via ReadOptions to
        /// db.NewIterator().  For prefix filtering to work properly,
        /// "prefix_extractor" and "comparator" must be such that the following
        /// properties hold:
        ///
        /// 1) key.starts_with(prefix(key))
        /// 2) Compare(prefix(key), key) <= 0.
        /// 3) If Compare(k1, k2) <= 0, then Compare(prefix(k1), prefix(k2)) <= 0
        /// 4) prefix(prefix(key)) == prefix(key)
        ///
        /// Default: nullptr
        /// </summary>
        public ColumnFamilyOptions SetPrefixExtractor(IntPtr sliceTransform)
        {
            Native.Instance.rocksdb_options_set_prefix_extractor(Handle, sliceTransform);
            return this;
        }

        /// <summary>
        /// If non-nullptr, use the specified function to determine the
        /// prefixes for keys.  These prefixes will be placed in the filter.
        /// Depending on the workload, this can reduce the number of read-IOP
        /// cost for scans when a prefix is passed via ReadOptions to
        /// db.NewIterator().  For prefix filtering to work properly,
        /// "prefix_extractor" and "comparator" must be such that the following
        /// properties hold:
        ///
        /// 1) key.starts_with(prefix(key))
        /// 2) Compare(prefix(key), key) <= 0.
        /// 3) If Compare(k1, k2) <= 0, then Compare(prefix(k1), prefix(k2)) <= 0
        /// 4) prefix(prefix(key)) == prefix(key)
        ///
        /// Default: nullptr
        /// </summary>
        public ColumnFamilyOptions SetPrefixExtractor(SliceTransform sliceTransform)
        {
            References.PrefixExtractor = sliceTransform;
            Native.Instance.rocksdb_options_set_prefix_extractor(Handle, sliceTransform.Handle);
            return this;
        }

        /// <summary>
        /// Number of levels for this database
        /// </summary>
        public ColumnFamilyOptions SetNumLevels(int value)
        {
            Native.Instance.rocksdb_options_set_num_levels(Handle, value);
            return this;
        }

        /// <summary>
        /// Number of files to trigger level-0 compaction. A value <0 means that
        /// level-0 compaction will not be triggered by number of files at all.
        ///
        /// Default: 4
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetLevel0FileNumCompactionTrigger(int value)
        {
            Native.Instance.rocksdb_options_set_level0_file_num_compaction_trigger(Handle, value);
            return this;
        }

        /// <summary>
        /// Soft limit on number of level-0 files. We start slowing down writes at this
        /// point. A value <0 means that no writing slow down will be triggered by
        /// number of files in level-0.
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetLevel0SlowdownWritesTrigger(int value)
        {
            Native.Instance.rocksdb_options_set_level0_slowdown_writes_trigger(Handle, value);
            return this;
        }

        /// <summary>
        /// Maximum number of level-0 files.  We stop writes at this point.
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
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

        /// <summary>
        /// Target file size for compaction.
        /// target_file_size_base is per-file size for level-1.
        /// Target file size for level L can be calculated by
        /// target_file_size_base * (target_file_size_multiplier ^ (L-1))
        /// For example, if target_file_size_base is 2MB and
        /// target_file_size_multiplier is 10, then each file on level-1 will
        /// be 2MB, and each file on level 2 will be 20MB,
        /// and each file on level-3 will be 200MB.
        ///
        /// Default: 2MB.
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetTargetFileSizeBase(ulong value)
        {
            Native.Instance.rocksdb_options_set_target_file_size_base(Handle, value);
            return this;
        }

        /// <summary>
        /// By default target_file_size_multiplier is 1, which means
        /// by default files in different levels will have similar size.
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetTargetFileSizeMultiplier(int value)
        {
            Native.Instance.rocksdb_options_set_target_file_size_multiplier(Handle, value);
            return this;
        }

        /// <summary>
        /// Control maximum total data size for a level.
        /// max_bytes_for_level_base is the max total for level-1.
        /// Maximum number of bytes for level L can be calculated as
        /// (max_bytes_for_level_base) * (max_bytes_for_level_multiplier ^ (L-1))
        /// For example, if max_bytes_for_level_base is 20MB, and if
        /// max_bytes_for_level_multiplier is 10, total data size for level-1
        /// will be 20MB, total file size for level-2 will be 200MB,
        /// and total file size for level-3 will be 2GB.
        ///
        /// Default: 10MB.
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetMaxBytesForLevelBase(ulong value)
        {
            Native.Instance.rocksdb_options_set_max_bytes_for_level_base(Handle, value);
            return this;
        }

        /// <summary>
        /// If true, RocksDB will pick target size of each level dynamically.
        /// We will pick a base level b >= 1. L0 will be directly merged into level b,
        /// instead of always into level 1. Level 1 to b-1 need to be empty.
        /// We try to pick b and its target size so that
        /// 1. target size is in the range of
        ///   (max_bytes_for_level_base / max_bytes_for_level_multiplier,
        ///    max_bytes_for_level_base]
        /// 2. target size of the last level (level num_levels-1) equals to extra size
        ///    of the level.
        /// At the same time max_bytes_for_level_multiplier and
        /// max_bytes_for_level_multiplier_additional are still satisfied.
        ///
        /// With this option on, from an empty DB, we make last level the base level,
        /// which means merging L0 data into the last level, until it exceeds
        /// max_bytes_for_level_base. And then we make the second last level to be
        /// base level, to start to merge L0 data to second last level, with its
        /// target size to be 1/max_bytes_for_level_multiplier of the last level's
        /// extra size. After the data accumulates more so that we need to move the
        /// base level to the third last one, and so on.
        ///
        /// For example, assume max_bytes_for_level_multiplier=10, num_levels=6,
        /// and max_bytes_for_level_base=10MB.
        /// Target sizes of level 1 to 5 starts with:
        /// [- - - - 10MB]
        /// with base level is level. Target sizes of level 1 to 4 are not applicable
        /// because they will not be used.
        /// Until the size of Level 5 grows to more than 10MB, say 11MB, we make
        /// base target to level 4 and now the targets looks like:
        /// [- - - 1.1MB 11MB]
        /// While data are accumulated, size targets are tuned based on actual data
        /// of level 5. When level 5 has 50MB of data, the target is like:
        /// [- - - 5MB 50MB]
        /// Until level 5's actual size is more than 100MB, say 101MB. Now if we keep
        /// level 4 to be the base level, its target size needs to be 10.1MB, which
        /// doesn't satisfy the target size range. So now we make level 3 the target
        /// size and the target sizes of the levels look like:
        /// [- - 1.01MB 10.1MB 101MB]
        /// In the same way, while level 5 further grows, all levels' targets grow,
        /// like
        /// [- - 5MB 50MB 500MB]
        /// Until level 5 exceeds 1000MB and becomes 1001MB, we make level 2 the
        /// base level and make levels' target sizes like this:
        /// [- 1.001MB 10.01MB 100.1MB 1001MB]
        /// and go on...
        ///
        /// By doing it, we give max_bytes_for_level_multiplier a priority against
        /// max_bytes_for_level_base, for a more predictable LSM tree shape. It is
        /// useful to limit worse case space amplification.
        ///
        /// max_bytes_for_level_multiplier_additional is ignored with this flag on.
        ///
        /// Turning this feature on or off for an existing DB can cause unexpected
        /// LSM tree structure so it's not recommended.
        ///
        /// NOTE: this option is experimental
        ///
        /// Default: false
        /// </summary>
        /// <returns></returns>
        public ColumnFamilyOptions SetLevelCompactionDynamicLevelBytes(bool value)
        {
            Native.Instance.rocksdb_options_set_level_compaction_dynamic_level_bytes(Handle, value);
            return this;
        }

        /// <summary>
        /// Default: 10.
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetMaxBytesForLevelMultiplier(double value)
        {
            Native.Instance.rocksdb_options_set_max_bytes_for_level_multiplier(Handle, value);
            return this;
        }

        /// <summary>
        /// Different max-size multipliers for different levels.
        /// These are multiplied by max_bytes_for_level_multiplier to arrive
        /// at the max-size of each level.
        ///
        /// Default: 1
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetMaxBytesForLevelMultiplierAdditional(int[] levelValues, ulong numLevels)
        {
            Native.Instance.rocksdb_options_set_max_bytes_for_level_multiplier_additional(Handle, levelValues, (UIntPtr)numLevels);
            return this;
        }

        /// <summary>
        /// The maximum number of write buffers that are built up in memory.
        /// The default and the minimum number is 2, so that when 1 write buffer
        /// is being flushed to storage, new writes can continue to the other
        /// write buffer.
        /// If max_write_buffer_number > 3, writing will be slowed down to
        /// options.delayed_write_rate if we are writing to the last write buffer
        /// allowed.
        ///
        /// Default: 2
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetMaxWriteBufferNumber(int value)
        {
            Native.Instance.rocksdb_options_set_max_write_buffer_number(Handle, value);
            return this;
        }

        /// <summary>
        /// The minimum number of write buffers that will be merged together
        /// before writing to storage.  If set to 1, then
        /// all write buffers are fushed to L0 as individual files and this increases
        /// read amplification because a get request has to check in all of these
        /// files. Also, an in-memory merge may result in writing lesser
        /// data to storage if there are duplicate records in each of these
        /// individual write buffers.  Default: 1
        /// </summary>
        public ColumnFamilyOptions SetMinWriteBufferNumberToMerge(int value)
        {
            Native.Instance.rocksdb_options_set_min_write_buffer_number_to_merge(Handle, value);
            return this;
        }

        /// <summary>
        /// The total maximum number of write buffers to maintain in memory including
        /// copies of buffers that have already been flushed.  Unlike
        /// max_write_buffer_number, this parameter does not affect flushing.
        /// This controls the minimum amount of write history that will be available
        /// in memory for conflict checking when Transactions are used.
        ///
        /// When using an OptimisticTransactionDB:
        /// If this value is too low, some transactions may fail at commit time due
        /// to not being able to determine whether there were any write conflicts.
        ///
        /// When using a TransactionDB:
        /// If Transaction::SetSnapshot is used, TransactionDB will read either
        /// in-memory write buffers or SST files to do write-conflict checking.
        /// Increasing this value can reduce the number of reads to SST files
        /// done for conflict detection.
        ///
        /// Setting this value to 0 will cause write buffers to be freed immediately
        /// after they are flushed.
        /// If this value is set to -1, 'max_write_buffer_number' will be used.
        ///
        /// Default:
        /// If using a TransactionDB/OptimisticTransactionDB, the default value will
        /// be set to the value of 'max_write_buffer_number' if it is not explicitly
        /// set by the user.  Otherwise, the default is 0.
        /// </summary>
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

        /// <summary>
        /// All writes will be slowed down to at least delayed_write_rate if estimated
        /// bytes needed to be compaction exceed this threshold.
        ///
        /// Default: 64GB
        /// </summary>
        public ColumnFamilyOptions SetSoftPendingCompactionBytesLimit(ulong value)
        {
            Native.Instance.rocksdb_options_set_soft_pending_compaction_bytes_limit(Handle, (UIntPtr)value);
            return this;
        }

        /// <summary>
        /// All writes are stopped if estimated bytes needed to be compaction exceed
        /// this threshold.
        ///
        /// Default: 256GB
        /// </summary>
        public ColumnFamilyOptions SetHardPendingCompactionBytesLimit(ulong value)
        {
            Native.Instance.rocksdb_options_set_hard_pending_compaction_bytes_limit(Handle, (UIntPtr)value);
            return this;
        }

        // DEPRECATED -- this options is no longer used
        [Obsolete("this option is no longer used")]
        public ColumnFamilyOptions SetRateLimitDelayMaxMilliseconds(uint value)
        {
            Native.Instance.rocksdb_options_set_rate_limit_delay_max_milliseconds(Handle, value);
            return this;
        }

        /// <summary>
        /// size of one block in arena memory allocation.
        /// If <= 0, a proper value is automatically calculated (usually 1/8 of
        /// writer_buffer_size, rounded up to a multiple of 4KB).
        ///
        /// There are two additional restriction of the The specified size:
        /// (1) size should be in the range of [4096, 2 << 30] and
        /// (2) be the multiple of the CPU word (which helps with the memory
        /// alignment).
        ///
        /// We'll automatically check and adjust the size number to make sure it
        /// conforms to the restrictions.
        ///
        /// Default: 0
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetArenaBlockSize(ulong value)
        {
            Native.Instance.rocksdb_options_set_arena_block_size(Handle, (UIntPtr)value);
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

        /// <summary>
        /// An iteration->Next() sequentially skips over keys with the same
        /// user-key unless this option is set. This number specifies the number
        /// of keys (with the same userkey) that will be sequentially
        /// skipped before a reseek is issued.
        ///
        /// Default: 8
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetMaxSequentialSkipInIterations(ulong value)
        {
            Native.Instance.rocksdb_options_set_max_sequential_skip_in_iterations(Handle, value);
            return this;
        }

        /// <summary>
        /// Disable automatic compactions. Manual compactions can still
        /// be issued on this column family
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetDisableAutoCompactions(int value)
        {
            Native.Instance.rocksdb_options_set_disable_auto_compactions(Handle, value);
            return this;
        }

        /// <summary>
        /// This flag specifies that the implementation should optimize the filters
        /// mainly for cases where keys are found rather than also optimize for keys
        /// missed. This would be used in cases where the application knows that
        /// there are very few misses or the performance in the case of misses is not
        /// important.
        ///
        /// For now, this flag allows us to not store filters for the last level i.e
        /// the largest level which contains data of the LSM store. For keys which
        /// are hits, the filters in this level are not useful because we will search
        /// for the data anyway. NOTE: the filters in other levels are still useful
        /// even for key hit because they tell us whether to look in that level or go
        /// to the higher level.
        ///
        /// Default: false
        /// </summary>
        public ColumnFamilyOptions SetOptimizeFiltersForHits(int value)
        {
            Native.Instance.rocksdb_options_set_optimize_filters_for_hits(Handle, value);
            return this;
        }

        public ColumnFamilyOptions SetMemtableVectorRep()
        {
            Native.Instance.rocksdb_options_set_memtable_vector_rep(Handle);
            return this;
        }

        public ColumnFamilyOptions SetMemtablePrefixBloomSizeRatio(double ratio)
        {
            Native.Instance.rocksdb_options_set_memtable_prefix_bloom_size_ratio(Handle, ratio);
            return this;
        }

        public ColumnFamilyOptions SetMaxCompactionBytes(ulong bytes)
        {
            Native.Instance.rocksdb_options_set_max_compaction_bytes(Handle, bytes);
            return this;
        }

        public ColumnFamilyOptions SetHashSkipListRep(ulong p1, int p2, int p3)
        {
            Native.Instance.rocksdb_options_set_hash_skip_list_rep(Handle, (UIntPtr)p1, p2, p3);
            return this;
        }

        public ColumnFamilyOptions SetHashLinkListRep(ulong value)
        {
            Native.Instance.rocksdb_options_set_hash_link_list_rep(Handle, (UIntPtr)value);
            return this;
        }

        public ColumnFamilyOptions SetPlainTableFactory(UInt32 p1, int p2, double p3, ulong p4)
        {
            Native.Instance.rocksdb_options_set_plain_table_factory(Handle, p1, p2, p3, (UIntPtr)p4);
            return this;
        }

        /// <summary>
        /// Different levels can have different compression policies. There
        /// are cases where most lower levels would like to use quick compression
        /// algorithms while the higher levels (which have more data) use
        /// compression algorithms that have better compression but could
        /// be slower. This array, if non-empty, should have an entry for
        /// each level of the database; these override the value specified in
        /// the previous field 'compression'.
        ///
        /// NOTICE if level_compaction_dynamic_level_bytes=true,
        /// compression_per_level[0] still determines L0, but other elements
        /// of the array are based on base level (the level L0 files are merged
        /// to), and may not match the level users see from info log for metadata.
        /// If L0 files are merged to level-n, then, for i>0, compression_per_level[i]
        /// determines compaction type for level n+i-1.
        /// For example, if we have three 5 levels, and we determine to merge L0
        /// data to L4 (which means L1..L3 will be empty), then the new files go to
        /// L4 uses compression type compression_per_level[1].
        /// If now L0 is merged to L2. Data goes to L2 will be compressed
        /// according to compression_per_level[1], L3 using compression_per_level[2]
        /// and L4 using compression_per_level[3]. Compaction for each level can
        /// change when data grows.
        /// </summary>
        public ColumnFamilyOptions SetMinLevelToCompress(int level)
        {
            Native.Instance.rocksdb_options_set_min_level_to_compress(Handle, level);
            return this;
        }

        /// <summary>
        /// Maximum number of successive merge operations on a key in the memtable.
        ///
        /// When a merge operation is added to the memtable and the maximum number of
        /// successive merges is reached, the value of the key will be calculated and
        /// inserted into the memtable instead of the merge operation. This will
        /// ensure that there are never more than max_successive_merges merge
        /// operations in the memtable.
        ///
        /// Default: 0 (disabled)
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetMaxSuccessiveMerges(ulong value)
        {
            Native.Instance.rocksdb_options_set_max_successive_merges(Handle, (UIntPtr)value);
            return this;
        }

        /// <summary>
        /// Control locality of bloom filter probes to improve cache miss rate.
        /// This option only applies to memtable prefix bloom and plaintable
        /// prefix bloom. It essentially limits every bloom checking to one cache line.
        /// This optimization is turned off when set to 0, and positive number to turn
        /// it on.
        /// Default: 0
        /// </summary>
        public ColumnFamilyOptions SetBloomLocality(uint value)
        {
            Native.Instance.rocksdb_options_set_bloom_locality(Handle, value);
            return this;
        }

        /// <summary>
        /// Allows thread-safe inplace updates. If this is true, there is no way to
        /// achieve point-in-time consistency using snapshot or iterator (assuming
        /// concurrent updates). Hence iterator and multi-get will return results
        /// which are not consistent as of any point-in-time.
        /// If inplace_callback function is not set,
        ///   Put(key, new_value) will update inplace the existing_value iff
        ///   * key exists in current memtable
        ///   * new sizeof(new_value) <= sizeof(existing_value)
        ///   * existing_value for that key is a put i.e. kTypeValue
        /// If inplace_callback function is set, check doc for inplace_callback.
        /// Default: false.
        /// </summary>
        public ColumnFamilyOptions SetInplaceUpdateSupport(bool value)
        {
            Native.Instance.rocksdb_options_set_inplace_update_support(Handle, value);
            return this;
        }

        /// <summary>
        /// Number of locks used for inplace update
        /// Default: 10000, if inplace_update_support = true, else 0.
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetInplaceUpdateNumLocks(ulong value)
        {
            Native.Instance.rocksdb_options_set_inplace_update_num_locks(Handle, (UIntPtr)value);
            return this;
        }

        /// <summary>
        /// Measure IO stats in compactions and flushes, if true.
        /// Default: false 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ColumnFamilyOptions SetReportBgIoStats(bool value)
        {
            Native.Instance.rocksdb_options_set_report_bg_io_stats(Handle, value ? 0 : 1);
            return this;
        }

        /// <summary>
        /// Compress blocks using the specified compression algorithm.  This
        /// parameter can be changed dynamically.
        ///
        /// Default: kSnappyCompression, if it's supported. If snappy is not linked
        /// with the library, the default is kNoCompression.
        ///
        /// Typical speeds of kSnappyCompression on an Intel(R) Core(TM)2 2.4GHz:
        ///    ~200-500MB/s compression
        ///    ~400-800MB/s decompression
        /// Note that these speeds are significantly faster than most
        /// persistent storage speeds, and therefore it is typically never
        /// worth switching to kNoCompression.  Even if the input data is
        /// incompressible, the kSnappyCompression implementation will
        /// efficiently detect that and will switch to uncompressed mode.
        /// </summary>
        public ColumnFamilyOptions SetCompression(Compression value)
        {
            Native.Instance.rocksdb_options_set_compression(Handle, value);
            return this;
        }

        /// <summary>
        /// The compaction style. Default: kCompactionStyleLevel
        /// </summary>
        public ColumnFamilyOptions SetCompactionStyle(Compaction value)
        {
            Native.Instance.rocksdb_options_set_compaction_style(Handle, value);
            return this;
        }

        /// <summary>
        /// The options needed to support Universal Style compactions
        /// </summary>
        public ColumnFamilyOptions SetUniversalCompactionOptions(IntPtr universalCompactionOptions)
        {
            Native.Instance.rocksdb_options_set_universal_compaction_options(Handle, universalCompactionOptions);
            return this;
        }

        /// <summary>
        /// The options for FIFO compaction style
        /// </summary>
        public ColumnFamilyOptions SetFifoCompactionOptions(IntPtr fifoCompactionOptions)
        {
            Native.Instance.rocksdb_options_set_fifo_compaction_options(Handle, fifoCompactionOptions);
            return this;
        }

        /// <summary>
        /// Page size for huge page TLB for bloom in memtable. If <=0, not allocate
        /// from huge page TLB but from malloc.
        /// Need to reserve huge pages for it to be allocated. For example:
        ///      sysctl -w vm.nr_hugepages=20
        /// See linux doc Documentation/vm/hugetlbpage.txt
        ///
        /// Dynamically changeable through SetOptions() API
        /// </summary>
        public ColumnFamilyOptions SetMemtableHugePageSize(ulong size)
        {
            Native.Instance.rocksdb_options_set_memtable_huge_page_size(Handle, (UIntPtr)size);
            return this;
        }

    };
}
