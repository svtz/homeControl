$projectPath = Split-Path -parent $PSScriptRoot
CD $projectPath

$publishDirectory = Join-Path -Path $projectPath -ChildPath "publish"

if (Test-Path $publishDirectory -PathType Container) {
    Remove-Item $publishDirectory -Force -Recurse
}

iex "dotnet publish homeControl.NooliteF.csproj --output $($publishDirectory)"
iex "docker build --tag svtz/homecontrol-noolitef:latest --file Properties/Dockerfile ."

Remove-Item $publishDirectory -Force -Recurse

"Complete!"