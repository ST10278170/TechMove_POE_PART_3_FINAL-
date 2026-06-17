# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files for restore
COPY ["TechMove.GLMS/TechMove.GLMS.csproj", "TechMove.GLMS/"]
COPY ["TechMove.GLMS.API/TechMove.GLMS.API.csproj", "TechMove.GLMS.API/"]
RUN dotnet restore "TechMove.GLMS/TechMove.GLMS.csproj"

# Copy all files and build
COPY TechMove.GLMS/ TechMove.GLMS/
COPY TechMove.GLMS.API/ TechMove.GLMS.API/
WORKDIR "/src/TechMove.GLMS"
RUN dotnet build "TechMove.GLMS.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "TechMove.GLMS.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TechMove.GLMS.dll"]
