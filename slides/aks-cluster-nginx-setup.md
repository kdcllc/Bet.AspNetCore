---
theme: gaia
paginate: true
backgroundColor: #fff
marp: true
backgroundImage: url('./artifacts/background.png')
footer: '![image w:25 h:25](./artifacts/logo.png)'
---

<!-- _class: lead -->

![bg left:40% 80%](./artifacts/ms-azure-logo.png)

# **AKS Cluster Guide**

Setup NGINX Ingress Controller with Static IP Address

---

# Install NGINX Ingress Controller

```ps1
    # Create a namespace for your ingress resources
    kubectl create namespace ingress-basic

    # Add the official stable repository for helm packages
    helm repo add stable https://kubernetes-charts.storage.googleapis.com/
```
[Create an ingress controller with a static public IP address in Azure Kubernetes Service (AKS)](https://docs.microsoft.com/en-us/azure/aks/ingress-static-ip)

---

# Get Public IP Address

```ps1
    $micRG = az aks show --resource-group kdcllc-aks-cluster-rg --name kdcllc-aks --query nodeResourceGroup -o tsv

    $ipName = "nginxPublicIP"

    # create static ip address
    az network public-ip create  --resource-group $micRG  --name $ipName  --allocation-method static

    $IP=az network public-ip show --resource-group  $micRG --name $ipName --query ipAddress --output tsv

```

---

# X-Forwarded-For

If you would like to enable client source IP preservation for requests to containers in your cluster, add --set controller.service.externalTrafficPolicy=Local to the Helm install command. The client source IP is stored in the request header under X-Forwarded-For. When using an ingress controller with client source IP preservation enabled, TLS pass-through will not work.

---

# Install NGINX Ingress Controller

```ps1

    # Use Helm to deploy an NGINX ingress controller
    helm install nginx-ingress stable/nginx-ingress `
        --namespace ingress-basic `
        --set controller.replicaCount=2 `
        --set controller.service.externalTrafficPolicy=Local `
        --set controller.nodeSelector."beta\.kubernetes\.io/os"=linux `
        --set defaultBackend.nodeSelector."beta\.kubernetes\.io/os"=linux `
        --set controller.service.loadBalancerIP=$IP `
        --set controller.service.annotations."service\.beta\.kubernetes\.io/azure-dns-label-name"="[dns]-aks-dns"
```

# Questions :satisfied:?
