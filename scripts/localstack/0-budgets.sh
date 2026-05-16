#!/bin/bash

TABLE="billing-dev-budgets"
awslocal dynamodb create-table \
  --table-name "$TABLE" \
  --attribute-definitions \
    AttributeName=id,AttributeType=S \
    AttributeName=workOrderId,AttributeType=S \
    AttributeName=customerId,AttributeType=S \
  --key-schema AttributeName=id,KeyType=HASH \
  --global-secondary-indexes '[
    {
      "IndexName": "workOrderId-index",
      "KeySchema": [
        {"AttributeName": "workOrderId", "KeyType": "HASH"}
      ],
      "Projection": {"ProjectionType": "ALL"}
    },
    {
      "IndexName": "customerId-index",
      "KeySchema": [
        {"AttributeName": "customerId", "KeyType": "HASH"}
      ],
      "Projection": {"ProjectionType": "ALL"}
    }
  ]' \
  --billing-mode PAY_PER_REQUEST > /dev/null
echo "Table '$TABLE' created."
