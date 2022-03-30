# Microservices architecture.

## Tech stack
 * .NET 5.0
 * PostgreSQL
 * JWT (Symmetric)
 * docker

## How to start:

```
 $ cp docker-compose.yml /yourFolder

 $ docker-compose up 
```

## How to use:

```
# MS_Auth (default http://localhost:8081)

POST /api/auth/login 
{
    "Username": "",
    "Password": ""
} - return JWS token | refresh token in Cookie

POST /api/auth/refresh-token - return new JWS token | refresh token in Cookie

POST /api/auth/logout 
{
    "Token":
} - revoke refresh token from database

GET http://localhost:8081/api/users - get all users

```

```
# MS_Posts (default http://localhost:8082)

GET  /api/posts - get all posts | require [Role = Admin]

GET  /api/posts/{id} - get post by id

POST /api/posts 
{
    Title: "", 
    Body: "", 
    UserId: "" 
} - create new post

```

## Example

```
// Role Admin
POST http://localhost:8081/api/auth/login
{
    "Username": "Admin",
    "Password": "_Admin123"
}
// Role = User
POST http://localhost:8081/api/auth/login
{
    "Username": "testuser",
    "Password": "_Testuser123"
}


Header Authorization Bearer @token@
POST http://localhost:8082/api/posts
{
    "Title": "Title", 
    "Body": "Body", 
    "UserId": "a78eb4e6-349d-4a08-bdb6-5e9205f4f09a"
}

Header Authorization Bearer @token@
GET http://localhost:8082/api/posts

```

## Future TODO
 * API docx (Swagger)
 * Web-Client (ReactJS)
 * APIGateway
 * UnitTests
 * RabbitMQTT
 * Kubernetes

