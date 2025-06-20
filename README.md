
# 🚀 Developer Evaluation Project – Sales API

Este projeto foi desenvolvido como parte de uma avaliação técnica voltada a candidatos à posição de Desenvolvedor Master. Seu propósito é validar competências técnicas e práticas de engenharia de software por meio da implementação completa de uma API de Gerenciamento de Vendas, estruturada com tecnologias modernas e padrões de arquitetura robustos.

Ao longo da solução, são abordados os seguintes pilares:

✅ Proficiência com C# e .NET 8.0
🧱 Separação de responsabilidades com Clean Architecture e DDD
🗃️ Persistência com PostgreSQL e aberto para integrar com MongoDB
🧩 Aplicação de padrões como CQRS e Mediator
🔁 Mapeamento com Entity Framework Core e AutoMapper
🧪 Testes automatizados com xUnit, NSubstitute e Bogus
🔐 Segurança com autenticação JWT e proteção de endpoints
⚙️ Construção de APIs RESTful com paginação, ordenação e filtros
🧯 Tratamento padronizado de erros com middleware customizado
🐳 Integração com Docker e Docker Compose
🌀 Controle de versão com Git, uso de Git Flow e commits semânticos
🚀 Foco em performance, clareza de código e implementação precisa das regras de negócio

Esta avaliação não apenas testa a proficiência técnica, mas também a capacidade de aplicar boas práticas em um cenário realista, como se estivesse inserido diretamente em um projeto de produção. É um exercício completo de engenharia de software moderna, alinhado às expectativas de um profissional sênior.

---

## ✨ Visão Geral

A Sales API oferece operações completas de **CRUD** para o gerenciamento de vendas, com aplicação de regras de negócio complexas e uso de padrões arquiteturais consagrados, garantindo escalabilidade, manutenibilidade e clareza de domínio.

---

## 📚 Tecnologias Utilizadas

- **Backend:** C#, .NET 8.0, ASP.NET Core
- **Banco de Dados:** PostgreSQL (via Entity Framework Core)
- **Testes:** xUnit, NSubstitute, FluentAssertions, Bogus (Faker)
- **Contêineres:** Docker, Docker Compose

### Bibliotecas e Frameworks:

- **MediatR** – Padrões CQRS e Mediator  
- **AutoMapper** – Mapeamento de objetos  
- **FluentValidation** – Validação de dados  
- **EF Core + Npgsql** – ORM para PostgreSQL  
- **System.Linq.Dynamic.Core** – Consultas dinâmicas  
- **Serilog** – Logging estruturado

---

## 🎯 Funcionalidades Principais

### API de Vendas (CRUD Completo)
- Criação, leitura (por ID e lista), atualização e cancelamento de vendas.
- Adição e remoção de itens em vendas existentes.

### Regras de Negócio Implementadas
- Descontos progressivos por quantidade:
  - 10% para 4–9 unidades do mesmo item
  - 20% para 10–20 unidades
- Limite máximo de 20 unidades por item.
- Operações bloqueadas em vendas/itens cancelados.

### Arquitetura Limpa (Clean Architecture)
- Camadas bem definidas: `Domain`, `Application`, `Infrastructure`, `WebAPI`.
- Uso de **Agregados** (`Sale`, `SaleItem`) com encapsulamento de lógica.
- Referência a entidades externas (Cliente, Produto, Filial) de forma desnormalizada.

### Padrão CQRS com MediatR
- Separação entre comandos e queries:
  - Ex.: `CreateSaleCommand`, `UpdateSaleCommand`, `ListSalesQuery`

### Persistência Avançada com EF Core
- Mapeamento via Migrations
- Concurrency control com `RowVersion`
- Controle de gráficos de objetos entre `Sale` e `SaleItems`

### API RESTful
- Endpoints com uso semântico de status HTTP (`200`, `201`, `400`, `404`, `401`)
- Suporte a paginação, ordenação e filtros dinâmicos via query params

### Middleware de Erros
- Tratamento padronizado de exceções:
  - `DomainValidationException` → 400
  - `ResourceNotFoundException` → 404

### Segurança com JWT
- Endpoints protegidos via `[Authorize]`
- Integração com sistema de autenticação existente

### Testes Automatizados
- Cobertura unitária da camada `Application` e `Domain`
- Verificação de regras de negócio e fluxo de orquestração

### Eventos de Domínio
- Publicação simulada de eventos:
  - `SaleCreatedEvent`, `SaleModifiedEvent`, `ItemCancelledEvent`

---

## ⚙️ Passo a Passo para Executar o Projeto

### 1. Instalar o SDK do .NET 8.0

Acesse o site oficial do .NET e baixe o SDK:

👉 https://dotnet.microsoft.com/en-us/download/dotnet/8.0

Escolha o instalador conforme seu sistema operacional (Windows, macOS, Linux) e conclua a instalação.

Após a instalação, verifique se está funcionando corretamente:

```bash
dotnet --version
```

O resultado deve ser algo como `8.0.xxx`.

---

### 2. Instalar a ferramenta de linha de comando do EF Core

Execute o seguinte comando no terminal para instalar a ferramenta global:

```bash
dotnet tool install --global dotnet-ef --version 8.0.0
```

Depois, confirme a instalação:

```bash
dotnet ef
```

---

### 3. Instalar o Docker Desktop

Acesse o site oficial do Docker:

👉 https://www.docker.com/products/docker-desktop/

Baixe e instale o Docker Desktop. Após instalado, **abra o Docker e mantenha-o em execução**.

Verifique se está funcionando:

```bash
docker --version
```

E teste se os containers podem ser executados:

```bash
docker run hello-world
```

---

### 4. Clonar o Projeto

#### a. Criar uma pasta local

Crie uma pasta onde você deseja armazenar o projeto. Exemplo: na Área de Trabalho (Windows):

```bash
cd %USERPROFILE%\Desktop
mkdir MeuRepositorio
cd MeuRepositorio
```

No Linux/macOS:

```bash
cd ~/Desktop
mkdir MeuRepositorio
cd MeuRepositorio
```

#### b. Clonar o repositório

Substitua abaixo com o endereço do seu repositório real:

```bash
git clone https://github.com/David-Smith-Thomaz/AmbevDeveloperEvaluation.git cd SEU_REPOSITORIO
```

---

### 5. Subir os Containers com Docker Compose

Antes de tudo, garanta que nenhum container antigo esteja rodando:

```bash
docker-compose down --volumes
```

Agora suba os serviços:

```bash
docker-compose up -d
```

Espere até que todos os containers estejam em estado "Up":

```bash
docker-compose ps
```

---

### 6. Aplicar as Migrações do Entity Framework Core

Execute o comando abaixo a partir da raiz do projeto (onde está o arquivo `.sln`):

```bash
dotnet ef database update \
  --project src/Ambev.DeveloperEvaluation.ORM/Ambev.DeveloperEvaluation.ORM.csproj \
  --startup-project src/Ambev.DeveloperEvaluation.WebApi/Ambev.DeveloperEvaluation.WebApi.csproj \
  --context DefaultContext
```

Esse comando criará todas as tabelas no banco PostgreSQL rodando no container.

---

### 7. Executar a API

Navegue até a pasta do projeto WebApi:

```bash
cd src/Ambev.DeveloperEvaluation.WebApi
```

Execute a API com o perfil HTTPS:

```bash
dotnet run --launch-profile https
```

---

## 🧪 Testando a API via Swagger

### 1. Criar um Usuário

```json
POST /api/users
{
  "username": "testuser",
  "email": "test@example.com",
  "phone": "+5511987654321",
  "password": "Password123!",
  "status": "Active",
  "role": "Customer"
}
```

---

### 2. Autenticar e Obter JWT

```json
POST /api/auth/authenticate
{
  "email": "test@example.com",
  "password": "Password123!"
}
```

Copie o token retornado.

---

### 3. Autorizar no Swagger

Clique em **Authorize** e insira:

```
Bearer SEU_TOKEN_AQUI
```

---

### 4. Testar Endpoints de Vendas

- `Criação de Venda: POST /api/sales`
- `Listagem de Vendas: GET /api/sales (suporta paginação, ordenação e filtros)`
- `Obtenção de Venda por ID: GET /api/sales/{id}`
- `Atualização de Venda: PUT /api/sales/{id}`
- `Cancelamento de Venda: PUT /api/sales/{id}/cancel`
- `Adição de Item à Venda: POST /api/sales/{saleId}/items`
- `Remoção de Item de Venda: DELETE /api/sales/{saleId}/items/{itemId}`
- `Obtenção de Item de Venda por ID: GET /api/sales/{saleId}/items/{itemId}`

---

### 5. Testar Acesso sem Token

- Clique em **Logout** em Swagger
- Tente acessar novamente os endpoints
- Resultado esperado: `401 Unauthorized`

---

## ✅ Executando os Testes Automatizados

Na raiz da solução:

```bash
dotnet test Ambev.DeveloperEvaluation.sln
```

Todos os testes da `Application` e `Domain` devem passar com sucesso.

---

## 📬 Contato

Dúvidas ou sugestões? Fique à vontade para abrir uma issue ou pull request.
