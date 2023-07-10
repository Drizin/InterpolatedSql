$msbuild = ( 
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\msbuild.exe",
          "$Env:programfiles\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild.exe",
    "${Env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe",
    "${Env:ProgramFiles(x86)}\MSBuild\12.0\Bin\MSBuild.exe"
) | Where-Object { Test-Path $_ } | Select-Object -first 1

$configuration = 'Release'

Remove-Item -Recurse -Force -ErrorAction Ignore ".\packages-local"
Remove-Item -Recurse -Force -ErrorAction Ignore "$env:HOMEDRIVE$env:HOMEPATH\.nuget\packages\interpolatedsql"
Remove-Item -Recurse -Force -ErrorAction Ignore "$env:HOMEDRIVE$env:HOMEPATH\.nuget\packages\interpolatedsql.strongname"

New-Item -ItemType Directory -Force -Path ".\packages-local"

dotnet clean InterpolatedSql.sln
dotnet clean InterpolatedSql\InterpolatedSql.csproj
dotnet clean InterpolatedSql.StrongName\InterpolatedSql.StrongName.csproj

# InterpolatedSql + nupkg/snupkg (dotnet build is the best at restoring packages; but for deterministic builds we need msbuild)
dotnet build -c release InterpolatedSql\InterpolatedSql.csproj
& $msbuild "InterpolatedSql\InterpolatedSql.csproj" `
           /t:Pack                                        `
           /p:PackageOutputPath="..\packages-local\"      `
           '/p:targetFrameworks="netstandard2.0;net462;net472;net6.0;net7.0"'  `
           /p:Configuration=$configuration                `
           /p:IncludeSymbols=true                         `
           /p:SymbolPackageFormat=snupkg                  `
           /verbosity:minimal                             `
           /p:ContinuousIntegrationBuild=true

# InterpolatedSql.StrongName + nupkg/snupkg (dotnet build is the best at restoring packages; but for deterministic builds we need msbuild)
dotnet build -c release InterpolatedSql.StrongName\InterpolatedSql.StrongName.csproj
& $msbuild "InterpolatedSql.StrongName\InterpolatedSql.StrongName.csproj" `
           /t:Pack                                        `
           /p:PackageOutputPath="..\packages-local\"      `
           '/p:targetFrameworks="netstandard2.0;net462;net472;net6.0;net7.0"'  `
           /p:Configuration=$configuration                `
           /p:IncludeSymbols=true                         `
           /p:SymbolPackageFormat=snupkg                  `
           /verbosity:minimal                             `
           /p:ContinuousIntegrationBuild=true




# Unit tests
if ($configuration -eq "Debug")
{
    dotnet build -c release InterpolatedSql.Tests\InterpolatedSql.Tests.csproj
    dotnet test  InterpolatedSql.Tests\InterpolatedSql.Tests.csproj
}
