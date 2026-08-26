# First-run check: does everything build, test and validate on this machine?
# Mirrors .github/workflows/ci.yml so a green run here means a green pull request.
$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")

Write-Host "== .NET SDK"
dotnet --version

Write-Host "== Restore, build, test"
dotnet restore Citiz.slnx
dotnet build Citiz.slnx -c Release --no-restore --nologo
dotnet test Citiz.slnx -c Release --no-build --nologo

Write-Host "== Content and language packs"
dotnet run --project src/Citiz.Cli -c Release --no-build -- content validate
dotnet run --project src/Citiz.Cli -c Release --no-build -- localization validate

Write-Host ""
Write-Host "Citiz is ready."
Write-Host "  Web app:   dotnet run --project src/Citiz.Web"
Write-Host "  Terminal:  dotnet run --project src/Citiz.Cli -- exam simulate"
