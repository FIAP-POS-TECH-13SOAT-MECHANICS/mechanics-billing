#!/bin/bash

TABLE="billing-dev-payments"
awslocal dynamodb create-table \
  --table-name "$TABLE" \
  --attribute-definitions \
    AttributeName=id,AttributeType=S \
    AttributeName=workOrderId,AttributeType=S \
    AttributeName=budgetId,AttributeType=S \
    AttributeName=externalReference,AttributeType=S \
    AttributeName=mercadoPagoPaymentId,AttributeType=S \
  --key-schema AttributeName=id,KeyType=HASH \
  --global-secondary-indexes '[
    {
      "IndexName": "workOrderId-index",
      "KeySchema": [{"AttributeName": "workOrderId", "KeyType": "HASH"}],
      "Projection": {"ProjectionType": "ALL"}
    },
    {
      "IndexName": "budgetId-index",
      "KeySchema": [{"AttributeName": "budgetId", "KeyType": "HASH"}],
      "Projection": {"ProjectionType": "ALL"}
    },
    {
      "IndexName": "externalReference-index",
      "KeySchema": [{"AttributeName": "externalReference", "KeyType": "HASH"}],
      "Projection": {"ProjectionType": "ALL"}
    },
    {
      "IndexName": "mercadoPagoPaymentId-index",
      "KeySchema": [{"AttributeName": "mercadoPagoPaymentId", "KeyType": "HASH"}],
      "Projection": {"ProjectionType": "ALL"}
    }
  ]' \
  --billing-mode PAY_PER_REQUEST > /dev/null
echo "Table '$TABLE' created."
