# Feature: Authentication

## Objetivo
Implementar autenticação completa usando JWT + Refresh Token, incluindo login social com Google.

## Requisitos
- Registro com email/senha
- Login com email/senha
- Login com Google OAuth
- Hash de senha (BCrypt)
- JWT com expiração curta (15min)
- Refresh token persistido no banco
- Endpoint de refresh
- Ao registrar usuário (email/senha ou Google), criar automaticamente uma Pessoa owner vinculada, caso ainda não exista
- A Pessoa criada no registro deve ser marcada como owner
- Pessoa owner não pode ser excluída da lista de pessoas
- Cadastro de pessoas deve suportar tipo de pessoa (Owner ou Guest)
- Somente a pessoa do tenant pode ser owner
- Pessoas criadas pela tela devem ser registradas como Guest/participante

## Campos
- Pessoa.Nome
- Pessoa.DataCriacao
- Pessoa.DataExclusao
- Pessoa.Tipo (Owner ou Guest)

## Backend
- Criar entidade User com suporte a senha (hash BCrypt) e GoogleId (ambos opcionais, mas ao menos um obrigatório)
- Garantir criação de Pessoa owner vinculada ao User em ambos os fluxos (registro email/senha e primeiro login Google), caso ainda não exista
- Criar coleção RefreshTokens no banco
- Criar AuthController
- Implementar endpoint de autenticação Google
- Validar id_token do Google no backend (audience/clientId e expiração)
- Ajustar CRUD de pessoas para persistir e retornar o tipo da pessoa (Owner/Guest)

## Endpoints
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/google

## Segurança
- Nunca armazenar senha em texto plano (usar BCrypt)
- JWT deve conter userId e tenantId
- JWT deve ter expiração de 15 minutos
- Validar audience (clientId) e expiração do id_token Google
- Rejeitar token Google inválido
- Não permitir promoção manual para owner via cadastro/edição de pessoas

## Frontend
- Exibir tipo da pessoa apenas para visualização
- Não permitir definir owner manualmente na tela de pessoas

## Testes
- Registro via email/senha cria User + Pessoa owner
- Primeiro login Google cria User + Pessoa owner
- Login Google subsequente não duplica Pessoa owner
- Login Google válido retorna JWT
- Login Google inválido retorna 401
- Login email/senha válido retorna JWT
- Login email/senha inválido retorna 401
- Refresh token válido renova JWT
- Refresh token inválido/expirado retorna 401
- Tentativa de excluir Pessoa owner deve falhar
- Criação de pessoa pela tela define tipo Guest
- Retorno da lista de pessoas inclui o campo de tipo
- Bloquear definição manual de owner no cadastro/edição de pessoas

## Restrições
- Não usar lógica no controller
- Usar services
- Regra de owner imutável para exclusão deve ficar na camada de aplicação/domínio