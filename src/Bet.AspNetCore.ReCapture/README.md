# Bet.AspNetCore.ReCapture

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.AspNetCore.ReCapture.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore.ReCapture)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.AspNetCore.ReCapture/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.AspNetCore.ReCapture/latest/download)


This library was design to support `Goolge ReCapture`.
The Design was to keep it as simple as possible.

Pre-release packages are distributed via feedz.io `https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json`.

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
