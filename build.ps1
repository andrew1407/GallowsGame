# Builds the game locally with the project's Unity version and zips each build for release downloads.
#
#   ./build.ps1                      # Windows + WebGL
#   ./build.ps1 -Platforms Windows   # one platform
#   ./build.ps1 -Platforms Windows,Linux,macOS,WebGL
#
# Close the project in the editor first: batch mode can't open a project that is already open.
# Builds land in Builds/<Platform>/ and archives in Builds/GallowsGame-<Platform>.zip
param(
    [ValidateSet('Windows', 'Linux', 'macOS', 'WebGL')]
    [string[]] $Platforms = @('Windows', 'WebGL'),
    [string] $UnityPath
)

$ErrorActionPreference = 'Stop'
$projectPath = $PSScriptRoot
$buildsPath = Join-Path $projectPath 'Builds'

if (-not $UnityPath) {
    $version = (Select-String -Path (Join-Path $projectPath 'ProjectSettings/ProjectVersion.txt') -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value
    $hubEditors = Join-Path $env:ProgramFiles 'Unity/Hub/Editor'
    # Build support module folder of each platform, to pick the install that can build them all
    $modules = @{ Windows = 'WindowsStandaloneSupport'; Linux = 'LinuxStandaloneSupport'; macOS = 'MacStandaloneSupport'; WebGL = 'WebGLSupport' }
    $installs = Get-ChildItem $hubEditors -Directory -Filter "$version*" | Where-Object { Test-Path (Join-Path $_.FullName 'Editor/Unity.exe') }
    if (-not $installs) { throw "Unity $version isn't installed in $hubEditors (pass -UnityPath)" }
    $install = $installs | Where-Object {
        $engines = Join-Path $_.FullName 'Editor/Data/PlaybackEngines'
        -not ($Platforms | Where-Object { -not (Test-Path (Join-Path $engines $modules[$_])) })
    } | Select-Object -First 1
    if (-not $install) { throw "No Unity $version install has build support for all of: $($Platforms -join ', ') (add the modules in Unity Hub)" }
    $UnityPath = Join-Path $install.FullName 'Editor/Unity.exe'
}

New-Item -ItemType Directory -Force $buildsPath | Out-Null

foreach ($platform in $Platforms) {
    $log = Join-Path $buildsPath "build-$platform.log"
    Write-Host "Building $platform with $UnityPath (log: $log)"
    $process = Start-Process $UnityPath -Wait -PassThru -NoNewWindow -ArgumentList @(
        '-batchmode', '-quit', '-nographics',
        '-projectPath', "`"$projectPath`"",
        '-executeMethod', "BuildScript.Build$($platform -replace 'macOS', 'MacOS')",
        '-logFile', "`"$log`""
    )
    if ($process.ExitCode -ne 0) { throw "$platform build failed (exit code $($process.ExitCode)), see $log" }

    $archive = Join-Path $buildsPath "GallowsGame-$platform.zip"
    Remove-Item $archive -ErrorAction SilentlyContinue
    # Debugging leftovers Unity marks as not for shipping
    $files = Get-ChildItem (Join-Path $buildsPath $platform) | Where-Object { $_.Name -notlike '*DoNotShip' -and $_.Name -notlike '*DontShipItWithYourGame' }
    Compress-Archive -Path $files.FullName -DestinationPath $archive
    Write-Host "$platform -> $archive"
}
