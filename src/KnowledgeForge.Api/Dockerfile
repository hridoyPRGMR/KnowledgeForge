FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY KnowledgeForge.sln ./
COPY src/KnowledgeForge.Domain/KnowledgeForge.Domain.csproj src/KnowledgeForge.Domain/
COPY src/KnowledgeForge.Application/KnowledgeForge.Application.csproj src/KnowledgeForge.Application/
COPY src/KnowledgeForge.Infrastructure/KnowledgeForge.Infrastructure.csproj src/KnowledgeForge.Infrastructure/
COPY src/KnowledgeForge.Api/KnowledgeForge.Api.csproj src/KnowledgeForge.Api/

RUN dotnet restore src/KnowledgeForge.Api/KnowledgeForge.Api.csproj

COPY src/ src/
RUN dotnet publish src/KnowledgeForge.Api/KnowledgeForge.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "KnowledgeForge.Api.dll"]
