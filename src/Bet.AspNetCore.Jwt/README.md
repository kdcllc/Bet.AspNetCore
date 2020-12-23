# Bet.AspNetCore.Jwt

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.AspNetCore.Jwt.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore.Jwt)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.AspNetCore.Jwt/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.AspNetCore.Jwt/latest/download)

> The second letter in the Hebrew alphabet is the ב bet/beit. Its meaning is "house". In the ancient pictographic Hebrew it was a symbol resembling a tent on a landscape.

*Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json).*

## Summary

The main purpose of this repo is to create a basic but reusable library for:

- JWT authentication
- Flexile Authentication providers. Default is based on values stored in Configurations.

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Install

```csharp
    dotnet add package Bet.AspNetCore.Jwt
```

```JSON
  "JwtTokenAuthOptions": {

    "Issuer": "kingdavidconsulting.com",
    "Salt":"Rm9yIEdvZCBzbyBsb3ZlZCB0aGUgd29ybGQsIHRoYXQgaGUgZ2F2ZSBoaXMgb25seSBiZWdvdHRlbiBTb24sIHRoYXQgd2hvc29ldmVyIGJlbGlldmV0aCBpbiBoaW0gc2hvdWxkIG5vdCBwZXJpc2gsIGJ1dCBoYXZlIGV2ZXJsYXN0aW5nIGxpZmUK",
    "Secret": "Sm9obiAzOjE2Cg==",
    "Audience": "api"
  },

"UserStoreOptions": {
    "Users": [
      {
        "Id": 1,
        "UserName": "user1",
        "Password": "P@ssword!"
      },
      {
        "Id": 2,
        "UserName": "user2",
        "Password": "P@ssword2!"
      }
    ]
  }
```

```bash
    # generate secret
    echo John 3:16 | base64

    # generate salt
    echo 'For God so loved the world, that he gave his only begotten Son, that whosoever believeth in him should not perish, but have everlasting life' | base64

    # decode

    echo '' | base64 -d
```


## Json Serilization

```
dotnet add package --version 5.6.2 Swashbuckle.AspNetCore.Newtonsoft
services.AddSwaggerGenNewtonsoftSupport(); // explicit opt-in - needs to be placed after AddSwaggerGen()
```
