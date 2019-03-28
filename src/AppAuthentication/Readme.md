# AppAuthentication Cli Tool
This dotnet cli tool provides ability to accesses Azure Vault in Docker Container when this container is ran on the local machine.

Requirements for this tool:
- Windows 10 and Visual Studio.NET/AzureCli
- Linux and AzureCli

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

- Install cli tool

```cmd
    dotnet tool install --global appauthentication
```

- Make sure that all of the existing `Containers` and `Images` are removed from the system as well.

- Go to Tools --> Options --> Azure Service Authentication --> Account Selection and Select the account that will be used for authenticating with your company's Azure Vault.


- Retrieve Azure AD Directory Id by navigating to [Azure Portal](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/Properties)

- Run this CLI tool with the following sample command:
`Default provider is Visual Studio provider`
```bash
    appauthentication run -a  https://login.microsoftonline.com/{companyDirectoryGuidId} -v
    
    #or

    appauthentication run -a  {companyDirectoryGuidId} -v


    #or azure cli
    
    appauthentication run -a  {companyDirectoryGuidId} -v --token-provider AzureCli

```
If AzureCli provider is used please make sure you log into Azure with the following commands:

```bash
    az login
    az account list
    az account set –subscription “YourSubscriptionName”
```

- Now you are ready to run your docker container locally and use Azure Vault.
