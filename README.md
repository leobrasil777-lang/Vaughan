# Vaughan

Sistema acadêmico de gestão para bares e restaurantes, com mesas, comandas, produtos, estoque, usuários e relatórios de vendas. Deselvivdo com C#, ASP.NET Core 8 e banco de dados SQL Server Express.

## Tecnologias

| Camada | Implementação |
| --- | --- |
| Interface | HTML, CSS, Bootstrap e jQuery |
| Servidor | C# / .NET 8, aplicação console com `HttpListener` |
| API | Roteamento manual e respostas JSON |
| Persistência | SQL Server e ADO.NET (`Microsoft.Data.SqlClient`) |
| Organização | Handlers, repositórios, modelos e gerenciamento de sessões |

## Funcionalidades implementadas

- Login e distinção entre os cargos de gerente e garçom.
- Consulta de mesas e gestão de comandas.
- Cadastro e manutenção de produtos.
- Inclusão e remoção de itens, com movimentação de estoque.
- Registro do preço do produto no momento da venda.
- Relatórios de vendas e consulta de estoque baixo.
- Gestão de usuários restrita ao gerente.

## Consultas com SQL

As imagens abaixo são capturas de consultas já presentes no projeto acadêmico. Não são capturas novas da aplicação nem resultados de testes desta edição.

### Pedidos, clientes, garçons e mesas

![Consulta de pedidos com cliente, garçom e mesa](docs/images/pedidos-clientes-mesas.png)

### Itens e valores de pedidos

![Consulta dos itens e preço total](docs/images/itens-pedidos.png)

### Preço médio por categoria

![Consulta da média de preços por categoria](docs/images/preco-medio-categoria.png)

## Organização

| Caminho | Conteúdo |
| --- | --- |
| `VaughanBar/Http/` | Servidor, roteamento, requisições e arquivos estáticos |
| `VaughanBar/Handlers/` | Tratamento das operações da API |
| `VaughanBar/Repositories/` | Consultas e operações SQL |
| `VaughanBar/Models/` | Modelos de dados |
| `VaughanBar/Security/` | Sessões em memória |
| `VaughanBar/wwwroot/` | Páginas, estilos e JavaScript |
| `*.sql` na raiz | Scripts acadêmicos de criação, extensão, dados e consultas |
| `docs/` | Guias, limitações, capturas e documentação técnica |

