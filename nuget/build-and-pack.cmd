@echo off
pushd "%~dp0"
pushd "%~dp0"
where /q msbuild
IF ERRORLEVEL 1 (
	call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\Tools\VsMSBuildCmd.bat"
)
popd

cd ..

REM echo "Downloading native..."
REM call download-native.cmd

nuget restore
@if %errorlevel% neq 0 goto oops

msbuild /p:Configuration=Release /t:Rebuild,Pack RocksDbNative\RocksDbNative.csproj
@if %errorlevel% neq 0 goto oops

msbuild /p:Configuration=Release /t:Rebuild,Pack RocksDbSharp\RocksDbSharp.csproj
@if %errorlevel% neq 0 goto oops

move /y RocksDbSharp\bin\Release\*.nupkg .\nuget\
@if %errorlevel% neq 0 goto oops

move /y RocksDbNative\bin\Release\*.nupkg .\nuget\
@if %errorlevel% neq 0 goto oops

:good
popd
exit /b 0

:oops
set rval=%errorlevel%
echo "ERROR"
popd
exit /b %rval%