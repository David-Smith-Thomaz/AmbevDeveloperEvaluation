
# 🚀 Developer Evaluation Project – Sales API

Este projeto apresenta a implementação de uma API robusta para **Gerenciamento de Vendas**, desenvolvida como parte de uma avaliação técnica para desenvolvedores masters. A solução demonstra domínio em tecnologias modernas e boas práticas de arquitetura de software, incluindo **.NET 8.0**, **Domain-Driven Design (DDD)**, **Clean Architecture**, testes automatizados e **segurança JWT**.

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

## ⚙️ Como Executar o Projeto

### Pré-requisitos
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- Docker Desktop
- Ferramenta EF Core CLI:
  ```bash
  dotnet tool install --global dotnet-ef --version 8.0.0
  ```

### 1. Clonar o Repositório

```bash
git clone https://github.com/SEU_USUARIO/SEU_REPOSITORIO.git
cd SEU_REPOSITORIO
```

> Substitua `SEU_USUARIO` e `SEU_REPOSITORIO` conforme o seu repositório GitHub.

---

### 2. Configurar Docker Compose

Verifique o arquivo `docker-compose.yml`:

#### Banco de Dados (PostgreSQL)

```yaml
environment:
  POSTGRES_DB: developer_evaluation
  POSTGRES_USER: developer
  POSTGRES_PASSWORD: ev@luAt10n
```

#### API Web

```yaml
environment:
  - ConnectionStrings__DefaultConnection=Host=ambev.developerevaluation.database;Port=5432;Database=developer_evaluation;Username=developer;Password=ev@luAt10n
```

#### Subir os Serviços:

```bash
docker-compose down --volumes   # Limpeza de execuções anteriores
docker-compose up -d            # Inicia os contêineres
```

Verifique os serviços com:

```bash
docker-compose ps
```

---

### 3. Aplicar Migrações (EF Core)

```bash
dotnet ef database update   --project src/Ambev.DeveloperEvaluation.ORM/Ambev.DeveloperEvaluation.ORM.csproj   --startup-project src/Ambev.DeveloperEvaluation.WebApi/Ambev.DeveloperEvaluation.WebApi.csproj   --context DefaultContext
```

---

### 4. Executar a API

```bash
cd src/Ambev.DeveloperEvaluation.WebApi
dotnet run --launch-profile https
```

URLs disponíveis serão exibidas no console, como:

- `https://localhost:7181/swagger`

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
