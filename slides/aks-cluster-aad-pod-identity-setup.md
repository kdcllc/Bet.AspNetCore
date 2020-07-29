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

Setup AAD Pod Identity Configuration in AKS

---

# AAD Pod Identity Project

[AAD Pod Identity](https://github.com/Azure/aad-pod-identity) enables Kubernetes applications to access cloud resources securely with Azure Active Directory (AAD).

Using Kubernetes primitives, administrators configure identities and bindings to match pods. Then without any code modifications, your containerized applications can leverage any resource in the cloud that depends on AAD as an identity provider.

---

# Design Concept

![width:478px height:341px](https://github.com/Azure/aad-pod-identity/raw/master/docs/design/concept.png)

---

# Deploy aad-pod-identity

Use `kube-system` namespace

Deploy `aad-pod-identity` components to an RBAC-enabled cluster:

```bash
    kubectl apply -f https://raw.githubusercontent.com/Azure/aad-pod-identity/master/deploy/infra/deployment-rbac.yaml

    # For AKS clusters, deploy the MIC and AKS add-on exception by running -
    kubectl apply -f https://raw.githubusercontent.com/Azure/aad-pod-identity/master/deploy/infra/mic-exception.yaml
```

---

# Setup Environment variables

```bash
    # resource group that is not MS_ prefixed
    $rg="aks-cluster-rg"
    $podId="aks-identity"

    # cluster name
    $name="a-aks"
```

---

# Create Managed Identity (user-assigned identity)

```bash
    # Create Identity
    az identity create -g $rg -n $podId -o json

    # Assign Role
    # use this value for aadpodidentity.yaml clientId value
    $clientId = az aks show -g $rg -n $name --query servicePrincipalProfile.clientId -o tsv
    # use this value for aadpodidentity-binding.yaml resourceId
    $resourceId = az identity show -g $rg -n $podId --query "id" --output tsv
    az role assignment create --role "Managed Identity Operator" --assignee $clientId --scope $resourceId
```
[Role Assignment](https://github.com/Azure/aad-pod-identity/blob/master/docs/readmes/README.role-assignment.md)

---

# Create AzureIdentity with `aadpodidentity.yaml`
```bash
    echo $clientId
    echo $resourceId
```
```yml
apiVersion: "aadpodidentity.k8s.io/v1"
kind: AzureIdentity
metadata:
  name: [$podId]
spec:
  type: 0   #user assigned MS or type:1 Service Principal
  # az identity show -g $rg -n $podId --query "id" --output tsv
  resourceID: /subscriptions/2092a2b8-c72d-4127-98ff-3c16161d9721/resourcegroups/$rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/$podId
  # az identity show -g $rg -n $podId --query clientId -otsv
  clientID: e977848a-d9d5-4666-aeb2-6bbfb79f118c
```
Run: `kubectl apply -f aadpodidentity.yaml`

---

# Create AzureIdentityBinding with `aadpodidentity-binding.yaml`
```yml
apiVersion: "aadpodidentity.k8s.io/v1"
kind: AzureIdentityBinding
metadata:
  name: $podId-binding
spec:
  azureIdentity: $podId
  selector: $podId_access
```
Run: `kubectl apply -f aadpodidentity-binding.yaml`

---

# Check install

1. `kubectl get azureidentities -o yaml` or `kubectl describe azureidentity`

2. `kubectl get azureidentitybindings -o yaml` or `kubectl describe azureidentitybinding`

---

# Using MSI with Deployment

```yml
spec:
  template:
    metadata:
      labels:
        # $podId_access
       aadpodidbinding: "<value of Selector in AzureIdentityBinding>"
```

---

# Configure Azure Vault MSI Access

- Configure Managed Identity has `Reader` role on Azure Key Vault resource

```bash
    $vault="v1"
    $vaultId = az keyvault show --name $vault --query "id" --output tsv
    $msiClientId = az identity show -g $rg -n $podId --query "clientId" --output tsv

    # assign role
    az role assignment create --role "Reader" --assignee $msiClientId --scope $vaultId

    # allow read for secrets
    az keyvault set-policy -n $vault --secret-permissions get list --spn $msiClientId
```

---

# Questions :satisfied:?
