# Use official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["ProductManagementSystem.csproj", "./"]
RUN dotnet restore "ProductManagementSystem.csproj"

# Copy entire project and build
COPY . .
RUN dotnet publish "ProductManagementSystem.csproj" -c Release -o /app/publish

# Create runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProductManagementSystem.dll"]