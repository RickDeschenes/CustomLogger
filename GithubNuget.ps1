# Ensure script runs from repository root
Set-Location 'C:\Source\CustomLogger\CustomLogger'
$owner = 'rickdeschenes'

# 1) Set token in an env var for this session
$env:NUGET_API_KEY = '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'

# Ensure token is available
if (-not $env:NUGET_API_KEY) { Write-Error 'Environment variable NUGET_API_KEY is not set.'; exit 1 }

# pick newest nupkg
$pkg = Get-ChildItem -Path .\artifacts\*.nupkg -File | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $pkg) { Write-Error 'No .nupkg found in .\artifacts'; exit 1 }

# parse package id/version robustly: find the dot where the remainder starts with a digit (version)
$base = [System.IO.Path]::GetFileNameWithoutExtension($pkg.Name)
$pkgId = $null; $version = $null
for ($i = 0; $i -lt $base.Length; $i++) {
    if ($base[$i] -ne '.') { continue }
    $rest = $base.Substring($i + 1)
    if ($rest -match '^[0-9]+(?:\.[0-9]+)*(?:[-+].+)?$') {
        $pkgId = $base.Substring(0, $i)
        $version = $rest
        break
    }
}
if (-not $pkgId) {
    # fallback: last-dot split (less reliable for ids with dots)
    $lastDot = $base.LastIndexOf('.')
    if ($lastDot -lt 0) { Write-Error "Cannot determine package id/version from file name: $($pkg.Name)"; exit 1 }
    $pkgId = $base.Substring(0, $lastDot)
    $version = $base.Substring($lastDot + 1)
}

# query feed versions using Basic auth (username:token)
$versionsUri = "https://nuget.pkg.github.com/$owner/download/$($pkgId.ToLower())/index.json"
$basic = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("{$owner}:$env:NUGET_API_KEY"))

try {
    $resp = Invoke-RestMethod -Uri $versionsUri -Headers @{ Authorization = "Basic $basic" } -ErrorAction Stop
} catch {
    $status = $null
    if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
        $status = [int]$_.Exception.Response.StatusCode.Value__
    }
    if ($status -eq 404) {
        $resp = $null
    } elseif ($status -eq 401) {
        Write-Error 'Failed to query feed: (401) Unauthorized - token missing package permissions.'
        exit 1
    } else {
        Write-Error "Failed to query feed: $($_.Exception.Message)"
        exit 1
    }
}

if ($resp -and $resp.versions -and ($resp.versions -contains $version)) {
    Write-Output "Package $pkgId $version already exists on feed. Exiting."
    exit 0
}

# Validate token (returns authenticated user JSON)
Invoke-RestMethod -Uri $versionsUri -Headers @{ Authorization = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("RickDeschenes:$env:NUGET_API_KEY")) } | Out-Null

# Check token scopes (shows OAuth scopes header)
(Invoke-WebRequest -Uri 'https://api.github.com/user' -Headers @{ Authorization = "Bearer $env:NUGET_API_KEY" }).Headers['x-oauth-scopes']

# Push the package (from repository root)
dotnet nuget push .\artifacts\*.nupkg --skip-duplicate --api-key $env:NUGET_API_KEY --source "https://nuget.pkg.github.com/rickdeschenes/index.json"

# Remove the env var from this session
Remove-Item Env:NUGET_API_KEY
