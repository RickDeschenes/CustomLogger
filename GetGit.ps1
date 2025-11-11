# Erase credentials for NuGet GitHub package feed
@"
protocol=https
host=nuget.pkg.github.com
"@ | git credential-manager-core erase

# Erase GitHub.com credentials (if present)
@"
protocol=https
host=github.com
"@ | git credential-manager-core erase