# Bet.Extensions.LetsEncrypt

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.AspNetCore/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.LetsEncrypt.svg)](https://www.nuget.org/packages?q=Bet.Extensions.LetsEncrypt)
[![NuGet](https://img.shields.io/nuget/v/Bet.AspNetCore.svg)](https://www.nuget.org/packages?q=Bet.Extensions.LetsEncrypt)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.Extensions.LetsEncrypt/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.Extensions.LetsEncrypt/latest/download)

> The second letter in the Hebrew alphabet is the ב bet/beit. Its meaning is "house". In the ancient pictographic Hebrew it was a symbol resembling a tent on a landscape.

*Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json).*

## Summary

The basic logic for Let's Encrypt SSL certificate generation.

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Install

```csharp
    dotnet add package Bet.Extensions.LetsEncrypt
```

## Troubleshooting DNS Challenge Validation

### Querying TXT records

`nslookup` is the primary tool most people use to do DNS queries from Windows because it's installed by default and available on every Windows OS since basically forever.

```cmd
    C:\>nslookup -q=txt _acme-challenge.example.com.
```

**Don't forget the final `.` on the domain name.**

This is the basic command that will query your local DNS server. But it's usually wise to specifically query a public DNS resolver like Google (8.8.8.8) or CloudFlare (1.1.1.1) in case you're in a split-brain DNS environment. However, some businesses are starting to deploy firewalls that block outbound DNS requests like this. So make sure you are able to query a known good record before tearing your hair out troubleshooting the record you're having trouble with. Here's how to explicitly query an external resolver.

```cmd
    C:\>nslookup -q=txt _acme-challenge.example.com. 1.1.1.1
```

### Windows 10 flushing DNS

```cmd
    ipconfig /flushdns
```

