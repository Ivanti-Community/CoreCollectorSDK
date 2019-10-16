<#
 ---------------------------------------------------------------------------
  Copyright (C) Ivanti Corporation 2019. All rights reserved.
 
  This file contains trade secrets of the Ivanti Corporation. No part
  may be reproduced or transmitted in any form by any means or for any purpose
  without the express written permission of the Ivanti Corporation.
 ---------------------------------------------------------------------------
 Notes: Builds the Core Collector SDK soulution.
 Author: Roy Morris roy.morris@ivanti.com
#>

param
(
[Bool] $restore = $TRUE,   
[String] $config = "Debug",
[Boolean] $packageLocally = $FALSE,
[String] $version = "3.0.0"
)

$lastDir = Get-Location

$workingDir = $PSSCRIPTROOT
Write-Host "Working dir $workingDir"
$parentDir = (Get-Item $workingDir).parent.FullName
Write-Host "Parent dir $parentDir"

Import-Module -Name "$workingDir\SdkFunctions.psm1"
Write-Host "Resolving MsBuild..."
$msbuild = Resolve-MsBuild

$nugetExe = "$parentDir\nuget.exe"
$nugetLocalRepo = "D:\Dev\Package-Repo"

$workingDir = "$parentDir\Src"

Set-Location -Path "$workingDir"

[Environment]::CurrentDirectory = $workingDir

###################### Restore Nuget Packages ###################################
if ($restore) 
{
    Write-Host "Restoring nuget packages: local repo : $nugetLocalRepo"
    Start-Process "$nugetExe" "restore" -Wait -NoNewWindow
}
###################### Build the SDK ############################################
$solutionFile = "$parentDir\Src\CoreCollectorSDK.sln"
Write-Host "Solution file: " $solutionFile

Write-Host "Building Collector SDK"
Write-Host $msbuild $solutionFile /t:Rebuild /fl /clp:"v=m;Summary" /p:Configuration=$config /p:Platform="Any CPU"
& $msbuild $solutionFile /t:Rebuild /fl /clp:"v=m;Summary" /p:Configuration=$config /p:Platform="Any CPU"

if ($packageLocally) 
{
    ###################### Create Nuget Package #####################################
    Write-Host "Creating Collector SDK Nuget Package..."
    $collectorSdkDir = "$workingDir\Core.Collector.SDK"
    $libDir = "$collectorSdkDir\lib"
    $frameworkDir = "$libDir\netcoreapp2.2"
    $binDir = "$collectorSdkDir\obj\$config\netcoreapp2.2"

    if (!(Test-Path -Path $libDir))
    {
        [System.IO.Directory]::CreateDirectory($libDir) > $null
    }
    if (!(Test-Path -Path $frameworkDir))
    {
        [System.IO.Directory]::CreateDirectory($frameworkDir) > $null
    }

    Copy-Item -Path "$binDir\Core.Collector.SDK.dll" -Destination $frameworkDir -Force
    Start-Process "$nugetExe" "pack $collectorSdkDir\Core.Collector.SDK.nuspec -Version $version" -Wait -NoNewWindow
    Copy-Item -Path "$workingDir\Ivanti.Core.Collector.SDK.$version.nupkg" -Destination $nugetLocalRepo -Force
}

Set-Location $lastDir

Write-Host "Done..."