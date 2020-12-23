# Bet.K8s.Web.Sample project

> The second letter in the Hebrew alphabet is the ב bet/beit. Its meaning is "house". In the ancient pictographic Hebrew it was a symbol resembling a tent on a landscape.

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Summary

This sample application is designed to test Kubernetes deployment to Azure Cloud.

- Azure Blob Storage (used for SQLite database with file share enabled)
- Azure Key Vault
- Azure Container Registry

Tests

- Docker containers to be pulled from the private ACR repo (requires k8s authentication)
- Access

## Package Sample Application

This app has a private Azure Container Registry (ACR), if desired to test with your cluster you can use a public docker image of this sample application.

```bash

    # run commands from the root directory of this project
    docker build --rm -f "src\Bet.K8s.Web.Sample\Dockerfile" -t {DOCKER_REGISTRY}/bet:k8sweb .

    # publish
    docker push {DOCKER_REGISTRY}/bet:k8sweb

```
Or with `docker compose`

```bash

    # local or dev build and run
    docker-compose -f "docker-compose.yml" -f "docker-compose.override.yml" up -d bet.hosting

    # production build
    docker-compose -f "docker-compose.yml" up -d --build --no-recreate bet.k8sweb

```
## Configure AKS for this sample application

For Azure Vault to work

1. had to refresh msi pods
2. Added reader permission for key vault

### Azure Container Registry (ACR)

Set up environment variables

```bash
    $acrName="betacr"
    $acrRG="bet-rg"
```

1. Create ACR if not already created

```bash
    # creates azure container registry acr
    az acr create -n $acrName -g bet-rg --sku Basic --admin-enabled --location centralus
```

2. Create Kubernetes Secret for ACR access

This must be created per kubernetes namespace.

```bash

    # set credentials for acr
    $acrCredPass = az acr credential show -n $acrName --query "passwords[1].value" -o tsv
    $acrCredUser = az acr credential show -n $acrName --query "username" -o tsv
    $acrServer = az acr list -g bet-rg --query "[0].loginServer" -o tsv
    $acrEmail = "email@gmail.com"

    # create kubernetes secret
    kubectl create secret docker-registry "betacr-acr" --docker-server=$acrServer --docker-username=$acrCredUser --docker-password=$acrCredPass --docker-email=$acrEmail

    # get the value of the docker registry
    kubectl describe secret/betacr-acr

    # renew password for rotations
    az acr credential renew -n $arcName --password-name password2
```

3. Deploy docker image to ACR

```bash
    # login to acr
    az acr login -n $acrName

    # publish to acr
    docker push betacr.azurecr.io/bet:k8sweb
```

### Configure Azure Blob Storage

```ps1

    $storageName="betstorage"
    $storageKey
    # setup fileshare storage secret for azurefile provider
    kubectl create secret generic betshare-secret --from-literal=azurestorageaccountname=$storageName --from-literal=azurestorageaccountkey=$storageKey
    kubectl describe secret/betshare-secret

```

## Azure Key Vault

Adding the required Secret to Azure Vault with Azure CLI command:

```bash
    az keyvault secret set -n betk8sweb--testValue --vault-name [vaultName] --value MySuperSecretThatIDontWantToShareWithYou!
```

### Helm Application Management

Please run this commands from the root of the application.

```bash

    # local install
    helm install betk8sweb k8s/betk8sweb --set ingress.enabled=false,local.enable=true

    # install in the cluster
    helm install betk8sweb k8s/betk8sweb --set ingress.enabled=true,aadpodidbinding=[podMsiId]

    # uninstall
    helm uninstall betk8sweb

    # troubleshooting
    kubectl get pods

    kubectl describe pod betk8sweb-[id]


    # login to pods
    kubectl exec --stdin --tty  betk8sweb-[id] -- /bin/sh
    # list mounts
    df -aTh

    # lists all of the claims
    kubectl get pv
```


## References

- [https://kubernetes.io/docs/reference/kubectl/cheatsheet/](https://kubernetes.io/docs/reference/kubectl/cheatsheet/)
- [Create an ingress controller with a static public IP address in Azure Kubernetes Service (AKS)](https://docs.microsoft.com/en-us/azure/aks/ingress-static-ip)
