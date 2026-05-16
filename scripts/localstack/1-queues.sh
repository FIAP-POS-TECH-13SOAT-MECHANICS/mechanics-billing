#!/bin/bash

QUEUES=(
  "fiap-mechanics-dev-budget-created"
  "fiap-mechanics-dev-customer-created"
  "fiap-mechanics-dev-payment-created"
  "fiap-mechanics-dev-payment-processed"
)

for QUEUE in "${QUEUES[@]}"; do
  awslocal sqs create-queue --queue-name "$QUEUE"
done
