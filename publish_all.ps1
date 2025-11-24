param(
    [string]$version = "latest"
)


$project = "LocalLobbyServer.csproj"
$out = "publish"
$assemblyName = "LocalLobbyServer"

function Wait-ForFile {
    param([string]$path)

    $timeout = 100 # 10 seconds
    $count = 0

    while (-not (Test-Path $path)) {
        Start-Sleep -Milliseconds 100
        $count++
        if ($count -ge $timeout) {
            Write-Error "Timeout waiting for file: $path"
            return
        }
    }
}

# CLEAN PUBLISH FOLDER
if (Test-Path $out) {
    Write-Host "Deleting existing publish folder..."
    Remove-Item -Recurse -Force $out
}

New-Item -ItemType Directory -Path $out | Out-Null

# WINDOWS
dotnet publish $project -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o "$out/win"
Wait-ForFile $out/win/LocalLobbyServer.exe
Rename-Item "$out/win/LocalLobbyServer.exe" "LocalLobbyServer-win.exe"

# LINUX
dotnet publish $project -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true -o "$out/linux"
Wait-ForFile $out/linux/LocalLobbyServer
Rename-Item "$out/linux/LocalLobbyServer" "LocalLobbyServer-linux"

# MAC
dotnet publish $project -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true -o "$out/mac"
Wait-ForFile $out/mac/LocalLobbyServer
Rename-Item "$out/mac/LocalLobbyServer" "LocalLobbyServer-mac"

Write-Host "All platform binaries built and renamed successfully!"

# Determine target GitHub release tag
if ($version -eq "latest") {
    Write-Host "Resolving latest release tag..."
    $releaseTag = (gh release list --limit 1 |
        Select-Object -First 1).Split(" ")[0]

    Write-Host "Uploading assets to latest release ($releaseTag)"
} else {
    $releaseTag = $version
    Write-Host "Uploading assets to $releaseTag"
}

# Check if the release exists by attempting to view it
Write-Host "Checking if release '$releaseTag' exists..."
gh release view $releaseTag 2>$null
$exists = $LASTEXITCODE -eq 0

if (-not $exists) {
    Write-Host "Release '$releaseTag' not found. Creating a new release..."
    gh release create $releaseTag --notes "Auto-generated release $releaseTag"
} else {
    Write-Host "Release '$releaseTag' found. Updating assets..."
}

# Upload assets (overwrite if needed)
Write-Host "Uploading assets..."
gh release upload $releaseTag publish/win/LocalLobbyServer-win.exe   --clobber
gh release upload $releaseTag publish/linux/LocalLobbyServer-linux   --clobber
gh release upload $releaseTag publish/mac/LocalLobbyServer-mac       --clobber

Write-Host "Assets uploaded to release '$releaseTag' successfully!"
