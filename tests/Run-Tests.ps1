$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$buildDir = Join-Path $projectRoot 'tests\build'
New-Item -ItemType Directory -Path $buildDir -Force | Out-Null
$source = Get-Content -LiteralPath (Join-Path $projectRoot 'CreateGrid2D.iLogicVb') -Raw
$source = $source.Replace('Imports System.Collections.Generic', '')
$header = @'
Imports System
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports Microsoft.VisualBasic
Imports Inventor
Public Class RuleUnderTest
    Public ThisApplication As Inventor.Application
'@
$tests = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'GeometryTests.vb') -Raw
$wrapper = $header + [Environment]::NewLine + $source + [Environment]::NewLine + 'End Class' + [Environment]::NewLine + $tests
$wrapperPath = Join-Path $buildDir 'RuleTests.vb'
Set-Content -LiteralPath $wrapperPath -Value $wrapper -Encoding UTF8
$compiler = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\vbc.exe'
$interop = 'C:\Program Files\Autodesk\Inventor 2027\Bin\Autodesk.Inventor.Interop.dll'
$executable = Join-Path $buildDir 'RuleTests.exe'
& $compiler /nologo /target:exe /optionexplicit+ /optionstrict- /main:GeometryTests "/out:$executable" /reference:System.dll /reference:System.Core.dll /reference:System.Windows.Forms.dll "/reference:$interop" $wrapperPath
if ($LASTEXITCODE -ne 0) { throw 'VB compilation failed.' }
# The interop is needed for metadata resolution, even though tests do not create COM objects.
Copy-Item -LiteralPath $interop -Destination $buildDir -Force
& $executable
if ($LASTEXITCODE -ne 0) { throw 'Regression tests failed.' }
