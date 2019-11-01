# Bet.Extensions.DataProtection

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.DataProtection.svg)](https://www.nuget.org/packages?q=Bet.Extensions.DataProtection)
[![MyGet](https://img.shields.io/myget/kdcllc/v/Bet.Extensions.DataProtection.svg?label=myget)](https://www.myget.org/F/kdcllc/api/v2)

This library provides with ability to add `DataProtection` to any DotNetCore appliction.


```bash
    dotnet add package Bet.Extensions.DataProtection
```

## Azure Resources Setup

### Azure Key Vault

[Manage Key Vault in Azure Stack using PowerShell](https://docs.microsoft.com/en-us/azure-stack/user/azure-stack-key-vault-manage-powershell?view=azs-1908)

1. Create Azure Key In the Azure Key Vault

![Azure Key Creation](https://raw.githubusercontent.com/kdcllc/Bet.AspNetCore/master/img/azure-key-vault-key-creation.jpg)

2. Access Policies -> Add Access Policy
![Azure Key Policy Creation](https://raw.githubusercontent.com/kdcllc/Bet.AspNetCore/master/img/azure-key-vault-key-policy.jpg)

### Azure Blob Storage

Make sure developer account and MSI account have `Storage Blob Data Contributor` Role assigned to the storage account.

## Usage

Razor Page Usage

```csharp

```

MVC Controller

```csharp
public class HomeController : Controller
{
    private const string CookieName = "TestCookie";
    private readonly IDataProtector _dataProtector;

    public HomeController(IDataProtectionProvider dataProtectionProvider)
    {
        _dataProtector = dataProtectionProvider.CreateProtector("Test");
    }

    public IActionResult Index()
    {
        if (!Request.Cookies.TryGetValue(CookieName, out var cookieValue))
        {
            string valueToSetInCookie = $"Some text set in cookie at {DateTime.Now.ToString()}";
            var encryptedValue = _dataProtector.Protect(valueToSetInCookie);
            Response.Cookies.Append(CookieName, encryptedValue, new Microsoft.AspNetCore.Http.CookieOptions
            {
                IsEssential = true
            });
            return RedirectToAction("Index");
        }

        ViewBag.CookieValue = _dataProtector.Unprotect(cookieValue);
        return View();
    }
}
```

```csharp
@{
    ViewData["Title"] = "Home Page";
}

<h2>Decrypted value from cookie:</h2>
<p>@ViewBag.CookieValue</p>
```