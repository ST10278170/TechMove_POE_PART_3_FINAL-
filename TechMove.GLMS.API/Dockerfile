# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["TechMove.GLMS.API/TechMove.GLMS.API.csproj", "TechMove.GLMS.API/"]
RUN dotnet restore "TechMove.GLMS.API/TechMove.GLMS.API.csproj"

# Copy the remaining files and build
COPY TechMove.GLMS.API/ TechMove.GLMS.API/
WORKDIR "/src/TechMove.GLMS.API"
RUN dotnet build "TechMove.GLMS.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "TechMove.GLMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TechMove.GLMS.API.dll"]
