using System.Text.Json.Serialization;
using ContratacaoService.Application;
using ContratacaoService.Infrastructure;
using Messaging;
using SharedKernel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddContratacaoInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

await app.EnsureContratacaoDatabaseAsync();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (DomainException exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { erro = exception.Message });
    }
});

app.MapGet("/health", async (ContratacaoDbContext dbContext, CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
    return canConnect
        ? Results.Ok(new { status = "Healthy", service = "ContratacaoService", database = "Connected" })
        : Results.Problem("Banco de dados indisponivel.");
});

var contratacoes = app.MapGroup("/api/contratacoes");

contratacoes.MapPost("/", async (ContratarPropostaRequest request, ContratacaoAppService service, CancellationToken cancellationToken) =>
{
    var response = await service.ContratarAsync(request, cancellationToken);
    return Results.Created($"/api/contratacoes/{response.Id}", response);
});

contratacoes.MapGet("/", async (ContratacaoAppService service, CancellationToken cancellationToken) =>
{
    var response = await service.ListarAsync(cancellationToken);
    return Results.Ok(response);
});

contratacoes.MapGet("/{id:guid}", async (Guid id, ContratacaoAppService service, CancellationToken cancellationToken) =>
{
    var response = await service.BuscarAsync(id, cancellationToken);
    return response is null ? Results.NotFound() : Results.Ok(response);
});

var eventos = app.MapGroup("/api/eventos/propostas");

eventos.MapPost("/aprovada", async (PropostaAprovadaEvent evento, PropostaResumoAppService service, CancellationToken cancellationToken) =>
{
    await service.AplicarAsync(evento, cancellationToken);
    return Results.Accepted();
});

eventos.MapPost("/rejeitada", async (PropostaRejeitadaEvent evento, PropostaResumoAppService service, CancellationToken cancellationToken) =>
{
    await service.AplicarAsync(evento, cancellationToken);
    return Results.Accepted();
});

eventos.MapPost("/cancelada", async (PropostaCanceladaEvent evento, PropostaResumoAppService service, CancellationToken cancellationToken) =>
{
    await service.AplicarAsync(evento, cancellationToken);
    return Results.Accepted();
});

app.Run();
