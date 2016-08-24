#!/bin/sh
[ -f native.zip ] || curl -L https://github.com/warrenfalk/rocksdb-sharp/releases/download/v4.9.0/native-e70aabc3.zip --output native.zip
[ -f native.zip ] && { mkdir -p native; cd native && unzip ../native.zip; }
[ -f native.zip -a -f native/amd64/librocksdb.so ] && rm native.zip

