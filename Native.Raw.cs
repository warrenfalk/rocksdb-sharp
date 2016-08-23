/*
    This file is the lowest level interface between the unmanged RocksDB C API and managed code.
    This is the lowest level access exposed by this library, and probably the lowest level possible.

    Most of this file derives directly from the C API header exported by RocksDB.
    In particular, it was originally derived from version e70aabc3
    https://github.com/facebook/rocksdb/blob/e70aabc3/include/rocksdb/c.h
    And this should be treated as an ongoing "port" of that file into idomatic C#.
    Changes to c.h should be incorporated here.  View those changes with:
    cd native-build/rocksdb && git diff e70aabc3 HEAD -- ./include/rocksdb/c.h
    And then once the changes are made, come back here and replace e70aabc3 with whatever HEAD is

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

namespace RocksDbSharp
{
//void (*put)(IntPtr s, /*(const char*)*/ IntPtr k, /*(size_t)*/ ulong klen, /*(const char*)*/ IntPtr v, /*(size_t)*/ ulong vlen),
public delegate void WriteBatchIteratePutCallback(IntPtr s, /*(const char*)*/ IntPtr k, /*(size_t)*/ ulong klen, /*(const char*)*/ IntPtr v, /*(size_t)*/ ulong vlen);
//void (*deleted)(void*, const char* k, /*(size_t)*/ ulong klen)
public delegate void WriteBatchIterateDeleteCallback(IntPtr s, /*(const char*)*/ IntPtr k, /*(size_t)*/ ulong klen);
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
typedef struct rocksdb_env_t             rocksdb_env_t;
typedef struct rocksdb_fifo_compaction_options_t rocksdb_fifo_compaction_options_t;
typedef struct rocksdb_filelock_t        rocksdb_filelock_t;
typedef struct rocksdb_filterpolicy_t    rocksdb_filterpolicy_t;
typedef struct rocksdb_flushoptions_t    rocksdb_flushoptions_t;
typedef struct rocksdb_iterator_t        rocksdb_iterator_t;
typedef struct rocksdb_logger_t          rocksdb_logger_t;
typedef struct rocksdb_mergeoperator_t   rocksdb_mergeoperator_t;
typedef struct rocksdb_options_t         rocksdb_options_t;
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
typedef struct rocksdb_writeoptions_t    rocksdb_writeoptions_t;
typedef struct rocksdb_universal_compaction_options_t rocksdb_universal_compaction_options_t;
typedef struct rocksdb_livefiles_t     rocksdb_livefiles_t;
typedef struct rocksdb_column_family_handle_t rocksdb_column_family_handle_t;

#endif
#endregion

#region DB operations
#if ROCKSDB_DB_OPERATIONS

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

public abstract /* rocksdb_t* */ IntPtr rocksdb_open_column_families(
    /* const rocksdb_options_t* */ IntPtr options, string name, int num_column_families,
    /*(const char**)*/ string[] column_family_names,
    /*(const rocksdb_options_t**)*/ IntPtr[] column_family_options,
    /*(rocksdb_column_family_handle_t**)*/ IntPtr[] column_family_handles, out IntPtr errptr);

public abstract /* rocksdb_t* */ IntPtr rocksdb_open_for_read_only_column_families(
    /* const rocksdb_options_t* */ IntPtr options, string name, int num_column_families,
    /*(const char**)*/ IntPtr column_family_names,
    /*(const rocksdb_options_t**)*/ IntPtr column_family_options,
    /*(rocksdb_column_family_handle_t**)*/ IntPtr column_family_handles,
    bool error_if_log_file_exist, out IntPtr errptr);

public abstract /* char** */ IntPtr rocksdb_list_column_families(
            /* const rocksdb_options_t* */ IntPtr options, string name, /*(size_t*)*/ out ulong lencf,
    out IntPtr errptr);

public abstract void rocksdb_list_column_families_destroy(
            /*(char**)*/ IntPtr list, ulong len);

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
    ulong keylen, /*const*/ byte* val, ulong vallen, out IntPtr errptr);

// "long" was chosen over "ulong" for the lengths below because long is all that is possible for clr arrays anyway
public abstract void rocksdb_put(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions, /*const*/ byte[] key,
    long keylen, /*const*/ byte[] val, long vallen, out IntPtr errptr);

public unsafe abstract void rocksdb_put_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*const*/ byte* key,
    ulong keylen, /*const*/ byte* val, ulong vallen, out IntPtr errptr);

public abstract void rocksdb_put_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*const*/ byte[] key,
    long keylen, /*const*/ byte[] val, long vallen, out IntPtr errptr);

public unsafe abstract void rocksdb_delete(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions, /*const*/ byte* key,
    ulong keylen, out IntPtr errptr);

public abstract void rocksdb_delete(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions, /*const*/ byte[] key,
    long keylen, out IntPtr errptr);

public unsafe abstract void rocksdb_delete_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*const*/ byte* key,
    long keylen, out IntPtr errptr);

public abstract void rocksdb_delete_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*const*/ byte[] key,
    long keylen, out IntPtr errptr);

public unsafe abstract void rocksdb_merge(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions, /*const*/ byte* key,
    ulong keylen, /*const*/ byte* val, ulong vallen, out IntPtr errptr);

public unsafe abstract void rocksdb_merge_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*const*/ byte* key,
    ulong keylen, /*const*/ byte* val, ulong vallen, out IntPtr errptr);

public abstract void rocksdb_write(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_writeoptions_t**/ IntPtr writeOptions,
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, out IntPtr errptr);

/* Returns NULL if not found.  A malloc()ed array otherwise.
   Stores the length of the array in *vallen. */
public unsafe abstract /* char* */ IntPtr rocksdb_get(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options, /*const*/ byte* key,
            long keylen, /*(size_t*)*/ out long vallen, out IntPtr errptr);

/* Returns NULL if not found.  A malloc()ed array otherwise.
   Stores the length of the array in *vallen. */
public unsafe abstract /* char* */ IntPtr rocksdb_get(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options, /*const*/ byte[] key,
            long keylen, /*(size_t*)*/ out long vallen, out IntPtr errptr);

public unsafe abstract /* char* */ IntPtr rocksdb_get_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*const*/ byte* key,
            long keylen, /*(size_t*)*/ out long vallen, out IntPtr errptr);

public unsafe abstract /* char* */ IntPtr rocksdb_get_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family, /*const*/ byte[] key,
            long keylen, /*(size_t*)*/ out long vallen, out IntPtr errptr);

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
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options, ulong num_keys,
    /*const char* const**/ IntPtr keys_list, /*const size_t**/ IntPtr keys_list_sizes,
            /*(char**)*/ IntPtr values_list, /*size_t**/ IntPtr values_list_sizes, /*(char**)*/ IntPtr errlist);

public abstract void rocksdb_multi_get_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options,
            /*(const rocksdb_column_family_handle_t* const*)*/ IntPtr column_families,
            ulong num_keys, /*(const char* const*)*/ IntPtr keys_list,
            /*(const size_t*)*/ IntPtr keys_list_sizes, /*(char**)*/ IntPtr values_list,
            /*(size_t*)*/ IntPtr values_list_sizes, /*(char**)*/ IntPtr errList);

public abstract /* rocksdb_iterator_t* */ IntPtr rocksdb_create_iterator(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options);

public abstract /* rocksdb_iterator_t* */ IntPtr rocksdb_create_iterator_cf(
    /*rocksdb_t**/ IntPtr db, /*const rocksdb_readoptions_t**/ IntPtr read_options,
    /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family);

public abstract void rocksdb_create_iterators(
    /*(rocksdb_t *)*/ IntPtr db, /*(rocksdb_readoptions_t*)*/ IntPtr opts,
    /*(rocksdb_column_family_handle_t**)*/ IntPtr column_families,
    /*(rocksdb_iterator_t**)*/ IntPtr iterators, /*(size_t)*/ ulong size, /*(char**)*/ out IntPtr errptr);

public abstract /* const rocksdb_snapshot_t* */ IntPtr rocksdb_create_snapshot(
    /*rocksdb_t**/ IntPtr db);

public abstract void rocksdb_release_snapshot(
            /*rocksdb_t**/ IntPtr db, /*(const rocksdb_snapshot_t*)*/ IntPtr snapshot);

/* Returns NULL if property name is unknown.
   Else returns a pointer to a malloc()-ed null-terminated value. */
public abstract /* char* */ IntPtr rocksdb_property_value(/*rocksdb_t**/ IntPtr db,
                                                        string propname);

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
              ulong start_key_len,
            /*(const char*)*/ byte* limit_key,
              ulong limit_key_len);

public unsafe abstract void rocksdb_compact_range_cf(
    /*rocksdb_t**/ IntPtr db, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*(const char*)*/ byte* start_key, ulong start_key_len, /*(const char*)*/ byte* limit_key,
    ulong limit_key_len);

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

#endif
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
#if ROCKSDB_ITERATOR

public abstract void rocksdb_iter_destroy(/*(rocksdb_iterator_t*)*/ IntPtr iter);
public abstract bool rocksdb_iter_valid(
    /*(const rocksdb_iterator_t*)*/ IntPtr iter);
public abstract void rocksdb_iter_seek_to_first(/*(rocksdb_iterator_t*)*/ IntPtr iter);
public abstract void rocksdb_iter_seek_to_last(/*(rocksdb_iterator_t*)*/ IntPtr iter);
public unsafe abstract void rocksdb_iter_seek(/*(rocksdb_iterator_t*)*/ IntPtr iter,
                                                  /*(const char*)*/ byte* k, /*(size_t)*/ ulong klen);
public abstract void rocksdb_iter_seek(/*(rocksdb_iterator_t*)*/ IntPtr iter,
                                                  /*(const char*)*/ byte[] k, /*(size_t)*/ ulong klen);
public abstract void rocksdb_iter_next(/*(rocksdb_iterator_t*)*/ IntPtr iter);
public abstract void rocksdb_iter_prev(/*(rocksdb_iterator_t*)*/ IntPtr iter);
public abstract /* const char* */ IntPtr rocksdb_iter_key(
    /*(const rocksdb_iterator_t*)*/ IntPtr iter, /*(size_t*)*/ out ulong klen);
public abstract /* const char* */ IntPtr rocksdb_iter_value(
    /*(const rocksdb_iterator_t*)*/ IntPtr iter, /*(size_t*)*/ out ulong vlen);
public abstract void rocksdb_iter_get_error(
    /*(const rocksdb_iterator_t*)*/ IntPtr iter, out IntPtr errptr);

#endif
#endregion

#region Write batch
#if ROCKSDB_WRITE_BATCH

public abstract /* rocksdb_writebatch_t* */ IntPtr rocksdb_writebatch_create();
public abstract /* rocksdb_writebatch_t* */ IntPtr rocksdb_writebatch_create_from(
    /*(const char*)*/ byte[] rep, /*(size_t)*/ long size);
public abstract void rocksdb_writebatch_destroy(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch);
public abstract void rocksdb_writebatch_clear(/*(rocksdb_writebatch_t*)*/ IntPtr writeBatch);
public abstract int rocksdb_writebatch_count(/*(rocksdb_writebatch_t*)*/ IntPtr writeBatch);
public abstract void rocksdb_writebatch_put(/*(rocksdb_writebatch_t*)*/ IntPtr writeBatch,
                                                       /*const*/ byte[] key,
                                                       /*(size_t)*/ ulong klen,
                                                       /*const*/ byte[] val,
                                                       /*(size_t)*/ ulong vlen);
public unsafe abstract void rocksdb_writebatch_put(/*(rocksdb_writebatch_t*)*/ IntPtr writeBatch,
                                                       /*const*/ byte* key,
                                                       /*(size_t)*/ ulong klen,
                                                       /*const*/ byte* val,
                                                       /*(size_t)*/ ulong vlen);
public abstract void rocksdb_writebatch_put_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*const*/ byte[] key, /*(size_t)*/ ulong klen, /*const*/ byte[] val, /*(size_t)*/ ulong vlen);
public unsafe abstract void rocksdb_writebatch_put_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*const*/ byte* key, /*(size_t)*/ ulong klen, /*const*/ byte* val, /*(size_t)*/ ulong vlen);
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
                                                         /*(size_t)*/ ulong klen,
                                                         /*const*/ byte[] val,
                                                         /*(size_t)*/ ulong vlen);
public unsafe abstract void rocksdb_writebatch_merge(/*(rocksdb_writebatch_t*)*/ IntPtr writeBatch,
                                                         /*const*/ byte* key,
                                                         /*(size_t)*/ ulong klen,
                                                         /*const*/ byte* val,
                                                         /*(size_t)*/ ulong vlen);
public abstract void rocksdb_writebatch_merge_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*const*/ byte[] key, /*(size_t)*/ ulong klen, /*const*/ byte[] val, /*(size_t)*/ ulong vlen);
public unsafe abstract void rocksdb_writebatch_merge_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*const*/ byte* key, /*(size_t)*/ ulong klen, /*const*/ byte* val, /*(size_t)*/ ulong vlen);
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
                                                          /*(size_t)*/ ulong klen);
public unsafe abstract void rocksdb_writebatch_delete(/*(rocksdb_writebatch_t*)*/ IntPtr writeBatch,
                                                          /*const*/ byte* key,
                                                          /*(size_t)*/ ulong klen);
public abstract void rocksdb_writebatch_delete_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*const*/ byte[] key, /*(size_t)*/ ulong klen);
public unsafe abstract void rocksdb_writebatch_delete_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    /*const*/ byte* key, /*(size_t)*/ ulong klen);
public abstract void rocksdb_writebatch_deletev(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, int num_keys, /*(const char* const*)*/ IntPtr keys_list,
    /*(const size_t*)*/ IntPtr keys_list_sizes);
public abstract void rocksdb_writebatch_deletev_cf(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(rocksdb_column_family_handle_t*)*/ IntPtr column_family,
    int num_keys, /*(const char* const*)*/ IntPtr keys_list, /*(const size_t*)*/ IntPtr keys_list_sizes);
public abstract void rocksdb_writebatch_put_log_data(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, byte[] blob, ulong len);
public abstract void rocksdb_writebatch_iterate(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(void*)*/ IntPtr state,
    //void (*put)(IntPtr s, /*(const char*)*/ IntPtr k, /*(size_t)*/ ulong klen, /*(const char*)*/ IntPtr v, /*(size_t)*/ ulong vlen),
    WriteBatchIteratePutCallback put,
    //void (*deleted)(void*, const char* k, /*(size_t)*/ ulong klen)
    WriteBatchIterateDeleteCallback deleted);
public abstract /* const char* */ IntPtr rocksdb_writebatch_data(
    /*(rocksdb_writebatch_t*)*/ IntPtr writeBatch, /*(size_t*)*/ ulong size);
#endif
#endregion

#region Block based table options
#if ROCKSDB_BLOCK_BASED_TABLE_OPTIONS

public abstract /* rocksdb_block_based_table_options_t* */ IntPtr rocksdb_block_based_options_create();
public abstract void rocksdb_block_based_options_destroy(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr options);
public abstract void rocksdb_block_based_options_set_block_size(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr options, /*(size_t)*/ ulong block_size);
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
public abstract void rocksdb_block_based_options_set_skip_table_builder_flush(
    /*(rocksdb_block_based_table_options_t*)*/ IntPtr options, /*(unsigned char)*/ bool skip_table_builder_flush);
public abstract void rocksdb_options_set_block_based_table_factory(
    /* rocksdb_options_t* */ IntPtr opt, /*(rocksdb_block_based_table_options_t*)*/ IntPtr table_options);
#endif
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
#if ROCKSDB_OPTIONS

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
    /* rocksdb_options_t* */ IntPtr options, /* size_t */ ulong size);
public abstract void rocksdb_options_set_comparator(
            /* rocksdb_options_t* */ IntPtr options, /*(rocksdb_comparator_t*)*/ IntPtr comparator);
public abstract void rocksdb_options_set_merge_operator(
            /* rocksdb_options_t* */ IntPtr options, /*(rocksdb_mergeoperator_t*)*/ IntPtr merge_operator);
public abstract void rocksdb_options_set_uint64add_merge_operator(
            /* rocksdb_options_t* */ IntPtr options);
public abstract void rocksdb_options_set_compression_per_level(
            /* rocksdb_options_t* */ IntPtr opt, /*(int*)*/ int[] level_values, ulong num_levels);
public abstract void rocksdb_options_set_create_if_missing(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_create_missing_column_families(/* rocksdb_options_t* */ IntPtr options,
            bool value);
public abstract void rocksdb_options_set_error_if_exists(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_paranoid_checks(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_env(/* rocksdb_options_t* */ IntPtr options,
            /*(rocksdb_env_t*)*/ IntPtr env);
public abstract void rocksdb_options_set_info_log(/* rocksdb_options_t* */ IntPtr options,
            /*(rocksdb_logger_t*)*/ IntPtr logger);
public abstract void rocksdb_options_set_info_log_level(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_write_buffer_size(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void rocksdb_options_set_db_write_buffer_size(
    /* rocksdb_options_t* */ IntPtr options, /* size_t */ ulong size);
public abstract void rocksdb_options_set_max_open_files(
    /* rocksdb_options_t* */ IntPtr options, int value);
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
public abstract void rocksdb_options_set_max_bytes_for_level_multiplier(/* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_expanded_compaction_factor(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_max_grandparent_overlap_factor(/* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_max_bytes_for_level_multiplier_additional(
            /* rocksdb_options_t* */ IntPtr options, /*(int*)*/ int[] level_values, ulong num_levels);
public abstract void rocksdb_options_enable_statistics(
    /* rocksdb_options_t* */ IntPtr options);

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
public abstract void rocksdb_options_set_max_background_flushes(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_max_log_file_size(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void rocksdb_options_set_log_file_time_to_roll(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void rocksdb_options_set_keep_log_file_num(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void rocksdb_options_set_recycle_log_file_num(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void rocksdb_options_set_soft_rate_limit(
            /* rocksdb_options_t* */ IntPtr options, double value);
public abstract void rocksdb_options_set_hard_rate_limit(
            /* rocksdb_options_t* */ IntPtr options, double value);
public abstract void rocksdb_options_set_rate_limit_delay_max_milliseconds(/* rocksdb_options_t* */ IntPtr options,
            uint value);
public abstract void rocksdb_options_set_max_manifest_file_size(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void rocksdb_options_set_table_cache_numshardbits(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_table_cache_remove_scan_count_limit(/* rocksdb_options_t* */ IntPtr options,
                                                        int value);
public abstract void rocksdb_options_set_arena_block_size(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
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
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void rocksdb_options_set_purge_redundant_kvs_while_flush(/* rocksdb_options_t* */ IntPtr options,
            bool value);
public abstract void rocksdb_options_set_allow_os_buffer(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_allow_mmap_reads(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_allow_mmap_writes(
            /* rocksdb_options_t* */ IntPtr options, bool value);
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
public abstract void rocksdb_options_set_verify_checksums_in_compaction(/* rocksdb_options_t* */ IntPtr options,
            bool value);
public abstract void rocksdb_options_set_filter_deletes(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_max_sequential_skip_in_iterations(/* rocksdb_options_t* */ IntPtr options,
            ulong value);
public abstract void rocksdb_options_set_disable_data_sync(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_disable_auto_compactions(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_set_delete_obsolete_files_period_micros(/* rocksdb_options_t* */ IntPtr options,
            ulong value);
public abstract void rocksdb_options_set_source_compaction_factor(
    /* rocksdb_options_t* */ IntPtr options, int value);
public abstract void rocksdb_options_prepare_for_bulk_load(
    /* rocksdb_options_t* */ IntPtr options);
public abstract void rocksdb_options_set_memtable_vector_rep(
    /* rocksdb_options_t* */ IntPtr options);
public abstract void rocksdb_options_set_hash_skip_list_rep(
            /* rocksdb_options_t* */ IntPtr options, ulong p1, int p2, int p3);
public abstract void rocksdb_options_set_hash_link_list_rep(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void rocksdb_options_set_plain_table_factory(
            /* rocksdb_options_t* */ IntPtr options, ulong p1, int p2, double p3, ulong p4);

public abstract void rocksdb_options_set_min_level_to_compress(
    /* rocksdb_options_t* */ IntPtr opt, int level);

public abstract void rocksdb_options_set_memtable_prefix_bloom_bits(
            /* rocksdb_options_t* */ IntPtr options, uint value);
public abstract void rocksdb_options_set_memtable_prefix_bloom_probes(/* rocksdb_options_t* */ IntPtr options, int value);

public abstract void rocksdb_options_set_memtable_prefix_bloom_huge_page_tlb_size(
    /* rocksdb_options_t* */ IntPtr options, /*(size_t)*/ ulong size);

public abstract void rocksdb_options_set_max_successive_merges(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void rocksdb_options_set_min_partial_merge_operands(
            /* rocksdb_options_t* */ IntPtr options, uint value);
public abstract void rocksdb_options_set_bloom_locality(
            /* rocksdb_options_t* */ IntPtr options, uint value);
public abstract void rocksdb_options_set_inplace_update_support(
            /* rocksdb_options_t* */ IntPtr options, bool value);
public abstract void rocksdb_options_set_inplace_update_num_locks(
            /* rocksdb_options_t* */ IntPtr options, ulong value);
public abstract void rocksdb_options_set_report_bg_io_stats(
            /* rocksdb_options_t* */ IntPtr options, int value);

}
public enum CompressionTypeEnum {
  rocksdb_no_compression = 0,
  rocksdb_snappy_compression = 1,
  rocksdb_zlib_compression = 2,
  rocksdb_bz2_compression = 3,
  rocksdb_lz4_compression = 4,
  rocksdb_lz4hc_compression = 5,
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
#if ROCKSDB_COMPARATOR

public abstract /* rocksdb_comparator_t* */ IntPtr rocksdb_comparator_create(
    void* state, void (*destructor)(void*),
    int (*compare)(void*, const char* a, size_t alen, const char* b,
                   size_t blen),
    const char* (*name)(void*));
public abstract void rocksdb_comparator_destroy(
    rocksdb_comparator_t*);

#endif
#endregion

#region Filter policy
#if ROCKSDB_FILTER_POLICY
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

#endif
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
#if ROCKSDB_READ_OPTIONS

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
    /*(rocksdb_readoptions_t*)*/ IntPtr read_options, /*const*/ byte* key, ulong keylen);
public abstract void rocksdb_readoptions_set_iterate_upper_bound(
    /*(rocksdb_readoptions_t*)*/ IntPtr read_options, /*const*/ byte[] key, ulong keylen);
public abstract void rocksdb_readoptions_set_read_tier(
    /*(rocksdb_readoptions_t*)*/ IntPtr read_options, int value);
public abstract void rocksdb_readoptions_set_tailing(
    /*(rocksdb_readoptions_t*)*/ IntPtr read_options, bool value);
public abstract void rocksdb_readoptions_set_readahead_size(
    /*(rocksdb_readoptions_t*)*/ IntPtr read_options, /*(size_t)*/ ulong size);

#endif
#endregion

#region Write options
#if ROCKSDB_WRITE_OPTIONS

public abstract /* rocksdb_writeoptions_t* */ IntPtr rocksdb_writeoptions_create();
public abstract void rocksdb_writeoptions_destroy(
    /*(rocksdb_writeoptions_t*)*/ IntPtr write_options);
public abstract void rocksdb_writeoptions_set_sync(
    /*(rocksdb_writeoptions_t*)*/ IntPtr write_options, bool value);
public abstract void rocksdb_writeoptions_disable_WAL(
    /*(rocksdb_writeoptions_t*)*/ IntPtr write_options, int disable);

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

#endif
#endregion

#region Env
#if ROCKSDB_ENV

public abstract /* rocksdb_env_t* */ IntPtr rocksdb_create_default_env();
public abstract rocksdb_env_t* rocksdb_create_mem_env();
public abstract void rocksdb_env_set_background_threads(
    rocksdb_env_t* env, int n);
public abstract void rocksdb_env_set_high_priority_background_threads(rocksdb_env_t* env, int n);
public abstract void rocksdb_env_join_all_threads(
    rocksdb_env_t* env);
public abstract void rocksdb_env_destroy(rocksdb_env_t*);

#endif
#endregion

#region SliceTransform
#if ROCKSDB_SLICETRANSFORM
#if ROCKSDB_SLICETRANSFORM_CUSTOM
public abstract /* rocksdb_slicetransform_t* */ IntPtr rocksdb_slicetransform_create(
    void* state, void (*destructor)(void*),
    char* (*transform)(void*, /*const*/ byte* key, ulong length,
                       size_t* dst_length),
    unsigned char (*in_domain)(void*, /*const*/ byte* key, ulong length),
    unsigned char (*in_range)(void*, /*const*/ byte* key, ulong length),
    const char* (*name)(void*));
#endif
public abstract /* rocksdb_slicetransform_t* */ IntPtr rocksdb_slicetransform_create_fixed_prefix(/*(size_t)*/ ulong fixed_prefix_length);
public abstract /* rocksdb_slicetransform_t* */ IntPtr rocksdb_slicetransform_create_noop();
public abstract void rocksdb_slicetransform_destroy(
    /*(rocksdb_slicetransform_t*)*/ IntPtr slicetransform);

#endif
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

// referring to convention (3), this should be used by client
// to free memory that was malloc()ed
public abstract void rocksdb_free(IntPtr ptr);

/* END c.h */
}
}

