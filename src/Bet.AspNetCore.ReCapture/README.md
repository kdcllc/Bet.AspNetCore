# Bet.AspNetCore.ReCapture

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.AspNetCore/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.AspNetCore.ReCapture.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore.ReCapture)
![Nuget](https://img.shields.io/nuget/dt/Bet.AspNetCore.ReCapture)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.AspNetCore.ReCapture/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.AspNetCore.ReCapture/latest/download)

> The second letter in the Hebrew alphabet is the ב bet/beit. Its meaning is "house". In the ancient pictographic Hebrew it was a symbol resembling a tent on a landscape.

*Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json).*

## Summary

This library was design to support `Goolge ReCapture` with `AspNetCore` Razor Pages or MVC Views. The Design was to keep it as simple as possible.

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Install

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
