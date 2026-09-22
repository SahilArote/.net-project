# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["CollegeComplaintSystem.Web/CollegeComplaintSystem.Web.csproj", "CollegeComplaintSystem.Web/"]
RUN dotnet restore "CollegeComplaintSystem.Web/CollegeComplaintSystem.Web.csproj"

COPY . .
WORKDIR "/src/CollegeComplaintSystem.Web"
RUN dotnet build "CollegeComplaintSystem.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "CollegeComplaintSystem.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CollegeComplaintSystem.Web.dll"]
