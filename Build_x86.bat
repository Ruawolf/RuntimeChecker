cd RuntimeChecker

dotnet publish RuntimeChecker.csproj --runtime win-x86 --output "bin/publish/" /p:Configuration="Release" /p:Platform="x86" /p:PublishAot=false /p:SelfContained=true /p:PublishSingleFile=true /p:PublishReadyToRun=false /p:PublishTrimmed=true