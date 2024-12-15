# Orbit42 - Backend

## Introduction

The backend part of the Orbit42 project.

## Solution structure 

* API layer: Orback.Api/Orback.Api.csproj
* Application layer: Orback.Application/Orback.Application.csproj
* Domain layer: Orback.Domain/Orback.Domain.csproj
* Infrastructure layer: Orback.Infrastructure/Orback.Infrastructure.csproj

## Useful commands

* List solution's projects
```sh
dotnet sln lst
```

* Do SQL migrations
```sh
dotnet ef migrations add MyCustomMiGrationName --project Orback.Infrastructure --startup-project Orback.Api
```

* Update SQL database
```sh 
 dotnet ef database update --project Orback.Infrastructure --startup-project Orback.Api 
```
