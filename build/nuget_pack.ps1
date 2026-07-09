Set-Location $PSScriptRoot

Write-Host @"
███╗   ██╗██╗   ██╗ ██████╗ ███████╗████████╗
████╗  ██║██║   ██║██╔════╝ ██╔════╝╚══██╔══╝
██╔██╗ ██║██║   ██║██║  ███╗█████╗     ██║   
██║╚██╗██║██║   ██║██║   ██║██╔══╝     ██║   
██║ ╚████║╚██████╔╝╚██████╔╝███████╗   ██║   
╚═╝  ╚═══╝ ╚═════╝  ╚═════╝ ╚══════╝   ╚═╝   
"@

$projects = @(
    "..\src\Wpf.Ui.Violeta"
)

foreach ($proj in $projects) {
    Push-Location $proj
    Write-Host "Processing $proj..."
    dotnet restore /p:Configuration=Release
    dotnet build -c Release --no-restore
    dotnet pack -c Release -o ../../build/
    Pop-Location
}

Write-Host "`nPress any key to exit..."
[void][System.Console]::ReadKey($true)
