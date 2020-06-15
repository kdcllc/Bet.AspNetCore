# Kuberbetes

[Manually create and use a volume with Azure Files share in Azure Kubernetes Service (AKS)](https://docs.microsoft.com/en-us/azure/aks/azure-files-volume)

```bash
    # install
    kubectl apply -f azurefile-betazurefile-pv.yaml

    # delete
    kubectl delete -f azurefile-betazurefile-pv.yaml


    # install
    kubectl apply -f azurefile-betazurefile-pvc.yaml

    # delete
    kubectl delete -f azurefile-betazurefile-pvc.yaml
```