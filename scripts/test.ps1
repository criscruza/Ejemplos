<#
.SYNOPSIS
    Runs tests for GitHub for Visual Studio
.DESCRIPTION
    Build GHfVS
.PARAMETER Clean
    When true, all untracked (and ignored) files will be removed from the work
    tree and all submodules. Defaults to false.
#>
[CmdletBinding()]

Param(
    [ValidateSet('Debug', 'Release')]
    [string]
    $Config = "Release"
    ,
    [int]
    $TimeoutDuration = 180
    ,
    [switch]
    $AppVeyor = $false
    ,
    [switch]
    $Trace = $false

)

Set-StrictMode -Version Latest
if ($Trace) {
    Set-PSDebug -Trace 1
}

$env:PATH = "$$PSScriptRoot;$env:PATH"

$exitcode = 0

Write-Output "Running Tracking Collection Tests..."
Run-NUnit src TrackingCollectionTests $TimeoutDuration $config -AppVeyor:$AppVeyor
if (!$?) {
    $exitcode = 1
}

Write-Output "Running GitHub.UI.UnitTests..."
Run-NUnit test GitHub.UI.UnitTests $TimeoutDuration $config -AppVeyor:$AppVeyor
if (!$?) {
    $exitcode = 2
}

Write-Output "Running UnitTests..."
Run-XUnit src UnitTests $TimeoutDuration $config -AppVeyor:$AppVeyor
if (!$?) {
    $exitcode = 3
}

Write-Output "Running GitHub.InlineReviews.UnitTests..."
Run-XUnit test GitHub.InlineReviews.UnitTests $TimeoutDuration $config -AppVeyor:$AppVeyor
if (!$?) {
    $exitcode = 4
}

if ($exitcode -ne 0 -and $AppVeyor) {
    $host.SetShouldExit($exitcode)
}
exit $exitcode