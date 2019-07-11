# Certes  and Let's Encrypt

## Setting up Azure DNS


## Creating Manually SSL Certificate

```
    certes account new --out cert-account.pem email@email.com
```


```

certes az dns https://acme-v02.api.letsencrypt.org/acme/order/1/2 `
  --resource-group my-res-grp                                     `
  --subscription-id 00000000-0000-0000-0000-000000000000          `
  --tenant-id 00000000-0000-0000-0000-000000000000                `
  --client-id 00000000-0000-0000-0000-000000000000                `
  --client-secret my-pwd
```


## Usage

```
    webBuilder.UseLetsEncrypt(configure =>
    {
        configure.Email = "email@domain.com";
        configure.HostNames = new[] { DomainName, "domain.com" };

        configure.UseStagingServer = true;

        configure.CertificateFriendlyName = DomainName;
        configure.CertificatePassword = "7A1FE7EE-8DAF-423D-B43B-A55E6794DCD9";

        configure.CertificateSigningRequest = new CsrInfo()
        {
            CountryName = "US",
            Organization = "KDCLLC",
            OrganizationUnit = "Dev",
            CommonName = DomainName
        };
    }); 
```
