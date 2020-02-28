# Bet.Extensions.AzureStorage

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.AzureStorage.svg)](https://www.nuget.org/packages?q=Bet.Extensions.AzureStorage)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.Extensions.AzureStorage/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.Extensions.AzureStorage/latest/download)

Pre-release packages are distributed via feedz.io `https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json`.

Add the following to the project

```csharp
    dotnet add package Bet.Extensions.AzureStorage
```

This library contains collection of Azure Storage functionality.

- It provides with a configurable model that centralizes creation of the instances of `CloudStorageAccount` per configuration.
- It allows configuration of AspNetCore Static File options to use Azure Storage Blob Container.

Sample App requires [Azure storage emulator for development](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator)

## Usage

### `StorageAccountOptions` class enables configuration of the `CloudStorageAccount`

Azure Storage Account can be created with the following configurations settings:

1. MSI authentication simply specify Azure Storage Name of the account.

```json
 "AzureStorage": {
    "DefaultAccount": {
      "Name": "storageName"
    }
  }
```

2. SAS Token Authentication

```json
 "AzureStorage": {
    "DefaultAccount": {
      "Name": "storageName",
      "Token":  "tokenValue",
      "ConnectionString": ""
    }
  }
```

3. ConnectionString

```json
 "AzureStorage": {
    "DefaultAccount": {
      "ConnectionString": "full-connection-string"
    }
  }
```

### Azure Managed Identities support

By default the following roles are not assigned:

- Storage Blob Data Contributor
- Storage Queue Data Contributor

### AspNetCore StaticFilesOptions as Azure Storage Blob Container

1. Make sure that default configuration exists for `CloudStorageAccount`.
In the below configuration MSI authentication will be used to connect to the container.
As you can see no need to provide secure SAS token.

```json
 "AzureStorage": {
    "DefaultAccount": {
      "Name": "teststorage"
    }
  }
```

2. Create `UploadsStorageBlobsOptions` class

```csharp
    public class UploadsProviderOptions : StorageFileProviderOptions
    {
    }

````

3. Configure the Azure Blob Container

```json
  "StorageFileProviders": {
    "UploadsProviderOptions": {
      "RequestPath": "/uploads",
      "ContainerName": "uploads",
      "EnableDirectoryBrowsing": true
    }
  }
```

4. Add Service registration

```csharp
    services.AddAzureStorageAccount()
            .AddAzureBlobContainer<UploadsBlobOptions>()
            .AddAzureStorageForStaticFiles<UploadsBlobStaticFilesOptions>();
```

5. Enable middleware

```charp
   app.UseAzureStorageForStaticFiles<UploadsProviderOptions>();
```

## Resources

[Services that support managed identities for Azure resources](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/services-support-managed-identities#azure-storage-blobs-and-queues)
[Authenticate access to blobs and queues with Azure Active Directory and managed identities for Azure Resources](https://docs.microsoft.com/en-us/azure/storage/common/storage-auth-aad-msi)
