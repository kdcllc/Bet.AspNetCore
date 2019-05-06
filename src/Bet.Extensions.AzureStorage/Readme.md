# Bet.Extensions.AzureStorage

This library contains collection of Azure Storage functionality.

## `StorageAccountOptions` class enables configuration of the `CloudStorageAccount`

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

### Auzre Managed Identies support 

By default the following roles are not assigned:

- Storage Blob Data Contributor
- Storage Queue Data Contributor

[Services that support managed identities for Azure resources](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/services-support-managed-identities#azure-storage-blobs-and-queues)