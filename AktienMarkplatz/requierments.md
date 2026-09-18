dotnet --version
dotnet dev-certs https --trust
dotnet tool install --global dotnet-ef
dotnet restore
dotnet ef database update