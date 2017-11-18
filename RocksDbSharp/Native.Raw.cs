/*
    This file is the lowest level interface between the unmanged RocksDB C API and managed code.
    This is the lowest level access exposed by this library, and probably the lowest level possible.

    Most of this file derives directly from the C API header exported by RocksDB.
    In particular, it was originally derived from version 266ac24
    https://github.com/facebook/rocksdb/blob/266ac24/include/rocksdb/c.h
    And this should be treated as an ongoing "port" of that file into idomatic C#.
    Changes to c.h should be incorporated here.  View those changes by going to the native rocksdb
    source and fetching the desired version like this:
    cd native-build/rocksdb
    git fetch https://github.com/warrenfalk/rocksdb.git rocksdb_sharp
    git fetch https://github.com/facebook/rocksdb.git v5.8
    git checkout FETCH_HEAD
    git diff 266ac24 HEAD -- ./include/rocksdb/c.h
    And then once the changes are made, come back here and replace 266ac24 with whatever HEAD is

    Or:
    https://github.com/facebook/rocksdb/compare/266ac24...(version-here)#diff-53c37e7ee364f00f0280f55d1b53dccc

    This file should therefore contain no managed wrapper functions.
    It is permissible to have overloads here where appropriate (such as byte* and byte[] versions).
      These should be adjacent to each other in the source.

    Corresponding changes to c.h since the version above should be migrated to this file.
    This file should remain based on the version of c.h from the version of rocksdb packaged here.
    At the time of this writing, that is the latest commit on master because there are build issues
    on windows in all released versions.

    These are currently disabled in blocks by preprocessor defines as these are under development.

    The following regular expression helped to parse out many of the exports from c.h
    ^extern ROCKSDB_LIBRARY_API (char\*\*|int64_t|void|size_t|uint32_t|uint64_t|const char\*|char\*|unsigned char|int|rocksdb_[a-zA-Z0-9_]+\*|const rocksdb_[a-zA-Z0-9_]+\*)\s+rocksdb_([a-z]+)(_[a-zA-Z0-9_]+)?\(
*/

using System;
using System.Runtime.InteropServices;

#pragma warning disable IDE1006 // Intentionally violating naming conventions because this is meant to match the C API
namespace RocksDbSharp
{
    using size_t = System.UIntPtr;

    //void (*put)(IntPtr s, /*(const char*)*/ IntPtr k, /*(size_t)*/ UIntPtr klen, /*(const char*)*/ IntPtr v, /*(size_t)*/ UIntPtr vlen),
    public delegate void WriteBatchIteratePutCallback(IntPtr s, /*(const char*)*/ IntPtr k, /*(size_t)*/ size_t klen, /*(const char*)*/ IntPtr v, /*(size_t)*/ size_t vlen);
//void (*deleted)(void*, const char* k, /*(size_t)*/ UIntPtr klen)
public delegate void WriteBatchIterateDeleteCallback(IntPtr s, /*(const char*)*/ IntPtr k, /*(size_t)*/ size_t klen);
public abstract partial class Native
{
/* BEGIN c.h */

#region TYPES
#if ROCKSDB_TYPES

typedef struct rocksdb_t                 rocksdb_t;
typedef struct rocksdb_backup_engine_t   rocksdb_backup_engine_t;
typedef struct rocksdb_backup_engine_info_t   rocksdb_backup_engine_info_t;
typedef struct rocksdb_restore_options_t rocksdb_restore_options_t;
typedef struct rocksdb_cache_t           rocksdb_cache_t;
typedef struct rocksdb_compactionfilter_t rocksdb_compactionfilter_t;
typedef struct rocksdb_compactionfiltercontext_t
    rocksdb_compactionfiltercontext_t;
typedef struct rocksdb_compactionfilterfactory_t
    rocksdb_compactionfilterfactory_t;
typedef struct rocksdb_comparator_t      rocksdb_comparator_t;
typedef struct rocksdb_dbpath_t          rocksdb_dbpath_t;
typedef struct rocksdb_env_t             rocksdb_env_t;
typedef struct rocksdb_fifo_compaction_options_t rocksdb_fifo_compaction_options_t;
typedef struct rocksdb_filelock_t        rocksdb_filelock_t;
typedef struct rocksdb_filterpolicy_t    rocksdb_filterpolicy_t;
typedef struct rocksdb_flushoptions_t    rocksdb_flushoptions_t;
typedef struct rocksdb_iterator_t        rocksdb_iterator_t;
typedef struct rocksdb_logger_t          rocksdb_logger_t;
typedef struct rocksdb_mergeoperator_t   rocksdb_mergeoperator_t;
typedef struct rocksdb_options_t         rocksdb_options_t;
typedef struct rocksdb_compactoptions_t rocksdb_compactoptions_t;
typedef struct rocksdb_block_based_table_options_t
    rocksdb_block_based_table_options_t;
typedef struct rocksdb_cuckoo_table_options_t
    rocksdb_cuckoo_table_options_t;
typedef struct rocksdb_randomfile_t      rocksdb_randomfile_t;
typedef struct rocksdb_readoptions_t     rocksdb_readoptions_t;
typedef struct rocksdb_seqfile_t         rocksdb_seqfile_t;
typedef struct rocksdb_slicetransform_t  rocksdb_slicetransform_t;
typedef struct rocksdb_snapshot_t        rocksdb_snapshot_t;
typedef struct rocksdb_writablefile_t    rocksdb_writablefile_t;
typedef struct rocksdb_writebatch_t      rocksdb_writebatch_t;
typedef struct rocksdb_writebatch_wi_t   rocksdb_writebatch_wi_t;
typedef struct rocksdb_writeoptions_t    rocksdb_writeoptions_t;
typedef struct rocksdb_universal_compaction_options_t rocksdb_universal_compaction_options_t;
typedef struct rocksdb_livefiles_t     rocksdb_livefiles_t;
typedef struct rocksdb_column_family_handle_t rocksdb_column_family_handle_t;
typedef struct rocksdb_envoptions_t      rocksdb_envoptions_t;
typedef struct rocksdb_ingestexternalfileoptions_t rocksdb_ingestexternalfileoptions_t;
typedef struct rocksdb_sstfilewriter_t   rocksdb_sstfilewriter_t;
typedef struct rocksdb_ratelimiter_t     rocksdb_ratelimiter_t;
typedef struct rocksdb_pinnableslice_t rocksdb_pinnableslice_t;
typedef struct rocksdb_transactiondb_options_t rocksdb_transactiondb_options_t;
typedef struct rocksdb_transactiondb_t rocksdb_transactiondb_t;
typedef struct rocksdb_transaction_options_t rocksdb_transaction_options_t;
typedef struct rocksdb_optimistictransactiondb_t rocksdb_optimistictransactiondb_t;
typedef struct rocksdb_optimistictransaction_options_t rocksdb_optimistictransaction_options_t;
typedef struct rocksdb_transaction_t rocksdb_transaction_t;
typedef struct rocksdb_checkpoint_t rocksdb_checkpoint_t;

#endif
#endregion

#region DB operations

public abstract /* rocksdb_t* */ IntPtr rocksdb_open(
    /* const rocksdb_options_t* */ IntPtr options, string name, out IntPtr errptr);

public abstract /* rocksdb_t* */ IntPtr rocksdb_open_for_read_only(
    /* const rocksdb_options_t* */ IntPtr options, string name,
    bool error_if_log_file_exist, out IntPtr errptr);

public abstract /* rocksdb_backup_engine_t* */ IntPtr rocksdb_backup_engine_open(
    /* const rocksdb_options_t* */ IntPtr options, string path, out IntPtr errptr);

public abstract void rocksdb_backup_engine_create_new_backup(
    /*rocksdb_backup_engine_t**/ IntPtr backupEngine, /*rocksdb_t**/ IntPtr db, out IntPtr errptr);

public abstract void rocksdb_backup_engine_purge_old_backups(
    /*(rocksdb_backup_engine_t*)*/ IntPtr be, /*(uint32_t)*/ uint num_backups_to_keep, /*(char**)*/ out IntPtr errptr);

public abstract /* rocksdb_restore_options_t* */ IntPtr rocksdb_restore_options_create();
public abstract void rocksdb_restore_options_destroy(
    /*(rocksdb_restore_options_t*)*/ IntPtr restore_options);
public abstract void rocksdb_restore_options_set_keep_log_files(
    /*(rocksdb_restore_options_t*)*/ IntPtr restore_options, int v);

public abstract void rocksdb_backup_engine_restore_db_from_latest_backup(
    /*rocksdb_backup_engine_t**/ IntPtr backupEngine, string db_dir, string wal_dir,
    /*const rocksdb_restore_options_t**/ IntPtr restore_options, out IntPtr errptr);

public abstract /* const rocksdb_backup_engine_info_t* */ IntPtr rocksdb_backup_engine_get_backup_info(/*rocksdb_backup_engine_t**/ IntPtr backupEngine);

public abstract int rocksdb_backup_engine_info_count(
    /*const rocksdb_restore_options_t**/ IntPtr restore_options);

public abstract long rocksdb_backup_engine_info_timestamp(/*const rocksdb_backup_engine_info_t**/ IntPtr backupEngineInfo,
                                     int index);

public abstract uint rocksdb_backup_engine_info_backup_id(/*const rocksdb_backup_engine_info_t**/ IntPtr backupEngineInfo,
                                     int index);

public abstract ulong rocksdb_backup_engine_info_size(/*const rocksdb_backup_engine_info_t**/ IntPtr backupEngineInfo,
                                int index);

public abstract uint rocksdb_backup_engine_info_number_files(
    /*const rocksdb_backup_engine_info_t**/ IntPtr backupEngineInfo, int index);

public abstract void rocksdb_backup_engine_info_destroy(
    /*const rocksdb_backup_engine_info_t**/ IntPtr backupEngineInfo);

public abstract void rocksdb_backup_engine_close(
    /*rocksdb_backup_engine_t**/ IntPtr backupEngine);

public abstract /*(rocksdb_checkpoint_t*)*/ IntPtr
rocksdb_checkpoint_object_create(/*(rocksdb_t*)*/ IntPtr db, /*(char**)*/ out IntPtr errptr);

public abstract void rocksdb_checkpoint_create(
    /*(rocksdb_checkpoint_t*)*/ IntPtr checkpoint, /*(const char*)*/ string checkpoint_dir,
    /*(uint64_t)*/ ulong log_size_for_flush, /*(char**)*/ out IntPtr errptr);

public abstract void rocksdb_checkpoint_object_destroy(
    /*(rocksdb_checkpoint_t*)*/ IntPtr checkpoint);

public abstract /* rocksdb_t* */ IntPtr rocksdb_open_column_families(
    /* const rocksdb_options_t* */ IntPtr options, string name, int num_column_families,
    /*(const char**)*/ string[] column_family_names,
    /*(const rocksdb_options_t**)*/ IntPtr[] column_family_options,
    /*(rocksdb_column_family_handle_t**)*/ IntPtr[] column_family_handles, out IntPtr errptr);

public abstract /* rocksdb_t* */ IntPtr rocksdb_open_for_read_only_column_families(
    /* const rocksdb_options_t* */ IntPtr options, string name, int num_column_families,
    /*(const char**)*/ string[] column_family_names,
    /*(const rocksdb_options_t**)*/ IntPtr[] column_family_options,
    /*(rocksdb_column_family_handle_t**)*/ IntPtr[] column_family_handles,
    bool error_if_log_file_exist, out IntPtr errptr);

public abstract /* char** */ IntPtr rocksdb_list_column_families(
            /* const rocksdb_options_t* */ IntPtr options, string name, /*(size_t*)*/ out size_t lencf,
    out IntPtr errptr);

public abstract void rocksdb_list_column_families_destroy(
            /*(char**)*/ IntPtr list, UIntPtr len);

public abstract /* rocksdb_column_family_handle_t* */ IntPtr rocksdb_create_column_family(/*rocksdb_t**/ IntPtr db,
                             /* const rocksdb_options_t* */ IntPtr column_family_options,
                             string column_family_name, out IntPtr errptr);

public abstract void rocksdb_drop_column_family(
    /*rocksdb_t**/ IntPtr db, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family_handle, out IntPtr errptr);

public abstract void rocksdb_column_family_handle_destroy(
    /*rocksdb_column_family_handle_t**/ IntPtr column_family_handle);

public abstract void rocksdb_close(/*rocksdb_t**/ IntPtr db);

public unsafe abstract void rocksdb_put(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions, /*const*/ byte* key,
    UIntPtr keylen, /*const*/ byte* val, UIntPtr vallen, out IntPtr errptr);

// "long" was chosen over "ulong" for the lengths below because long is all that is possible for clr arrays anyway
public abstract void rocksdb_put(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions, /*const*/ byte[] key,
    UIntPtr keylen, /*const*/ byte[] val, UIntPtr vallen, out IntPtr errptr);

public unsafe abstract void rocksdb_put_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*const*/ byte* key,
    UIntPtr keylen, /*const*/ byte* val, UIntPtr vallen, out IntPtr errptr);

public abstract void rocksdb_put_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*const*/ byte[] key,
    UIntPtr keylen, /*const*/ byte[] val, UIntPtr vallen, out IntPtr errptr);

public unsafe abstract void rocksdb_delete(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions, /*const*/ byte* key,
    UIntPtr keylen, out IntPtr errptr);

public abstract void rocksdb_delete(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions, /*const*/ byte[] key,
    UIntPtr keylen, out IntPtr errptr);

public unsafe abstract void rocksdb_delete_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*const*/ byte* key,
    UIntPtr keylen, out IntPtr errptr);

public abstract void rocksdb_delete_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*const*/ byte[] key,
    UIntPtr keylen, out IntPtr errptr);

public unsafe abstract void rocksdb_merge(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions, /*const*/ byte* key,
    UIntPtr keylen, /*const*/ byte* val, UIntPtr vallen, out IntPtr errptr);

public unsafe abstract void rocksdb_merge_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*const*/ byte* key,
    UIntPtr keylen, /*const*/ byte* val, UIntPtr vallen, out IntPtr errptr);

public abstract void rocksdb_write(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, out IntPtr errptr);

/* Returns NULL if not found.  A malloc()ed array otherwise.
   Stores the length of the array in *vallen. */
public unsafe abstract /* char* */ IntPtr rocksdb_get(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options, /*const*/ byte* key,
            UIntPtr keylen, /*(size_t*)*/ out size_t vallen, out IntPtr errptr);

/* Returns NULL if not found.  A malloc()ed array otherwise.
   Stores the length of the array in *vallen. */
public unsafe abstract /* char* */ IntPtr rocksdb_get(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options, /*const*/ byte[] key,
            UIntPtr keylen, /*(size_t*)*/ out size_t vallen, out IntPtr errptr);

public unsafe abstract /* char* */ IntPtr rocksdb_get_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*const*/ byte* key,
            UIntPtr keylen, /*(size_t*)*/ out size_t vallen, out IntPtr errptr);

public unsafe abstract /* char* */ IntPtr rocksdb_get_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*const*/ byte[] key,
            UIntPtr keylen, /*(size_t*)*/ out size_t vallen, out IntPtr errptr);

// if values_list[i] == NULL and errs[i] == NULL,
// then we got status.IsNotFound(), which we will not return.
// all errors except status status.ok() and status.IsNotFound() are returned.
//
// errs, values_list and values_list_sizes must be num_keys in length,
// allocated by the caller.
// errs is a list of strings as opposed to the conventional one error,
// where errs[i] is the status for retrieval of keys_list[i].
// each non-NULL errs entry is a malloc()ed, null terminated string.
// each non-NULL values_list entry is a malloc()ed array, with
// the length for each stored in values_list_sizes[i].
public abstract void rocksdb_multi_get(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options, UIntPtr num_keys,
    /*const char* const**/ IntPtr keys_list, /*const size_t**/ size_t keys_list_sizes,
            /*(char**)*/ IntPtr values_list, /*size_t**/ size_t values_list_sizes, /*(char**)*/ IntPtr errlist);

public abstract void rocksdb_multi_get_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options,
            /*(const rocksdb_column_family_handle_t* const*)*/ IntPtr column_families,
            size_t num_keys, /*(const char* const*)*/ IntPtr keys_list,
            /*(const size_t*)*/ IntPtr keys_list_sizes, /*(char**)*/ IntPtr values_list,
            /*(size_t*)*/ IntPtr values_list_sizes, /*(char**)*/ IntPtr errList);

public abstract void rocksdb_multi_get(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options, UIntPtr num_keys,
    /*const char* const**/ IntPtr[] keys_list, /*const size_t**/ size_t[] keys_list_sizes,
            /*(char**)*/ IntPtr[] values_list, /*size_t**/ size_t[] values_list_sizes, /*(char**)*/ IntPtr[] errlist);

public abstract void rocksdb_multi_get_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options,
            /*(const rocksdb_column_family_handle_t* const*)*/ IntPtr[] column_families,
            UIntPtr num_keys, /*(const char* const*)*/ IntPtr[] keys_list,
            /*(const size_t*)*/ size_t[] keys_list_sizes, /*(char**)*/ IntPtr[] values_list,
            /*(size_t*)*/ size_t[] values_list_sizes, /*(char**)*/ IntPtr[] errList);

public abstract /* rocksdb_iterator_t* */ IntPtr rocksdb_create_iterator(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options);

public abstract /* rocksdb_iterator_t* */ IntPtr rocksdb_create_iterator_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family);

public abstract void rocksdb_create_iterators(
    /*(rocksdb_t *)*/ IntPtr db, /*(rocksdb_readoptions_t*)*/ IntPtr opts,
    /*(rocksdb_column_family_handle_t**)*/ IntPtr column_families,
    /*(rocksdb_iterator_t**)*/ IntPtr iterators, /*(size_t)*/ size_t size, /*(char**)*/ out IntPtr errptr);

public abstract /* const rocksdb_snapshot_t* */ IntPtr rocksdb_create_snapshot(
    /*rocksdb_t**/ IntPtr db);

public abstract void rocksdb_release_snapshot(
            /*rocksdb_t**/ IntPtr db, /*(const rocksdb_snapshot_t*)*/ IntPtr snapshot);

/* Returns NULL if property name is unknown.
   Else returns a pointer to a malloc()-ed null-terminated value. */
public abstract /* char* */ IntPtr rocksdb_property_value(/*rocksdb_t**/ IntPtr db,
                                                        string propname);
/* returns 0 on success, -1 otherwise */
/*
int rocksdb_property_int(
    rocksdb_t* db,
    const char* propname, uint64_t *out_val);
*/
public abstract /* char* */ IntPtr rocksdb_property_value_cf(
    /*rocksdb_t**/ IntPtr db, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    string propname);

public unsafe abstract void rocksdb_approximate_sizes(
            /*rocksdb_t**/ IntPtr db, int num_ranges, /*(const char* const*)*/ byte** range_start_key,
            /*(const size_t*)*/ IntPtr range_start_key_len, /*(const char* const*)*/ IntPtr range_limit_key,
            /*(const size_t*)*/ IntPtr range_limit_key_len, /*(uint64_t*)*/ IntPtr sizes);

public unsafe abstract void rocksdb_approximate_sizes_cf(
    /*rocksdb_t**/ IntPtr db, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
            int num_ranges, /*(const char* const*)*/ byte** range_start_key,
            /*(const size_t*)*/ IntPtr range_start_key_len, /*(const char* const*)*/ IntPtr range_limit_key,
            /*(const size_t*)*/ IntPtr range_limit_key_len, /*(uint64_t*)*/ IntPtr sizes);

public unsafe abstract void rocksdb_compact_range(/*rocksdb_t**/ IntPtr db,
            /*(const char*)*/ byte* start_key,
              size_t start_key_len,
            /*(const char*)*/ byte* limit_key,
              size_t limit_key_len);
public unsafe abstract void rocksdb_compact_range(/*rocksdb_t**/ IntPtr db,
            /*(const char*)*/ byte[] start_key,
              size_t start_key_len,
            /*(const char*)*/ byte[] limit_key,
              size_t limit_key_len);

public unsafe abstract void rocksdb_compact_range_cf(
    /*rocksdb_t**/ IntPtr db, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte* start_key, size_t start_key_len, /*(const char*)*/ byte* limit_key,
    size_t limit_key_len);
public unsafe abstract void rocksdb_compact_range_cf(
    /*rocksdb_t**/ IntPtr db, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte[] start_key, size_t start_key_len, /*(const char*)*/ byte[] limit_key,
    size_t limit_key_len);

public unsafe abstract void rocksdb_compact_range_opt(
    /*(rocksdb_t*)*/ IntPtr db, /*(rocksdb_compactoptions_t*)*/ IntPtr opt, /*(const char*)*/ byte* start_key,
    /*(size_t)*/ size_t start_key_len, /*(const char*)*/ byte* limit_key, /*(size_t)*/ size_t limit_key_len);

public unsafe abstract void rocksdb_compact_range_cf_opt(
    /*(rocksdb_t*)*/ IntPtr db, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(rocksdb_compactoptions_t*)*/ IntPtr opt, /*(const char*)*/ byte* start_key, /*(size_t)*/ size_t start_key_len,
    /*(const char*)*/ byte* limit_key, /*(size_t)*/ size_t limit_key_len);

public abstract void rocksdb_delete_file(/*rocksdb_t**/ IntPtr db,
                                                    string name);

public abstract /* const rocksdb_livefiles_t* */ IntPtr rocksdb_livefiles(
    /*rocksdb_t**/ IntPtr db);

public abstract void rocksdb_flush(
    /*rocksdb_t**/ IntPtr db, /*(const rocksdb_flushoptions_t*)*/ IntPtr flush_options, out IntPtr errptr);

public abstract void rocksdb_disable_file_deletions(/*rocksdb_t**/ IntPtr db,
                                                               out IntPtr errptr);

public abstract void rocksdb_enable_file_deletions(
    /*rocksdb_t**/ IntPtr db, bool force, out IntPtr errptr);

#endregion

#region Management operations
#if ROCKSDB_MANAGEMENT_OPERATIONS

public abstract void rocksdb_destroy_db(
    /* const rocksdb_options_t* */ IntPtr options, string name, out IntPtr errptr);

public abstract void rocksdb_repair_db(
    /* const rocksdb_options_t* */ IntPtr options, string name, out IntPtr errptr);

#endif
#endregion

#region Iterator

public abstract void rocksdb_iter_destroy(/*(rocksdb_iterator_t*)*/ IntPtr iter);
public abstract bool rocksdb_iter_valid(
    /*(const rocksdb_iterator_t*)*/ IntPtr iter);
public abstract void rocksdb_iter_seek_to_first(/*(rocksdb_iterator_t*)*/ IntPtr iter);
public abstract void rocksdb_iter_seek_to_last(/*(rocksdb_iterator_t*)*/ IntPtr iter);
public unsafe abstract void rocksdb_iter_seek(/*(rocksdb_iterator_t*)*/ IntPtr iter,
                                                  /*(const char*)*/ byte* k, /*(size_t)*/ size_t klen);
public abstract void rocksdb_iter_seek(/*(rocksdb_iterator_t*)*/ IntPtr iter,
                                                  /*(const char*)*/ byte[] k, /*(size_t)*/ size_t klen);
public unsafe abstract void rocksdb_iter_seek_for_prev(/*(rocksdb_iterator_t*)*/ IntPtr iter,
                                                /*(const char*)*/ byte* k,
                                                /*(size_t)*/ size_t klen);
public abstract void rocksdb_iter_seek_for_prev(/*(rocksdb_iterator_t*)*/ IntPtr iter,
                                                /*(const char*)*/ byte[] k,
                                                /*(size_t)*/ size_t klen);
public abstract void rocksdb_iter_next(/*(rocksdb_iterator_t*)*/ IntPtr iter);
public abstract void rocksdb_iter_prev(/*(rocksdb_iterator_t*)*/ IntPtr iter);
public abstract /* const char* */ IntPtr rocksdb_iter_key(
    /*(const rocksdb_iterator_t*)*/ IntPtr iter, /*(size_t*)*/ out size_t klen);
public abstract /* const char* */ IntPtr rocksdb_iter_value(
    /*(const rocksdb_iterator_t*)*/ IntPtr iter, /*(size_t*)*/ out size_t vlen);
public abstract void rocksdb_iter_get_error(
    /*(const rocksdb_iterator_t*)*/ IntPtr iter, out IntPtr errptr);

#endregion

#region Write batch

public abstract /* rocksdb_writebatch_t* */ IntPtr rocksdb_writebatch_create();
public abstract /* rocksdb_writebatch_t* */ IntPtr rocksdb_writebatch_create_from(
    /*(const char*)*/ byte[] rep, /*(size_t)*/ size_t size);
public abstract void rocksdb_writebatch_destroy(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch);
public abstract void rocksdb_writebatch_clear(/*(rocksdb_writebatch_t*)*/ IntPtr writeBatch);
public abstract int rocksdb_writebatch_count(/*(rocksdb_writebatch_t*)*/ IntPtr writeBatch);
public abstract void rocksdb_writebatch_put(/*(rocksdb_writebatch_t*)*/ IntPtr writeBatch,
                                                       /*const*/ byte[] key,
                                                       /*(size_t)*/ size_t klen,
                                                       /*const*/ byte[] val,
                                                       /*(size_t)*/ size_t vlen);
public unsafe abstract void rocksdb_writebatch_put(/*(rocksdb_writebatch_t*)*/ IntPtr writeBatch,
                                                       /*const*/ byte* key,
                                                       /*(size_t)*/ size_t klen,
                                                       /*const*/ byte* val,
                                                       /*(size_t)*/ size_t vlen);
public abstract void rocksdb_writebatch_put_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*const*/ byte[] key, /*(size_t)*/ size_t klen, /*const*/ byte[] val, /*(size_t)*/ size_t vlen);
public unsafe abstract void rocksdb_writebatch_put_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*const*/ byte* key, /*(size_t)*/ size_t klen, /*const*/ byte* val, /*(size_t)*/ size_t vlen);
public abstract void rocksdb_writebatch_putv(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, int num_keys, /*(const char* const*)*/ IntPtr keys_list,
    /*(const size_t*)*/ IntPtr keys_list_sizes, int num_values,
    /*(const char* const*)*/ IntPtr values_list, /*(const size_t*)*/ IntPtr values_list_sizes);
public abstract void rocksdb_writebatch_putv_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    int num_keys, /*(const char* const*)*/ IntPtr keys_list, /*(const size_t*)*/ IntPtr keys_list_sizes,
    int num_values, /*(const char* const*)*/ IntPtr values_list,
    /*(const size_t*)*/ IntPtr values_list_sizes);
public abstract void rocksdb_writebatch_merge(/*(rocksdb_writebatch_t*)*/ IntPtr writeBatch,
                                                         /*const*/ byte[] key,
                                                         /*(size_t)*/ size_t klen,
                                                         /*const*/ byte[] val,
                                                         /*(size_t)*/ size_t vlen);
public unsafe abstract void rocksdb_writebatch_merge(/*(rocksdb_writebatch_t*)*/ IntPtr writeBatch,
                                                         /*const*/ byte* key,
                                                         /*(size_t)*/ size_t klen,
                                                         /*const*/ byte* val,
                                                         /*(size_t)*/ size_t vlen);
public abstract void rocksdb_writebatch_merge_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*const*/ byte[] key, /*(size_t)*/ size_t klen, /*const*/ byte[] val, /*(size_t)*/ size_t vlen);
public unsafe abstract void rocksdb_writebatch_merge_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*const*/ byte* key, /*(size_t)*/ size_t klen, /*const*/ byte* val, /*(size_t)*/ size_t vlen);
public abstract void rocksdb_writebatch_mergev(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, int num_keys, /*(const char* const*)*/ IntPtr keys_list,
    /*(const size_t*)*/ IntPtr keys_list_sizes, int num_values,
    /*(const char* const*)*/ IntPtr values_list, /*(const size_t*)*/ IntPtr values_list_sizes);
public abstract void rocksdb_writebatch_mergev_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    int num_keys, /*(const char* const*)*/ IntPtr keys_list, /*(const size_t*)*/ IntPtr keys_list_sizes,
    int num_values, /*(const char* const*)*/ IntPtr values_list,
    /*(const size_t*)*/ IntPtr values_list_sizes);
public abstract void rocksdb_writebatch_delete(/*(rocksdb_writebatch_t*)*/ IntPtr writeBatch,
                                                          /*const*/ byte[] key,
                                                          /*(size_t)*/ size_t klen);
public unsafe abstract void rocksdb_writebatch_delete(/*(rocksdb_writebatch_t*)*/ IntPtr writeBatch,
                                                          /*const*/ byte* key,
                                                          /*(size_t)*/ size_t klen);
public abstract void rocksdb_writebatch_delete_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*const*/ byte[] key, /*(size_t)*/ size_t klen);
public unsafe abstract void rocksdb_writebatch_delete_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*const*/ byte* key, /*(size_t)*/ size_t klen);
public abstract void rocksdb_writebatch_deletev(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, int num_keys, /*(const char* const*)*/ IntPtr keys_list,
    /*(const size_t*)*/ IntPtr keys_list_sizes);
public abstract void rocksdb_writebatch_deletev_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    int num_keys, /*(const char* const*)*/ IntPtr keys_list, /*(const size_t*)*/ IntPtr keys_list_sizes);
public abstract void rocksdb_writebatch_delete_range(
    /*(rocksdb_writebatch_t*)*/ IntPtr b, /*(const char*)*/ byte[] start_key, /*(size_t)*/ size_t start_key_len,
    /*(const char*)*/ byte[] end_key, /*(size_t)*/ size_t end_key_len);
public unsafe abstract void rocksdb_writebatch_delete_range(
    /*(rocksdb_writebatch_t*)*/ IntPtr b, /*(const char*)*/ byte* start_key, /*(size_t)*/ size_t start_key_len,
    /*(const char*)*/ byte* end_key, /*(size_t)*/ size_t end_key_len);
public abstract void rocksdb_writebatch_delete_range_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte[] start_key, /*(size_t)*/ size_t start_key_len, /*(const char*)*/ byte[] end_key,
    /*(size_t)*/ size_t end_key_len);
public unsafe abstract void rocksdb_writebatch_delete_range_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte* start_key, /*(size_t)*/ size_t start_key_len, /*(const char*)*/ byte* end_key,
    /*(size_t)*/ size_t end_key_len);
public unsafe abstract void rocksdb_writebatch_delete_rangev(
    /*(rocksdb_writebatch_t*)*/ IntPtr b, int num_keys, /*(const char* const*)*/ IntPtr start_keys_list,
    /*(const size_t)*/ IntPtr start_keys_list_sizes, /*(const char* const*)*/ IntPtr end_keys_list,
    /*(const size_t)*/ IntPtr end_keys_list_sizes);
public unsafe abstract void rocksdb_writebatch_delete_rangev_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    int num_keys, /*(const char* const*)*/ IntPtr start_keys_list,
    /*(const size_t)*/ IntPtr start_keys_list_sizes, /*(const char* const*)*/ IntPtr end_keys_list,
    /*(const size_t)*/ IntPtr end_keys_list_sizes);
public abstract void rocksdb_writebatch_put_log_data(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, byte[] blob, UIntPtr len);
public abstract void rocksdb_writebatch_iterate(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(void*)*/ IntPtr state,
    //void (*put)(IntPtr s, /*(const char*)*/ IntPtr k, /*(size_t)*/ ulong klen, /*(const char*)*/ IntPtr v, /*(size_t)*/ ulong vlen),
    WriteBatchIteratePutCallback put,
    //void (*deleted)(void*, const char* k, /*(size_t)*/ ulong klen)
    WriteBatchIterateDeleteCallback deleted);
public abstract /* const char* */ IntPtr rocksdb_writebatch_data(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(size_t*)*/ out size_t size);
public abstract void rocksdb_writebatch_set_save_point(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch);
public abstract void rocksdb_writebatch_rollback_to_save_point(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, out IntPtr errptr);
public abstract void rocksdb_writebatch_pop_save_point(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(char**)*/ out IntPtr errptr);
#endregion

#region Write Batch with index

public abstract /*(rocksdb_writebatch_wi_t*)*/ IntPtr rocksdb_writebatch_wi_create(
                                                       /*(size_t)*/ size_t reserved_bytes,
                                                       /*(unsigned char)*/ bool overwrite_keys);
#if false // not actually implemented
public abstract /*(rocksdb_writebatch_wi_t*)*/ IntPtr rocksdb_writebatch_wi_create_from(
    /*(const char*)*/ byte[] rep, /*(size_t)*/ ulong size);
#endif
public abstract void rocksdb_writebatch_wi_destroy(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b);
public abstract void rocksdb_writebatch_wi_clear(/*(rocksdb_writebatch_wi_t*)*/ IntPtr b);
public abstract int rocksdb_writebatch_wi_count(/*(rocksdb_writebatch_wi_t*)*/ IntPtr b);
public abstract void rocksdb_writebatch_wi_put(/*(rocksdb_writebatch_wi_t*)*/ IntPtr b,
                                                       /*(const char*)*/ byte[] key,
                                                       /*(size_t)*/ size_t klen,
                                                       /*(const char*)*/ byte[] val,
                                                       /*(size_t)*/ size_t vlen);
public abstract unsafe void rocksdb_writebatch_wi_put(/*(rocksdb_writebatch_wi_t*)*/ IntPtr b,
                                                       /*(const char*)*/ byte* key,
                                                       /*(size_t)*/ size_t klen,
                                                       /*(const char*)*/ byte* val,
                                                       /*(size_t)*/ size_t vlen);
public abstract void rocksdb_writebatch_wi_put_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte[] key, /*(size_t)*/ size_t klen, /*(const char*)*/ byte[] val, /*(size_t)*/ size_t vlen);
public abstract unsafe void rocksdb_writebatch_wi_put_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte* key, /*(size_t)*/ size_t klen, /*(const char*)*/ byte* val, /*(size_t)*/ size_t vlen);
public abstract void rocksdb_writebatch_wi_putv(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, int num_keys, /*(const char* const*)*/ IntPtr keys_list,
    /*(const size_t*)*/ IntPtr keys_list_sizes, int num_values,
    /*(const char* const*)*/ IntPtr values_list, /*(const size_t*)*/ IntPtr values_list_sizes);
public abstract void rocksdb_writebatch_wi_putv_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    int num_keys, /*(const char* const*)*/ IntPtr keys_list, /*(const size_t*)*/ IntPtr keys_list_sizes,
    int num_values, /*(const char* const*)*/ IntPtr values_list,
    /*(const size_t*)*/ IntPtr values_list_sizes);
public abstract void rocksdb_writebatch_wi_merge(/*(rocksdb_writebatch_wi_t*)*/ IntPtr b,
                                                         /*(const char*)*/ byte[] key,
                                                         /*(size_t)*/ size_t klen,
                                                         /*(const char*)*/ byte[] val,
                                                         /*(size_t)*/ size_t vlen);
public abstract unsafe void rocksdb_writebatch_wi_merge(/*(rocksdb_writebatch_wi_t*)*/ IntPtr b,
                                                         /*(const char*)*/ byte* key,
                                                         /*(size_t)*/ size_t klen,
                                                         /*(const char*)*/ byte* val,
                                                         /*(size_t)*/ size_t vlen);
public abstract void rocksdb_writebatch_wi_merge_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte[] key, /*(size_t)*/ size_t klen, /*(const char*)*/ byte[] val, /*(size_t)*/ size_t vlen);
public abstract unsafe void rocksdb_writebatch_wi_merge_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte* key, /*(size_t)*/ size_t klen, /*(const char*)*/ byte* val, /*(size_t)*/ size_t vlen);
public abstract void rocksdb_writebatch_wi_mergev(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, int num_keys, /*(const char* const*)*/ IntPtr keys_list,
    /*(const size_t*)*/ IntPtr keys_list_sizes, int num_values,
    /*(const char* const*)*/ IntPtr values_list, /*(const size_t*)*/ IntPtr values_list_sizes);
public abstract void rocksdb_writebatch_wi_mergev_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    int num_keys, /*(const char* const*)*/ IntPtr keys_list, /*(const size_t*)*/ IntPtr keys_list_sizes,
    int num_values, /*(const char* const*)*/ IntPtr values_list,
    /*(const size_t*)*/ IntPtr values_list_sizes);
public abstract void rocksdb_writebatch_wi_delete(/*(rocksdb_writebatch_wi_t*)*/ IntPtr b,
                                                          /*(const char*)*/ byte[] key,
                                                          /*(size_t)*/ size_t klen);
public abstract unsafe void rocksdb_writebatch_wi_delete(/*(rocksdb_writebatch_wi_t*)*/ IntPtr b,
                                                          /*(const char*)*/ byte* key,
                                                          /*(size_t)*/ size_t klen);
public abstract void rocksdb_writebatch_wi_delete_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte[] key, /*(size_t)*/ size_t klen);
public abstract unsafe void rocksdb_writebatch_wi_delete_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte* key, /*(size_t)*/ size_t klen);
public abstract void rocksdb_writebatch_wi_deletev(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, int num_keys, /*(const char* const*)*/ IntPtr keys_list,
    /*(const size_t*)*/ IntPtr keys_list_sizes);
public abstract void rocksdb_writebatch_wi_deletev_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    int num_keys, /*(const char* const*)*/ IntPtr keys_list, /*(const size_t*)*/ IntPtr keys_list_sizes);
public abstract void rocksdb_writebatch_wi_delete_range(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(const char*)*/ byte[] start_key, /*(size_t)*/ size_t start_key_len,
    /*(const char*)*/ byte[] end_key, /*(size_t)*/ size_t end_key_len);
public abstract unsafe void rocksdb_writebatch_wi_delete_range(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(const char*)*/ byte* start_key, /*(size_t)*/ size_t start_key_len,
    /*(const char*)*/ byte* end_key, /*(size_t)*/ size_t end_key_len);
public abstract void rocksdb_writebatch_wi_delete_range_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte[] start_key, /*(size_t)*/ size_t start_key_len, /*(const char*)*/ byte[] end_key,
    /*(size_t)*/ size_t end_key_len);
public abstract unsafe void rocksdb_writebatch_wi_delete_range_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte* start_key, /*(size_t)*/ size_t start_key_len, /*(const char*)*/ byte* end_key,
    /*(size_t)*/ size_t end_key_len);
public abstract void rocksdb_writebatch_wi_delete_rangev(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, int num_keys, /*(const char* const*)*/ IntPtr start_keys_list,
    /*(const size_t*)*/ IntPtr start_keys_list_sizes, /*(const char* const*)*/ IntPtr end_keys_list,
    /*(const size_t*)*/ IntPtr end_keys_list_sizes);
public abstract void rocksdb_writebatch_wi_delete_rangev_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    int num_keys, /*(const char* const*)*/ IntPtr start_keys_list,
    /*(const size_t*)*/ IntPtr start_keys_list_sizes, /*(const char* const*)*/ IntPtr end_keys_list,
    /*(const size_t*)*/ IntPtr end_keys_list_sizes);
public abstract void rocksdb_writebatch_wi_put_log_data(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(const char*)*/ byte[] blob, /*(size_t)*/ size_t len);
public abstract void rocksdb_writebatch_wi_put_log_data(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(const char*)*/ IntPtr blob, /*(size_t)*/ size_t len);
public abstract void rocksdb_writebatch_wi_iterate(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b,
    /*(void*)*/ IntPtr state,
    /*(void (*put)(void*, const char* k, size_t klen, const char* v, size_t vlen))*/ WriteBatchIteratePutCallback put,
    /*(void (*deleted)(void*, const char* k, size_t klen))*/ WriteBatchIterateDeleteCallback deleted);
public abstract /*(const char*)*/ IntPtr rocksdb_writebatch_wi_data(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b,
    /*(size_t*)*/ out size_t size);
public abstract void rocksdb_writebatch_wi_set_save_point(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b);
public abstract void rocksdb_writebatch_wi_rollback_to_save_point(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr b, /*(char**)*/ out IntPtr errptr);
public abstract /*(char*)*/ IntPtr rocksdb_writebatch_wi_get_from_batch(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr wbwi,
    /*(const rocksdb_options_t*)*/ IntPtr options,
    /*(const char*)*/ byte[] key, /*(size_t)*/ size_t keylen,
    /*(size_t*)*/ out size_t vallen,
    /*(char**)*/ out IntPtr errptr);
public abstract unsafe /*(char*)*/ IntPtr rocksdb_writebatch_wi_get_from_batch(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr wbwi,
    /*(const rocksdb_options_t*)*/ IntPtr options,
    /*(const char*)*/ byte* key, /*(size_t)*/ size_t keylen,
    /*(size_t*)*/ out size_t vallen,
    /*(char**)*/ out IntPtr errptr);
public abstract /*(char*)*/ IntPtr rocksdb_writebatch_wi_get_from_batch_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr wbwi,
    /*(const rocksdb_options_t*)*/ IntPtr options,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte[] key, /*(size_t)*/ size_t keylen,
    /*(size_t*)*/ out size_t vallen,
    /*(char**)*/ out IntPtr errptr);
public abstract unsafe /*(char*)*/ IntPtr rocksdb_writebatch_wi_get_from_batch_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr wbwi,
    /*(const rocksdb_options_t*)*/ IntPtr options,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte* key, /*(size_t)*/ size_t keylen,
    /*(size_t*)*/ out size_t vallen,
    /*(char**)*/ out IntPtr errptr);
public abstract /*(char*)*/ IntPtr rocksdb_writebatch_wi_get_from_batch_and_db(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr wbwi,
    /*(rocksdb_t*)*/ IntPtr db,
    /*(const rocksdb_readoptions_t*)*/ IntPtr read_options,
    /*(const char*)*/ byte[] key, /*(size_t)*/ size_t keylen,
    /*(size_t*)*/ out size_t vallen,
    /*(char**)*/ out IntPtr errptr);
public abstract unsafe /*(char*)*/ IntPtr rocksdb_writebatch_wi_get_from_batch_and_db(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr wbwi,
    /*(rocksdb_t*)*/ IntPtr db,
    /*(const rocksdb_readoptions_t*)*/ IntPtr read_options,
    /*(const char*)*/ byte* key, /*(size_t)*/ size_t keylen,
    /*(size_t*)*/ out size_t vallen,
    /*(char**)*/ out IntPtr errptr);
public abstract /*(char*)*/ IntPtr rocksdb_writebatch_wi_get_from_batch_and_db_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr wbwi,
    /*(rocksdb_t*)*/ IntPtr db,
    /*(const rocksdb_readoptions_t*)*/ IntPtr read_options,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte[] key, /*(size_t)*/ size_t keylen,
    /*(size_t*)*/ out size_t vallen,
    /*(char**)*/ out IntPtr errptr);
public abstract unsafe /*(char*)*/ IntPtr rocksdb_writebatch_wi_get_from_batch_and_db_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr wbwi,
    /*(rocksdb_t*)*/ IntPtr db,
    /*(const rocksdb_readoptions_t*)*/ IntPtr read_options,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte* key, /*(size_t)*/ size_t keylen,
    /*(size_t*)*/ out size_t vallen,
    /*(char**)*/ out IntPtr errptr);
public abstract void rocksdb_write_writebatch_wi(
    /*(rocksdb_t*)*/ IntPtr db,
    /*(const rocksdb_writeoptions_t*)*/ IntPtr write_options,
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr wbwi,
    /*(char**)*/ out IntPtr errptr);
public abstract /*(rocksdb_iterator_t*)*/ IntPtr rocksdb_writebatch_wi_create_iterator_with_base(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr wbwi,
    /*(rocksdb_iterator_t*)*/ IntPtr base_iterator);
public abstract /*(rocksdb_iterator_t*)*/ IntPtr rocksdb_writebatch_wi_create_iterator_with_base_cf(
    /*(rocksdb_writebatch_wi_t*)*/ IntPtr wbwi,
    /*(rocksdb_iterator_t*)*/ IntPtr base_iterator,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr cf);

#endregion

#region Block based table options

public abstract /* rocksdb_block_based_table_options_t* */ IntPtr rocksdb_block_based_options_create();
public abstract void rocksdb_block_based_options_destroy(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr options);
public abstract void rocksdb_block_based_options_set_block_size(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr options, /*(size_t)*/ size_t block_size);
public abstract void rocksdb_block_based_options_set_block_size_deviation(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr options, int block_size_deviation);
public abstract void rocksdb_block_based_options_set_block_restart_interval(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr options, int block_restart_interval);
public abstract void rocksdb_block_based_options_set_filter_policy(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr options,
    /*(rocksdb_filterpolicy_t*)*/ IntPtr filter_policy);
public abstract void rocksdb_block_based_options_set_no_block_cache(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr options, /*(unsigned char)*/ bool no_block_cache);
public abstract void rocksdb_block_based_options_set_block_cache(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr options, /*(rocksdb_cache_t*)*/ IntPtr block_cache);
public abstract void rocksdb_block_based_options_set_block_cache_compressed(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr options,
    /*(rocksdb_cache_t*)*/ IntPtr block_cache_compressed);
public abstract void rocksdb_block_based_options_set_whole_key_filtering(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr bbto, /*(unsigned char)*/ bool whole_key_filtering);
public abstract void rocksdb_block_based_options_set_format_version(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr bbto, int format_version);
}
public enum BlockBasedTableIndexType {
  BinarySearch = 0,
  HashSearch = 1,
  TwoLevelIndexSearch = 2,
};
public abstract partial class Native {
public abstract void rocksdb_block_based_options_set_index_type(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr bbto, BlockBasedTableIndexType index_type);  // uses one of the above enums
public abstract void rocksdb_block_based_options_set_hash_index_allow_collision(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr bbto, /*(unsigned char)*/ bool allow_collision);
public abstract void rocksdb_block_based_options_set_cache_index_and_filter_blocks(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr bbto, /*(unsigned char)*/ bool cache_index_and_filter_blocks);
public abstract void rocksdb_block_based_options_set_pin_l0_filter_and_index_blocks_in_cache(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr bbto, /*(unsigned char)*/ bool pin_l0_filter_and_index_blocks_in_cache);
public abstract void rocksdb_options_set_block_based_table_factory(
    /* rocksdb_options_t* */ IntPtr opt, /*(rocksdb_block_based_table_options_t*)*/ IntPtr table_options);
#endregion

#region Cuckoo table options
#if ROCKSDB_CUCKOO_TABLE_OPTIONS

public abstract /* rocksdb_cuckoo_table_options_t* */ IntPtr rocksdb_cuckoo_options_create();
public abstract void rocksdb_cuckoo_options_destroy(
    rocksdb_cuckoo_table_options_t* options);
public abstract void rocksdb_cuckoo_options_set_hash_ratio(
    rocksdb_cuckoo_table_options_t* options, double v);
public abstract void rocksdb_cuckoo_options_set_max_search_depth(
    rocksdb_cuckoo_table_options_t* options, uint32_t v);
public abstract void rocksdb_cuckoo_options_set_cuckoo_block_size(
    rocksdb_cuckoo_table_options_t* options, uint32_t v);
public abstract void rocksdb_cuckoo_options_set_identity_as_first_hash(
    rocksdb_cuckoo_table_options_t* options, unsigned char v);
public abstract void rocksdb_cuckoo_options_set_use_module_hash(
    rocksdb_cuckoo_table_options_t* options, unsigned char v);
public abstract void rocksdb_options_set_cuckoo_table_factory(
    /* rocksdb_options_t* */ IntPtr opt, rocksdb_cuckoo_table_options_t* table_options);

#endif
#endregion

#region Options
public abstract void rocksdb_set_options(
    /*(rocksdb_t*)*/ IntPtr db, int count, /*(const char* const)*/ string[] keys, /*(const char* const)*/ string[] values, /*(char **)*/ out IntPtr errptr);

public abstract /* rocksdb_options_t* */ IntPtr rocksdb_options_create();
public abstract void rocksdb_options_destroy(/* rocksdb_options_t* */ IntPtr options);
public abstract void rocksdb_options_increase_parallelism(
    /* rocksdb_options_t* */ IntPtr opt, int total_threads);
public abstract void rocksdb_options_optimize_for_point_lookup(
    /* rocksdb_options_t* */ IntPtr opt, ulong block_cache_size_mb);
public abstract void rocksdb_options_optimize_level_style_compaction(
    /* rocksdb_options_t* */ IntPtr opt, ulong memtable_memory_budget);
public abstract void rocksdb_options_optimize_universal_style_compaction(
    /* rocksdb_options_t* */ IntPtr opt, ulong memtable_memory_budget);
public abstract void rocksdb_options_set_compaction_filter(
            /* rocksdb_options_t* */ IntPtr options, /*(rocksdb_compactionfilter_t*)*/ IntPtr compaction_filter);
public abstract void rocksdb_options_set_compaction_filter_factory(
            /* rocksdb_options_t* */ IntPtr options, /*(rocksdb_compactionfilterfactory_t*)*/ IntPtr compaction_filter_factory);
public abstract void rocksdb_options_compaction_readahead_size(
    /* rocksdb_options_t* */ IntPtr options, /* size_t */ size_t size);
public abstract void rocksdb_options_set_comparator(
            /* rocksdb_options_t* */ IntPtr options, /*(rocksdb_comparator_t*)*/ IntPtr comparator);
public abstract void rocksdb_options_set_merge_operator(
            /* rocksdb_options_t* */ IntPtr options, /*(rocksdb_mergeoperator_t*)*/ IntPtr merge_operator);
public abstract void rocksdb_options_set_uint64add_merge_operator(
            /* rocksdb_options_t* */ IntPtr options);
public abstract void rocksdb_options_set_compression_per_level(
            /* rocksdb_options_t* */ IntPtr opt, /*(int*)*/ int[] level_values, UIntPtr num_levels);
public abstract void rocksdb_options_set_create_if_missing(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_create_missing_column_families(/* rocksdb_options_t* */ IntPtr options,
            bool value);
public abstract void rocksdb_options_set_error_if_exists(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_paranoid_checks(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_db_paths(/*(rocksdb_options_t*)*/ IntPtr options,
            /*(const rocksdb_dbpath_t**)*/ IntPtr path_values, 
            size_t num_paths);
public abstract void rocksdb_options_set_env(/* rocksdb_options_t* */ IntPtr options,
            /*(rocksdb_env_t*)*/ IntPtr env);
public abstract void rocksdb_options_set_info_log(/* rocksdb_options_t* */ IntPtr options,
            /*(rocksdb_logger_t*)*/ IntPtr logger);
public abstract void rocksdb_options_set_info_log_level(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_write_buffer_size(
            /* rocksdb_options_t* */ IntPtr options, UIntPtr value);
public abstract void rocksdb_options_set_db_write_buffer_size(
    /* rocksdb_options_t* */ IntPtr options, /* size_t */ size_t size);
public abstract void rocksdb_options_set_max_open_files(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_max_file_opening_threads(
    /*(rocksdb_options_t*)*/ IntPtr options, int value);
public abstract void rocksdb_options_set_max_total_wal_size(
    /* rocksdb_options_t* */ IntPtr opt, ulong n);
public abstract void rocksdb_options_set_compression_options(
    /* rocksdb_options_t* */ IntPtr options, int p1, int p2, int p3, int p4);
public abstract void rocksdb_options_set_prefix_extractor(
            /* rocksdb_options_t* */ IntPtr options, /*(rocksdb_slicetransform_t*)*/ IntPtr slice_transform);
public abstract void rocksdb_options_set_num_levels(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_level0_file_num_compaction_trigger(/* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_level0_slowdown_writes_trigger(/* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_level0_stop_writes_trigger(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_max_mem_compaction_level(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_target_file_size_base(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void rocksdb_options_set_target_file_size_multiplier(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_max_bytes_for_level_base(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void
rocksdb_options_set_level_compaction_dynamic_level_bytes(/* rocksdb_options_t* */ IntPtr options,
                                                         bool value);
public abstract void rocksdb_options_set_max_bytes_for_level_multiplier(/* rocksdb_options_t* */ IntPtr options, double value);
public abstract void rocksdb_options_set_max_bytes_for_level_multiplier_additional(
            /* rocksdb_options_t* */ IntPtr options, /*(int*)*/ int[] level_values, UIntPtr num_levels);
public abstract void rocksdb_options_enable_statistics(
    /* rocksdb_options_t* */ IntPtr options);
public abstract void rocksdb_options_set_skip_stats_update_on_db_open(
    /*(rocksdb_options_t*)*/ IntPtr options, /*(unsigned char)*/ bool val);

/* returns a pointer to a malloc()-ed, null terminated string */
public abstract /* char* */ IntPtr rocksdb_options_statistics_get_string(
    /* rocksdb_options_t* */ IntPtr opt);

public abstract void rocksdb_options_set_max_write_buffer_number(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_min_write_buffer_number_to_merge(/* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_max_write_buffer_number_to_maintain(/* rocksdb_options_t* */ IntPtr options,
                                                        int value);
public abstract void rocksdb_options_set_max_background_compactions(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_base_background_compactions(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_max_background_flushes(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_max_log_file_size(
            /* rocksdb_options_t* */ IntPtr options, UIntPtr value);
public abstract void rocksdb_options_set_log_file_time_to_roll(
            /* rocksdb_options_t* */ IntPtr options, UIntPtr value);
public abstract void rocksdb_options_set_keep_log_file_num(
            /* rocksdb_options_t* */ IntPtr options, UIntPtr value);
public abstract void rocksdb_options_set_recycle_log_file_num(
            /* rocksdb_options_t* */ IntPtr options, UIntPtr value);
public abstract void rocksdb_options_set_soft_rate_limit(
            /* rocksdb_options_t* */ IntPtr options, double value);
public abstract void rocksdb_options_set_hard_rate_limit(
            /* rocksdb_options_t* */ IntPtr options, double value);
public abstract void rocksdb_options_set_soft_pending_compaction_bytes_limit(
    /*(rocksdb_options_t*)*/ IntPtr opt, /*(size_t)*/ size_t v);
public abstract void rocksdb_options_set_hard_pending_compaction_bytes_limit(
    /*(rocksdb_options_t*)*/ IntPtr opt, /*(size_t)*/ size_t v);
public abstract void rocksdb_options_set_rate_limit_delay_max_milliseconds(/* rocksdb_options_t* */ IntPtr options,
            uint value);
public abstract void rocksdb_options_set_max_manifest_file_size(
            /* rocksdb_options_t* */ IntPtr options, UIntPtr value);
public abstract void rocksdb_options_set_table_cache_numshardbits(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_table_cache_remove_scan_count_limit(/* rocksdb_options_t* */ IntPtr options,
                                                        int value);
public abstract void rocksdb_options_set_arena_block_size(
            /* rocksdb_options_t* */ IntPtr options, UIntPtr value);
public abstract void rocksdb_options_set_use_fsync(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_db_log_dir(
            /* rocksdb_options_t* */ IntPtr options, string value);
public abstract void rocksdb_options_set_wal_dir(/* rocksdb_options_t* */ IntPtr options,
            string value);
public abstract void rocksdb_options_set_WAL_ttl_seconds(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void rocksdb_options_set_WAL_size_limit_MB(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void rocksdb_options_set_manifest_preallocation_size(
            /* rocksdb_options_t* */ IntPtr options, UIntPtr value);
public abstract void rocksdb_options_set_purge_redundant_kvs_while_flush(/* rocksdb_options_t* */ IntPtr options,
            bool value);
public abstract void rocksdb_options_set_allow_mmap_reads(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_allow_mmap_writes(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_use_direct_reads(
    /*(rocksdb_options_t*)*/ IntPtr options, /*(unsigned char)*/ bool enable);
public abstract void 
rocksdb_options_set_use_direct_io_for_flush_and_compaction(/*(rocksdb_options_t*)*/ IntPtr options,
    /*(unsigned char)*/ bool enable);
public abstract void rocksdb_options_set_is_fd_close_on_exec(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_skip_log_error_on_recovery(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_stats_dump_period_sec(
    /* rocksdb_options_t* */ IntPtr options, uint value);
public abstract void rocksdb_options_set_advise_random_on_open(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_access_hint_on_compaction_start(/* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_use_adaptive_mutex(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_bytes_per_sync(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void
rocksdb_options_set_allow_concurrent_memtable_write(/* rocksdb_options_t* */ IntPtr options,
                                                    bool value);
public abstract void
rocksdb_options_set_enable_write_thread_adaptive_yield(/* rocksdb_options_t* */ IntPtr options,
                                                       bool value);
public abstract void rocksdb_options_set_max_sequential_skip_in_iterations(/* rocksdb_options_t* */ IntPtr options,
            ulong value);
public abstract void rocksdb_options_set_disable_auto_compactions(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_optimize_filters_for_hits(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_delete_obsolete_files_period_micros(/* rocksdb_options_t* */ IntPtr options,
            ulong value);
public abstract void rocksdb_options_prepare_for_bulk_load(
    /* rocksdb_options_t* */ IntPtr options);
public abstract void rocksdb_options_set_memtable_vector_rep(
    /* rocksdb_options_t* */ IntPtr options);
public abstract void rocksdb_options_set_memtable_prefix_bloom_size_ratio(
    /* rocksdb_options_t* */ IntPtr options, double ratio);
public abstract void rocksdb_options_set_max_compaction_bytes(
    /* rocksdb_options_t* */ IntPtr options, /* uint64_t */ ulong bytes);
public abstract void rocksdb_options_set_hash_skip_list_rep(
            /* rocksdb_options_t* */ IntPtr options, UIntPtr p1, int p2, int p3);
public abstract void rocksdb_options_set_hash_link_list_rep(
            /* rocksdb_options_t* */ IntPtr options, UIntPtr value);
public abstract void rocksdb_options_set_plain_table_factory(
            /* rocksdb_options_t* */ IntPtr options, UInt32 p1, int p2, double p3, UIntPtr p4);

public abstract void rocksdb_options_set_min_level_to_compress(
    /* rocksdb_options_t* */ IntPtr opt, int level);

public abstract void rocksdb_options_set_memtable_huge_page_size(
    /* rocksdb_options_t* */ IntPtr options, /*(size_t)*/ size_t size);

public abstract void rocksdb_options_set_max_successive_merges(
            /* rocksdb_options_t* */ IntPtr options, UIntPtr value);
public abstract void rocksdb_options_set_bloom_locality(
            /* rocksdb_options_t* */ IntPtr options, uint value);
public abstract void rocksdb_options_set_inplace_update_support(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_inplace_update_num_locks(
            /* rocksdb_options_t* */ IntPtr options, UIntPtr value);
public abstract void rocksdb_options_set_report_bg_io_stats(
            /* rocksdb_options_t* */ IntPtr options, int value);

}
public enum WalRecoveryMode {  
  rocksdb_tolerate_corrupted_tail_records_recovery = 0,
  rocksdb_absolute_consistency_recovery = 1,
  rocksdb_point_in_time_recovery = 2,
  rocksdb_skip_any_corrupted_records_recovery = 3
}
public abstract partial class Native {
public abstract void rocksdb_options_set_wal_recovery_mode(
    /* rocksdb_options_t* */ IntPtr options, WalRecoveryMode mode);
}

public enum CompressionTypeEnum {
  rocksdb_no_compression = 0,
  rocksdb_snappy_compression = 1,
  rocksdb_zlib_compression = 2,
  rocksdb_bz2_compression = 3,
  rocksdb_lz4_compression = 4,
  rocksdb_lz4hc_compression = 5,
  rocksdb_xpress_compression = 6,
  rocksdb_zstd_compression = 7
}
public abstract partial class Native {
public abstract void rocksdb_options_set_compression(
            /* rocksdb_options_t* */ IntPtr options, CompressionTypeEnum value);
}
public enum CompactionStyleEnum {
  rocksdb_level_compaction = 0,
  rocksdb_universal_compaction = 1,
  rocksdb_fifo_compaction = 2,
}
public abstract partial class Native {
public abstract void rocksdb_options_set_compaction_style(
    /* rocksdb_options_t* */ IntPtr options, CompactionStyleEnum value);
public abstract void rocksdb_options_set_universal_compaction_options(
            /* rocksdb_options_t* */ IntPtr options, /*(rocksdb_universal_compaction_options_t*)*/ IntPtr universal_compaction_options);
public abstract void rocksdb_options_set_fifo_compaction_options(
            /* rocksdb_options_t* */ IntPtr opt, /*(rocksdb_fifo_compaction_options_t*)*/ IntPtr fifo_compaction_options);
#endregion
#region Rate Limiter
#if ROCKSDB_RATE_LIMITER
public abstract void rocksdb_options_set_ratelimiter(
    /* rocksdb_options_t* */ IntPtr opt, rocksdb_ratelimiter_t* limiter);

/* RateLimiter */
public abstract /*(rocksdb_ratelimiter_t*)*/ IntPtr rocksdb_ratelimiter_create(
    int64_t rate_bytes_per_sec, int64_t refill_period_us, int32_t fairness);
public abstract void rocksdb_ratelimiter_destroy(rocksdb_ratelimiter_t*);
#endif
#endregion


#region Compaction Filter
#if ROCKSDB_COMPACTION_FILTER

public abstract /* rocksdb_compactionfilter_t* */ IntPtr rocksdb_compactionfilter_create(
    void* state, void (*destructor)(void*),
    unsigned char (*filter)(void*, int level, /*const*/ byte* key,
                            size_t key_length, const char* existing_value,
                            size_t value_length, char** new_value,
                            size_t* new_value_length,
                            unsigned char* value_changed),
    const char* (*name)(void*));
public abstract void rocksdb_compactionfilter_set_ignore_snapshots(
    rocksdb_compactionfilter_t*, unsigned char);
public abstract void rocksdb_compactionfilter_destroy(
    rocksdb_compactionfilter_t*);

#endif
#endregion

#region Compaction Filter Context
#if ROCKSDB_COMPACTION_FILTER_CONTEXT

public abstract bool rocksdb_compactionfiltercontext_is_full_compaction(
    rocksdb_compactionfiltercontext_t* context);

public abstract bool rocksdb_compactionfiltercontext_is_manual_compaction(
    rocksdb_compactionfiltercontext_t* context);

#endif
#endregion

#region Compaction Filter Factory
#if ROCKSDB_COMPACTION_FILTER_FACTORY

public abstract /* rocksdb_compactionfilterfactory_t* */ IntPtr rocksdb_compactionfilterfactory_create(
    void* state, void (*destructor)(void*),
    rocksdb_compactionfilter_t* (*create_compaction_filter)(
        void*, rocksdb_compactionfiltercontext_t* context),
    const char* (*name)(void*));
public abstract void rocksdb_compactionfilterfactory_destroy(
    rocksdb_compactionfilterfactory_t*);

#endif
#endregion

#region Comparator
public abstract /* rocksdb_comparator_t* */ IntPtr rocksdb_comparator_create(
    /*(void*)*/ IntPtr state, /*(void (*destructor)(void*))*/ IntPtr destructor,
                   /*(int (*compare)(void*, const char* a, size_t alen, const char* b,
                                  size_t blen))*/ IntPtr compare,
    /*(const char* (*name)(void*))*/ IntPtr getName);
public abstract void rocksdb_comparator_destroy(
    /*(rocksdb_comparator_t*)*/IntPtr comparator);

#endregion

#region Filter policy
#if ROCKSDB_FILTER_POLICY_FULL
public abstract /* rocksdb_filterpolicy_t* */ IntPtr rocksdb_filterpolicy_create(
    void* state, void (*destructor)(void*),
    char* (*create_filter)(void*, const char* const* key_array,
                           const size_t* key_length_array, int num_keys,
                           size_t* filter_length),
    unsigned char (*key_may_match)(void*, /*const*/ byte* key, ulong length,
                                   const char* filter, size_t filter_length),
    void (*delete_filter)(void*, const char* filter, size_t filter_length),
    const char* (*name)(void*));
#endif
public abstract void rocksdb_filterpolicy_destroy(
            /*(rocksdb_filterpolicy_t*)*/ IntPtr filter_policy);

public abstract /* rocksdb_filterpolicy_t* */ IntPtr rocksdb_filterpolicy_create_bloom(int bits_per_key);

public abstract /* rocksdb_filterpolicy_t* */ IntPtr rocksdb_filterpolicy_create_bloom_full(int bits_per_key);

#endregion

#region Merge Operator
#if ROCKSDB_MERGE_OPERATOR

public abstract /* rocksdb_mergeoperator_t* */ IntPtr rocksdb_mergeoperator_create(
    void* state, void (*destructor)(void*),
    char* (*full_merge)(void*, /*const*/ byte* key, size_t key_length,
                        const char* existing_value,
                        size_t existing_value_length,
                        const char* const* operands_list,
                        const size_t* operands_list_length, int num_operands,
                        unsigned char* success, size_t* new_value_length),
    char* (*partial_merge)(void*, /*const*/ byte* key, size_t key_length,
                           const char* const* operands_list,
                           const size_t* operands_list_length, int num_operands,
                           unsigned char* success, size_t* new_value_length),
    void (*delete_value)(void*, /*const*/ byte* value, size_t value_length),
    const char* (*name)(void*));
public abstract void rocksdb_mergeoperator_destroy(
    rocksdb_mergeoperator_t*);

#endif
#endregion

#region Read options

public abstract /* rocksdb_readoptions_t* */ IntPtr rocksdb_readoptions_create();
public abstract void rocksdb_readoptions_destroy(
    /*(rocksdb_readoptions_t*)*/ IntPtr read_options);
public abstract void rocksdb_readoptions_set_verify_checksums(
    /*(rocksdb_readoptions_t*)*/ IntPtr read_options, bool value);
public abstract void rocksdb_readoptions_set_fill_cache(
    /*(rocksdb_readoptions_t*)*/ IntPtr read_options, bool value);
public abstract void rocksdb_readoptions_set_snapshot(
    /*(rocksdb_readoptions_t*)*/ IntPtr read_options, /*(const rocksdb_snapshot_t*)*/ IntPtr snapshot);
public unsafe abstract void rocksdb_readoptions_set_iterate_upper_bound(
    /*(rocksdb_readoptions_t*)*/ IntPtr read_options, /*const*/ byte* key, UIntPtr keylen);
public abstract void rocksdb_readoptions_set_iterate_upper_bound(
    /*(rocksdb_readoptions_t*)*/ IntPtr read_options, /*const*/ IntPtr key, UIntPtr keylen);
public abstract void rocksdb_readoptions_set_read_tier(
    /*(rocksdb_readoptions_t*)*/ IntPtr read_options, int value);
public abstract void rocksdb_readoptions_set_tailing(
    /*(rocksdb_readoptions_t*)*/ IntPtr read_options, bool value);
public abstract void rocksdb_readoptions_set_readahead_size(
    /*(rocksdb_readoptions_t*)*/ IntPtr read_options, /*(size_t)*/ size_t size);
public abstract void rocksdb_readoptions_set_pin_data(
    /*(rocksdb_readoptions_t*)*/ IntPtr options, /*(unsigned char)*/ bool enable);
public abstract void rocksdb_readoptions_set_total_order_seek(
    /*(rocksdb_readoptions_t*)*/ IntPtr options, /*(unsigned char)*/ bool enable);

#endregion

#region Write options

public abstract /* rocksdb_writeoptions_t* */ IntPtr rocksdb_writeoptions_create();
public abstract void rocksdb_writeoptions_destroy(
    /*(rocksdb_writeoptions_t*)*/ IntPtr write_options);
public abstract void rocksdb_writeoptions_set_sync(
    /*(rocksdb_writeoptions_t*)*/ IntPtr write_options, bool value);
public abstract void rocksdb_writeoptions_disable_WAL(
    /*(rocksdb_writeoptions_t*)*/ IntPtr write_options, int disable);

#endregion

#region Compact range options
#if ROCKSDB_COMPACT_RANGE_OPTIONS
public abstract /*(rocksdb_compactoptions_t*)*/ IntPtr
rocksdb_compactoptions_create();
public abstract void rocksdb_compactoptions_destroy(
    /*(rocksdb_compactoptions_t*)*/ IntPtr options);
public abstract void
rocksdb_compactoptions_set_exclusive_manual_compaction(
    /*(rocksdb_compactoptions_t*)*/ IntPtr options, /*(unsigned char)*/ bool value);
public abstract void rocksdb_compactoptions_set_change_level(
    /*(rocksdb_compactoptions_t*)*/ IntPtr options, /*(unsigned char)*/ bool value);
public abstract void rocksdb_compactoptions_set_target_level(
    /*(rocksdb_compactoptions_t*)*/ IntPtr options, int value);
#endif
#endregion

#region Flush options
#if ROCKSDB_FLUSH_OPTIONS

public abstract /* rocksdb_flushoptions_t* */ IntPtr rocksdb_flushoptions_create();
public abstract void rocksdb_flushoptions_destroy(
    rocksdb_flushoptions_t*);
public abstract void rocksdb_flushoptions_set_wait(
    rocksdb_flushoptions_t*, unsigned char);

#endif
#endregion

#region Cache
#if ROCKSDB_CACHE

public abstract /* rocksdb_cache_t* */ IntPtr rocksdb_cache_create_lru(
    size_t capacity);
public abstract void rocksdb_cache_destroy(rocksdb_cache_t* cache);
public abstract void rocksdb_cache_set_capacity(
    rocksdb_cache_t* cache, size_t capacity);
public abstract /*(size_t)*/ ulong
rocksdb_cache_get_usage(/*(rocksdb_cache_t*)*/ IntPtr cache);
public abstract /*(size_t)*/ ulong
rocksdb_cache_get_pinned_usage(/*(rocksdb_cache_t*)*/ IntPtr cache);

#endif
#endregion

#region Env

public abstract /*(rocksdb_env_t*)*/ IntPtr rocksdb_create_default_env();
public abstract /*(rocksdb_env_t*)*/ IntPtr rocksdb_create_mem_env();
public abstract void rocksdb_env_set_background_threads(
    /*(rocksdb_env_t*)*/ IntPtr env, int n);
public abstract void rocksdb_env_set_high_priority_background_threads(/*(rocksdb_env_t*)*/ IntPtr env, int n);
public abstract void rocksdb_env_join_all_threads(
    /*(rocksdb_env_t*)*/ IntPtr env);
public abstract void rocksdb_env_destroy(/*(rocksdb_env_t*)*/ IntPtr env);

public abstract /*(rocksdb_envoptions_t*)*/ IntPtr rocksdb_envoptions_create();
public abstract void rocksdb_envoptions_destroy(
    /*(rocksdb_envoptions_t*)*/ IntPtr opt);
#endregion

#region SstFile

/* SstFile */

public abstract /*(rocksdb_sstfilewriter_t*)*/ IntPtr
rocksdb_sstfilewriter_create(/*(const rocksdb_envoptions_t*)*/ IntPtr env,
                             /*(const rocksdb_options_t*)*/ IntPtr io_options);
public abstract /*(rocksdb_sstfilewriter_t*)*/ IntPtr
rocksdb_sstfilewriter_create_with_comparator(
    /*(const rocksdb_envoptions_t*)*/ IntPtr env, /*(const rocksdb_options_t*)*/ IntPtr io_options,
    /*(const rocksdb_comparator_t*)*/ IntPtr comparator);
public abstract void rocksdb_sstfilewriter_open(
    /*(rocksdb_sstfilewriter_t*)*/ IntPtr writer, /*(const char*)*/ string name, /*(char** errptr)*/ out IntPtr errptr);
public abstract unsafe void rocksdb_sstfilewriter_add(
    /*(rocksdb_sstfilewriter_t*)*/ IntPtr writer, /*(const char*)*/ byte* key, /*(size_t)*/ size_t keylen,
    /*(const char*)*/ byte* val, /*(size_t)*/ size_t vallen, /*(char** errptr)*/ out IntPtr errptr);
public abstract void rocksdb_sstfilewriter_add(
    /*(rocksdb_sstfilewriter_t*)*/ IntPtr writer, /*(const char*)*/ byte[] key, /*(size_t)*/ size_t keylen,
    /*(const char*)*/ byte[] val, /*(size_t)*/ size_t vallen, /*(char** errptr)*/ out IntPtr errptr);
public abstract unsafe void rocksdb_sstfilewriter_put(
    /*(rocksdb_sstfilewriter_t*)*/ IntPtr writer, /*(const char*)*/ byte* key, size_t keylen,
    /*(const char*)*/ byte* val, size_t vallen, /*(char**)*/ out IntPtr errptr);
public abstract void rocksdb_sstfilewriter_put(
    /*(rocksdb_sstfilewriter_t*)*/ IntPtr writer, /*(const char*)*/ byte[] key, size_t keylen,
    /*(const char*)*/ byte[] val, size_t vallen, /*(char**)*/ out IntPtr errptr);
public abstract unsafe void rocksdb_sstfilewriter_merge(
    /*(rocksdb_sstfilewriter_t*)*/ IntPtr writer, /*(const char*)*/ byte* key, size_t keylen,
    /*(const char*)*/ byte* val, size_t vallen, /*(char**)*/ out IntPtr errptr);
public abstract void rocksdb_sstfilewriter_merge(
    /*(rocksdb_sstfilewriter_t*)*/ IntPtr writer, /*(const char*)*/ byte[] key, size_t keylen,
    /*(const char*)*/ byte[] val, size_t vallen, /*(char**)*/ out IntPtr errptr);
public abstract unsafe void rocksdb_sstfilewriter_delete(
    /*(rocksdb_sstfilewriter_t*)*/ IntPtr writer, /*(const char*)*/ byte* key, size_t keylen,
    /*(char**)*/ out IntPtr errptr);
public abstract void rocksdb_sstfilewriter_delete(
    /*(rocksdb_sstfilewriter_t*)*/ IntPtr writer, /*(const char*)*/ byte[] key, size_t keylen,
    /*(char**)*/ out IntPtr errptr);
public abstract void rocksdb_sstfilewriter_finish(
    /*(rocksdb_sstfilewriter_t*)*/ IntPtr writer, /*(char** errptr)*/ out IntPtr errptr);
public abstract void rocksdb_sstfilewriter_destroy(
    /*(rocksdb_sstfilewriter_t*)*/ IntPtr writer);

public abstract /*(rocksdb_ingestexternalfileoptions_t*)*/ IntPtr
rocksdb_ingestexternalfileoptions_create();
public abstract void
rocksdb_ingestexternalfileoptions_set_move_files(
    /*(rocksdb_ingestexternalfileoptions_t*)*/ IntPtr opt, bool move_files);
public abstract void
rocksdb_ingestexternalfileoptions_set_snapshot_consistency(
    /*(rocksdb_ingestexternalfileoptions_t*)*/ IntPtr opt,
    bool snapshot_consistency);
public abstract void
rocksdb_ingestexternalfileoptions_set_allow_global_seqno(
    /*(rocksdb_ingestexternalfileoptions_t*)*/ IntPtr opt, bool allow_global_seqno);
public abstract void
rocksdb_ingestexternalfileoptions_set_allow_blocking_flush(
    /*(rocksdb_ingestexternalfileoptions_t*)*/ IntPtr opt,
    bool allow_blocking_flush);
public abstract void rocksdb_ingestexternalfileoptions_destroy(
    /*(rocksdb_ingestexternalfileoptions_t*)*/ IntPtr opt);

public abstract void rocksdb_ingest_external_file(
    /*(rocksdb_t*)*/ IntPtr db, /*(const char* const*)*/ string[] file_list, /*(const size_t)*/ size_t list_len,
    /*(const rocksdb_ingestexternalfileoptions_t*)*/ IntPtr opt, /*(char** errptr)*/ out IntPtr errptr);
public abstract void rocksdb_ingest_external_file_cf(
    /*(rocksdb_t*)*/ IntPtr db, /*(rocksdb_column_family_handle_t*)*/ IntPtr handle,
    /*(const char* const*)*/ string[] file_list, /*(const size_t)*/ size_t list_len,
    /*(const rocksdb_ingestexternalfileoptions_t*)*/ IntPtr opt, /*(char** errptr)*/ out IntPtr errptr);
#endregion

#region SliceTransform
#if ROCKSDB_SLICETRANSFORM_CUSTOM
public abstract /* rocksdb_slicetransform_t* */ IntPtr rocksdb_slicetransform_create(
    void* state, void (*destructor)(void*),
    char* (*transform)(void*, /*const*/ byte* key, ulong length,
                       size_t* dst_length),
    unsigned char (*in_domain)(void*, /*const*/ byte* key, ulong length),
    unsigned char (*in_range)(void*, /*const*/ byte* key, ulong length),
    const char* (*name)(void*));
#endif
public abstract /* rocksdb_slicetransform_t* */ IntPtr rocksdb_slicetransform_create_fixed_prefix(/*(size_t)*/ size_t fixed_prefix_length);
public abstract /* rocksdb_slicetransform_t* */ IntPtr rocksdb_slicetransform_create_noop();
public abstract void rocksdb_slicetransform_destroy(
    /*(rocksdb_slicetransform_t*)*/ IntPtr slicetransform);

#endregion

#region Universal Compaction options
#if ROCKSDB_UNIVERSAL_COMPACTION_OPTIONS

enum {
  rocksdb_similar_size_compaction_stop_style = 0,
  rocksdb_total_size_compaction_stop_style = 1
};

public abstract /* rocksdb_universal_compaction_options_t* */ IntPtr rocksdb_universal_compaction_options_create();
public abstract void rocksdb_universal_compaction_options_set_size_ratio(
    rocksdb_universal_compaction_options_t*, int);
public abstract void rocksdb_universal_compaction_options_set_min_merge_width(
    rocksdb_universal_compaction_options_t*, int);
public abstract void rocksdb_universal_compaction_options_set_max_merge_width(
    rocksdb_universal_compaction_options_t*, int);
public abstract void rocksdb_universal_compaction_options_set_max_size_amplification_percent(
    rocksdb_universal_compaction_options_t*, int);
public abstract void rocksdb_universal_compaction_options_set_compression_size_percent(
    rocksdb_universal_compaction_options_t*, int);
public abstract void rocksdb_universal_compaction_options_set_stop_style(
    rocksdb_universal_compaction_options_t*, int);
public abstract void rocksdb_universal_compaction_options_destroy(
    rocksdb_universal_compaction_options_t*);

public abstract /* rocksdb_fifo_compaction_options_t* */ IntPtr rocksdb_fifo_compaction_options_create();
public abstract void rocksdb_fifo_compaction_options_set_max_table_files_size(
    rocksdb_fifo_compaction_options_t* fifo_opts, uint64_t size);
public abstract void rocksdb_fifo_compaction_options_destroy(
    rocksdb_fifo_compaction_options_t* fifo_opts);

public abstract int rocksdb_livefiles_count(
    const rocksdb_livefiles_t*);
public abstract /* const char* */ IntPtr rocksdb_livefiles_name(
    const rocksdb_livefiles_t*, int index);
public abstract int rocksdb_livefiles_level(
    const rocksdb_livefiles_t*, int index);
public abstract /* size_t */ ulong rocksdb_livefiles_size(const rocksdb_livefiles_t*, int index);
public abstract /* const char* */ IntPtr rocksdb_livefiles_smallestkey(
    const rocksdb_livefiles_t*, int index, size_t* size);
public abstract /* const char* */ IntPtr rocksdb_livefiles_largestkey(
    const rocksdb_livefiles_t*, int index, size_t* size);
public abstract void rocksdb_livefiles_destroy(
    const rocksdb_livefiles_t*);

#endif
#endregion

#region Utility Helpers
#if ROCKSDB_UTILITY_HELPERS

public abstract void rocksdb_get_options_from_string(
    /* const rocksdb_options_t* */ IntPtr base_options, const char* opts_str,
    /* rocksdb_options_t* */ IntPtr new_options, out IntPtr errptr);
public abstract void rocksdb_delete_file_in_range(
    /* rocksdb_t* */ IntPtr db, const char* start_key, size_t start_key_len,
    const char* limit_key, size_t limit_key_len, char** errptr);

public abstract void rocksdb_delete_file_in_range_cf(
    /* rocksdb_t* */ IntPtr db, rocksdb_column_family_handle_t* column_family,
    const char* start_key, size_t start_key_len, const char* limit_key,
    size_t limit_key_len, char** errptr);


#endif
#endregion

#region Transactions
#if ROCKSDB_TRANSACTIONS
/* Transactions */

public abstract /*(rocksdb_column_family_handle_t*)*/ IntPtr
rocksdb_transactiondb_create_column_family(
    rocksdb_transactiondb_t* txn_db,
    const rocksdb_options_t* column_family_options,
    const char* column_family_name, char** errptr);

public abstract /*(rocksdb_transactiondb_t*)*/ IntPtr rocksdb_transactiondb_open(
    const rocksdb_options_t* options,
    const rocksdb_transactiondb_options_t* txn_db_options, const char* name,
    char** errptr);

public abstract const rocksdb_snapshot_t*
rocksdb_transactiondb_create_snapshot(rocksdb_transactiondb_t* txn_db);

public abstract void rocksdb_transactiondb_release_snapshot(
    rocksdb_transactiondb_t* txn_db, const rocksdb_snapshot_t* snapshot);

public abstract /*(rocksdb_transaction_t*)*/ IntPtr rocksdb_transaction_begin(
    rocksdb_transactiondb_t* txn_db,
    const rocksdb_writeoptions_t* write_options,
    const rocksdb_transaction_options_t* txn_options,
    rocksdb_transaction_t* old_txn);

public abstract void rocksdb_transaction_commit(
    rocksdb_transaction_t* txn, char** errptr);

public abstract void rocksdb_transaction_rollback(
    rocksdb_transaction_t* txn, char** errptr);

public abstract void rocksdb_transaction_destroy(
    rocksdb_transaction_t* txn);

// This snapshot should be freed using rocksdb_free
public abstract const rocksdb_snapshot_t*
rocksdb_transaction_get_snapshot(rocksdb_transaction_t* txn);

public abstract /*(char*)*/ IntPtr rocksdb_transaction_get(
    rocksdb_transaction_t* txn, const rocksdb_readoptions_t* options,
    const char* key, size_t klen, size_t* vlen, char** errptr);

public abstract /*(char*)*/ IntPtr rocksdb_transaction_get_cf(
    rocksdb_transaction_t* txn, const rocksdb_readoptions_t* options,
    rocksdb_column_family_handle_t* column_family, const char* key, size_t klen,
    size_t* vlen, char** errptr);

public abstract /*(char*)*/ IntPtr rocksdb_transaction_get_for_update(
    rocksdb_transaction_t* txn, const rocksdb_readoptions_t* options,
    const char* key, size_t klen, size_t* vlen, unsigned char exclusive,
    char** errptr);

public abstract /*(char*)*/ IntPtr rocksdb_transactiondb_get(
    rocksdb_transactiondb_t* txn_db, const rocksdb_readoptions_t* options,
    const char* key, size_t klen, size_t* vlen, char** errptr);

public abstract /*(char*)*/ IntPtr rocksdb_transactiondb_get_cf(
    rocksdb_transactiondb_t* txn_db, const rocksdb_readoptions_t* options,
    rocksdb_column_family_handle_t* column_family, const char* key,
    size_t keylen, size_t* vallen, char** errptr);

public abstract void rocksdb_transaction_put(
    rocksdb_transaction_t* txn, const char* key, size_t klen, const char* val,
    size_t vlen, char** errptr);

public abstract void rocksdb_transaction_put_cf(
    rocksdb_transaction_t* txn, rocksdb_column_family_handle_t* column_family,
    const char* key, size_t klen, const char* val, size_t vlen, char** errptr);

public abstract void rocksdb_transactiondb_put(
    rocksdb_transactiondb_t* txn_db, const rocksdb_writeoptions_t* options,
    const char* key, size_t klen, const char* val, size_t vlen, char** errptr);

public abstract void rocksdb_transactiondb_put_cf(
    rocksdb_transactiondb_t* txn_db, const rocksdb_writeoptions_t* options,
    rocksdb_column_family_handle_t* column_family, const char* key,
    size_t keylen, const char* val, size_t vallen, char** errptr);

public abstract void rocksdb_transactiondb_write(
    rocksdb_transactiondb_t* txn_db, const rocksdb_writeoptions_t* options,
    rocksdb_writebatch_t *batch, char** errptr);

public abstract void rocksdb_transaction_merge(
    rocksdb_transaction_t* txn, const char* key, size_t klen, const char* val,
    size_t vlen, char** errptr);

public abstract void rocksdb_transactiondb_merge(
    rocksdb_transactiondb_t* txn_db, const rocksdb_writeoptions_t* options,
    const char* key, size_t klen, const char* val, size_t vlen, char** errptr);

public abstract void rocksdb_transaction_delete(
    rocksdb_transaction_t* txn, const char* key, size_t klen, char** errptr);

public abstract void rocksdb_transaction_delete_cf(
    rocksdb_transaction_t* txn, rocksdb_column_family_handle_t* column_family,
    const char* key, size_t klen, char** errptr);

public abstract void rocksdb_transactiondb_delete(
    rocksdb_transactiondb_t* txn_db, const rocksdb_writeoptions_t* options,
    const char* key, size_t klen, char** errptr);

public abstract void rocksdb_transactiondb_delete_cf(
    rocksdb_transactiondb_t* txn_db, const rocksdb_writeoptions_t* options,
    rocksdb_column_family_handle_t* column_family, const char* key,
    size_t keylen, char** errptr);

public abstract /*(rocksdb_iterator_t*)*/ IntPtr
rocksdb_transaction_create_iterator(rocksdb_transaction_t* txn,
                                    const rocksdb_readoptions_t* options);

public abstract /*(rocksdb_iterator_t*)*/ IntPtr
rocksdb_transactiondb_create_iterator(rocksdb_transactiondb_t* txn_db,
                                      const rocksdb_readoptions_t* options);

public abstract void rocksdb_transactiondb_close(
    rocksdb_transactiondb_t* txn_db);

public abstract /*(rocksdb_checkpoint_t*)*/ IntPtr
rocksdb_transactiondb_checkpoint_object_create(rocksdb_transactiondb_t* txn_db,
                                               char** errptr);

public abstract /*(rocksdb_optimistictransactiondb_t*)*/ IntPtr
rocksdb_optimistictransactiondb_open(const rocksdb_options_t* options,
                                     const char* name, char** errptr);

public abstract /*(rocksdb_transaction_t*)*/ IntPtr
rocksdb_optimistictransaction_begin(
    rocksdb_optimistictransactiondb_t* otxn_db,
    const rocksdb_writeoptions_t* write_options,
    const rocksdb_optimistictransaction_options_t* otxn_options,
    rocksdb_transaction_t* old_txn);

public abstract void rocksdb_optimistictransactiondb_close(
    rocksdb_optimistictransactiondb_t* otxn_db);

/* Transaction Options */

public abstract /*(rocksdb_transactiondb_options_t*)*/ IntPtr
rocksdb_transactiondb_options_create();

public abstract void rocksdb_transactiondb_options_destroy(
    rocksdb_transactiondb_options_t* opt);

public abstract void rocksdb_transactiondb_options_set_max_num_locks(
    rocksdb_transactiondb_options_t* opt, int64_t max_num_locks);

public abstract void rocksdb_transactiondb_options_set_num_stripes(
    rocksdb_transactiondb_options_t* opt, size_t num_stripes);

public abstract void
rocksdb_transactiondb_options_set_transaction_lock_timeout(
    rocksdb_transactiondb_options_t* opt, int64_t txn_lock_timeout);

public abstract void
rocksdb_transactiondb_options_set_default_lock_timeout(
    rocksdb_transactiondb_options_t* opt, int64_t default_lock_timeout);

public abstract /*(rocksdb_transaction_options_t*)*/ IntPtr
rocksdb_transaction_options_create();

public abstract void rocksdb_transaction_options_destroy(
    rocksdb_transaction_options_t* opt);

public abstract void rocksdb_transaction_options_set_set_snapshot(
    rocksdb_transaction_options_t* opt, unsigned char v);

public abstract void rocksdb_transaction_options_set_deadlock_detect(
    rocksdb_transaction_options_t* opt, unsigned char v);

public abstract void rocksdb_transaction_options_set_lock_timeout(
    rocksdb_transaction_options_t* opt, int64_t lock_timeout);

public abstract void rocksdb_transaction_options_set_expiration(
    rocksdb_transaction_options_t* opt, int64_t expiration);

public abstract void
rocksdb_transaction_options_set_deadlock_detect_depth(
    rocksdb_transaction_options_t* opt, int64_t depth);

public abstract void
rocksdb_transaction_options_set_max_write_batch_size(
    rocksdb_transaction_options_t* opt, size_t size);


public abstract /*(rocksdb_optimistictransaction_options_t*)*/ IntPtr
rocksdb_optimistictransaction_options_create();

public abstract void rocksdb_optimistictransaction_options_destroy(
    rocksdb_optimistictransaction_options_t* opt);

public abstract void
rocksdb_optimistictransaction_options_set_set_snapshot(
    rocksdb_optimistictransaction_options_t* opt, unsigned char v);

#endif
#endregion

// referring to convention (3), this should be used by client
// to free memory that was malloc()ed
public abstract void rocksdb_free(IntPtr ptr);

public abstract unsafe /*(rocksdb_pinnableslice_t*)*/ IntPtr rocksdb_get_pinned(
    /*(rocksdb_t*)*/ IntPtr db, /*(const rocksdb_readoptions_t*)*/ IntPtr options, /*(const char*)*/ byte* key,
    size_t keylen, /*(char**)*/ out IntPtr errptr);
public abstract /*(rocksdb_pinnableslice_t*)*/ IntPtr rocksdb_get_pinned(
    /*(rocksdb_t*)*/ IntPtr db, /*(const rocksdb_readoptions_t*)*/ IntPtr options, /*(const char*)*/ byte[] key,
    size_t keylen, /*(char**)*/ out IntPtr errptr);
public abstract unsafe /*(rocksdb_pinnableslice_t*)*/ IntPtr rocksdb_get_pinned_cf(
    /*(rocksdb_t*)*/ IntPtr db, /*(const rocksdb_readoptions_t*)*/ IntPtr options,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*(const char*)*/ byte* key,
    size_t keylen, /*(char**)*/ out IntPtr errptr);
public abstract /*(rocksdb_pinnableslice_t*)*/ IntPtr rocksdb_get_pinned_cf(
    /*(rocksdb_t*)*/ IntPtr db, /*(const rocksdb_readoptions_t*)*/ IntPtr options,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*(const char*)*/ byte[] key,
    size_t keylen, /*(char**)*/ out IntPtr errptr);
public abstract void rocksdb_pinnableslice_destroy(
    /*(rocksdb_pinnableslice_t*)*/ IntPtr v);
public abstract unsafe /*(const char*)*/ byte* rocksdb_pinnableslice_value(
    /*(const rocksdb_pinnableslice_t*)*/ IntPtr t, size_t* vlen);
public abstract /*(const char*)*/ IntPtr rocksdb_pinnableslice_value(
    /*(const rocksdb_pinnableslice_t*)*/ IntPtr t, /*(size_t*)*/ ref size_t vlen);

/* END c.h */
}
}

