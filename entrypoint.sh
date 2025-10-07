set -e

# Apply migrations
dotnet ef database update --project JobSearcher.csproj

# Start app
dotnet JobSearcher.dll