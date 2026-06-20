FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY KnowledgeForge.sln ./
COPY src/KnowledgeForge.Domain/KnowledgeForge.Domain.csproj src/KnowledgeForge.Domain/
COPY src/KnowledgeForge.Application/KnowledgeForge.Application.csproj src/KnowledgeForge.Application/
COPY src/KnowledgeForge.Infrastructure/KnowledgeForge.Infrastructure.csproj src/KnowledgeForge.Infrastructure/
COPY src/KnowledgeForge.Worker/KnowledgeForge.Worker.csproj src/KnowledgeForge.Worker/

RUN dotnet restore src/KnowledgeForge.Worker/KnowledgeForge.Worker.csproj

COPY src/ src/
RUN dotnet publish src/KnowledgeForge.Worker/KnowledgeForge.Worker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KnowledgeForge.Worker.dll"]
