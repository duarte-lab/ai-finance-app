# Feature: Tenant as Person

## Objetivo 
Registrar o novo tenant no cadastro de pessoas participantes usado no fechamento mensal.

# Requisitos
- Criar Pessoa
- Definir o tipo da pessoa como Owner das contas.

# Campos
- Data de criação
- Nome
- Data de exclusão
- Tipo de pessoa (Owner ou Guest)

# Regras
- Somente o tenant deve ser o owner de suas contas.
- Não permitir que o cadastro de pessoas deixe definir o owner na tela.
- Ao cadastrar uma pessoa pela tela, ela deve ser registrada como participante.

## Backend
- CRUD ajustado com o tipo da pessoa.
- Demais funcionalidades devem ser mantidas.

## Frontend
- Exibir o tipo da pessoa, somente para visualização.

## Testes
- Criacao de pessoa (Owner e Guest)
- Retorno da lista de pessoas com o campo de tipo.