$projectPath = $PSScriptRoot
CD $projectPath
iex "docker build --tag svtz/homecontrol-rabbitmqserver:latest ."

"Complete!"