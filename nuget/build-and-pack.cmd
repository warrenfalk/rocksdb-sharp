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

@REM --------------------------------
@echo Building RocksDbNative
cd RocksDbNative
@echo Restoring...
msbuild /t:Restore
@if %errorlevel% neq 0 goto oops
@echo Building...
msbuild /p:Configuration=Release /t:Rebuild,Pack
@if %errorlevel% neq 0 goto oops
@echo Installing...
move /y bin\Release\*.nupkg ..\nuget\
@if %errorlevel% neq 0 goto oops
cd ..

@REM --------------------------------
@echo Building RocksDbSharp
cd RocksDbSharp
@echo Restoring...
msbuild /t:Restore
@if %errorlevel% neq 0 goto oops
@echo Building...
msbuild /p:Configuration=Release /t:Rebuild,Pack
@if %errorlevel% neq 0 goto oops
@echo Installing...
move /y bin\Release\*.nupkg ..\nuget\
@if %errorlevel% neq 0 goto oops
cd ..

:good
popd
exit /b 0

:oops
set rval=%errorlevel%
echo "ERROR"
popd
exit /b %rval%