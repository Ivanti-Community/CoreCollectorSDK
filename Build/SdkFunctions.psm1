# Test if Non-Interactive
function Test-IsNonInteractive()
{
    # ref: http://www.powershellmagazine.com/2013/05/13/pstip-detecting-if-the-console-is-in-interactive-mode/
    # powershell -NoProfile -NoLogo -NonInteractive -File .\IsNonInteractive.ps1
    # return [bool]([Environment]::GetCommandLineArgs() -Contains '-NonInteractive')

	 if ([Environment]::UserInteractive) {
        foreach ($arg in [Environment]::GetCommandLineArgs()) {
            # Test each Arg for match of abbreviated '-NonInteractive' command.
            if ($arg -like '-NonI*') {
                return $true
            }
        }
    }

    return $false
}

# Find MSBuild
function Resolve-MsBuild 
{
	$msb2019 = Resolve-Path "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\*\MSBuild\Current\bin\msbuild.exe" -ErrorAction SilentlyContinue
	if ($msb2019) {
		Write-Host "Found MSBuild 2019 (or later)."
		Write-Host $msb2019
		return $msb2019
	}
	
	Write-Host "Checking Drives for MSBuild"
	$drives = Get-PSDrive -PSProvider "FileSystem"
	foreach ($item in $drives.Root) {
		Write-Host "Searching $item for MSBuild"
		$msb = Resolve-Path "${item}Program Files (x86)\Microsoft Visual Studio\2019\*\MSBuild\Current\bin\msbuild.exe" -ErrorAction SilentlyContinue
		if ($msb) {
			Write-Host "Found MSBuild 2017 (or later). $msb"
			return $msb
		}
	}
}

<#
.SYNOPSIS
	Used to load VS2017 variables into the current powershell environment.

.DESCRIPTION
	Invokes a command script and captures the resulting variables, then updates the environment with that information.
#>
function Invoke-CmdScript
(
	[string] $script
)
{
	$private:tempFile = [IO.Path]::GetTempFileName()

	$test = " `"$script`" x86 && set > ""$private:tempFile"""
	if ([environment]::Is64BitProcess)
	{
		$test = " `"$script`" amd64 && set > ""$private:tempFile"""
	}

	## Store the output of cmd.exe. We also ask cmd.exe to output
	## the environment table after the batch file completes 
	Write-Host $test
	& cmd /c $test

	## Go through the environment variables in the temp file.
	## For each of them, set the variable in our local environment.
	Get-Content $private:tempFile | Foreach-Object {
	    if($_ -match "^(.*?)=(.*)$")
	    {
	        Set-Content "env:\$($matches[1])" $matches[2]
	    }
	}

	Remove-Item $private:tempFile
}

<#
.SYNOPSIS
	Used to ensure that the visual studio environment is loaded into the Powershell runspace.

.DESCRIPTION
	This function is inteded to be invoked in every subsequent script to allow for a uniform development
	environment in all scripts used by the framework.
#>
function Assert-Environment
{
	if (Test-Path -Path Env:\WindowsSdkDir)
	{
		return
	}

	$dir = $PSSCRIPTROOT
	$referencePath = [System.IO.Path]::Combine($dir, ".vs2017path")
	Write-Host "Script dir:" $dir

	if ((Test-Path -Path $referencePath) -ne $true)
	{
		Write-Error "Unable to determine the Visual Studio Environment. Run Test-Prereqs.ps1 before running this script."
		throw "Unable to determine the Visual Studio Environment. Run Test-Prereqs.ps1 before running this script."
	}

	$vsPath = Get-Content -Path $referencePath
	if ([String]::IsNullOrWhiteSpace($vsPath))
	{
		Write-Error "Unable to determine the Visual Studio Environment. Run Test-Prereqs.ps1 before running this script."
		throw "Unable to determine the Visual Studio Environment. Run Test-Prereqs.ps1 before running this script."
	}
	$vcvarsPath = [System.IO.Path]::Combine($vsPath, "VC\Auxiliary\Build\vcvarsall.bat")

	Invoke-CmdScript($vcvarsPath, $architectureParameter)
}

<#
.SYNOPSIS
	Used to ensure that the Powershell runtime meets minimum requirements.

.DESCRIPTION
	This function is will succeed if the version of powershell is at least version 3.0.
#>
function Assert-Powershell
{
	if ($PSVersionTable -eq $null)
	{
		throw "Powershell 1.0 is not supported. Powershell 3.0 or later is required."
	}

	if ($PSVersionTable.PSVersion.Major -lt 3)
	{
		$error = "Powershell " + $PSVersionTable.PSVersion.ToString() + " is not supported. Powershell 3.0 or later is required."
		throw $error
	}
}

<#
.SYNOPSIS
	Used to ensure that the launching user is an administrator.

.DESCRIPTION
	This function is will succeed if the launching user is an administrator.
#>
function Assert-Administrator
{
	[System.Security.Principal.WindowsIdentity] $identity = [System.Security.Principal.WindowsIdentity]::GetCurrent()
	$principal = New-Object -TypeName System.Security.Principal.WindowsPrincipal -ArgumentList $identity
	$isAdministrator = $principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)

	[System.IDisposable]$disposable = $identity
	$disposable.Dispose()

	if (-not $isAdministrator)
	{
		Write-Error "This script requires administrator rights."
		throw "Script not run with administrator rights."
	}
}

<#
.SYNOPSIS
	Used to sign a file so that the agent will trust it.

.DESCRIPTION
	Used to sign a file so that the agent will trust it.

.PARAMETER File
	The path to the file to sign.
#>
function Add-SignatureForAgent([string] $File)
{
	$dir = $PSSCRIPTROOT
	$referencePath = [System.IO.Path]::Combine($dir, ".signingSubject")
	$signingSubject = Get-Content -Path $referencePath

	signtool.exe sign /n $signingSubject /fd sha256 /td sha256 /tr http://tsa.starfieldtech.com/  $file
}

<#
.SYNOPSIS
	Used to sign a file so that the agent will trust it.

.DESCRIPTION
	Used to sign a file so that the agent will trust it.

.PARAMETER File
	The path to the file to sign.
#>
function Set-AgentSigningSubject([string] $subject)
{
	$dir = $PSSCRIPTROOT
	$referencePath = [System.IO.Path]::Combine($dir, ".signingSubject")
	Set-Content -Path $referencePath -Value $subject
}

<#
.SYNOPSIS
	Used to load ST.Core.dll into the runspace.

.DESCRIPTION
	Used to load ST.Core.dll into the runspace.
#>
function Use-STCore
{
	if ([System.Environment]::Is64BitProcess) 
	{
		$systemArchitecture = "x64"
	}
	else
	{
		$systemArchitecture = "x86"
	}

	$scriptDirectory = $PSSCRIPTROOT
	$parentDir = (Get-Item $scriptDirectory).parent.FullName
	$stCorePath = "$parentDir\Collector.Agent.Engine\AgentSDK\Bin\$systemArchitecture\Release\ST.Core.dll"
	if ([IO.File]::Exists($stCorePath))
	{
		[void][Reflection.Assembly]::LoadFrom($stCorePath)
	}
	else
	{
		$stCorePath = "$parentDir\Collector.Agent.Engine\AgentSDK\Bin\$systemArchitecture\Debug\ST.Core.dll"
		Write-Host $stCorePath
		if ([IO.File]::Exists($stCorePath))
		{
			[void][Reflection.Assembly]::LoadFrom($stCorePath)
		}
		else
		{
			throw ("Cannot find " + $stCorePath)
		}
	}
}

<#
.SYNOPSIS
	Used to retrieve the registry path for SDK settings.

.DESCRIPTION
	Used to retrieve the registry path for SDK settings.

.PARAMETER testProductName
	The name of the test product.
#>
function Get-ProductRegistryPath([String] $testProductName)
{
	return "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Agent SDK\" + $testProductName
}

#
# Registry names used by settings functions.
#
$rootCertThumbprintName = "RootCertThumbprint"
$consoleCertThumbprintName = "ConsoleCertThumbprint"
$consoleIdName = "ConsoleId"
$databaseServerInstanceName = "DatabaseServerInstance"
$databaseCatalogName = "DatabaseName"
$applicationServerPortName = "ApplicationServerPort"

<#
.SYNOPSIS
	Used to load SDK test settings into the powershell runspace.

.DESCRIPTION
	Used to load SDK test settings into the powershell runspace.

.PARAMETER testProductName
	The name of the test product.
#>
function Get-SDKSettings([String] $testProductName)
{
	$productRegistryKeyPath = Get-ProductRegistryPath -testProductName $testProductName

	$private:exists = Test-Path -Path "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Agent SDK"
	if (!$private:exists)
	{
		New-Item -Path "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Agent SDK" > $null
	}

	$result = New-Object -TypeName SDKSettings
	$result.ProductName = $testProductName
	$result.RegistryPath = $productRegistryKeyPath
	$result.SDKConsoleId = ""
	$result.SDKConsoleCertThumbprint = ""
	$result.SDKRootCertThumbprint = ""
	$result.SDKDatabaseServerInstance = "v13.0"
	$result.SDKDatabaseName = ""
	$result.SDKApplicationServerPort = ""

	$private:exists = Test-Path -Path $productRegistryKeyPath
	if ($private:exists)
	{
		$private:registry = Get-Item -Path $productRegistryKeyPath
		
		$result.SDKConsoleId = $private:registry.GetValue($consoleIdName, "")
		$result.SDKConsoleCertThumbprint = $private:registry.GetValue($consoleCertThumbprintName, "")
		$result.SDKRootCertThumbprint = $private:registry.GetValue($rootCertThumbprintName, "")
		$result.SDKDatabaseServerInstance = $private:registry.GetValue($databaseServerInstanceName, "v13.0")
		$result.SDKDatabaseName = $private:registry.GetValue($databaseCatalogName, "")
		$result.SDKApplicationServerPort = $private:registry.GetValue($applicationServerPortName, "")
	}

	return $result
}

function Save-SDKSettings([SDKSettings] $settings)
{
	Write-Host "Saving the following settings:"
	$settings

	Remove-SDKSettings -testProductName $settings.ProductName

	$productRegistryKeyPath = Get-ProductRegistryPath -testProductName $settings.ProductName

	$dbInstanceTmp =[String]::Format("(localDB)\{0}", $settings.SDKDatabaseServerInstance)

	New-Item -Path $productRegistryKeyPath > $null
	New-ItemProperty -Path $productRegistryKeyPath -Name $consoleIdName -Value $settings.SDKConsoleId > $null
	New-ItemProperty -Path $productRegistryKeyPath -Name $consoleCertThumbprintName -Value $settings.SDKConsoleCertThumbprint > $null
	New-ItemProperty -Path $productRegistryKeyPath -Name $rootCertThumbprintName -Value $settings.SDKRootCertThumbprint > $null
	New-ItemProperty -Path $productRegistryKeyPath -Name $databaseServerInstanceName -Value $dbInstanceTmp  > $null
	New-ItemProperty -Path $productRegistryKeyPath -Name $databaseCatalogName -Value $settings.SDKDatabaseName > $null
	New-ItemProperty -Path $productRegistryKeyPath -Name $applicationServerPortName -PropertyType DWord -Value $settings.SDKApplicationServerPort > $null
}

function Remove-SDKSettings([String] $testProductName)
{
	$productRegistryKeyPath = Get-ProductRegistryPath -testProductName $testProductName

	$private:exists = Test-Path -Path $productRegistryKeyPath
	if (!$private:exists)
	{
		return
	}
	
	Remove-Item -Path $productRegistryKeyPath > $null
}

function New-SelfSignedCodeSigningCertificate([String] $signingCertSubject = "Ivanti")
{
	$query = "CN=$signingCertSubject,*"
	$existing = Get-ChildItem -Path cert:\CurrentUser\My -CodeSigning | Where-Object {$_.Subject -like $query}

	if ($existing -ne $null)
	{
		Write-Host "Code signing certificate already exists."
		return
	}

	$subject = "CN=$signingCertSubject, OU=TEST"

	makecert.exe -r -pe -n $subject -a sha256 -sky signature -eku 1.3.6.1.5.5.7.3.3 -ss My
	
	# Now copy this certificate into the trusted root store.
	$c = Get-ChildItem Cert:\CurrentUser\My | Where-Object -FilterScript {$_.Subject -eq $subject}
	if ($c -eq $null)
	{
		throw "Failed to create self signed certificate"
	}

	Export-Certificate -FilePath "signingCertificateRoot.cer" -Cert $c > $null
	Import-Certificate -FilePath "signingCertificateRoot.cer" -CertStoreLocation "Cert:\LocalMachine\Root" > $null
	Write-Host "The certificate in 'signingCertificateRoot.cer' must be imported into the LocalMachine\Root cert store for any machine where agent binaries will be run."
}

function Select-ValidSigningCerts
{
	Get-ChildItem -Path cert:\CurrentUser\My -CodeSigning | Where-Object {$_.HasPrivateKey}
}

function Get-SelectedSigningSubject
{
	$dir = $PSSCRIPTROOT
	$referencePath = [System.IO.Path]::Combine($dir, ".signingSubject")
	if (-not (Test-Path -Path $referencePath))
	{
		return $null
	}
	Get-Content -Path $referencePath
}

function Select-CodeSigningCert([switch]$interactive)
{
	$cs = Select-ValidSigningCerts
	if ($cs.Length -eq 0 -and -not $interactive)
	{
		throw "No valid code signing certificates available. Import one or create a self signed certificate."
	}
	if ($cs.Length -eq 0 -and $interactive)
	{
		Write-Host "No valid code signing certificates exist on this computer. Do you want to create a self-signed code signing certificate?"
		$create = Read-Host -Prompt "[Y] or [N]"
		if ($create -ieq "Y")
		{
			New-SelfSignedCodeSigningCertificate
		}
		else
		{
			throw "No valid code signing certificates available. Import one or create a self signed certificate."
		}
	}

	$lastChance = Select-ValidSigningCerts
	if ($lastChance.Length -eq 0)
	{

		throw "No valid code signing certificates available. Import one or create a self signed certificate."
	}

	$index = 0

	if ($interactive -and $lastChance.Length -eq 1)
	{
		Write-Host "Choosing the following certificate:"
		Write-Host "       Thumbprint                               Subject"
		Write-Host  $lastChance[$index].Thumbprint " | " $lastChance[$index].Subject
	}

	if ($interactive -and $lastChance.Length -gt 1)
	{
		Write-Host "The following certificates were found:"
		#           [10] | 75492DC6928D7600C20E435A1E91783BFE0C742D | CN=foo
		Write-Host "Index        Thumbprint                               Subject"
		for ($i = 0; $i -lt $lastChance.Length; $i++)
		{
			Write-Host "[$i] |" $lastChance[$i].Thumbprint " | " $lastChance[$i].Subject
		}
		$index = Read-Host -Prompt "Choose the index of the certificate you would like to use."
	}

	$subject = $lastChance[$index].Subject -Split "," | Where-Object -FilterScript{$_.StartsWith("CN=")} 
	$subject = $subject.Replace("CN=", "")
	Set-AgentSigningSubject -subject $subject
}

<#
.SYNOPSIS
	Used to find the root authority certificate by thumbprint.

.DESCRIPTION
	Used to find the root authority certificate by thumbprint.

.PARAMETER rootCertThumbprint
	The thumbprint of the certificate.
#>
function Find-RootAuthorityCertificate([String]$rootCertThumbprint)
{
	$certStore = new-object System.Security.Cryptography.X509Certificates.X509Store([System.Security.Cryptography.X509Certificates.StoreName]::Root, [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine)
	$certStore.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadOnly)

	$certs = $certStore.Certificates.Find([System.Security.Cryptography.X509Certificates.X509FindType]::FindByThumbprint, $rootCertThumbprint, $true)

	$certStore.Close()

	if ($certs.Count -ge 0 -and $certs[0].HasPrivateKey)
	{
		return $certs[0]
	}
	else
	{
		return $null
	}
}

<#
.SYNOPSIS
	Used delete a certificate from a store on the system.

.DESCRIPTION
	Used delete a certificate from a store on the system.

.PARAMETER thumbprint
	The thumbprint of the certificate.

.PARAMETER storeName
	The name of the store where the certificate is stored.

.PARAMETER ignorePrivateKey
	If set, don't delete the private key when the certificate is deleted.
#>
function Remove-Certificate([String] $thumbprint,
                            [String] $storeName,
                            [switch] $ignorePrivateKey)
{
	$private:store = new-object System.Security.Cryptography.X509Certificates.X509Store($storeName,	[System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine)
	$private:store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)

	$private:certs = $private:store.Certificates.Find([System.Security.Cryptography.X509Certificates.X509FindType]::FindByThumbprint, $thumbprint, $false)
	if ($private:certs.Count -eq 0)
	{
		return
	}
	
	$private:cert = $private:certs[0]

	$private:store.Remove($private:cert)

	$private:store.Close()
	
	if ($ignorePrivateKey)
	{
		return
	}
	
	if ($private:cert.HasPrivateKey)
	{
		[System.Security.Cryptography.RSACryptoServiceProvider] $private:csp = $private:cert.PrivateKey
		[ST.Core.Security.Cryptography.Certificate]::DeleteKeyContainer($private:csp.CspKeyContainerInfo)
	}
}

<#
.SYNOPSIS
	Used to create a new root certificate for the test system.

.DESCRIPTION
	Used to create a new root certificate for the test system.

.PARAMETER testProductName
	The name of the test product.
#>
function New-RootCertificate([String] $testProductName)
{
	Use-STCore

	$CertSignatureAlgorithm = [ST.Core.Security.Cryptography.SignatureHashAlgorithm]::Sha256
	$CertProviderType = [ST.Core.Security.Cryptography.CryptoProviderType]::RsaAes

	$private:oids = new-object System.Security.Cryptography.OidCollection
	$private:oids.Add([ST.Core.Security.Cryptography.OidConstants]::ClientAuthentication) > $null
	$private:oids.Add([ST.Core.Security.Cryptography.OidConstants]::ServerAuthentication) > $null
	$private:oids.Add([ST.Core.Security.Cryptography.OidConstants]::STAgent) > $null
	$private:oids.Add([ST.Core.Security.Cryptography.OidConstants]::STConsole) > $null
	$private:oids.Add([ST.Core.Security.Cryptography.OidConstants]::STRootAuthority) > $null

	$private:flags = [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyEncipherment -bor
	   [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyAgreement -bor
	   [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DigitalSignature -bor
	   [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::CrlSign -bor
	   [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyCertSign

	$private:extensions = new-object System.Security.Cryptography.X509Certificates.X509ExtensionCollection
	$private:extension = new-object System.Security.Cryptography.X509Certificates.X509KeyUsageExtension($private:flags, $false)
	[void]$private:extensions.Add($extension)
	$private:extension = new-object System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension($private:oids, $false)
	[void]$private:extensions.Add($extension)
	$private:extension = new-object System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension($true, $false, 0, $true)
	[void]$private:extensions.Add($extension)

	[String []] $private:domainParts = [ST.Core.EnvironmentExtensions]::GetComputerName([ST.Core.ComputerNameFormat]::DnsFullyQualified).Split(".")
	
	$private:rootName = "CN=ST Test Root Authority"
	foreach ($private:part in $private:domainParts)
	{
		$private:rootName = $private:rootName + ", DC=" + $private:part
	}
	$private:rootName = $private:rootName + ", O=" + $testProductName

	$private:keyConatiner = [System.String]::Format("{0}_root", [System.Guid]::NewGuid())

	$private:start = [System.DateTime]::UtcNow
	$private:end = $start.AddYears(10)

	$private:rootCertificate = [ST.Core.Security.Cryptography.Certificate]::CreateSelfSigned($private:rootName, $private:start, $private:end, $CertSignatureAlgorithm, $CertProviderType, $private:keyConatiner, 2048, $private:extensions)
	
	$private:certStore = new-object System.Security.Cryptography.X509Certificates.X509Store([System.Security.Cryptography.X509Certificates.StoreName]::Root, [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine)
	$private:certStore.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
	$private:certStore.Add($private:rootCertificate)
	$private:certStore.Close()

	return $private:rootCertificate
}

<#
.SYNOPSIS
	Used create a console certificate request.

.DESCRIPTION
	This function creates a PKCS#10 request for a console certificate.

.PARAMETER consoleId
	The id of the console. This value is placed in the certificate.

.PARAMETER enrollmentStoreName
	The name of the store where enrollment certificates are stored.

.PARAMETER enrollmentStoreLocation
	The location of the store where enrollment certificates are stored.
#>
function New-ConsoleCertificateRequest([String]$consoleId,
                                       [String]$enrollmentStoreName = "TestEnrollment",
                                       [System.Security.Cryptography.X509Certificates.StoreLocation]$enrollmentStoreLocation = [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine)
{
	Use-STCore

	$CertSignatureAlgorithm = [ST.Core.Security.Cryptography.SignatureHashAlgorithm]::Sha256
	$CertProviderType = [ST.Core.Security.Cryptography.CryptoProviderType]::RsaAes

	$oids = new-object System.Security.Cryptography.OidCollection
	$oids.Add([ST.Core.Security.Cryptography.OidConstants]::ClientAuthentication) > $null
	$oids.Add([ST.Core.Security.Cryptography.OidConstants]::ServerAuthentication) > $null
	$oids.Add([ST.Core.Security.Cryptography.OidConstants]::STConsole) > $null

	$flags = [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyEncipherment -bor
	   [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyAgreement -bor
	   [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DigitalSignature -bor
	   [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::CrlSign -bor
	   [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyCertSign

	[String[]]$subjAltNames = @([System.Net.Dns]::GetHostEntry("").HostName, [Environment]::MachineName)

	$extensions = new-object System.Security.Cryptography.X509Certificates.X509ExtensionCollection
	$extension = new-object System.Security.Cryptography.X509Certificates.X509KeyUsageExtension($flags, $true)
	[void]$extensions.Add($extension)
	$extension = new-object System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension($oids, $true)
	[void]$extensions.Add($extension)
	$extension = new-object System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension($true, $true, 1, $true)
	[void]$extensions.Add($extension)
	$extension = new-object ST.Core.Security.Cryptography.X509SubjectAlternativeNameExtension($subjAltNames, $false)
	[void]$extensions.Add($extension)

	$request = [ST.Core.Security.Cryptography.CertificateRequest]::GenerateKeyRequest("CN=$consoleId, OU=ST Console",
	$CertSignatureAlgorithm,
	$CertProviderType,
	"$($consoleId)_console",
	2048,
	$extensions,
	$enrollmentStoreName,
	$enrollmentStoreLocation)

	return ,$request
}

<#
.SYNOPSIS
	Used to deploy the test framework database.

.DESCRIPTION
	Used to deploy the test framework database.

.PARAMETER sourceFile
	The path to the test database dacpac.

.PARAMETER settings
	The test framework settings.
#>
function Publish-TestDatabase([string]$sourceFile, [SDKSettings] $settings)
{
	if (((sqllocaldb i $settings.SDKDatabaseServerInstance) | Select-String "Version:") -eq $null)
	{
		sqllocaldb c $settings.SDKDatabaseServerInstance 13.0
	}

	$sqlPackageCommand = $Env:VSINSTALLDIR + "Common7\IDE\Extensions\Microsoft\SQLDB\DAC\130\SqlPackage.exe"
	$sourceFileArgument = "/sf:`"$([System.IO.Path]::GetFullPath($sourceFile))`"" 
	$targetDatabaseArgument = "/tdn:" + $settings.SDKDatabaseName
	$targetServerName = "/tsn:(localDB)\" + $settings.SDKDatabaseServerInstance
	$argumentList = @('/a:Publish', '/of:true', $sourceFileArgument, $targetServerName, $targetDatabaseArgument, '/p:CreateNewDatabase=true' )

	Write-Host Executing: $sqlPackageCommand 
	Write-Host With arguments:
	$argumentList

	& $sqlPackageCommand $argumentList
}

Add-Type -TypeDefinition @"
public sealed class SDKSettings
{
	public string ProductName;
	public string RegistryPath;
	public string SDKConsoleId;
	public string SDKConsoleCertThumbprint;
	public string SDKRootCertThumbprint;
	public string SDKDatabaseServerInstance;
	public string SDKDatabaseName;
	public string SDKApplicationServerPort;
}
"@
Export-ModuleMember -Function Test-IsNonInteractive
Export-ModuleMember -Function Resolve-MsBuild
Export-ModuleMember -Function Assert-Environment
Export-ModuleMember -Function Assert-Powershell
Export-ModuleMember -Function Assert-Administrator
Export-ModuleMember -Function Add-SignatureForAgent
Export-ModuleMember -Function Use-STCore
Export-ModuleMember -Function Get-SDKSettings
Export-ModuleMember -Function Get-ProductRegistryPath
Export-ModuleMember -Function Save-SDKSettings
Export-ModuleMember -Function Remove-SDKSettings
Export-ModuleMember -Function Set-AgentSigningSubject
Export-ModuleMember -Function New-SelfSignedCodeSigningCertificate
Export-ModuleMember -Function Select-ValidSigningCerts
Export-ModuleMember -Function Select-CodeSigningCert
Export-ModuleMember -Function Get-SelectedSigningSubject
Export-ModuleMember -Function Find-RootAuthorityCertificate
Export-ModuleMember -Function Remove-Certificate
Export-ModuleMember -Function New-RootCertificate
Export-ModuleMember -Function New-ConsoleCertificateRequest
Export-ModuleMember -Function Publish-TestDatabase
