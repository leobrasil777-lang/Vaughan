# Vaughan Bar: documentação técnica

Veja a [apresentação do projeto](../README.md) e o [guia de execução revisado](../docs/EXECUCAO.md). O guia revisado é a referência para a sequência de instalação.


Sistema de gestão para bar/restaurante: mesas e comandas, cardápio, controle de
estoque, usuários com login e relatórios de vendas.

## Regras técnicas seguidas

- **Back-end sem framework**: console app em C# (.NET 8) que expõe uma API
  HTTP construída diretamente sobre `System.Net.HttpListener` (classe nativa
  do .NET). Não há ASP.NET Core MVC/WebAPI, nem roteamento automático — todo
  o roteamento está em `Http/Router.cs`, feito manualmente.
- **Sem ORM**: acesso a dados 100% via ADO.NET puro
  (`Microsoft.Data.SqlClient` + `SqlCommand`/`SqlDataReader`), com SQL escrito
  à mão e parametrizado (proteção contra SQL Injection). Veja a pasta
  `Repositories/`.
- **Front-end**: HTML5 + CSS3 + Bootstrap 5 + jQuery, sem nenhuma lib SPA
  (React/Angular/Vue). Site multi-página, cada módulo em seu próprio `.html`,
  com JavaScript puro/jQuery fazendo chamadas AJAX para a API.

## Estrutura do projeto

```
VaughanBar/
  Program.cs                  Ponto de entrada (inicia o HttpServer)
  VaughanBar.csproj           Projeto .NET (console app)
  Data/                       Configuração e conexão ADO.NET
  Models/                     Classes de dados (POCOs)
  Repositories/               Acesso a dados (SQL puro)
  Http/                       HttpListener, roteador, arquivos estáticos
  Handlers/                   "Controllers" manuais (um por módulo)
  Security/                   Sessão de login em memória (token)
  wwwroot/                    Front-end (HTML/CSS/JS servido pelo próprio C#)
```

## 1. Banco de dados

O projeto usa o schema do repositório original (`clientes_bar.sql`,
`produtostrabalho.sql`, `pedidostrabalho.sql`, `itenspedidostrabalho.sql`) e
o estende com o script incluído aqui.

Ordem de execução no SQL Server Management Studio (ou `sqlcmd`):

1. `databasetrabalho.sql` (cria o banco `VaughanBar`)
2. `clientes_bar.sql`
3. `produtostrabalho.sql`
4. `pedidostrabalho.sql`
5. `itenspedidostrabalho.sql`
6. (opcional) os `insertdados*.sql` do repositório original, para dados de exemplo
7. **`../02_extensao_schema.sql`** (na raiz do repositório) — adiciona:
   - Tabela `Usuarios` (login, senha em texto puro, cargo Garçom/Gerente)
   - Tabela `Mesas`
   - Colunas de estoque em `Produtos` (`QuantidadeEstoque`, `EstoqueMinimo`, `Ativo`)
   - Tabela `MovimentosEstoque` (histórico de entradas/saídas)
   - Colunas `MesaId`, `UsuarioId`, `Status` em `Pedidos`
   - Coluna `PrecoUnitario` em `ItensPedido`
   - Usuário `admin` / senha `admin123` (Gerente) e mesas de exemplo

O script é **idempotente**: pode ser executado mais de uma vez sem duplicar
tabelas/colunas/dados.

> **Limitação de segurança:** as senhas são armazenadas em texto puro. Esta versão deve ser usada somente como demonstração local. Uma evolução deve utilizar um algoritmo próprio para senhas, como Argon2id ou PBKDF2 com salt e parâmetros adequados. SHA-256 simples não é apropriado. Consulte as [limitações conhecidas](../docs/LIMITACOES.md).

## 2. Configurar a conexão

Edite `Data/AppConfig.cs` e ajuste a string de conexão para o seu SQL Server,
por exemplo:

```csharp
public static string ConnectionString { get; set; } =
    "Server=localhost;Database=VaughanBar;Trusted_Connection=True;TrustServerCertificate=True;";
```

Ou informe a connection string na hora de rodar (sem precisar recompilar):

```bash
dotnet run -- "Server=localhost;Database=VaughanBar;User Id=sa;Password=SuaSenha;TrustServerCertificate=True;"
```

## 3. Rodar o projeto

```bash
cd VaughanBar
dotnet restore
dotnet run
```

O servidor sobe em `http://localhost:5050/` e já serve tanto a API
(`/api/...`) quanto o front-end (arquivos de `wwwroot/`). Basta abrir
`http://localhost:5050/` no navegador.

Login de demonstração: **admin / admin123**

## 4. Módulos disponíveis

| Módulo | Tela | Descrição |
|---|---|---|
| Login | `index.html` | Autenticação (sessão em memória via token) |
| Mesas & Comandas | `mesas.html`, `comanda.html` | Abrir/fechar comanda, vincular a mesa e cliente, lançar itens |
| Cardápio | `cardapio.html` | CRUD de produtos, preço, categoria |
| Estoque | `estoque.html` | Entradas/saídas manuais, baixa automática por venda, alerta de estoque baixo |
| Relatórios | `relatorios.html` | Total vendido, ticket médio, produtos mais vendidos, vendas por garçom |
| Usuários | `usuarios.html` | CRUD de usuários (restrito ao cargo Gerente) |

## 5. Principais endpoints da API

Todas as respostas são JSON. Rotas autenticadas exigem o header
`X-Auth-Token: <token>` (obtido no login).

```
POST   /api/login                       { login, senha } -> { token, usuario }
POST   /api/logout
GET    /api/eu

GET    /api/produtos?ativos=1
POST   /api/produtos
PUT    /api/produtos/{id}
DELETE /api/produtos/{id}                (desativa, não apaga)

GET    /api/mesas
POST   /api/mesas
PUT    /api/mesas/{id}/status            { status: "Livre" | "Ocupada" }
DELETE /api/mesas/{id}

GET    /api/clientes
POST   /api/clientes
PUT    /api/clientes/{id}
DELETE /api/clientes/{id}

GET    /api/pedidos?status=Aberto&mesaId=1
GET    /api/pedidos/{id}
POST   /api/pedidos                      { mesaId?, clienteId? } -> abre comanda
POST   /api/pedidos/{id}/itens           { produtoId, quantidade }
DELETE /api/itens/{itemId}
POST   /api/pedidos/{id}/fechar
POST   /api/pedidos/{id}/cancelar

GET    /api/estoque/historico?produtoId=
GET    /api/estoque/baixo
POST   /api/estoque/movimento            { produtoId, tipo: "Entrada"|"Saida", quantidade, observacao }

GET    /api/relatorios/vendas?inicio=YYYY-MM-DD&fim=YYYY-MM-DD

GET    /api/usuarios                     (Gerente)
POST   /api/usuarios                     (Gerente)
PUT    /api/usuarios/{id}                (Gerente)
DELETE /api/usuarios/{id}                (Gerente)
```

## 6. Observações de arquitetura

- **Transações**: abrir comanda, lançar/remover item e fechar comanda usam
  `SqlTransaction` para manter comanda, estoque e status de mesa consistentes.
- **Baixa de estoque**: ao adicionar um item na comanda, o sistema verifica
  o estoque disponível e já debita a quantidade; remover o item devolve ao
  estoque. Toda movimentação fica registrada em `MovimentosEstoque` para
  auditoria/relatório.
- **Preço na venda**: `ItensPedido.PrecoUnitario` guarda o preço praticado no
  momento da venda, então alterar o preço de um produto no cardápio não
  altera comandas já lançadas.
- **CORS**: liberado (`Access-Control-Allow-Origin: *`) para facilitar testes
  locais; como o próprio C# já serve o front-end, isso normalmente nem é
  necessário em produção.
- **Bootstrap/jQuery via CDN**: as páginas carregam Bootstrap 5 e jQuery do
  `cdnjs.cloudflare.com`. Se o ambiente de implantação não tiver acesso à
  internet, baixe os arquivos `bootstrap.min.css/js` e `jquery.min.js` e
  referencie localmente em `wwwroot/`.
