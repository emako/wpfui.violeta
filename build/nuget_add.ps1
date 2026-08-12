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

# Verify that 'pmc' exists on the system before doing anything else.
$pmcPath = & where.exe pmc 2>$null
if ($LASTEXITCODE -ne 0 -or -not $pmcPath) {
    Write-Host "Error: 'pmc' command was not found on this system." -ForegroundColor Red
    Write-Host "Please install PMC and make sure it is available in your PATH, then run this script again." -ForegroundColor Red
    Write-Host "`nPress any key to exit..."
    [void][System.Console]::ReadKey($true)
    exit 1
}
Write-Host "pmc found: $($pmcPath -join ', ')"

# Collect all package files in the current directory.
$files = @(Get-ChildItem -Path . -Filter *.nupkg -File) + @(Get-ChildItem -Path . -Filter *.snupkg -File)
$files = $files | Sort-Object Name

if ($files.Count -eq 0) {
    Write-Host "No .nupkg or .snupkg files were found in $PSScriptRoot." -ForegroundColor Yellow
    Write-Host "`nPress any key to exit..."
    [void][System.Console]::ReadKey($true)
    exit 0
}

Write-Host "Adding $($files.Count) package file(s)..."
foreach ($file in $files) {
    Write-Host "  pmc add $($file.Name)"
    $LASTEXITCODE = $null
    pmc add $file.Name
    if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0) {
        Write-Host "  Warning: 'pmc add $($file.Name)' failed (exit code $LASTEXITCODE)." -ForegroundColor Yellow
    }
}

Write-Host "`nAll done."
Write-Host "Press any key to exit..."
[void][System.Console]::ReadKey($true)
