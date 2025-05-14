# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG CAAS_API_KEY
WORKDIR /src

# Copy only needed csproj files and restore
COPY API/API.csproj API/
COPY BusinessLogicLayer/BusinessLogicLayer.csproj BusinessLogicLayer/
COPY DataAccessLayer/DataAccessLayer.csproj DataAccessLayer/
RUN dotnet restore API/API.csproj

# Copy everything else
COPY . .

# Build and publish
WORKDIR /src/API
RUN dotnet publish -c Release -o /app/publish

# Final runtime image
FROM base
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8081
ENV CAT_API_KEY=${CAAS_API_KEY}
ENTRYPOINT ["dotnet", "API.dll"]
