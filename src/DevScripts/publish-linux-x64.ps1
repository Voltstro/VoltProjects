Push-Location "$PSScriptRoot/../VoltProjects.Server"
dotnet publish -c Release -r linux-x64 --no-self-contained
Pop-Location

Push-Location "$PSScriptRoot/../VoltProjects.Server/bin/Release/net6.0/linux-x64/publish/"
tar -czvf ../../linux-x64.tar.gz --numeric-owner **/
Pop-Location 