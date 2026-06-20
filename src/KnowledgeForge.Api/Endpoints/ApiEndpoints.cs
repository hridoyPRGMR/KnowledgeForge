namespace KnowledgeForge.Api.Endpoints;

public static class ApiEndpoints
{
    public static WebApplication MapApiEndpoints(this WebApplication app) =>
        app
            .MapHealthEndpoints()
            .MapBookEndpoints()
            .MapChatEndpoints()
            .MapSummaryEndpoints()
            .MapQuizEndpoints()
            .MapNotesEndpoints()
            .MapProfileEndpoints()
            .MapKnowledgeGraphEndpoints();
}
