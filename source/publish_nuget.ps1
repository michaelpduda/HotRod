# This file is part of the HotRod project, which is released under MIT License.
# See LICENSE.md or visit:
# https://github.com/michaelpduda/hotrod/blob/master/LICENSE.md

Param(
  [Parameter(Mandatory=$true)]
  [Security.SecureString] $apikey
)

Write-Host "`n`n*** Building projects ***`n`n"
dotnet build -c Release

Write-Host "`n`n*** Packing projects ***`n`n"
dotnet pack -c Release

$clearapikey = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($apikey))

Write-Host "`n`n*** Publishing HotRod to Nuget ***`n`n"
$version = [Version] $([Xml] (Get-Content .\HotRod\HotRod.csproj)).Project.PropertyGroup.Version
dotnet nuget push "HotRod\bin\Release\HotRod.$($version.Major).$($version.Minor).$($version.Build).nupkg" --api-key $clearapikey --source "https://api.nuget.org/v3/index.json"

Write-Host "`n`n*** Publishing HotRod.LiteDb to Nuget ***`n`n"
$version = [Version] $([Xml] (Get-Content .\HotRod.LiteDb\HotRod.LiteDb.csproj)).Project.PropertyGroup.Version
dotnet nuget push "HotRod.LiteDb\bin\Release\HotRod.LiteDb.$($version.Major).$($version.Minor).$($version.Build).nupkg" --api-key $clearapikey --source "https://api.nuget.org/v3/index.json"
