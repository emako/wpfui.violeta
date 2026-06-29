Set-Location $PSScriptRoot

Write-Host @"
 ██████╗  █████╗ ██╗     ██╗     ███████╗██████╗ ██╗   ██╗
██╔════╝ ██╔══██╗██║     ██║     ██╔════╝██╔══██╗╚██╗ ██╔╝
██║  ███╗███████║██║     ██║     █████╗  ██████╔╝ ╚████╔╝ 
██║   ██║██╔══██║██║     ██║     ██╔══╝  ██╔══██╗  ╚██╔╝  
╚██████╔╝██║  ██║███████╗███████╗███████╗██║  ██║   ██║   
 ╚═════╝ ╚═╝  ╚═╝╚══════╝╚══════╝╚══════╝╚═╝  ╚═╝   ╚═╝   
"@

Push-Location ..\src\Wpf.Ui.Test
Write-Host "Processing ..\src\Wpf.Ui.Test..."
dotnet restore
dotnet publish Wpf.Ui.Test.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=false -o ../../build/
Pop-Location

Write-Host "`nPress any key to exit..."
[void][System.Console]::ReadKey($true)
