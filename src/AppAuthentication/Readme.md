# AppAuthentication Cli Tool

Requirements for this tool:
- Windows 10
- Visual Studio.NET

## Usage

Before running this cli tool please make sure that your project files updated to support the following environment variables that this tool generates:

```
    MSI_ENDPOINT=http://host.docker.internal:5050/oauth2/token
    MSI_SECRET=9243C6B3-7E00-400C-B065-0DBF77D33E84
```

This can be done via `docker-compose.override.yml` like so:

```yml

    environment:
      - MSI_ENDPOINT=${MSI_ENDPOINT}
      - MSI_SECRET=${MSI_SECRET}
```

- Make sure that all of the existing `Containers` and `Images` are removed from the system as well.
- Select `Azure Service Authentication Account Selection` by going to Tools --> Options to the account that will be authenticated 
with your company's Azure Vault.

- Retrieve Azure AD Directory Id by navigating to [Azure Portal](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/Properties)

- Run this CLI tool with the following sample command:

```bash
    appauthentication run -a  https://login.microsoftonline.com/{companyDirectoryId} -v
```

-- Now you are ready to run your docker container locally and use Azure Vault.
