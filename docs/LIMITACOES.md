# Limitações e próximos passos

Esta edição é destinada a estudo e demonstração local. As melhorias abaixo ainda não foram implementadas.

## Segurança

- Senhas armazenadas e comparadas em texto puro em `UsuarioRepository.cs`.
- Sessões em memória sem expiração temporal em `SessionManager.cs`.
- String de conexão impressa no console em `Program.cs`.
- Mensagem de exceção devolvida ao cliente em `Router.cs`.

Antes de disponibilizar uma demonstração pública, implementar armazenamento de senhas com um algoritmo adequado, como Argon2id ou PBKDF2 com salt e parâmetros apropriados. SHA-256 simples não é adequado para esse objetivo. Referência: [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html).

Também será necessário revisar expiração de sessões, autorização, configuração por ambiente e tratamento de erros. Esta lista não substitui uma auditoria de segurança.

## Regras de negócio

O cancelamento agora estorna itens ao estoque, registra entradas e libera a mesa sem outras comandas abertas, em transação. A inclusão e a remoção de itens exigem comanda aberta. Essas alterações ainda requerem validação de integração no SQL Server. O fechamento de pedidos e a abertura de duas comandas para a mesma mesa precisam de regras explícitas antes de uma implantação real.

## Validação pendente

- Executar a instalação em um banco vazio.
- Validar login, estoque, fechamento e relatórios com dados fictícios.
- Criar testes de integração relevantes para estoque, permissões e cancelamentos.
- Produzir capturas atuais da interface depois da validação funcional.

As capturas SQL exibidas no README foram herdadas do material acadêmico. Não comprovam a execução desta revisão nem devem ser apresentadas como métricas de desempenho.
