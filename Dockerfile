# Stage 1: Build and publish the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy project file
COPY ["TechCorner_ECommerce/TechCorner_ECommerce.csproj", "TechCorner_ECommerce/"]

# Restore dependencies
RUN dotnet restore "TechCorner_ECommerce/TechCorner_ECommerce.csproj"

# Copy source code
COPY . .

# Publish application
RUN dotnet publish "TechCorner_ECommerce/TechCorner_ECommerce.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

# Install Kerberos library required by Npgsql
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

# Configure ASP.NET Core
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "TechCorner_ECommerce.dll"]
