using ContratacaoService.Application;
using ContratacaoService.Domain;
using ContratacaoService.Infrastructure;
using SharedKernel;

namespace ContratacaoService.Tests;

public sealed class ContratacaoAppServiceTests
{
    [Fact]
    public async Task ContratarAsync_DeveContratarPropostaAprovada()
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, "Aprovada", DateTime.UtcNow));
        var service = new ContratacaoAppService(contratacoes, propostas);

        var contratacao = await service.ContratarAsync(new ContratarPropostaRequest(propostaId));

        Assert.Equal(propostaId, contratacao.PropostaId);
    }

    [Fact]
    public async Task ContratarAsync_DeveBloquearPropostaNaoAprovada()
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, "Rejeitada", DateTime.UtcNow));
        var service = new ContratacaoAppService(contratacoes, propostas);

        await Assert.ThrowsAsync<DomainException>(() => service.ContratarAsync(new ContratarPropostaRequest(propostaId)));
    }

    [Fact]
    public async Task ContratarAsync_DeveImpedirDuplicidade()
    {
        var contratacoes = new InMemoryContratacaoRepository();
        var propostas = new InMemoryPropostaResumoRepository();
        var propostaId = Guid.NewGuid();
        await propostas.UpsertAsync(new PropostaResumo(propostaId, "Aprovada", DateTime.UtcNow));
        var service = new ContratacaoAppService(contratacoes, propostas);

        await service.ContratarAsync(new ContratarPropostaRequest(propostaId));

        await Assert.ThrowsAsync<DomainException>(() => service.ContratarAsync(new ContratarPropostaRequest(propostaId)));
    }
}
