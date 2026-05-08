using System.Text.Json.Serialization;
using PropostaService.Application;
using PropostaService.Domain;
using PropostaService.Infrastructure;
using SharedKernel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddPropostaInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

await app.EnsurePropostaDatabaseAsync();

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

app.MapGet("/health", async (PropostaDbContext dbContext, CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
    return canConnect
        ? Results.Ok(new { status = "Healthy", service = "PropostaService", database = "Connected" })
        : Results.Problem("Banco de dados indisponivel.");
});

var propostas = app.MapGroup("/api/propostas");

propostas.MapPost("/", async (CriarPropostaRequest request, PropostaAppService service, CancellationToken cancellationToken) =>
{
    var response = await service.CriarAsync(request, cancellationToken);
    return Results.Created($"/api/propostas/{response.Id}", response);
});

propostas.MapGet("/", async (PropostaStatus? status, PropostaAppService service, CancellationToken cancellationToken) =>
{
    var response = await service.ListarAsync(status, cancellationToken);
    return Results.Ok(response);
});

propostas.MapGet("/{id:guid}", async (Guid id, PropostaAppService service, CancellationToken cancellationToken) =>
{
    var response = await service.BuscarAsync(id, cancellationToken);
    return response is null ? Results.NotFound() : Results.Ok(response);
});

propostas.MapPatch("/{id:guid}/status", async (Guid id, AlterarStatusPropostaRequest request, PropostaAppService service, CancellationToken cancellationToken) =>
{
    var response = await service.AlterarStatusAsync(id, request, cancellationToken);
    return response is null ? Results.NotFound() : Results.Ok(response);
});

app.MapGet("/api/tipos-seguro", (TipoSeguroAppService service) => Results.Ok(service.Listar()));

app.Run();
