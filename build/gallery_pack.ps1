Set-Location $PSScriptRoot

# Avoid mojibake: Windows PowerShell defaults to the system ANSI/OEM code page (e.g. GBK),
# while Unicode banners and `dotnet` CLI localized output are UTF-8.
$script:Utf8NoBom = [System.Text.UTF8Encoding]::new($false)
try {
    [Console]::InputEncoding = $script:Utf8NoBom
    [Console]::OutputEncoding = $script:Utf8NoBom
}
catch {
    # Host may not expose a real console (redirected I/O).
}
$OutputEncoding = $script:Utf8NoBom
& chcp.com 65001 > $null
$env:DOTNET_CLI_UI_LANGUAGE = 'en'

Write-Host @"
 ██████╗  █████╗ ██╗     ██╗     ███████╗██████╗ ██╗   ██╗
██╔════╝ ██╔══██╗██║     ██║     ██╔════╝██╔══██╗╚██╗ ██╔╝
██║  ███╗███████║██║     ██║     █████╗  ██████╔╝ ╚████╔╝ 
██║   ██║██╔══██║██║     ██║     ██╔══╝  ██╔══██╗  ╚██╔╝  
╚██████╔╝██║  ██║███████╗███████╗███████╗██║  ██║   ██║   
 ╚═════╝ ╚═╝  ╚═╝╚══════╝╚══════╝╚══════╝╚═╝  ╚═╝   ╚═╝   
"@

Push-Location ..\src\Wpf.Ui.Violeta.Gallery
Write-Host "Processing Wpf.Ui.Violeta.Gallery.csproj"
# net48 does not support PublishSingleFile / SelfContained (NETSDK1125).
# Always pass the csproj: leftover *_wpftmp.csproj files make bare `dotnet restore` hit MSB1011.
dotnet restore Wpf.Ui.Violeta.Gallery.csproj
if ($LASTEXITCODE -ne 0) { Pop-Location; exit $LASTEXITCODE }
dotnet publish Wpf.Ui.Violeta.Gallery.csproj -c Release -o ../../build/Wpf.Ui.Violeta.Gallery/
if ($LASTEXITCODE -ne 0) { Pop-Location; exit $LASTEXITCODE }
Pop-Location

$zipPath = Join-Path $PSScriptRoot "Wpf.Ui.Violeta.Gallery.zip"
$galleryPath = Join-Path $PSScriptRoot "Wpf.Ui.Violeta.Gallery"

if (-not (Test-Path $galleryPath)) {
    Write-Error "Gallery output directory not found: $galleryPath"
    exit 1
}

Get-Process -Name "Wpf.Ui.Violeta.Gallery" -ErrorAction SilentlyContinue | Stop-Process -Force

Write-Host "Packing Gallery to $zipPath"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory(
    $galleryPath,
    $zipPath,
    [System.IO.Compression.CompressionLevel]::Optimal,
    $true)
Write-Host "Created $zipPath"

Write-Host "`nPress any key to exit..."
try {
    [void][Console]::ReadKey($true)
} catch {
    # Ignore when console input is unavailable (e.g. CI or automated runs).
}
