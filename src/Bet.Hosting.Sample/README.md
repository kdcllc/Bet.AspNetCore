# .NET Core Service Worker for Machine Learning (ML.NET) Model Building

This example demonstrates how to schedule Machine Learning Model building process for Spam and Sentiment.

The idea is that model would have the input of a new `datapoints` and would require the rebuild.

The sample runs as .NET Core Service Worker and can be hosted in Docker/Kubernetes and generate new models based on the new data as it becomes
available.

This sample is fully containerized to be used as:

1. As stand-alone Docker container that runs application every 30 min interval
2. As [Kubernetes CronJob](https://kubernetes.io/docs/concepts/workloads/controllers/cron-jobs/) with interval specified in the deployment.


## Testing K8 Cron Job in the local cluster

Please follow the setup instruction per [K8.DotNetCore.Workshop](https://github.com/kdcllc/K8.DotNetCore.Workshop).

1. Build the Image

```bash
    # builds and runs the container
    docker-compose -f "docker-compose.yml" -f "docker-compose.override.yml" up -d bet.hosting

    # simply builds the image
    docker-compose -f "docker-compose.yml" up -d --build --no-recreate bet.hosting
```

```yaml
# https://kubernetes.io/docs/tasks/job/automated-tasks-with-cron-jobs/

apiVersion: batch/v1beta1
kind: CronJob
metadata:
  name: mlmodelbuilder-job
spec:
  schedule: "*/5 * * * *"
  concurrencyPolicy: Forbid
  successfulJobsHistoryLimit: 1
  failedJobsHistoryLimit: 2
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: mlmodelbuilder-job
            image: kdcllc/bet-hosting-sample:v1
            command: ["./Bet.Hosting.Sample"]
            env:
              - name: MSI_ENDPOINT
                value: http://host.docker.internal:5050/oauth2/token
              - name: MSI_SECRET
                value: 1c791bd8-94e7-468f-97f3-4b46a1d0aea5
            args:
            - "--runAsCronJob=true"
          restartPolicy: OnFailure
```

- Install `kubectl apply -f "cronjob.yaml"`

- Remove `kubectl delete -f "cronjob.yaml"`
