dotnet --version

dotnet restore Kbiosystems.Scorpion.Driver

$binfo = "$($env:LIB_VERSION)$(".")$($env:APPVEYOR_BUILD_NUMBER)"

Write-Host "Assembly version info" $binfo

dotnet build -c Release Kbiosystems.Scorpion.Driver /p:Version=$binfo

dotnet pack Kbiosystems.Scorpion.Driver -c Release --no-build --include-symbols /p:Version=$env:APPVEYOR_BUILD_VERSION