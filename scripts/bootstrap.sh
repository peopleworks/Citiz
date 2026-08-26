#!/usr/bin/env bash
# First-run check: does everything build, test and validate on this machine?
# Mirrors .github/workflows/ci.yml so a green run here means a green pull request.
set -euo pipefail
cd "$(dirname "$0")/.."

echo "== .NET SDK"
dotnet --version

echo "== Restore, build, test"
dotnet restore Citiz.slnx
dotnet build Citiz.slnx -c Release --no-restore --nologo
dotnet test Citiz.slnx -c Release --no-build --nologo

echo "== Content and language packs"
dotnet run --project src/Citiz.Cli -c Release --no-build -- content validate
dotnet run --project src/Citiz.Cli -c Release --no-build -- localization validate

echo
echo "Citiz is ready."
echo "  Web app:   dotnet run --project src/Citiz.Web"
echo "  Terminal:  dotnet run --project src/Citiz.Cli -- exam simulate"
