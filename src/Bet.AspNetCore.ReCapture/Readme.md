# Bet.AspNetCore.ReCapture

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.AspNetCore.ReCapture.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore.ReCapture)
[![MyGet](https://img.shields.io/myget/kdcllc/v/Bet.AspNetCore.ReCapture.svg?label=myget)](https://www.myget.org/F/kdcllc/api/v2)

This library was design to support `Goolge ReCapture`.
The Design was to keep it as simple as possible.

Add the following to the project

```csharp
    dotnet add package Bet.AspNetCore.ReCapture
```

## Usage
Steps to enable this within your project

1. Add the following in `_ViewImports.cshtml`

```csharp
    @using Bet.AspNetCore.ReCapture
    @addTagHelper *, Bet.AspNetCore.ReCapture
```

2. Add In `Startup.cs` file

```csharp
   services.AddReCapture(Configuration);
```

3. On the form add this

```html
    <google-recaptcha></google-recaptcha>
```
