cd RuntimeChecker

dotnet publish RuntimeChecker.csproj --runtime win-x64 --output "bin/publish/" /p:Configuration="Release" /p:Platform="x64" /p:PublishAot=true