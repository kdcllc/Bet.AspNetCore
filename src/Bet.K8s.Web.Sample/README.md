# Bet.K8s.Web.Sample project

This sample application is designed to test Kubernetes deployment to Azure Cloud.

- Azure Blob Storage (used for SQLite database with file share enabled)
- Azure Key Vault
- Azure Container Registry

Tests

- Docker containers to be pulled from the private ACR repo (requires k8s authentication)
- Access

## Package Sample Application

This app has a private Container Registry, if desired to test with your cluster you can use a public docker image of this sample application.

```bash

    # run commands from the root directory of this project
    docker build --rm -f "src\Bet.K8s.Web.Sample\Dockerfile" -t {DOCKER_REGISTRY}/bet:k8sweb .

    # publish
    docker push {DOCKER_REGISTRY}/bet:k8sweb

```

### ACR

```bash

    $acrName="betacr"
    $acrRG="bet-rg"

    # creates azure container registry acr
    az acr create -n $acrName -g bet-rg --sku Basic --admin-enabled --location centralus

    # login to acr
    az acr login -n $acrName

    # publish to acr
    docker push betacr.azurecr.io/bet:k8sweb

    # renew password
    az acr credential renew -n $arcName --password-name password2

    # set credentials
    $acrCredPass = az acr credential show -n $acrName --query "passwords[1].value" -o tsv
    $acrCredUser = az acr credential show -n $acrName --query "username" -o tsv
    $acrServer = az acr list -g bet-rg --query "[0].loginServer" -o tsv
    $acrEmail = "kingdavidconsulting@gmail.com"

    kubectl create secret docker-registry "betacr-acr" --docker-server=$acrServer --docker-username=$acrCredUser --docker-password=$acrCredPass --docker-email=$acrEmail

    # get the value
    kubectl describe secret/betacr-acr


    # setup fileshare storage

    kubectl create secret generic betshare-secret --from-literal=azurestorageaccountname=$storageName --from-literal=azurestorageaccountkey=$storageKey
    kubectl describe secret/betshare-secret

    1. had to refresh msi pods
    2. Added reader permission for key vault

    # login to pods
    kubectl exec --stdin --tty  betk8sweb-576b969558-gghlw -- /bin/sh
    # list mounts
    df -aTh

    # install
     helm install betk8sweb k8s/betk8sweb --set ingress.enabled=true,aadpodidbinding={id} --disable-openapi-validation
     helm uninstall betk8sweb

    kubectl apply -f k8s/betshare-pv.yaml

    kubectl apply -f k8s/betshare-pvc.yaml

    kubectl get pods

    kubectl describe pod betk8sweb-6f4cc779b8-847f8

    # lists all of the claims
    kubectl get pv
```




```bash
    # local install
    helm install betk8sweb -n azuretest --set ingress.enable=false,

    # install in the cluster
    helm install betk8sweb --set local.enable=false  -n betk8sweb

    # remove
    helm delete  betk8sweb --purge
```

Adding the required Secret to Azure Vault with Azure CLI command:

```bash
    az keyvault secret set -n betk8sweb--testValue --vault-name [vaultName] --value MySuperSecretThatIDontWantToShareWithYou!
```

## Azure Key Vault

## References

- [https://kubernetes.io/docs/reference/kubectl/cheatsheet/](https://kubernetes.io/docs/reference/kubectl/cheatsheet/)