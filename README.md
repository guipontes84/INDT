# Seguro Plataforma

Plataforma simples para gerenciamento de propostas de seguro e contratacao de propostas aprovadas.

## Tecnologias

- .NET 8
- ASP.NET Core
- Arquitetura Hexagonal
- RabbitMQ
- SQL Server
- Angular
- Docker
- xUnit

## Estrutura

```text
back/
  src/
    BuildingBlocks/
    PropostaService/
    ContratacaoService/
  tests/
front/
  seguro-web/
```

## Executar backend local

```powershell
dotnet restore back/SeguroPlataforma.sln
dotnet run --project back/src/PropostaService/PropostaService.Api/PropostaService.Api.csproj
dotnet run --project back/src/ContratacaoService/ContratacaoService.Api/ContratacaoService.Api.csproj
```

Endpoints principais:

- `GET http://localhost:5001/api/propostas`
- `POST http://localhost:5001/api/propostas`
- `PATCH http://localhost:5001/api/propostas/{id}/status`
- `GET http://localhost:5002/api/contratacoes`
- `POST http://localhost:5002/api/contratacoes`
- `GET /health` em cada API

## Docker Compose

```powershell
docker compose -f back/docker-compose.yml up --build
```

RabbitMQ Management:

- URL: `http://localhost:15672`
- Usuario: `guest`
- Senha: `guest`

## Frontend

```powershell
cd front/seguro-web
npm install
npm start
```

## Testes

```powershell
dotnet test back/SeguroPlataforma.sln
```

## Banco de dados

As APIs usam SQL Server via EF Core e criam as tabelas automaticamente na inicializacao.

Credenciais do SQL Server no Docker:

- Usuario: `sa`
- Senha: `Seguro@12345`

Connection strings dos containers:

- PropostaService: `Server=sqlserver,1433;Database=SeguroPropostaDb;User Id=sa;Password=Seguro@12345;TrustServerCertificate=True;Encrypt=False`
- ContratacaoService: `Server=sqlserver,1433;Database=SeguroContratacaoDb;User Id=sa;Password=Seguro@12345;TrustServerCertificate=True;Encrypt=False`

## Status atual

A base inicial dos projetos foi criada com persistencia real em SQL Server via Docker. RabbitMQ ainda esta disponivel no Compose, mas a publicacao/consumo real com MassTransit fica como proxima evolucao.
