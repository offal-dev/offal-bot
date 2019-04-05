$ErrorActionPreference = "Stop"

$pemPath = Join-Path $PSScriptRoot ".\offal-bot.pem" -Resolve

Write-Output "Base 64ing file... $pemPath"
$fileContent = [System.IO.File]::ReadAllBytes($pemPath)
$fileContentEncoded = [System.Convert]::ToBase64String($fileContent)
$fileContentEncoded | Set-Content ($pemPath + ".b64") 