# AspNetCore WebApi Machine Learning Example for Sentiment and Spam predictions

This project demonstrates how to build Sentiment and Spam models on AspNetCore application
start. Before any of the requests are served the models are build and become available to the consumption.

`Bet.Extensions.ML.Sentiment` and `Bet.Extensions.ML.Spam` project contain the default data to be used
to generate the predictive models.

## Future work

- To enable the functionality to accept new data point and storing them inside of SQLite or other storage.
- To have a schedule job that rebuilds and verifies the models before making them available to predictive engine.
