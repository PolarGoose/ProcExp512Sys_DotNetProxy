Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

Function CheckReturnCodeOfPreviousCommand($msg) {
  if(-Not $?) {
    Write-Error "${msg}. Error code: $LastExitCode"
  }
}

$buildDir = Resolve-Path "$PSScriptRoot/../../build"

Write-Host "Download PROCEXP152.SYS file"

New-Item -ItemType Directory -Path $buildDir -ErrorAction SilentlyContinue > $null

# There is no direct link to download PROCEXP152.SYS driver. However, the Handle tool contains it as a resource.
Invoke-WebRequest -Uri https://www.nirsoft.net/utils/resourcesextract-x64.zip -OutFile $buildDir/resourcesextract-x64.zip
Expand-Archive -Path $buildDir/resourcesextract-x64.zip -DestinationPath $buildDir/resourcesextract -Force

Invoke-WebRequest -Uri https://download.sysinternals.com/files/Handle.zip -OutFile $buildDir/Handle.zip
Expand-Archive -Path $buildDir/Handle.zip -DestinationPath $buildDir/Handle -Force

# ResourcesExtract.exe only accepts path with "\" separator. Otherwise it fails silently
& $buildDir/resourcesextract/ResourcesExtract.exe /Source $buildDir\Handle\handle64.exe /DestFolder $buildDir /ExtractBinary 1 /FileExistMode 1 /OpenDestFolder 0
CheckReturnCodeOfPreviousCommand "ResourcesExtract failed"

# ResourcesExtract.exe works in parallel. We need to wait fot it to finish
Start-Sleep -Seconds 2

# After extraction, the file with the name handle64_103_BINRES.bin is the PROCEXP152.SYS driver
Remove-Item -Path $buildDir/PROCEXP152.SYS -Force -ErrorAction SilentlyContinue > $null
Rename-Item $buildDir/handle64_103_BINRES.bin -NewName PROCEXP152.SYS
