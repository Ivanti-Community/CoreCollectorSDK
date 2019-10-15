<#
	Builds the SDK
#>

param
(
[String] $config = "Debug"
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
Write-Host "Restoring nuget packages: local repo : $nugetLocalRepo"
Start-Process "$nugetExe" "restore" -Wait -NoNewWindow

###################### Build the SDK ############################################
$solutionFile = "$parentDir\Src\CoreCollectorSDK.sln"
Write-Host "Solution file: " $solutionFile

Write-Host "Building Collector SDK"
Write-Host $msbuild $solutionFile /t:Rebuild /fl /clp:"v=m;Summary" /p:Configuration=$config /p:Platform="Any CPU"
& $msbuild $solutionFile /t:Rebuild /fl /clp:"v=m;Summary" /p:Configuration=$config /p:Platform="Any CPU"

Set-Location $lastDir

Write-Host "Done..."