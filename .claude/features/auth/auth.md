# Feature: Authentication

## Objetivo
Implementar autenticação completa usando JWT + Refresh Token.

## Requisitos
- Login com email/senha
- Hash de senha (BCrypt)
- JWT com expiração curta (15min)
- Refresh token persistido no banco
- Endpoint de refresh

## Backend
- Criar entidade User
- Criar tabela RefreshTokens
- Criar AuthController

## Endpoints
POST /auth/login
POST /auth/refresh

## Segurança
- Nunca armazenar senha em texto plano
- Token deve conter userId

## Testes
- Login válido
- Login inválido
- Refresh token válido/inválido

## Restrições
- Não usar lógica no controller
- Usar services