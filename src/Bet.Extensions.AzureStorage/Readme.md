# Bet.Extensions.AzureStorage

This library contains collection of Azure Storage functionality.

- It provides with a configurable model that centralizes creation of the instances of `CloudStorageAccount` per configuration.
- It allows configuration of AspNetCore Static File options to use Azure Storage Blob Container.

Sample App requires [Azure storage emulator for development](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator)

## `StorageAccountOptions` class enables configuration of the `CloudStorageAccount`
Azure Storage Account can be created with the following configurations settings:

1. MSI authentication simply specify Azure Storage Name of the account.
```json
 "AzureStorage": {
    "Account": {
      "Name": "storageName",
      "Token":  "",
      "ConnectionString": ""
    }
  }
```

2. SAS Token Authentication
```json
 "AzureStorage": {
    "Account": {
      "Name": "storageName",
      "Token":  "tokenValue",
      "ConnectionString": ""
    }
  }
```

3. ConnectionString 
```json
 "AzureStorage": {
    "Account": {
      "Name": "",
      "Token":  "",
      "ConnectionString": "full-connection-string"
    }
  }
```

### Auzre Managed Identities support 

By default the following roles are not assigned:

- Storage Blob Data Contributor
- Storage Queue Data Contributor

## AspNetCore StaticFilesOptions as Azure Storage Blob Container

1. Make sure that default configuration exists for `CloudStorageAccount`. 
In the below configuration MSI authentication will be used to connect to the container. 
As you can see no need to provide secure SAS token.

```json
 "AzureStorage": {
    "Account": {
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
    services.AddStorageBlob()
            .AddBlobContainer<UploadsStorageBlobsOptions>()
```

5. Enable middleware

```charp
   app.UseAzureStorageForStaticFiles<UploadsProviderOptions>();
```

## Resources
[Services that support managed identities for Azure resources](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/services-support-managed-identities#azure-storage-blobs-and-queues)
[Authenticate access to blobs and queues with Azure Active Directory and managed identities for Azure Resources](https://docs.microsoft.com/en-us/azure/storage/common/storage-auth-aad-msi)
