#-------------------------------
# Installation
#-------------------------------

# Write-Host "Downloading latest .NET Core SDK..."

# (New-Object System.Net.WebClient).DownloadFile('https://www.microsoft.com/net/download/thank-you/dotnet-sdk-2.1.300-windows-x64-installer', "$env:appveyor_build_folder\dotnet-core-sdk.exe")
# Invoke-WebRequest "https://go.microsoft.com/fwlink/?linkid=841686" -OutFile "dotnet-core-sdk.exe"

# Write-Host "Installing .NET Core SDK..."

# Invoke-Command -ScriptBlock { ./dotnet-core-sdk.exe /S /v/qn }

# Write-Host "Installation succeeded." -ForegroundColor Green

dotnet restore Kbiosystems.Scorpion.Driver
