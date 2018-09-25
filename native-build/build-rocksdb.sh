#!/bin/bash
# WINDOWS:
#   If you are in Windows, this is designed to be run from git bash
#     You therefore should install git bash, Visual Studio 2015, and cmake
#     Your best bet in Windows is to open a Developer Command Prompt and then run bash from there.
# MAC:
#   You will need snappy (must build: homebrew version is not universal)
#     brew install automake
#     brew install libtool
#     git clone git@github.com:google/snappy.git
#     cd snappy
#     ./autogen.sh
#     ./configure --prefix=/usr/local CFLAGS="-arch i386 -arch x86_64" CXXFLAGS="-arch i386 -arch x86_64" LDFLAGS="-arch i386 -arch x86_64" --disable-dependency-tracking
#     make
#     sudo make install
#
# Instructions for upgrading rocksdb version
# 1. Fetch the desired version locally with something like:
#    cd native-build/rocksdb
#    git fetch https://github.com/facebook/rocksdb.git v4.13
#    git checkout FETCH_HEAD
#    git push -f warrenfalk HEAD:rocksdb_sharp
# 2. Get the hash of the commit for the version and replace below
# 3. Also see instructions for modifying Native.Raw.cs with updates to c.h since current revisions
# 4. Push the desired version to the rocksdb_sharp branch at https://github.com/warrenfalk/rocksdb
# 5. Search through code for old hash and old version number and replace
# 6. Run this script to build (see README.md for more info)

#ROCKSDBVERSION=6e0597951
ROCKSDBVERSION=tags/v5.15.10
ROCKSDBVNUM=5.15.10
ROCKSDBSHARPVNUM=5.15.10.10
SNAPPYVERSION=37aafc9e

ROCKSDBREMOTE=https://github.com/facebook/rocksdb
SNAPPYREMOTE=https://github.com/warrenfalk/snappy

CONCURRENCY=8

fail() {
	echo "FAILURE"
	>&2 echo -e "\033[1;31m$1\033[0m"
}

warn() {
	>&2 echo -e "\033[1;33m$1\033[0m"
}

run_rocksdb_test() {
	NAME=$1
	echo "Running test \"${NAME}\":"
	cmd //c "build\\Debug\\${NAME}.exe" || fail "Test failed"
}

checkout() {
	NAME="$1"
	REMOTE="$2"
	VERSION="$3"
	FETCHREF="$4"
	test -d .git || git init
	test -d .git || fail "unable to initialize $NAME repository"
	git fetch "$REMOTE" "${FETCHREF}" || fail "Unable to fetch latest ${FETCHREF} from {$REMOTE}"
	git checkout "$VERSION" || fail "Unable to checkout $NAME version ${VERSION}"
}

update_vcxproj(){
	echo "Patching vcxproj for static vc runtime"
	/bin/find . -type f -name '*.vcxproj' -exec sed -i 's/MultiThreadedDLL/MultiThreaded/g; s/MultiThreadedDebugDLL/MultiThreadedDebug/g' '{}' ';'
}

BASEDIR=$(dirname "$0")
OSINFO=$(uname)

if [[ $OSINFO == *"MSYS"* || $OSINFO == *"MINGW"* ]]; then
	echo "Detected Windows (MSYS)..."
	# Make sure git is installed
	hash git 2> /dev/null || { fail "Build requires git"; }
	test -z "$WindowsSdkDir" && fail "This must be run from a build environment such as the Developer Command Prompt"

	BASEDIRWIN=$(cd "${BASEDIR}" && pwd -W)

	mkdir -p snappy || fail "unable to create snappy directory"
	(cd snappy && {
		checkout "snappy" "$SNAPPYREMOTE" "$SNAPPYVERSION" "cmake"
		mkdir -p build
		(cd build && {
			cmake -G "Visual Studio 14 2015 Win64" .. || fail "Running cmake on snappy failed"
			update_vcxproj || warn "unable to patch snappy for static vc runtime"
		}) || fail "cmake build generation failed"

		test -z "$RUNTESTS" || {
			cmd //c "msbuild build/snappy.sln /p:Configuration=Debug /m:$CONCURRENCY" || fail "Build of snappy (debug config) failed"
		}
		cmd //c "msbuild build/snappy.sln /p:Configuration=Release /m:$CONCURRENCY" || fail "Build of snappy failed"
	}) || fail "Snappy build failed"


	#mkdir -p rocksdb || fail "unable to create rocksdb directory"
	rm -rf rocksdb
	git clone "$ROCKSDBREMOTE" || fail "unable to clone rocksdb"
	(cd rocksdb && {
		
		git checkout "$ROCKSDBVERSION"
		#checkout "rocksdb" "$ROCKSDBREMOTE" "$ROCKSDBVERSION" "rocksdb_sharp"

		git checkout -- thirdparty.inc
		patch -N < ../rocksdb.thirdparty.inc.patch || warn "Patching of thirdparty.inc failed"
		rm -f thirdparty.inc.rej thirdparty.inc.orig

		# The following was necessary to get it to stop trying to build the tools
		# which doesn't work because of relative paths to GFLAGS library
		git checkout -- CMakeLists.txt
		patch -N < ../rocksdb.nobuildtools.patch || warn "Patching of CMakeLists.txt failed"
		rm -f CMakeLists.txt.rej CMakeLists.txt.orig

		sed -i 's/\/MD/\/MT/g' CMakeLists.txt

		mkdir -p build
		(cd build && {
			cmake -G "Visual Studio 14 2015 Win64" -DOPTDBG=1 -DGFLAGS=0 -DSNAPPY=1 -DPORTABLE=1 -DWITH_AVX2=0 .. || fail "Running cmake failed"
			update_vcxproj || warn "failed to patch vcxproj files for static vc runtime"
		}) || fail "cmake build generation failed"

		export TEST_TMPDIR=$(cmd //c "echo %TMP%")

		# TODO: build debug version first and run tests
		test -z "$RUNTESTS" || {
			cmd //c "msbuild build/rocksdb.sln /p:Configuration=Debug /m:$CONCURRENCY" || fail "Rocksdb debug build failed"
			run_rocksdb_test db_test
			run_rocksdb_test db_iter_test
			run_rocksdb_test db_log_iter_test
			run_rocksdb_test db_compaction_filter_test
			run_rocksdb_test db_compaction_test
			run_rocksdb_test db_dynamic_level_test
			run_rocksdb_test db_inplace_update_test
			run_rocksdb_test db_tailing_iter_test
			run_rocksdb_test db_universal_compaction_test
			run_rocksdb_test db_wal_test
			run_rocksdb_test db_table_properties_test
			run_rocksdb_test block_hash_index_test
			run_rocksdb_test autovector_test
			run_rocksdb_test column_family_test
			run_rocksdb_test table_properties_collector_test
			run_rocksdb_test arena_test
			run_rocksdb_test auto_roll_logger_test
			run_rocksdb_test block_test
			run_rocksdb_test bloom_test
			run_rocksdb_test dynamic_bloom_test
			run_rocksdb_test c_test
			run_rocksdb_test cache_test
			run_rocksdb_test checkpoint_test
			run_rocksdb_test coding_test
			run_rocksdb_test corruption_test
			run_rocksdb_test crc32c_test
			run_rocksdb_test slice_transform_test
			run_rocksdb_test dbformat_test
			run_rocksdb_test env_test
			run_rocksdb_test fault_injection_test
			run_rocksdb_test filelock_test
			run_rocksdb_test filename_test
			run_rocksdb_test file_reader_writer_test
			run_rocksdb_test block_based_filter_block_test
			run_rocksdb_test full_filter_block_test
			run_rocksdb_test histogram_test
			run_rocksdb_test inlineskiplist_test
			run_rocksdb_test log_test
			run_rocksdb_test manual_compaction_test
			run_rocksdb_test memenv_test
			run_rocksdb_test mock_env_test
			run_rocksdb_test memtable_list_test
			run_rocksdb_test merge_helper_test
			run_rocksdb_test memory_test
			run_rocksdb_test merge_test
			run_rocksdb_test merger_test
			run_rocksdb_test options_file_test
			run_rocksdb_test redis_lists_test
			run_rocksdb_test reduce_levels_test
			run_rocksdb_test plain_table_db_test
			run_rocksdb_test comparator_db_test
			run_rocksdb_test prefix_test
			run_rocksdb_test skiplist_test
			run_rocksdb_test stringappend_test
			run_rocksdb_test ttl_test
			run_rocksdb_test backupable_db_test
			run_rocksdb_test document_db_test
			run_rocksdb_test json_document_test
			run_rocksdb_test spatial_db_test
			run_rocksdb_test version_edit_test
			run_rocksdb_test version_set_test
			run_rocksdb_test compaction_picker_test
			run_rocksdb_test version_builder_test
			run_rocksdb_test file_indexer_test
			run_rocksdb_test write_batch_test
			run_rocksdb_test write_batch_with_index_test
			run_rocksdb_test write_controller_test
			run_rocksdb_test deletefile_test
			run_rocksdb_test table_test
			run_rocksdb_test thread_local_test
			run_rocksdb_test geodb_test
			run_rocksdb_test rate_limiter_test
			run_rocksdb_test delete_scheduler_test
			run_rocksdb_test options_test
			run_rocksdb_test options_util_test
			run_rocksdb_test event_logger_test
			run_rocksdb_test cuckoo_table_builder_test
			run_rocksdb_test cuckoo_table_reader_test
			run_rocksdb_test cuckoo_table_db_test
			run_rocksdb_test flush_job_test
			run_rocksdb_test wal_manager_test
			run_rocksdb_test listener_test
			run_rocksdb_test compaction_iterator_test
			run_rocksdb_test compaction_job_test
			run_rocksdb_test thread_list_test
			run_rocksdb_test sst_dump_test
			run_rocksdb_test compact_files_test
			run_rocksdb_test perf_context_test
			run_rocksdb_test optimistic_transaction_test
			run_rocksdb_test write_callback_test
			run_rocksdb_test heap_test
			run_rocksdb_test compact_on_deletion_collector_test
			run_rocksdb_test compaction_job_stats_test
			run_rocksdb_test transaction_test
			run_rocksdb_test ldb_cmd_test
		}
		cmd //c "msbuild build/rocksdb.sln /p:Configuration=Release /m:$CONCURRENCY" || fail "Rocksdb release build failed"
		git checkout -- thirdparty.inc
		mkdir -p ../../native/amd64 && cp -v ./build/Release/rocksdb_shared.dll ../../native/amd64/rocksdb.dll
		mkdir -p ../../native-${ROCKSDBVERSION}/amd64 && cp -v ./build/Release/rocksdb.dll ../../native-${ROCKSDBVERSION}/amd64/rocksdb.dll
	}) || fail "rocksdb build failed"
elif [[ $OSDETECT == *"Darwin"* ]]; then
	fail "Mac OSX build is not yet operational"
else
	echo "Assuming a posix-like environment"
	if [ "$(uname)" == "Darwin" ]; then
		echo "Mac (Darwin) detected"
		CFLAGS=-I/usr/local/include
		LDFLAGS="-L/usr/local/lib"
		LIBEXT=.dylib
	else
		CFLAGS=-static-libstdc++
		LIBEXT=.so
	fi
	# Linux Dependencies
	#sudo apt-get install gcc-5-multilib g++-5-multilib
	#sudo apt-get install libsnappy-dev:i386 libbz2-dev:i386 libsnappy-dev libbz2-dev

	# Mac Dependencies
	#brew install snappy --universal
    # Don't have universal version of lz4 through brew, have to build manually
	# git clone git@github.com:Cyan4973/lz4.git
	# make -C lz4/lib
	# cp -L lz4/lib/liblz4.dylib ./liblz4_64.dylib
	# make -C lz4/lib clean
	# CFLAGS="-arch i386" CXXFLAGS="-arch i386" LDFLAGS="-arch i386" make -C lz4/lib
	# cp -L lz4/lib/liblz4.dylib ./liblz4_32.dylib
	# lipo -create ./liblz4_32.dylib ./liblz4_64.dylib -output ./liblz4.dylib
	# cp -v ./liblz4.dylib lz4/lib/$(readlink lz4/lib/liblz4.dylib)
	# touch lz4/lib/liblz4
	# make -C lz4/lib install


	mkdir -p rocksdb || fail "unable to create rocksdb directory"
	(cd rocksdb && {
		checkout "rocksdb" "$ROCKSDBREMOTE" "$ROCKSDBVERSION" "rocksdb_sharp"

		export CFLAGS
		export LDFLAGS
		(. ./build_tools/build_detect_platform detected~; {
			grep detected~ -e '-DSNAPPY' &> /dev/null || fail "failed to detect snappy, install libsnappy-dev"
			grep detected~ -e '-DZLIB' &> /dev/null || fail "failed to detect zlib, install libzlib-dev"
			grep detected~ -e '-DGFLAGS' &> /dev/null && fail "gflags detected, see https://github.com/facebook/rocksdb/issues/2310" || true
		}) || fail "dependency detection failed"
		echo "----- Build 64 bit --------------------------------------------------"
		make clean
		CFLAGS="${CFLAGS}" PORTABLE=1 make -j$CONCURRENCY shared_lib || fail "64-bit build failed"
		strip librocksdb${LIBEXT}
		mkdir -p ../../native/amd64 && cp -vL ./librocksdb${LIBEXT} ../../native/amd64/librocksdb${LIBEXT}
		#mkdir -p ../../native-${ROCKSDBVERSION}/amd64 && cp -vL ./librocksdb${LIBEXT} ../../native-${ROCKSDBVERSION}/amd64/librocksdb${LIBEXT}
		echo "----- Build 32 bit --------------------------------------------------"
		make clean
		CFLAGS="${CFLAGS} -m32" PORTABLE=1 make -j$CONCURRENCY shared_lib || fail "32-bit build failed"
		strip librocksdb${LIBEXT}
		mkdir -p ../../native/i386 && cp -vL ./librocksdb${LIBEXT} ../../native/i386/librocksdb${LIBEXT}
		mkdir -p ../../native-${ROCKSDBVERSION}/i386 && cp -vL ./librocksdb${LIBEXT} ../../native-${ROCKSDBVERSION}/i386/librocksdb${LIBEXT}


	}) || fail "rocksdb build failed"
fi


echo -n ${ROCKSDBSHARPVNUM} > ${BASEDIR}/../version
echo -n ${ROCKSDBVNUM} > ${BASEDIR}/../rocksdbversion

