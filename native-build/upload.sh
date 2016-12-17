#!/bin/bash

# The purpose of this script is to help to upload the binaries, once built, to the release folder
# It first collects them from the various build environments into a native-xxxxx folder
# Then zips that folder
# Then updloads the zip
# Collecting and uploading require privileged information, however, so it loads this from ~/.rocksdb-sharp-upload-info
export REVISION=$(cat ./build-rocksdb.sh | grep ROCKSDBVERSION | sed -n -e '/^ROCKSDBVERSION=/ s/ROCKSDBVERSION=\(.*\)/\1/p')
export VERSION=$(cat ../version)
export RDBVERSION=$(cat ../rocksdbversion)
echo "REVISION = ${REVISION}"

hash jq || { echo "jq is required, apt-get install jq"; exit 1; }

ROCKSDB_MAC_COPY="scp -r"
ROCKSDB_WINDOWS_COPY="scp -r"

. ~/.rocksdb-sharp-upload-info || echo "Failed to load collection and upload parameters"

cd $(dirname $0)

if [ ! -f ../native-${REVISION}/amd64/librocksdb.dylib ]; then
	${ROCKSDB_MAC_COPY} ${ROCKSDB_MAC}/native-${REVISION}/ ../
fi

if [ ! -f ../native-${REVISION}/amd64/rocksdb.dll ]; then
	${ROCKSDB_WINDOWS_COPY} ${ROCKSDB_WINDOWS}/native-${REVISION}/ ../
fi

echo "Contents:"
find ../native-${REVISION}

echo "Zipping..."
rm ../native-${REVISION}.zip
(cd ../native-${REVISION} && zip -r ../native-${REVISION}.zip ./)

echo "Creating Release..."
PAYLOAD="{\"tag_name\": \"v${VERSION}\", \"target_commitish\": \"master\", \"name\": \"v${VERSION}\", \"body\": \"RocksDbSharp v${VERSION} (rocksdb ${RDBVERSION})\", \"draft\": true, \"prelease\": false }"
echo ${PAYLOAD}
DRAFTINFO=$(curl -H "Content-Type: application/json" -X POST -d "${PAYLOAD}" -u "warrenfalk" ${CURLOPTIONS} https://api.github.com/repos/warrenfalk/rocksdb-sharp/releases)
UPLOADURL=`echo "${DRAFTINFO}" | jq .upload_url --raw-output`
UPLOADURLBASE="${UPLOADURL%\{*\}}"
echo "Uploading Zip..."
echo "to $UPLOADURLBASE"
curl -H "Content-Type: application/zip" -X POST --data-binary @../native-${REVISION}.zip -u "warrenfalk" ${CURLOPTIONS} ${UPLOADURLBASE}?name=native-${REVISION}.zip


