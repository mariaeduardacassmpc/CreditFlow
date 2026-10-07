# CreditFlow

API REST para gerenciamento de clientes e fluxo de análise de crédito, desenvolvida com **C# e ASP.NET Core**.
O projeto tem como objetivo construir uma aplicação backend completa, explorando conceitos de **autenticação, segurança, mensageria, processamento assíncrono e arquitetura de APIs REST**.

Durante o desenvolvimento, serão implementados recursos como **autenticação com JWT, autenticação de dois fatores (2FA), recuperação de senha e processamento de eventos utilizando Apache Kafka**.

## Tecnologias

* **C#**
* **.NET 9**
* **ASP.NET Core Web API**
* **Entity Framework Core**
* **PostgreSQL**
* **JWT (JSON Web Token)**
* **Apache Kafka**
* **Swagger / OpenAPI**
* **xUnit**
* **Moq**
* **Docker** *(planejado)*


## Objetivos do projeto

O CreditFlow será desenvolvido com foco em:

* Construção de APIs REST
* Organização e separação de responsabilidades
* Boas práticas de desenvolvimento backend
* Autenticação e autorização
* Segurança de aplicações
* Processamento assíncrono
* Arquitetura orientada a eventos
* Mensageria com Apache Kafka
* Persistência de dados com PostgreSQL
* Testes automatizados
* Documentação de APIs

## Funcionalidades planejadas

### Autenticação e segurança

Será implementado um fluxo de autenticação que deverá contemplar:

* Cadastro de usuários
* Login
* Autenticação utilizando JWT
* Autenticação de dois fatores (2FA)
* Geração e validação de código 2FA
* Expiração de códigos
* Recuperação de senha
* Redefinição de senha
* Expiração de tokens de recuperação
* Proteção de endpoints autenticados


### Clientes

A API deverá permitir:

* Cadastro de clientes
* Consulta de clientes
* Atualização de dados
* Inativação de clientes
* Validação dos dados cadastrais

### Análise de crédito

Será desenvolvido um fluxo para solicitação e acompanhamento de análises de crédito.

* Solicitação de análise de crédito
* Consulta de análise
* Histórico de análises
* Controle de status
* Registro dos dados utilizados na análise
* Processamento assíncrono das solicitações
* Definição de critérios para análise

A proposta é que o processamento seja automatizado com base nos dados disponíveis, sem que o sistema dependa de uma aprovação manual.

## Mensageria com Apache Kafka

O projeto deverá utilizar **Apache Kafka** para trabalhar com comunicação assíncrona e eventos.
A ideia é utilizar eventos para desacoplar processos que não precisam ser executados diretamente durante uma requisição HTTP.

O uso do Kafka permitirá explorar conceitos de **event-driven architecture**, producers, consumers, tópicos e processamento assíncrono.

## Recuperação de senha

Será desenvolvido um fluxo de recuperação de senha utilizando um token temporário.
O token deverá possuir tempo de expiração e não poderá ser reutilizado após a conclusão do processo.

## Autenticação de dois fatores (2FA)

Como parte da camada de segurança, será implementada autenticação de dois fatores.
A proposta é utilizar um código temporário após a validação das credenciais do usuário.
O código deverá possuir validade limitada e ser invalidado após sua utilização.

## Arquitetura

A aplicação será organizada em camadas, buscando separar responsabilidades e facilitar a manutenção do código.
Estrutura planejada:

CreditFlow
│
├── CreditFlow.Api
│   ├── Controllers
│   ├── Middleware
│   └── Configuration
│
├── CreditFlow.Application
│   ├── Services
│   ├── DTOs
│   ├── Interfaces
│   └── Validators
│
├── CreditFlow.Infrastructure
│   ├── Data
│   ├── Context
│   ├── Repositories
│   ├── Kafka
│   └── Migrations
│
└── CreditFlow.Domain
    ├── Entities
    ├── Enums
    └── Interfaces
    

A estrutura poderá ser ajustada conforme o projeto evoluir e novas necessidades surgirem.

## Banco de dados

O projeto utilizará **PostgreSQL** como banco de dados principal.
O acesso aos dados será realizado através do **Entity Framework Core**, e as alterações na estrutura do banco deverão ser controladas por migrations.
As credenciais reais não deverão ser versionadas no repositório.

## Documentação da API

A API utilizará **Swagger / OpenAPI** para documentar os endpoints.
A documentação deverá permitir visualizar e testar:

* Endpoints
* Parâmetros
* DTOs
* Respostas
* Códigos HTTP
* Autenticação

## Testes

Durante o desenvolvimento, serão adicionados testes automatizados para validar principalmente as regras de negócio e os serviços da aplicação.
As ferramentas planejadas são:

* **xUnit**
* **Moq**
* **Entity Framework Core InMemory**


## Conceitos que serão explorados

* C#
* ASP.NET Core
* APIs REST
* Entity Framework Core
* PostgreSQL
* JWT
* Autenticação e autorização
* 2FA
* Recuperação de senha
* Apache Kafka
* Producers e Consumers
* Event-driven architecture
* Processamento assíncrono
* Injeção de dependência
* Separação de responsabilidades
* Testes automatizados
* Swagger / OpenAPI
* Docker
