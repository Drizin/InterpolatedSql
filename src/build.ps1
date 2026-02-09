[cmdletbinding()]
param(
    [Parameter(Mandatory=$False)]
    [ValidateSet('Release','Debug')]
    [string]$configuration
)

# How to run:
# .\build.ps1
# or
# .\build.ps1 -configuration Debug


$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir

if (-not $PSBoundParameters.ContainsKey('configuration'))
{
	if (Test-Path Release.snk) { $configuration = "Release"; } else { $configuration = "Debug"; }
}
Write-Host "Using configuration $configuration..." -ForegroundColor Yellow

$msbuild = ( 
    "$Env:programfiles\Microsoft Visual Studio\2022\*\MSBuild\15.0\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2022\*\MSBuild\15.0\Bin\msbuild.exe",
    "$Env:programfiles\Microsoft Visual Studio\2022\*\MSBuild\*\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2022\*\MSBuild\*\Bin\msbuild.exe",
    "$Env:programfiles\Microsoft Visual Studio\2019\*\MSBuild\*\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2019\*\MSBuild\*\Bin\msbuild.exe",
    "${Env:ProgramFiles(x86)}\MSBuild\15.0\Bin\MSBuild.exe",
    "${Env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe"
) | Where-Object { Test-Path $_ } | Select-Object -first 1


Remove-Item -Recurse -Force -ErrorAction Ignore ".\packages-local"
Remove-Item -Recurse -Force -ErrorAction Ignore "$env:HOMEDRIVE$env:HOMEPATH\.nuget\packages\interpolatedsql"
Remove-Item -Recurse -Force -ErrorAction Ignore "$env:HOMEDRIVE$env:HOMEPATH\.nuget\packages\interpolatedsql.strongname"
Remove-Item -Recurse -Force -ErrorAction Ignore "$env:HOMEDRIVE$env:HOMEPATH\.nuget\packages\interpolatedsql.dapper"
Remove-Item -Recurse -Force -ErrorAction Ignore "$env:HOMEDRIVE$env:HOMEPATH\.nuget\packages\interpolatedsql.dapper.strongname"

New-Item -ItemType Directory -Force -Path ".\packages-local"


try {

	# when target frameworks are added/modified dotnet clean might fail and we may need to cleanup the old dependency tree
	Get-ChildItem .\ -Recurse | Where{$_.FullName -CMatch ".*\\bin$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
	Get-ChildItem .\ -Recurse | Where{$_.FullName -CMatch ".*\\obj$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
	Get-ChildItem .\ -Recurse | Where{$_.FullName -Match ".*\\obj\\.*project.assets.json$"} | Remove-Item
	Get-ChildItem .\ -Recurse | Where{$_.FullName -Match ".*\.csproj$" -and $_.FullName -NotMatch ".*\\VSExtensions\\" } | ForEach { dotnet clean $_.FullName }

	dotnet clean InterpolatedSql.sln
	dotnet clean InterpolatedSql.Dapper.sln
	dotnet clean InterpolatedSql\InterpolatedSql.csproj
	dotnet clean InterpolatedSql.StrongName\InterpolatedSql.StrongName.csproj
	dotnet clean InterpolatedSql.Dapper\InterpolatedSql.Dapper.csproj
	dotnet clean InterpolatedSql.Dapper.StrongName\InterpolatedSql.Dapper.StrongName.csproj
	
	dotnet restore InterpolatedSql.sln
	dotnet restore InterpolatedSql.Dapper.sln

	
	# InterpolatedSql + nupkg/snupkg (dotnet build is the best at restoring packages; but for deterministic builds we need msbuild)
	dotnet build -c release InterpolatedSql\InterpolatedSql.csproj
	& $msbuild "InterpolatedSql\InterpolatedSql.csproj" `
			   /t:Pack                                        `
			   /p:PackageOutputPath="..\packages-local\"      `
			   '/p:targetFrameworks="netstandard2.0;net462;net472;net5.0;net6.0;net7.0;net8.0;net9.0;net10.0"'  `
			   /p:Configuration=$configuration                `
			   /p:IncludeSymbols=true                         `
			   /p:SymbolPackageFormat=snupkg                  `
			   /verbosity:minimal                             `
			   /p:ContinuousIntegrationBuild=true
	if (! $?) { throw "msbuild failed" }

	# InterpolatedSql.StrongName + nupkg/snupkg (dotnet build is the best at restoring packages; but for deterministic builds we need msbuild)
	dotnet build -c release InterpolatedSql.StrongName\InterpolatedSql.StrongName.csproj
	& $msbuild "InterpolatedSql.StrongName\InterpolatedSql.StrongName.csproj" `
			   /t:Pack                                        `
			   /p:PackageOutputPath="..\packages-local\"      `
			   '/p:targetFrameworks="netstandard2.0;net462;net472;net5.0;net6.0;net7.0;net8.0;net9.0;net10.0"'  `
			   /p:Configuration=$configuration                `
			   /p:IncludeSymbols=true                         `
			   /p:SymbolPackageFormat=snupkg                  `
			   /verbosity:minimal                             `
			   /p:ContinuousIntegrationBuild=true
	if (! $?) { throw "msbuild failed" }

	# InterpolatedSql.Dapper + nupkg/snupkg (dotnet build is the best at restoring packages; but for deterministic builds we need msbuild)
	dotnet build -c release InterpolatedSql.Dapper\InterpolatedSql.Dapper.csproj
	& $msbuild "InterpolatedSql.Dapper\InterpolatedSql.Dapper.csproj" `
			   /t:Pack                                        `
			   /p:PackageOutputPath="..\packages-local\"      `
			   '/p:targetFrameworks="netstandard2.0;net462;net472;net5.0;net6.0;net7.0;net8.0;net9.0;net10.0"'  `
			   /p:Configuration=$configuration                `
			   /p:IncludeSymbols=true                         `
			   /p:SymbolPackageFormat=snupkg                  `
			   /verbosity:minimal                             `
			   /p:ContinuousIntegrationBuild=true
	if (! $?) { throw "msbuild failed" }

	# InterpolatedSql.Dapper.StrongName + nupkg/snupkg (dotnet build is the best at restoring packages; but for deterministic builds we need msbuild)
	dotnet build -c release InterpolatedSql.Dapper.StrongName\InterpolatedSql.Dapper.StrongName.csproj
	& $msbuild "InterpolatedSql.Dapper.StrongName\InterpolatedSql.Dapper.StrongName.csproj" `
			   /t:Pack                                        `
			   /p:PackageOutputPath="..\packages-local\"      `
			   '/p:targetFrameworks="netstandard2.0;net462;net472;net5.0;net6.0;net7.0;net8.0;net9.0;net10.0"'  `
			   /p:Configuration=$configuration                `
			   /p:IncludeSymbols=true                         `
			   /p:SymbolPackageFormat=snupkg                  `
			   /verbosity:minimal                             `
			   /p:ContinuousIntegrationBuild=true
	if (! $?) { throw "msbuild failed" }

} finally {
    Pop-Location
}


# Unit tests
if ($configuration -eq "Debug")
{
    dotnet build -c release InterpolatedSql.Tests\InterpolatedSql.Tests.csproj
    dotnet test  InterpolatedSql.Tests\InterpolatedSql.Tests.csproj
	
    dotnet build -c release InterpolatedSql.Dapper.Tests\InterpolatedSql.Dapper.Tests.csproj
    dotnet test  InterpolatedSql.Dapper.Tests\InterpolatedSql.Dapper.Tests.csproj

}
