# Bet.Extensions.DataProtection

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.AspNetCore/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.DataProtection.svg)](https://www.nuget.org/packages?q=Bet.Extensions.DataProtection)
![Nuget](https://img.shields.io/nuget/dt/Bet.Extensions.DataProtection)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.Extensions.DataProtection/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.Extensions.DataProtection/latest/download)

> The second letter in the Hebrew alphabet is the ב bet/beit. Its meaning is "house". In the ancient pictographic Hebrew it was a symbol resembling a tent on a landscape.

*Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json).*

## Summary

This library provides with ability to add `DataProtection` to any DotNetCore application.

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Install

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

1. Add the following to `Startup.cs` `ConfigureServices` method:

```csharp
    services.AddDataProtectionAzureStorage();
```

2. Add options to `appsettings.json`:

```json

  "DataProtectionAzureStorage": {
    "KeyVaultKeyId": "https://{name}.vault.azure.net/keys/{keyname}/{keyId}", // valut
    "ConnectionString": "",
    "Token": "",
    "Name": "",
    "ContainerName": "dataprotection",
    "KeyBlobName": "some-keys.xml"
  },
```

- Razor Page Usage

```csharp
public class IndexModel : PageModel
    {
        private const string CookieName = "TestCookie";

        private readonly IDataProtector _dataProtector;

        public IndexModel(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtector = dataProtectionProvider.CreateProtector("Test");
        }

        public string CookieValue { get; set; }

        public bool ShowCookieValue => !string.IsNullOrEmpty(CookieValue);

        public void OnGet()
        {
            if (!Request.Cookies.TryGetValue(CookieName, out var cookieValue))
            {
                var valueToSetInCookie = $"Some text set in cookie at {DateTime.Now.ToString()}";
                var encryptedValue = _dataProtector.Protect(valueToSetInCookie);
                Response.Cookies.Append(CookieName, encryptedValue, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    IsEssential = true
                });
                return;
            }

            CookieValue = _dataProtector.Unprotect(cookieValue);
        }
    }
```

Page View:

```html
    @if (Model.ShowCookieValue)
    {
        <h2>Decrypted value from cookie:</h2>
        <p>@Model.CookieValue</p>
    }
    else
    {

        <p>
            <strong>No Test Cookie exists:</strong> refresh browser.
        </p>
    }
```

- MVC Controller

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
