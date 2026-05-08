using Messaging;
using PropostaService.Application;
using PropostaService.Domain;
using PropostaService.Infrastructure;

namespace PropostaService.Tests;

public sealed class PropostaAppServiceTests
{
    [Fact]
    public async Task CriarAsync_DeveCriarPropostaEmAnaliseEPublicarEvento()
    {
        var eventBus = new InMemoryEventBus();
        var service = new PropostaAppService(new InMemoryPropostaRepository(), eventBus);

        var proposta = await service.CriarAsync(new CriarPropostaRequest("Ana", "123", TipoSeguro.Auto, 1500));

        Assert.Equal(PropostaStatus.EmAnalise, proposta.Status);
        Assert.Contains(eventBus.PublishedEvents, evento => evento is PropostaCriadaEvent);
    }

    [Fact]
    public async Task AlterarStatusAsync_DeveAprovarPropostaEPublicarEvento()
    {
        var eventBus = new InMemoryEventBus();
        var service = new PropostaAppService(new InMemoryPropostaRepository(), eventBus);
        var proposta = await service.CriarAsync(new CriarPropostaRequest("Ana", "123", TipoSeguro.Auto, 1500));

        var aprovada = await service.AlterarStatusAsync(proposta.Id, new AlterarStatusPropostaRequest(PropostaStatus.Aprovada));

        Assert.NotNull(aprovada);
        Assert.Equal(PropostaStatus.Aprovada, aprovada.Status);
        Assert.Contains(eventBus.PublishedEvents, evento => evento is PropostaAprovadaEvent);
    }
}
