@echo off
pushd "%~dp0"
pushd "%~dp0"
where /q msbuild
IF ERRORLEVEL 1 (
	call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsMSBuildCmd.bat"
)
popd

cd ..

echo "Downloading native..."
call download-native.cmd

nuget restore
@if %errorlevel% neq 0 exit /b %errorlevel%

msbuild /p:Configuration=Release RocksDbSharp\RocksDbSharp.csproj
@if %errorlevel% neq 0 exit /b %errorlevel%

cd nuget

nuget pack RocksDbSharp.nuspec
@if %errorlevel% neq 0 exit /b %errorlevel%

nuget pack RocksDbNative.nuspec
@if %errorlevel% neq 0 exit /b %errorlevel%

popd