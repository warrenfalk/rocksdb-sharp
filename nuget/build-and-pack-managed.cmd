msbuild /p:Configuration=Release ..\net40\RocksDbSharpNet40.csproj
@if %errorlevel% neq 0 exit /b %errorlevel%

msbuild /p:Configuration=Release ..\net45\RocksDbSharpNet45.csproj
@if %errorlevel% neq 0 exit /b %errorlevel%

msbuild /p:Configuration=Release ..\netcoreapp1.0\RocksDbSharpCore.xproj
@if %errorlevel% neq 0 exit /b %errorlevel%

nuget pack RocksDbSharp.nuspec
@if %errorlevel% neq 0 exit /b %errorlevel%
