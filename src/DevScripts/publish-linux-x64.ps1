Push-Location "$PSScriptRoot/../VoltProjects.Server"

dotnet publish -c Release -r linux-x64 --no-self-contained

Pop-Location 