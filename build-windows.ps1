param (
    [string]$BannerlordDir = "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord"
)

$ErrorActionPreference = "Stop"
$RootDir = $PSScriptRoot
Set-Location $RootDir

$HarmonyVersion = "2.2.2"
$HarmonyDir = Join-Path $RootDir ".build-tools\harmony"
$HarmonyDll = Join-Path $HarmonyDir "0Harmony.dll"

if (-Not (Test-Path $HarmonyDll)) {
    Write-Host "Downloading Harmony v$HarmonyVersion for build..."
    New-Item -ItemType Directory -Force -Path $HarmonyDir | Out-Null
    $ZipPath = Join-Path $HarmonyDir "harmony.zip"
    Invoke-WebRequest -Uri "https://www.nuget.org/api/v2/package/Lib.Harmony/$HarmonyVersion" -OutFile $ZipPath
    Expand-Archive -Path $ZipPath -DestinationPath $HarmonyDir -Force
    Move-Item -Path (Join-Path $HarmonyDir "lib\net472\0Harmony.dll") -Destination $HarmonyDll -Force
    Remove-Item -Recurse -Force (Join-Path $HarmonyDir "lib")
    Remove-Item -Recurse -Force (Join-Path $HarmonyDir "package")
    Remove-Item -Recurse -Force (Join-Path $HarmonyDir "_rels")
    Remove-Item -Force $ZipPath
}

msbuild LordCaptor.csproj /t:Rebuild /p:Configuration=Release "/p:BannerlordInstallDir=$BannerlordDir"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit $LASTEXITCODE
}

$DistDir = Join-Path $RootDir "dist\LordCaptor"
if (Test-Path $DistDir) { Remove-Item -Recurse -Force $DistDir }
$BinDir = Join-Path $DistDir "bin\Win64_Shipping_Client"
New-Item -ItemType Directory -Force -Path $BinDir | Out-Null
$ModuleDataDir = Join-Path $DistDir "ModuleData"
New-Item -ItemType Directory -Force -Path $ModuleDataDir | Out-Null

Copy-Item (Join-Path $RootDir "bin\Release\LordCaptor.dll") -Destination $BinDir

Copy-Item (Join-Path $RootDir "SubModule.xml") -Destination $DistDir
Copy-Item (Join-Path $RootDir "ModuleData\*") -Destination $ModuleDataDir -Recurse

Write-Host "Built self-contained dist\LordCaptor"
