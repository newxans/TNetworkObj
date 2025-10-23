title Networking Recreations
@echo OFF
echo NetworkObj v1.1.3 - Builder
echo Clean Publish
dotnet clean
echo Build Linux Version
dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true -p:SelfContained=true -p:PublishTrimmed=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=false
echo Build available in /bin/Release/linux-x86/publish/NetworkObj