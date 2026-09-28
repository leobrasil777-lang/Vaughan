# Execução local

## Ambiente

- Windows com SDK .NET 8.
- SQL Server local, incluindo Express, e SQL Server Management Studio (SSMS).
- Acesso à internet para restaurar o pacote NuGet e carregar as bibliotecas de interface disponibilizadas por CDN.

Este guia foi conferido por leitura dos scripts e do código. A aplicação e o banco não foram executados nesta revisão documental.

## 1. Preparar o banco

Use uma instância de desenvolvimento. Se já existir um banco `VaughanBar`, não o apague nem execute os scripts de criação sobre ele: use uma instância isolada para esta demonstração.

Abra os arquivos abaixo separadamente no SSMS e execute nesta ordem. Todos estão na raiz do repositório:

| Ordem | Arquivo | Resultado |
| --- | --- | --- |
| 1 | `databasetrabalho.sql` | Criação e seleção do banco `VaughanBar` |
| 2 | `clientes_bar.sql` | Tabela de clientes |
| 3 | `produtostrabalho.sql` | Tabela de produtos |
| 4 | `pedidostrabalho.sql` | Tabela de pedidos |
| 5 | `itenspedidostrabalho.sql` | Tabela de itens de pedido |
| 6 | `02_extensao_schema.sql` | Usuários, mesas, movimentações, colunas adicionais e dados iniciais |

Antes de executar cada arquivo a partir do segundo, selecione `VaughanBar` na lista de bancos do SSMS. Alguns scripts não possuem um comando `USE` próprio.

Os scripts de criação das tabelas não são idempotentes. Não repita a sequência inteira em um banco já preparado.

O script `02_extensao_schema.sql` reúne as extensões e cria usuários e mesas demonstrativos. Os arquivos `05_Pedidos_alteracoes.sql`, `06_ItensPedido_alteracoes.sql`, `07_Dados_iniciais.sql` e `08_Backfill_PrecoUnitario.sql` sobrepõem partes dessa extensão e não precisam ser executados novamente nesse caminho de instalação.

Os outros scripts de criação, inserts e consultas foram mantidos como materiais acadêmicos. Não execute todos os arquivos da raiz indiscriminadamente. Para começar, cadastre um produto pela interface e registre uma entrada de estoque antes de incluí-lo em uma comanda.

## 2. Iniciar o servidor

Na raiz do repositório:

```powershell
dotnet --version
dotnet restore .\VaughanBar\VaughanBar.csproj
dotnet run --project .\VaughanBar\VaughanBar.csproj
```

A configuração padrão de `VaughanBar/Data/AppConfig.cs` usa `Server=localhost;Database=VaughanBar;Trusted_Connection=True;TrustServerCertificate=True;`.

Para uma instância SQL Server Express chamada `SQLEXPRESS`, use autenticação do Windows:

```powershell
dotnet run --project .\VaughanBar\VaughanBar.csproj -- 'Server=localhost\SQLEXPRESS;Database=VaughanBar;Trusted_Connection=True;TrustServerCertificate=True;'
```

O código permite receber a string de conexão como primeiro argumento e o prefixo HTTP como segundo. A implementação atual imprime a string de conexão no console; evite fornecer credenciais sensíveis e não compartilhe logs sem revisão.

Abra `http://localhost:5050/`. Não é necessário um servidor separado para os arquivos HTML.

## 3. Acessar a demonstração

O script de extensão cria os seguintes acessos locais:

| Cargo | Login | Senha de demonstração |
| --- | --- | --- |
| Gerente | `admin` | `admin123` |
| Garçom | `garcom1` | `123456` |

Esses valores são exemplos conhecidos, não credenciais de produção. As senhas ainda são armazenadas em texto puro; não exponha esta versão à internet.

## 4. Roteiro de validação manual

Este roteiro é uma lista de verificações a executar, não uma lista de testes já aprovados.

1. Entrar como gerente e verificar o carregamento das mesas.
2. Cadastrar um produto e registrar uma entrada de estoque.
3. Abrir uma comanda e adicionar o produto.
4. Conferir total e redução de estoque.
5. Remover um item e conferir a devolução ao estoque.
6. Fechar a comanda e conferir o estado da mesa.
7. Entrar como garçom e verificar que a API de usuários rejeita o acesso.
8. Conferir relatórios para o período dos dados cadastrados.

9. Em outra comanda aberta, adicionar itens e cancelar: confirmar o estorno de estoque, o registro da movimentação e a liberação da mesa.
10. Tentar cancelar a mesma comanda novamente: a API deve rejeitar a operação sem lançar outro estorno.

Estas verificações ainda precisam ser executadas em uma instância SQL Server. O cancelamento foi corrigido no código, mas não teve validação de integração neste ambiente.

## Problemas comuns

| Sintoma | Verificação |
| --- | --- |
| Falha de conexão | Serviço SQL Server ativo, nome da instância, banco e permissões do usuário Windows |
| Tabela ou coluna inexistente | Banco selecionado e execução completa da sequência SQL |
| Estoque insuficiente | Entrada de estoque registrada para o produto |
| Porta indisponível | Outro processo usando 5050; ajustar o segundo argumento do programa |
| Interface sem estilos | Disponibilidade da CDN e conexão com a internet |
