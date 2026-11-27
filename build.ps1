$ErrorActionPreference = "Stop"
$version = if ($args.Count -gt 0) { $args[0] } else { "1.0.0" }

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw ".NET 8 SDK bulunamadı. https://dotnet.microsoft.com/download adresinden kur."
}

dotnet publish "$PSScriptRoot\src\PilTakip.csproj" -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true -p:Version=$version `
    -o "$PSScriptRoot\src\bin\publish"
if ($LASTEXITCODE -ne 0) { throw "Derleme başarısız oldu." }

$iscc = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) {
    throw "Inno Setup 6 bulunamadı. https://jrsoftware.org/isdl.php adresinden kur."
}

& $iscc "/DMyAppVersion=$version" "$PSScriptRoot\installer.iss"
if ($LASTEXITCODE -ne 0) { throw "Kurulum dosyası oluşturulamadı." }

Write-Host ""
Write-Host "Hazır: $PSScriptRoot\src\bin\installer\PilTakip-Setup-$version.exe"
