# Frontend do Vaughan Bar

Esta documentação descreve o estado atual do frontend localizado em wwwroot. Ela foi produzida a partir dos arquivos HTML, JavaScript e CSS existentes e das chamadas de API feitas por esses arquivos.

## 1. Visão geral

O frontend do Vaughan Bar é a interface administrativa de um sistema acadêmico de gestão de bar. Ele permite autenticação, gestão de mesas e comandas, manutenção do cardápio, movimentação de estoque, consulta de relatórios e administração de usuários.

Tecnologias utilizadas:

- HTML5 para a estrutura das páginas.
- CSS3 para identidade visual, layouts, estados e responsividade.
- JavaScript puro para regras de interface e manipulação de dados.
- jQuery 3.7.1 para eventos, manipulação do DOM, carregamento da sidebar e chamadas AJAX.
- Bootstrap 5.3.3 para grid responsivo, tabelas, formulários, botões, modais e utilitários.

Não há React, Vue, Angular, Vite, Tailwind ou sistema de componentes compilado. Os arquivos são servidos diretamente pelo backend .NET.

## 2. Estrutura de arquivos

Árvore atual:

    wwwroot/
    ├── README_FRONTEND.md
    ├── index.html
    ├── mesas.html
    ├── comandas.html
    ├── comanda.html
    ├── cardapio.html
    ├── estoque.html
    ├── relatorios.html
    ├── usuarios.html
    ├── css/
    │   └── site.css
    ├── js/
    │   ├── api.js
    │   ├── shell.js
    │   ├── login.js
    │   ├── mesas.js
    │   ├── comandas.js
    │   ├── comanda.js
    │   ├── cardapio.js
    │   ├── estoque.js
    │   ├── relatorios.js
    │   └── usuarios.js
    └── partials/
        └── sidebar.html

Responsabilidades:

| Arquivo | Responsabilidade |
|---|---|
| index.html | Tela de login. |
| mesas.html | Gestão de mesas, filtros, resumos e abertura de comanda vinculada a uma mesa. |
| comandas.html | Visão geral de todas as comandas, busca, consulta em modal e abertura de comanda com ou sem mesa. |
| comanda.html | Tela operacional de uma comanda específica: itens, total, fechamento e cancelamento. |
| cardapio.html | Cadastro, edição, desativação e reativação de produtos. |
| estoque.html | Registro manual de entradas e saídas e histórico recente de movimentações. |
| relatorios.html | Resumo de vendas, produtos mais vendidos e vendas por garçom. |
| usuarios.html | Administração de usuários do sistema. |
| css/site.css | Estilos globais, estilos compartilhados e estilos específicos de cada página. |
| js/api.js | Wrapper AJAX, token de autenticação, tratamento de HTTP 401, mensagens de erro e formatação monetária. |
| js/shell.js | Validação básica da sessão, carregamento da sidebar, perfil, item ativo e logout. |
| js/login.js | Validação e envio do formulário de login. |
| js/mesas.js | Dados, filtros, resumos, cadastro de mesas e abertura/consulta de comandas por mesa. |
| js/comandas.js | Listagem e busca de comandas, modal de detalhes e modal de nova comanda. |
| js/comanda.js | Operações da comanda individual e manutenção de seus itens. |
| js/cardapio.js | CRUD visual de produtos, incluindo desativação e reativação. |
| js/estoque.js | Seleção de produtos, registro de movimentos e histórico. |
| js/relatorios.js | Período padrão, filtro e renderização dos dados de vendas. |
| js/usuarios.js | Cadastro, edição, remoção lógica e reativação de usuários. |
| partials/sidebar.html | Navegação compartilhada, perfil do usuário e botão de logout. |

Não existem imagens, fontes locais ou outros arquivos estáticos dentro de wwwroot.

## 3. Arquitetura do frontend

### 3.1 Organização das páginas

Cada funcionalidade possui um arquivo HTML e, normalmente, um JavaScript de mesmo nome. As páginas protegidas carregam jQuery, Bootstrap Bundle quando necessário, js/api.js, js/shell.js e o script específico da página.

A página de login não usa shell.js e não carrega o JavaScript do Bootstrap, pois não possui sidebar nem modal.

As páginas administrativas usam app-shell, sidebar-placeholder e main-content. Classes no body, como mesas-page, comandas-page e usuarios-page, limitam estilos específicos e evitam interferência entre telas.

### 3.2 Sidebar compartilhada

As páginas autenticadas contêm sidebar-placeholder. iniciarShell(paginaAtual), definida em js/shell.js, carrega partials/sidebar.html com jQuery.

Depois do carregamento:

- lê o usuário de vb_usuario;
- preenche sidebar-usuario-nome e sidebar-usuario-cargo;
- remove nav-usuarios quando o cargo não é Gerente;
- adiciona active ao link cujo data-page corresponde à página atual;
- registra o evento de btn-logout.

A sidebar contém links para Mesas, Comandas, Cardápio, Estoque, Relatórios e Usuários.

### 3.3 Sessão e usuário autenticado

O frontend usa localStorage:

- vb_token: token enviado ao backend.
- vb_usuario: objeto JSON com dados do usuário autenticado.

iniciarShell verifica se vb_token existe. Se não existir, redireciona para index.html. O perfil da sidebar é preenchido com nome e cargo de vb_usuario. A autorização efetiva continua sendo responsabilidade do backend.

### 3.4 Comunicação com o backend

js/api.js expõe:

| Método | Função |
|---|---|
| VaughanApi.get(url) | Requisição GET. |
| VaughanApi.post(url, dados) | Requisição POST com JSON. |
| VaughanApi.put(url, dados) | Requisição PUT com JSON. |
| VaughanApi.del(url) | Requisição DELETE. |
| VaughanApi.token() | Retorna vb_token. |

As requisições usam jQuery $.ajax, contentType application/json e, quando há token, X-Auth-Token.

### 3.5 Tratamento de erros

mensagemErro(xhr, padrao) tenta interpretar xhr.responseText como JSON e retorna o campo erro. Se isso falhar, usa a mensagem padrão.

As páginas normalmente exibem mensagens em msg-area com vb-alert. Na maioria das telas, o alerta desaparece após quatro segundos. Tabelas também mostram estados de carregamento, vazio e falha.

Em HTTP 401, api.js remove vb_token e vb_usuario e redireciona páginas protegidas para index.html.

### 3.6 Valores monetários

formatarMoeda(valor) usa toLocaleString com pt-BR e BRL. É usada em Comandas, comanda individual, Cardápio e Relatórios.

## 4. Documentação das páginas

### 4.1 Login

- HTML: index.html
- JavaScript: js/login.js
- Objetivo: autenticar o usuário.
- Dados: campos Usuário e Senha, mensagem de erro e acesso acadêmico de demonstração.
- Ação: Entrar envia POST /api/login com login e senha.
- Resposta: token e usuario.
- Redirecionamentos: token existente ou login válido levam a mesas.html.
- Modais e filtros: não possui.
- Restrição de perfil: nenhuma.

### 4.2 Mesas

- HTML: mesas.html
- JavaScript: js/mesas.js
- Shell: iniciarShell('mesas').
- Objetivo: consultar e cadastrar mesas e abrir a comanda de uma mesa livre.

Exibe resumo de total, livres e ocupadas e cards com número, status e uma única ação. A busca local filtra por número e funciona junto com Todas, Livres e Ocupadas, sem novo GET a cada digitação.

Modais e ações:

- modalNovaMesa recebe Número e Capacidade, valida inteiros positivos e envia status Livre;
- modalAbrirComanda guarda modal-mesa-id, permite cliente opcional e cria o pedido;
- Abrir comanda redireciona para comanda.html?id={id};
- Ver comanda busca o pedido aberto da mesa e abre a página individual.

Endpoints: GET e POST /api/mesas, GET /api/clientes, POST /api/pedidos e GET /api/pedidos?status=Aberto&mesaId={id}.

Restrição: exige token, sem diferenciação entre Gerente e Garcom.

### 4.3 Comandas

- HTML: comandas.html
- JavaScript: js/comandas.js
- Shell: iniciarShell('comandas').
- Objetivo: listar, buscar e consultar todas as comandas e abrir uma nova.

A tabela mostra Comanda, Mesa ou Balcão, Cliente, Status, Total e Ver. A busca local considera ID, mesa e cliente, ignorando caixa e acentos.

Modais:

1. modalVerComanda usa GET /api/pedidos/{id} e mostra mesa, cliente, status, itens e total, somente para consulta.
2. modalNovaComanda oferece Comanda avulsa, mesa e cliente opcional. Após criar, recarrega a tabela e abre os detalhes.

Endpoints: GET /api/pedidos, GET /api/pedidos/{id}, POST /api/pedidos, GET /api/mesas e GET /api/clientes.

Restrição: exige token, sem diferenciação entre Gerente e Garcom.

### 4.4 Cardápio

- HTML: cardapio.html
- JavaScript: js/cardapio.js
- Shell: iniciarShell('cardapio').
- Objetivo: administrar produtos.

A tabela exibe Produto, Categoria, Preço, Estoque, Status e Ações. Quantidade menor ou igual ao estoque mínimo recebe Estoque baixo. Produtos ativos e inativos permanecem visíveis.

Ações:

- + Novo produto abre modalProduto vazio;
- Editar preenche o modal;
- Desativar confirma e envia DELETE;
- Reativar envia os dados atuais por PUT com ativo igual a true;
- Salvar usa POST ou PUT conforme produto-id.

O modal contém Nome, Categoria, Preço, Estoque inicial e Estoque mínimo. Estoque inicial aparece apenas no cadastro. Nome e categoria são escapados antes da interpolação na tabela.

Endpoints: GET, POST /api/produtos e PUT, DELETE /api/produtos/{id}.

Restrição: exige token, sem diferenciação entre Gerente e Garcom.

### 4.5 Estoque

- HTML: estoque.html
- JavaScript: js/estoque.js
- Shell: iniciarShell('estoque').
- Objetivo: registrar movimentos manuais e consultar o histórico.

O formulário possui Categoria, Produto, Tipo Entrada ou Saida, Quantidade, Observação e Registrar. O script:

- carrega produtos ativos;
- deriva e ordena categorias no frontend;
- filtra produtos pela categoria;
- mostra nome e estoque atual nas opções;
- valida produto e quantidade positiva;
- após registrar, limpa quantidade e observação e recarrega produtos e histórico.

A tabela Histórico recente mostra Data, Produto, Tipo, Qtd e Observação. Entrada é verde, Saída é vermelha e valores alternativos são neutros. Não há modal.

Endpoints: GET /api/produtos?ativos=1, GET /api/estoque/historico e POST /api/estoque/movimento.

Restrição: exige token, sem diferenciação entre Gerente e Garcom.

### 4.6 Relatórios

- HTML: relatorios.html
- JavaScript: js/relatorios.js
- Shell: iniciarShell('relatorios').
- Objetivo: consultar indicadores de vendas.

O período inicial corresponde aos últimos 30 dias. Os campos De e Até e o botão Filtrar atualizam:

- Total vendido;
- Comandas fechadas;
- Ticket médio;
- Produtos mais vendidos;
- Vendas por garçom.

As tabelas exibem Carregando, erro ou Sem dados no período quando necessário. Não há modal.

Endpoint: GET /api/relatorios/vendas?inicio={AAAA-MM-DD}&fim={AAAA-MM-DD}.

Restrição: exige token, sem diferenciação entre Gerente e Garcom.

### 4.7 Usuários

- HTML: usuarios.html
- JavaScript: js/usuarios.js
- Shell: iniciarShell('usuarios').
- Objetivo: administrar acessos.

A tabela mostra Nome, Login, Cargo, Status e Ações.

Ações:

- + Novo usuário abre modalUsuario vazio;
- Editar preenche o modal;
- Remover confirma e envia DELETE;
- Reativar envia nome, login, cargo e ativo igual a true por PUT;
- Salvar usa POST ou PUT conforme usuario-id.

O modal contém Nome, Login, Senha, Cargo Garcom ou Gerente e Usuário ativo. A senha é obrigatória no cadastro e opcional na edição.

Endpoints: GET, POST /api/usuarios e PUT, DELETE /api/usuarios/{id}.

Restrição:

- shell.js remove o link para quem não é Gerente;
- acesso direto à URL ainda é possível, portanto a autorização real depende da API;
- a API de usuários exige Gerente.

### 4.8 Comanda individual

- HTML: comanda.html
- JavaScript: js/comanda.js
- Shell: iniciarShell('mesas'), mantendo Mesas ativo.
- Objetivo: operar uma comanda identificada pelo parâmetro id.

Sem id, redireciona para mesas.html. Exibe mesa ou Balcão, cliente, garçom, status, abertura, itens, preços, subtotais e total.

Ações:

- Voltar para Mesas;
- Adicionar à comanda;
- Remover item;
- Fechar comanda;
- Cancelar comanda.

Produtos ativos são carregados e os sem estoque ficam desabilitados. Adicionar ou remover recarrega comanda e produtos. Se o status não for Aberto, ações de alteração são desabilitadas ou omitidas. Fechar e Cancelar pedem confirmação e redirecionam para mesas.html. Não há modal.

Endpoints: GET /api/pedidos/{id}, GET /api/produtos?ativos=1, POST /api/pedidos/{id}/itens, DELETE /api/itens/{id}, POST /api/pedidos/{id}/fechar e POST /api/pedidos/{id}/cancelar.

Restrição: exige token, sem diferenciação entre Gerente e Garcom.

## 5. Integração com a API

Somente endpoints chamados pelos JavaScript atuais:

| Método | Endpoint | Página | Finalidade | Dados enviados | Resposta utilizada |
|---|---|---|---|---|---|
| POST | /api/login | Login | Autenticar. | { login, senha } | { token, usuario: { id, nome, login, cargo } } |
| POST | /api/logout | Sidebar | Encerrar sessão. | Sem payload explícito. | { ok } |
| GET | /api/mesas | Mesas, Comandas | Listar mesas. | — | Array de { id, numero, capacidade, status }. |
| POST | /api/mesas | Mesas | Cadastrar mesa. | { numero, capacidade, status: "Livre" } | { id } |
| GET | /api/clientes | Mesas, Comandas | Preencher clientes. | — | Array de { id, nome, telefone }. |
| GET | /api/pedidos | Comandas | Listar comandas. | — | Array de pedidos com total e itens. |
| GET | /api/pedidos?status=Aberto&mesaId={id} | Mesas | Encontrar pedido aberto da mesa. | Query string. | Array de pedidos. |
| POST | /api/pedidos | Mesas, Comandas | Abrir comanda vinculada ou avulsa. | { mesaId?, clienteId? } | { id } |
| GET | /api/pedidos/{id} | Comandas, Comanda | Buscar comanda completa. | — | Pedido com dados, total e itens. |
| POST | /api/pedidos/{id}/itens | Comanda | Adicionar item. | { produtoId, quantidade } | { ok } |
| DELETE | /api/itens/{id} | Comanda | Remover item. | — | { ok } |
| POST | /api/pedidos/{id}/fechar | Comanda | Fechar comanda e liberar mesa. | Sem payload explícito. | { ok } |
| POST | /api/pedidos/{id}/cancelar | Comanda | Cancelar comanda. | Sem payload explícito. | { ok } |
| GET | /api/produtos | Cardápio | Listar produtos ativos e inativos. | — | Array de produtos. |
| GET | /api/produtos?ativos=1 | Estoque, Comanda | Listar produtos ativos. | Query string. | Array de produtos. |
| POST | /api/produtos | Cardápio | Criar produto. | { nome, preco, categoria, estoqueMinimo, ativo, quantidadeEstoque } | { id } |
| PUT | /api/produtos/{id} | Cardápio | Editar ou reativar. | Dados do produto; reativação usa ativo: true. | { ok } |
| DELETE | /api/produtos/{id} | Cardápio | Desativar produto. | — | { ok } |
| GET | /api/estoque/historico | Estoque | Carregar histórico. | — | Array de movimentos. |
| POST | /api/estoque/movimento | Estoque | Registrar movimento. | { produtoId, tipo, quantidade, observacao } | { ok } |
| GET | /api/relatorios/vendas?inicio={data}&fim={data} | Relatórios | Consultar vendas. | Query string. | { periodo, resumo, produtosMaisVendidos, vendasPorUsuario } |
| GET | /api/usuarios | Usuários | Listar usuários. | — | Array de { id, nome, login, cargo, ativo }. |
| POST | /api/usuarios | Usuários | Criar usuário. | { nome, login, senha, cargo, ativo } | { id } |
| PUT | /api/usuarios/{id} | Usuários | Editar ou reativar. | { nome, login, cargo, ativo, senha? } | { ok } |
| DELETE | /api/usuarios/{id} | Usuários | Remover/desativar. | — | { ok } |

Campos seguidos de ponto de interrogação são opcionais.

## 6. Autenticação

### vb_token

Após o login, resposta.token é salvo no localStorage como vb_token. VaughanApi.token() lê esse valor. As chamadas AJAX autenticadas enviam:

    X-Auth-Token: valor-de-vb_token

### vb_usuario

resposta.usuario é serializado com JSON.stringify e salvo como vb_usuario. O objeto contém id, nome, login e cargo. shell.js usa nome e cargo no perfil da sidebar.

### HTTP 401

Quando VaughanApi recebe 401:

1. remove vb_token;
2. remove vb_usuario;
3. redireciona páginas protegidas para index.html.

### Logout

Sair chama POST /api/logout. O bloco always limpa os dois itens locais e redireciona mesmo se o pedido de logout falhar.

### Restrição de Usuários

nav-usuarios é removido para quem não é Gerente. Isso é uma restrição visual; a API também valida o cargo e é responsável pela segurança.

## 7. Componentes e padrões visuais

### 7.1 Sidebar

Classes principais:

- app-shell: organiza sidebar e conteúdo;
- sidebar: navegação preta e fixa;
- sidebar-brand e brand-mark: marca;
- sidebar-nav: links;
- active: item atual com fundo cinza e barra lateral;
- sidebar-footer e sidebar-user: perfil e saída.

No desktop, a sidebar mede 240 px e fica fixa. Até 768 px, torna-se estática, ocupa toda a largura e fica acima do conteúdo.

### 7.2 Cabeçalhos e conteúdo

- main-content: área principal;
- page-title: cabeçalho base;
- classes como mesas-page-header e usuarios-page-header ajustam páginas específicas.

Mesas, Comandas, Cardápio, Estoque, Relatórios e Usuários usam fundo claro e seletores contextualizados pelo body. Login e comanda.html ainda usam principalmente a identidade global escura e âmbar.

### 7.3 Cards e painéis

- vb-panel: painel global;
- mesas-summary-card: indicadores de mesas;
- mesa-card: mesa individual;
- relatorio-resumo-card: indicador de vendas;
- relatorio-table-card, estoque-panel e painéis específicos: superfícies claras.

### 7.4 Tabelas

- vb-table: base compartilhada;
- table-responsive: rolagem horizontal Bootstrap;
- comandas-table, cardapio-table, estoque-table, relatorio-table e usuarios-table: variações por página;
- table-state, relatorio-table-state e usuarios-table-state: carregamento, vazio e erro.

### 7.5 Botões

- vb-btn-dark: ação principal preta;
- vb-btn-outline: ação secundária contornada;
- btn-amber e btn-outline-amber: padrão legado usado no login e na comanda individual;
- utilitários Bootstrap como btn, btn-sm e w-100 complementam o layout.

### 7.6 Badges e estados

- mesa-status-badge com status-livre ou status-ocupada;
- status-tag com status-aberto, status-fechado ou status-cancelado;
- product-status-active e product-status-inactive;
- movement-entry, movement-exit e movement-neutral;
- user-status-active e user-status-inactive;
- stock-low-label.

Ativos, livres e entradas usam verde. Inativos, ocupadas e saídas usam vermelho. Cancelados e estados alternativos usam cinza.

### 7.7 Modais

Modais Bootstrap existentes:

- modalNovaMesa;
- modalAbrirComanda;
- modalVerComanda;
- modalNovaComanda;
- modalProduto;
- modalUsuario.

Os scripts usam bootstrap.Modal e, em Comandas e Mesas, eventos hidden.bs.modal ou shown.bs.modal.

### 7.8 Alertas

- vb-alert: base;
- vb-alert-danger: erro;
- vb-alert-success: sucesso.

Páginas claras aplicam aparência neutra aos alertas por meio de seletores contextualizados.

### 7.9 Formulários

Campos usam form-control, form-select, form-label e form-check do Bootstrap. O CSS possui padrão global escuro e sobrescritas claras nas páginas modernas e seus modais. As validações são feitas em JavaScript, sem plugin adicional.

### 7.10 Responsividade

- grids Bootstrap reorganizam cards e colunas;
- até 991,98 px, filtros de Mesas e Relatórios são reorganizados;
- até 768 px, sidebar e conteúdo ficam verticais;
- até 575,98 px, barra de Comandas e filtros empilham;
- tabelas largas permanecem em table-responsive.

## 8. Fluxos principais

### 8.1 Login

1. Usuário abre index.html.
2. Token já existente redireciona para mesas.html.
3. Usuário informa login e senha.
4. Frontend valida campos não vazios.
5. POST /api/login é enviado.
6. token e usuario são salvos.
7. Navegador abre mesas.html.

### 8.2 Abertura de comanda em uma mesa

1. Mesas carrega GET /api/mesas.
2. Usuário clica Abrir comanda em uma mesa Livre.
3. Modal guarda mesaId e permite cliente opcional.
4. POST /api/pedidos envia mesaId e clienteId quando selecionado.
5. Backend retorna id.
6. Frontend abre comanda.html?id={id}.

### 8.3 Abertura de comanda avulsa

1. Usuário abre comandas.html.
2. Clica + Nova comanda.
3. Mantém Comanda avulsa sem mesa.
4. Pode selecionar cliente.
5. POST /api/pedidos é enviado sem mesaId.
6. A tabela é recarregada.
7. Após o modal fechar, os detalhes da nova comanda são abertos.

### 8.4 Consulta de uma comanda

Há dois caminhos:

- em comandas.html, Ver chama GET /api/pedidos/{id} e apresenta modal somente de leitura;
- em mesas.html, Ver comanda procura o pedido aberto da mesa e navega para comanda.html?id={id}.

Na página individual também é possível adicionar e remover itens.

### 8.5 Fechamento e cancelamento

1. Usuário acessa a comanda individual.
2. Clica Fechar ou Cancelar.
3. confirm() solicita confirmação.
4. Frontend chama o endpoint correspondente.
5. Em sucesso, redireciona para mesas.html.
6. Ao fechar, o backend libera a mesa vinculada.

### 8.6 Cadastro e gerenciamento de produtos

1. Cardápio carrega GET /api/produtos.
2. + Novo produto abre o modal.
3. POST cria.
4. Editar usa PUT.
5. Desativar usa DELETE após confirmação.
6. Produto inativo oferece Reativar.
7. Reativar usa PUT com ativo igual a true.
8. A tabela é recarregada.

### 8.7 Controle de estoque

1. Estoque carrega produtos ativos e histórico.
2. Categorias são derivadas no frontend.
3. Usuário escolhe categoria, produto, tipo, quantidade e observação.
4. POST /api/estoque/movimento registra.
5. Histórico e produtos são recarregados.

Movimentos automáticos de comandas também podem aparecer no histórico quando retornados pela API.

### 8.8 Gerenciamento de usuários

1. Gerente abre usuarios.html.
2. GET /api/usuarios preenche a tabela.
3. Novo usuário exige nome, login e senha.
4. Edição permite senha vazia.
5. Remover usa confirmação e DELETE.
6. Inativo oferece Reativar por PUT.
7. A lista é recarregada.

### 8.9 Logout

1. Usuário clica Sair.
2. shell.js envia POST /api/logout.
3. vb_token e vb_usuario são removidos.
4. Navegador retorna para index.html.

## 9. Como executar

O frontend não possui servidor próprio. Ele é servido pelo mesmo processo .NET que fornece a API.

Na pasta que contém o arquivo .csproj:

    dotnet run

Depois, abra o endereço informado pelo backend, normalmente:

    http://localhost:5050/

O terminal precisa permanecer aberto enquanto o sistema estiver sendo utilizado. Encerrar o processo interrompe frontend e API.

## 10. Cuidados para manutenção

- Não renomeie IDs usados pelos JavaScript sem atualizar todas as referências.
- Preserve endpoints, métodos HTTP, query strings, payloads e formatos JSON.
- Não adicione React, Vue, Angular, Vite, Tailwind ou dependências fora da arquitetura.
- Mantenha HTML5, CSS3, JavaScript puro, jQuery e Bootstrap como base.
- Centralize alterações visuais em css/site.css e prefira seletores da página.
- Preserve a ordem de jQuery, Bootstrap, api.js, shell.js e script da página.
- Páginas autenticadas devem manter sidebar-placeholder e iniciarShell com o data-page correto.
- Preserve vb_token, vb_usuario e X-Auth-Token enquanto o contrato não mudar de forma coordenada.
- Ao inserir dados externos no HTML, prefira text() ou escape explícito.
- Mantenha IDs dos modais usados por bootstrap.Modal e eventos jQuery.
- Teste carregamento, vazio, erro, sucesso e HTTP 401.
- Verifique desktop e telas menores, sobretudo sidebar, grids, modais e tabelas.
- Alterações em C# exigem reiniciar o backend.
- Alterações somente em HTML, CSS ou JavaScript normalmente são lidas ao atualizar o navegador.

## 11. Resumo de navegação

| Origem | Destino | Evento |
|---|---|---|
| index.html | mesas.html | Token existente ou login bem-sucedido. |
| Sidebar | mesas.html | Mesas. |
| Sidebar | comandas.html | Comandas. |
| Sidebar | cardapio.html | Cardápio. |
| Sidebar | estoque.html | Estoque. |
| Sidebar | relatorios.html | Relatórios. |
| Sidebar | usuarios.html | Usuários, visível para Gerente. |
| mesas.html | comanda.html?id={id} | Comanda criada ou mesa ocupada. |
| comanda.html | mesas.html | Voltar, fechar, cancelar ou falta de id. |
| Página protegida | index.html | Sem token, HTTP 401 ou logout. |

comandas.html consulta comandas em modais e não navega para comanda.html pelo botão Ver.
