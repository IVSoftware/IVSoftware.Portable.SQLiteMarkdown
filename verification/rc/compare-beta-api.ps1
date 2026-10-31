$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$runtimePath = Join-Path $repoRoot 'IVSoftware.Portable.SQLiteMarkdown.RC.Nemesis.Tests/bin/Debug/net8.0'
$betaPath = Join-Path $env:USERPROFILE '.nuget/packages/ivsoftware.portable.sqlitemarkdown/2.0.0-beta/lib/netstandard2.0/IVSoftware.Portable.SQLiteMarkdown.dll'

function Format-TypeName([Type] $type) {
    if ($type.IsByRef) { return (Format-TypeName $type.GetElementType()) + '&' }
    if ($type.IsArray) { return (Format-TypeName $type.GetElementType()) + '[]' }
    if ($type.IsGenericParameter) { return '!' + $type.GenericParameterPosition }
    if ($type.IsGenericType) {
        return $type.GetGenericTypeDefinition().FullName + '[' + (($type.GetGenericArguments() | ForEach-Object { Format-TypeName $_ }) -join ',') + ']'
    }
    return $type.FullName
}

function Get-PublicApi([string] $path) {
    $context = [System.Runtime.Loader.AssemblyLoadContext]::new([Guid]::NewGuid().ToString(), $true)
    foreach ($name in @('SQLite-net', 'Newtonsoft.Json', 'IVSoftware.Portable.Common', 'IVSoftware.Portable.Threading', 'IVSoftware.Portable.Disposable', 'IVSoftware.Portable.WatchdogTimer', 'IVSoftware.Portable.Xml.Linq.XBoundObject')) {
        $null = $context.LoadFromAssemblyPath((Join-Path $runtimePath ($name + '.dll')))
    }
    $assembly = $context.LoadFromAssemblyPath($path)
    $result = [System.Collections.Generic.HashSet[string]]::new()
    $flags = [System.Reflection.BindingFlags]'Public,Instance,Static,DeclaredOnly'
    foreach ($type in $assembly.GetExportedTypes()) {
        $null = $result.Add('T:' + $type.FullName)
        foreach ($method in @($type.GetMethods($flags)) + @($type.GetConstructors($flags))) {
            $returnType = if ($method -is [System.Reflection.MethodInfo]) { Format-TypeName $method.ReturnType } else { 'ctor' }
            $parameters = ($method.GetParameters() | ForEach-Object { Format-TypeName $_.ParameterType }) -join ','
            $null = $result.Add('M:' + $type.FullName + '|' + $method.Name + '(' + $parameters + ')->' + $returnType)
        }
        foreach ($field in $type.GetFields($flags)) {
            $null = $result.Add('F:' + $type.FullName + '|' + $field.Name + ':' + (Format-TypeName $field.FieldType))
        }
    }
    return ,$result
}

$beta = Get-PublicApi $betaPath
$current = Get-PublicApi (Join-Path $runtimePath 'IVSoftware.Portable.SQLiteMarkdown.dll')
$missing = @($beta | Where-Object { -not $current.Contains($_) } | Sort-Object)
$report = [ordered]@{ Baseline = '2.0.0-beta'; BaselineMembers = $beta.Count; CurrentMembers = $current.Count; Missing = $missing }
$report | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $PSScriptRoot 'beta-api-comparison.json')
$report | ConvertTo-Json -Depth 5
if ($missing.Count -ne 0) { exit 1 }
