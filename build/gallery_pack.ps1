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
dotnet publish Wpf.Ui.Violeta.Gallery.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -o ../../build/
Pop-Location

Write-Host "`nPress any key to exit..."
[void][System.Console]::ReadKey($true)
