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
$galleryPath = Join-Path $PSScriptRoot "gallery"
Write-Host "Packing gallery to $zipPath"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}
Compress-Archive -Path $galleryPath -DestinationPath $zipPath
Write-Host "Created $zipPath"

Write-Host "`nPress any key to exit..."
[void][System.Console]::ReadKey($true)
