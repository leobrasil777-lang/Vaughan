# Alterações de apresentação

## Escopo desta etapa

Preparação documental da edição Vaughan, baseada no commit `46981c7cb960ccdeb773d60a69bf9e191c52cbda` do repositório acadêmico `leobrasil777-lang/bancodedadossqlvaugharbar`.

- README principal com objetivo, stack, funcionalidades, organização e navegação.
- Guia de instalação com caminhos e ordem dos scripts conferidos no código.
- Galeria com capturas SQL existentes, explicitamente identificadas como material acadêmico.
- Organização das sete capturas PNG da raiz em `docs/images`, com nomes descritivos.
- Correção do caminho do script de extensão no README técnico.
- Correção da orientação sobre armazenamento de senhas e link para limitações.
- Separação entre funcionalidades existentes, validações pendentes e melhorias futuras.

Nesta primeira etapa, nenhum arquivo C#, SQL, HTML, CSS ou JavaScript foi modificado. Não foram criados workflows, configurados domínios ou efetuados deploys. O repositório acadêmico original não foi modificado.

## Segunda etapa: regras de comanda

- Cancelamento passa a exigir comanda aberta e a estornar estoque e registrar movimentações em uma transação.
- Mesa é liberada no cancelamento somente quando não houver outra comanda aberta vinculada.
- Repetir o cancelamento não gera um segundo estorno.
- Inclusão e remoção de itens passam a exigir comanda aberta; a consulta de estoque ao incluir itens usa bloqueio de atualização para reduzir conflitos simultâneos.
- O roteiro de validação e as limitações foram atualizados. Ainda falta executar testes de integração com SQL Server.

## Verificação

Links locais e imagens da nova documentação devem resolver para arquivos existentes. A execução da aplicação permanece pendente, pois este ambiente não possui o SDK .NET e uma instância SQL Server preparada.
