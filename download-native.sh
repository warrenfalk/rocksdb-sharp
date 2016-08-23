#!/bin/sh
[ -f native.zip ] || curl -L https://github.com/warrenfalk/rocksdb-sharp/releases/download/v0.4.9/native-e70aabc3.zip --output native.zip
[ -f native.zip ] && { mkdir -p native; cd native && unzip ../native.zip; }
[ -f native.zip -a -f native/amd64/librocksdb.so ] && rm native.zip

