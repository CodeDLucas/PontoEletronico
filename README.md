# Sistema de Ponto Eletrônico

Sistema completo de controle de ponto eletrônico desenvolvido com Angular 15 e .NET 6, permitindo que funcionários registrem e consultem suas marcações de forma segura e intuitiva.

## Índice

- [Pré-requisitos](#pré-requisitos)
- [Configuração e Instalação](#configuração-e-instalação)
  - [1. Clone do Repositório](#1-clone-do-repositório)
  - [2. Configuração do Backend](#2-configuração-do-backend)
  - [3. Configuração do Frontend](#3-configuração-do-frontend)
- [Funcionalidades](#funcionalidades)
  - [Autenticação e Segurança](#autenticação-e-segurança)
  - [Registro de Ponto](#registro-de-ponto)
  - [Visualização e Consulta](#visualização-e-consulta)
- [Arquitetura](#arquitetura)
  - [Backend (.NET 6)](#backend-net-6)
  - [Frontend (Angular 15)](#frontend-angular-15)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [API Endpoints](#api-endpoints)
  - [Autenticação](#autenticação)
  - [Registros de Ponto](#registros-de-ponto)
  - [Usuários](#usuários)
- [Documentação da API](#documentação-da-api)
- [Banco de Dados](#banco-de-dados)
- [Segurança](#segurança)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Configuração para Produção](#configuração-para-produção)
- [Desenvolvimento](#desenvolvimento)
- [Contribuição](#contribuição)
- [Licença](#licença)
- [Suporte](#suporte)

## Pré-requisitos

### Desenvolvimento
- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Node.js 16+](https://nodejs.org/)
- [Angular CLI 15+](https://angular.io/cli)
- [SQL Server LocalDB](https://docs.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) ou SQL Server

### Produção
- Servidor com .NET 6 Runtime
- SQL Server 2019+ ou Azure SQL Database
- Servidor web (IIS, Nginx, ou similar)

## Configuração e Instalação
Para rodar este projeto, você precisará de **duas janelas de prompt de comando separadas**: uma para o backend e outra para o frontend. Ambos os servidores precisam estar rodando simultaneamente para que a aplicação funcione.

### 1. Clone do Repositório
```bash
git clone https://github.com/CodeDLucas/PontoEletronico.git
cd PontoEletronico
```

### 2. Configuração do Backend

#### 2.1 Configurar Connection String
Edite o arquivo `PontoEletronico.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PontoEletronicoDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "SUA_CHAVE_SECRETA_AQUI_MINIMO_32_CARACTERES",
    "Issuer": "PontoEletronicoAPI",
    "Audience": "PontoEletronicoApp",
    "ExpirationMinutes": 60
  }
}
```

#### 2.2 Configurar Banco de Dados
```bash
cd PontoEletronico.Api
dotnet ef database update
```

#### 2.3 Executar API
```bash
cd PontoEletronico.Api
dotnet run
```

A API estará disponível em: `https://localhost:7059`

### 3. Configuração do Frontend

#### 3.1 Instalar Dependências
```bash
cd PontoEletronico.App
npm install
```

#### 3.2 Configurar Environment
Verifique se o arquivo `src/environments/environment.ts` aponta para a API:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7059'
};
```

#### 3.3 Executar Frontend
```bash
ng serve
```

A aplicação estará disponível em: `http://localhost:4200`

## Funcionalidades

### Autenticação e Segurança
- Login seguro com JWT (JSON Web Tokens)
- Registro de novos usuários
- Controle de sessão e renovação automática de tokens
- Logout seguro
- Proteção de rotas com guards

### Registro de Ponto
- Marcação de entrada (Clock In)
- Marcação de saída (Clock Out)
- Registro automático de data e hora
- Descrições opcionais para cada marcação

### Visualização e Consulta
- Dashboard com status do dia em tempo real
- Histórico completo de marcações
- Filtros por período (data início/fim)
- Paginação de resultados
- Ordenação por colunas (data/hora, tipo, descrição)
- Relógio digital em tempo real

## Arquitetura

### Backend (.NET 6)
- **API REST** com ASP.NET Core 6
- **Autenticação** via ASP.NET Identity
- **Banco de dados** SQL Server com Entity Framework Core
- **Validação** com FluentValidation
- **Documentação** automática com Swagger/OpenAPI

### Frontend (Angular 15)
- **Framework** Angular 15.2.0
- **UI Components** Angular Material Design
- **Autenticação** JWT com interceptors
- **Formulários** reativos com validação
- **Responsivo** para desktop e mobile

## Estrutura do Projeto

```
PontoEletronico/
├── PontoEletronico.Api/          # Backend .NET 6
│   ├── Controllers/              # Endpoints da API
│   ├── Services/                 # Lógica de negócio
│   ├── Models/                   # Modelos de dados
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Data/                     # Contexto do banco
│   ├── Validators/               # Validações FluentValidation
│   ├── Middleware/               # Middlewares customizados
│   └── Migrations/               # Migrações do banco
├── PontoEletronico.App/          # Frontend Angular
│   ├── src/app/components/       # Componentes da aplicação
│   ├── src/app/services/         # Serviços Angular
│   ├── src/app/models/           # Interfaces TypeScript
│   ├── src/app/interceptors/     # Interceptors HTTP
│   ├── src/app/guards/           # Route Guards
│   └── src/assets/               # Recursos estáticos
└── README.md                     # Este arquivo
```

## API Endpoints

### Autenticação
- `POST /api/auth/login` - Realizar login
- `POST /api/auth/register` - Registrar novo usuário
- `POST /api/auth/logout` - Realizar logout
- `POST /api/auth/refresh` - Renovar token
- `GET /api/auth/verify` - Verificar token

### Registros de Ponto
- `POST /api/timerecord` - Criar nova marcação
- `GET /api/timerecord` - Listar marcações (com filtros)
- `GET /api/timerecord/{id}` - Obter marcação específica
- `GET /api/timerecord/today` - Marcações do dia atual
- `GET /api/timerecord/summary` - Resumo agrupado por dia
- `DELETE /api/timerecord/{id}` - Remover marcação

### Usuários
- `GET /api/user/profile` - Obter perfil do usuário
- `PUT /api/user/profile` - Atualizar perfil
- `POST /api/user/change-password` - Alterar senha

## Documentação da API

Após executar o backend, acesse a documentação interativa Swagger em:
`https://localhost:7059/swagger`

## Banco de Dados

### Modelo de Dados

#### ApplicationUser
- Id (string) - Identificador único
- FullName (string) - Nome completo
- Email (string) - Email único
- EmployeeCode (string, opcional) - Código do funcionário
- CreatedAt (DateTime) - Data de criação
- IsActive (bool) - Status ativo/inativo

#### TimeRecord
- Id (int) - Identificador único
- UserId (string) - FK para ApplicationUser
- Timestamp (DateTime) - Data/hora da marcação
- Type (enum) - Tipo da marcação (ClockIn/ClockOut/BreakStart/BreakEnd)
- Description (string, opcional) - Descrição da marcação
- CreatedAt (DateTime) - Data de criação do registro

## Segurança

### Medidas Implementadas
- **HTTPS**: Redirecionamento forçado para conexões seguras
- **JWT**: Tokens seguros com expiração configurável
- **CORS**: Configuração específica para domínios permitidos
- **Validação**: Input validation client-side e server-side
- **SQL Injection**: Prevenção via Entity Framework parametrizado
- **Autorização**: Controle de acesso por usuário autenticado

### Configurações de Senha
- Mínimo 6 caracteres
- Pelo menos uma letra minúscula
- Pelo menos uma letra maiúscula
- Pelo menos um número
- Bloqueio após 5 tentativas incorretas

## Tecnologias Utilizadas

### Backend
- .NET 6
- ASP.NET Core Web API
- Entity Framework Core 6.0.33
- ASP.NET Identity
- FluentValidation 11.3.0
- JWT Bearer Authentication
- SQL Server
- Swagger/OpenAPI

### Frontend
- Angular 15.2.0
- Angular Material 15.2.9
- TypeScript 4.9.4
- RxJS 7.8.0
- SCSS
- Angular CLI

## Configuração para Produção

### Backend
1. Atualizar `appsettings.Production.json` com:
   - Connection string para SQL Server de produção
   - Chave JWT segura e única
   - Configurações de logging apropriadas

2. Configurar CORS para domínio de produção

### Frontend
1. Atualizar `environment.prod.ts` com URL da API de produção
2. Executar build de produção: `ng build --configuration=production`
3. Servir arquivos estáticos gerados em `dist/`

## Desenvolvimento

### Comandos Úteis

#### Backend
```bash
# Restaurar pacotes
dotnet restore

# Executar testes
dotnet test

# Criar nova migration
dotnet ef migrations add NomeDaMigration

# Atualizar banco
dotnet ef database update

# Executar em modo desenvolvimento
dotnet run --environment Development
```

#### Frontend
```bash
# Instalar dependências
npm install

# Executar em modo desenvolvimento
ng serve

# Executar testes
ng test

# Build de produção
ng build --configuration=production

# Análise de bundle
ng build --stats-json
npx webpack-bundle-analyzer dist/ponto-eletronico-app/stats.json
```

## Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/nova-funcionalidade`)
3. Commit suas mudanças (`git commit -am 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/nova-funcionalidade`)
5. Abra um Pull Request

## Licença

Este projeto está licenciado sob a MIT License - veja o arquivo [LICENSE.txt](LICENSE.txt) para detalhes.

## Suporte

Para questões relacionadas ao desenvolvimento ou configuração, consulte:
- Documentação da API: `https://localhost:7059/swagger`
- Logs da aplicação: Console do navegador (F12)
- Logs do servidor: Output do terminal onde o backend está executando
