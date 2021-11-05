[CmdletBinding()]
Param(
    [Parameter(Mandatory=$false)]
    [switch]$Run,
    [Parameter(Mandatory=$false)]
    [switch]$Test,
    [Parameter(Mandatory=$false)]
    [switch]$Cover
)

Write-Verbose "PowerShell $($PSVersionTable.PSEdition) version $($PSVersionTable.PSVersion)"

Set-StrictMode -Version 2.0; $ErrorActionPreference = "Stop"; $ConfirmPreference = "None"; trap { Write-Error $_ -ErrorAction Continue; exit 1 }
$PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent

###########################################################################
# CONFIGURATION
###########################################################################

$DotNetGlobalFile = "$PSScriptRoot\\global.json"
$DotNetInstallUrl = "https://dot.net/v1/dotnet-install.ps1"
$DotNetChannel = "Current"

$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 1
$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1
$env:DOTNET_MULTILEVEL_LOOKUP = 0

###########################################################################
# EXECUTION
###########################################################################
function ExecSafe([scriptblock] $cmd) {
    & $cmd
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

# If dotnet CLI is installed globally and it matches requested version, use for execution
if ($null -ne (Get-Command "dotnet" -ErrorAction SilentlyContinue) -and `
     $(dotnet --version) -and $LASTEXITCODE -eq 0) {
    $env:DOTNET_EXE = (Get-Command "dotnet").Path
}
else {
    # Download install script
    $DotNetInstallFile = "$TempDirectory\dotnet-install.ps1"
    New-Item -ItemType Directory -Path $TempDirectory -Force | Out-Null
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
    (New-Object System.Net.WebClient).DownloadFile($DotNetInstallUrl, $DotNetInstallFile)

    # If global.json exists, load expected version
    if (Test-Path $DotNetGlobalFile) {
        $DotNetGlobal = $(Get-Content $DotNetGlobalFile | Out-String | ConvertFrom-Json)
        if ($DotNetGlobal.PSObject.Properties["sdk"] -and $DotNetGlobal.sdk.PSObject.Properties["version"]) {
            $DotNetVersion = $DotNetGlobal.sdk.version
        }
    }

    # Install by channel or version
    $DotNetDirectory = "$TempDirectory\dotnet-win"
    if (!(Test-Path variable:DotNetVersion)) {
        ExecSafe { & $DotNetInstallFile -InstallDir $DotNetDirectory -Channel $DotNetChannel -NoPath }
    } else {
        ExecSafe { & $DotNetInstallFile -InstallDir $DotNetDirectory -Version $DotNetVersion -NoPath }
    }
    $env:DOTNET_EXE = "$DotNetDirectory\dotnet.exe"
}

Write-Verbose "Microsoft (R) .NET Core SDK version $(& $env:DOTNET_EXE --version)"

$ProjectFile= "$PSScriptRoot/src/MetricsProxy.Web/MetricsProxy.Web.csproj"
$TestProjectFile = "$PSScriptRoot/src/MetricsProxy.Tests/MetricsProxy.Tests.csproj"

if($Run){
    ExecSafe { & $env:DOTNET_EXE run --project $ProjectFile --no-build}
}elseif ($Test) {
    ExecSafe { & $env:DOTNET_EXE test $TestProjectFile}
}elseif ($Cover) {
    ExecSafe {if (& $env:DOTNET_EXE tool list --tool-path "$PSScriptRoot/tools" | Select-String "dotnet-reportgenerator-globaltool") {
            Write-Host -f Yellow 'Skipping install of dotnet-reportgenerator-globaltool. It''s already installed'
        }
        else {
            Invoke-Block { & $env:DOTNET_EX tool install --tool-path "$PSScriptRoot/tools" dotnet-reportgenerator-globaltool }
        }}
    ExecSafe { & $env:DOTNET_EXE test $TestProjectFile --results-directory "$PSScriptRoot/test-results" --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura}
    # https://github.com/danielpalme/ReportGenerator - different types of reports
    ExecSafe { & "$PSScriptRoot/tools/reportgenerator.exe" -reports:"$PSScriptRoot/test-results/*/coverage.cobertura.xml" -targetdir:"$PSScriptRoot/test-results" -reporttypes:"Html;TextSummary"}
}