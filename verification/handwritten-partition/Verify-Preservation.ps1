# Read-only provenance check for the executable handwritten views.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$sqlRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$xmlRoot = [IO.Path]::GetFullPath((Join-Path $sqlRoot '../../Xml.Linq/IVSoftware.Portable.Xml.Linq.Collections'))
$original = Join-Path $sqlRoot 'IVSoftware.Portable.SQLiteMarkdown.MSTest'
$manifest = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'method-manifest.json') -Raw | ConvertFrom-Json
$sdk = Get-ChildItem -LiteralPath (Join-Path $env:ProgramFiles 'dotnet/sdk') -Directory |
    Where-Object Name -Like '9.*' | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
if (!$sdk) { throw 'This read-only verifier requires the installed .NET 9 Roslyn parser.' }
Add-Type -Path (Join-Path $sdk.FullName 'Roslyn/bincore/Microsoft.CodeAnalysis.dll')
Add-Type -Path (Join-Path $sdk.FullName 'Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll')
$utf8 = [Text.UTF8Encoding]::new($false)
$observed = @{}
foreach ($group in ($manifest | Group-Object Source)) {
    $path = Join-Path $original $group.Name
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $group.Group[0].SourceFileSHA256) {
        throw "Original handwritten source changed: $($group.Name)"
    }
}
$copies = @(Get-ChildItem -LiteralPath (Join-Path $sqlRoot 'IVSoftware.Portable.SQLiteMarkdown.Core.Handwritten.Tests') -Filter '*.Core.cs')
$copies += Get-Item -LiteralPath (Join-Path $xmlRoot 'SQLiteMarkdown.ModelIntegration.Handwritten.Tests/Test_Detect_QueryENB.Handwritten.cs')
foreach ($file in $copies) {
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText([IO.File]::ReadAllText($file.FullName))
    foreach ($method in ($tree.GetRoot().DescendantNodes() | Where-Object {
        $_.GetType().Name -eq 'MethodDeclarationSyntax' -and $_.AttributeLists.ToString().Contains('TestMethod')
    })) {
        $name = $method.Identifier.ValueText
        $entry = @($manifest | Where-Object Method -EQ $name)
        if ($entry.Count -ne 1 -or $observed.ContainsKey($name)) { throw "Ambiguous or duplicated method: $name" }
        $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($utf8.GetBytes($method.ToString())))
        if ($hash -ne $entry[0].MethodSHA256) { throw "Handwritten method changed: $name" }
        $observed[$name] = $true
    }
}
if ($observed.Count -ne 30) { throw "Expected 30 executable preserved methods; found $($observed.Count)." }
Write-Output 'Preservation verified: 3 original files unchanged; 30 executable handwritten methods match exactly.'
