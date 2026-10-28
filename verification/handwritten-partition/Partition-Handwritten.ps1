# Separates executable verification views; never rewrites the source handwritten files.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$sqlRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$xmlRoot = [IO.Path]::GetFullPath((Join-Path $sqlRoot '../../Xml.Linq/IVSoftware.Portable.Xml.Linq.Collections'))
$original = Join-Path $sqlRoot 'IVSoftware.Portable.SQLiteMarkdown.MSTest'
$core = Join-Path $sqlRoot 'IVSoftware.Portable.SQLiteMarkdown.Core.Handwritten.Tests'
$modeled = Join-Path $xmlRoot 'SQLiteMarkdown.ModelIntegration.Handwritten.Tests'
$shared = Join-Path $PSScriptRoot 'Shared'
$sdkRoot = Join-Path $env:ProgramFiles 'dotnet/sdk'
$sdk = Get-ChildItem -LiteralPath $sdkRoot -Directory | Where-Object Name -Like '9.*' | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
if (!$sdk) { throw 'This source partitioner requires the installed .NET 9 Roslyn parser.' }
Add-Type -Path (Join-Path $sdk.FullName 'Roslyn/bincore/Microsoft.CodeAnalysis.dll')
Add-Type -Path (Join-Path $sdk.FullName 'Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll')
foreach ($folder in @($core, $modeled, $shared)) { New-Item -ItemType Directory -Force -Path $folder | Out-Null }
$utf8 = [Text.UTF8Encoding]::new($false)
function Write-Source([string]$path, [string]$text) { [IO.File]::WriteAllText($path, $text, $utf8) }
function Hash-Text([string]$text) { [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($utf8.GetBytes($text))) }
function Header([string]$text) {
    # Namespace imports are scaffolding, not handwritten test bodies.
    $lines = $text -split "`n" | Where-Object { $_ -match '^using ' }
    $lines = $lines | Where-Object { $_ -notmatch 'Microsoft.ApplicationInsights|SQLiteMarkdown.MSTest.DemoDB;' }
    return ($lines -join "`n") + "`n"
}
function Wrap([string]$header, [string]$name, [string]$members, [bool]$fixture) {
    $attribute = if ($fixture) { '[TestClass]' } else { '' }
    return "// Handwritten verification view; source provenance is in the adjacent manifest.`n" +
        $header + "`nnamespace IVSoftware.Portable.SQLiteMarkdown.MSTest`n{`n$attribute`npublic partial class $name`n{`n$members`n}`n}`n"
}
$records = @()
$modeledNames = @('Test_QueryFilterFSMs', 'Test_Detect_QueryENB_or_QueryEN_when_IsFiltering', 'Test_DemoFlow')
foreach ($file in @('TestClass_PublicContract.cs', 'TestClass_SQLiteMarkdown.cs', 'TestClass_ObservableQueryFilterSource.cs')) {
    $path = Join-Path $original $file
    $text = [IO.File]::ReadAllText($path)
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($text)
    $class = $tree.GetRoot().DescendantNodes() | Where-Object {
        $_.GetType().Name -eq 'ClassDeclarationSyntax' -and $_.AttributeLists.ToString().Contains('TestClass')
    } | Select-Object -First 1
    $coreMethods = @(); $modelMethods = @(); $fixtures = @()
    foreach ($member in $class.Members) {
        if ($member.GetType().Name -ne 'MethodDeclarationSyntax' -or !$member.AttributeLists.ToString().Contains('TestMethod')) {
            $fixtures += $member.ToString()
            continue
        }
        $name = $member.Identifier.ValueText
        $body = $member.ToString()
        $destination = 'SQLiteMarkdown core'
        if ($name -eq 'Test_PolicyException') { $destination = 'Historical affinity-fixture policy; owner review' }
        elseif ($name -eq 'Test_NoSpuriousFilterQueryDatabaseInstantiation') { $destination = 'XML Nemesis replacement plus SQLite core coverage' }
        elseif ($name -in $modeledNames) {
            $modelMethods += $body
            $destination = 'Preserved integration reference; migration pending'
            if ($name -eq 'Test_Detect_QueryENB_or_QueryEN_when_IsFiltering') {
                $destination = 'XML handwritten integration; executable unchanged'
                Write-Source (Join-Path $modeled 'Test_Detect_QueryENB.Handwritten.cs') (Wrap (Header $text) $class.Identifier.ValueText $body $false)
            }
        }
        else { $coreMethods += $body }
        $records += [pscustomobject]@{
            Source = $file; Class = $class.Identifier.ValueText; Method = $name
            SourceFileSHA256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
            MethodSHA256 = Hash-Text $body; Destination = $destination
            HandwrittenMethodChanged = $false
        }
    }
    $header = Header $text
    Write-Source (Join-Path $shared "$file.Fixtures.cs") (Wrap $header $class.Identifier.ValueText ($fixtures -join "`n") $true)
    Write-Source (Join-Path $core "$file.Core.cs") (Wrap $header $class.Identifier.ValueText ($coreMethods -join "`n") $false)
    if ($modelMethods.Count) { Write-Source (Join-Path $modeled "$file.Modeled.cs.reference") (Wrap $header $class.Identifier.ValueText ($modelMethods -join "`n") $false) }
}
# Preserve a local copy of the historical population helper with its namespace import updated.
$helper = [IO.File]::ReadAllText((Join-Path $original 'MSTestExtensions.cs'))
$helper = $helper.Replace('using IVSoftware.Portable.Collections.Preview;', 'using IVSoftware.Portable.MSTest.Preview;')
Write-Source (Join-Path $shared 'MSTestExtensions.cs') $helper
$records | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'method-manifest.json') -Encoding utf8
Write-Output "Partitioned $($records.Count) methods; originals unchanged."
