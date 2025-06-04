# Base image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Fix OpenSSL to allow older TLS (not ideal, but needed for SQL Server 2014)
RUN sed -i 's/^\[openssl_init\]/# [openssl_init]/' /etc/ssl/openssl.cnf && \
    echo "" >> /etc/ssl/openssl.cnf && \
    echo "[openssl_init]" >> /etc/ssl/openssl.cnf && \
    echo "ssl_conf = ssl_sect" >> /etc/ssl/openssl.cnf && \
    echo "" >> /etc/ssl/openssl.cnf && \
    echo "[ssl_sect]" >> /etc/ssl/openssl.cnf && \
    echo "system_default = ssl_default_sect" >> /etc/ssl/openssl.cnf && \
    echo "" >> /etc/ssl/openssl.cnf && \
    echo "[ssl_default_sect]" >> /etc/ssl/openssl.cnf && \
    echo "MinProtocol = TLSv1" >> /etc/ssl/openssl.cnf && \
    echo "CipherString = DEFAULT@SECLEVEL=0" >> /etc/ssl/openssl.cnf

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project file and restore dependencies
COPY ["ConfirmMe/ConfirmMe.csproj", "ConfirmMe/"]
RUN dotnet restore "ConfirmMe/ConfirmMe.csproj"

# Copy the rest of the application and build
COPY . .
WORKDIR "/src/ConfirmMe"
RUN dotnet build "ConfirmMe.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ConfirmMe.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ConfirmMe.dll"]
