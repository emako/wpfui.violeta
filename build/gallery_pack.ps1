Set-Location $PSScriptRoot

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
dotnet restore
dotnet publish Wpf.Ui.Violeta.Gallery.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -o ../../build/gallery/
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
