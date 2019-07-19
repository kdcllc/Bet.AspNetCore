# .NET Core Service Worker for Machine Learning (ML.NET) Model Building

This example demonstrates how to schedule Machine Learning Model building process for Spam and Sentiment on 30 min interval.
The idea is that model would have the input of a new datapoints and would require the rebuild.

It runs as .NET Core Service Worker and can be hosted in Docker/Kubernetes and generate new models based on the new data as it becomes
available.

