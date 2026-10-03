param([string]$InnoCompiler)
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    if (-not $InnoCompiler) {
        $command = Get-Command ISCC.exe -ErrorAction SilentlyContinue
        if ($command) { $InnoCompiler = $command.Source }
        else {
            $candidates = @(
                (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
                (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe'),
                (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe')
            )
            $InnoCompiler = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
        }
    }
    if (-not $InnoCompiler -or -not (Test-Path -LiteralPath $InnoCompiler)) {
        throw 'Install Inno Setup 6 or pass -InnoCompiler with the full path to ISCC.exe.'
    }
    dotnet publish .\src\PDFgearClassic\PDFgearClassic.csproj -c Release -r win-x64 --self-contained true -o .\publish
    if ($LASTEXITCODE -ne 0) { throw 'Publishing PDFgear Classic failed.' }
    New-Item -ItemType Directory -Path .\publish\licenses -Force | Out-Null
    Copy-Item .\licenses\*.txt .\publish\licenses -Force
    $dependencies = Get-Content -LiteralPath .\publish\PDFgearClassic.deps.json -Raw | ConvertFrom-Json
    $packageRoot = if ($env:NUGET_PACKAGES) { $env:NUGET_PACKAGES } else { Join-Path $env:USERPROFILE '.nuget\packages' }
    foreach ($library in $dependencies.libraries.PSObject.Properties.Name) {
        if ($library -match '^runtimepack\.(Microsoft\.(NETCore|WindowsDesktop)\.App\.Runtime\.win-x64)/(.+)$') {
            $packageName = $Matches[1].ToLowerInvariant()
            $packageVersion = $Matches[3]
            $packageDirectory = Join-Path (Join-Path $packageRoot $packageName) $packageVersion
            $noticeDirectory = Join-Path .\publish\licenses $packageName
            New-Item -ItemType Directory -Path $noticeDirectory -Force | Out-Null
            Get-ChildItem -LiteralPath $packageDirectory -File | Where-Object { $_.Name -match '^(LICENSE|THIRD-PARTY-NOTICES)' } | Copy-Item -Destination $noticeDirectory -Force
        }
    }
    & $InnoCompiler .\installer\PDFgearClassic.iss
    if ($LASTEXITCODE -ne 0) { throw 'Building the installer failed.' }
} finally {
    Pop-Location
}
