#!/bin/bash

# Local test version: manually set variables, skip AWS CLI calls

CLUSTER_NAME="test-cluster"
SERVICE_NAME="test-service"
TARGET_GROUP_ARN="test-target-group-arn"
TARGET_GROUP_NAME="test-target-group"

# Debug output for variable values
echo "CLUSTER_NAME=$CLUSTER_NAME"
echo "SERVICE_NAME=$SERVICE_NAME"
echo "TARGET_GROUP_ARN=$TARGET_GROUP_ARN"
echo "TARGET_GROUP_NAME=$TARGET_GROUP_NAME"

# Safely quote as JSON strings
CLUSTER_NAME_JSON=$(jq -Rn --arg v "$CLUSTER_NAME" '$v')
SERVICE_NAME_JSON=$(jq -Rn --arg v "$SERVICE_NAME" '$v')
TARGET_GROUP_NAME_JSON=$(jq -Rn --arg v "$TARGET_GROUP_NAME" '$v')
TARGET_GROUP_ARN_JSON=$(jq -Rn --arg v "$TARGET_GROUP_ARN" '$v')

cat << EOF > dashboard.json
{
  "widgets": [
    {
      "type": "metric",
      "properties": {
        "metrics": [
          ["AWS/ECS", "CPUUtilization", "ClusterName", $CLUSTER_NAME_JSON, "ServiceName", $SERVICE_NAME_JSON]
        ],
        "period": 300,
        "stat": "Average",
        "region": "us-east-2",
        "title": "CPU Utilization"
      }
    },
    {
      "type": "metric",
      "properties": {
        "metrics": [
          ["AWS/ECS", "MemoryUtilization", "ClusterName", $CLUSTER_NAME_JSON, "ServiceName", $SERVICE_NAME_JSON]
        ],
        "period": 300,
        "stat": "Average",
        "region": "us-east-2",
        "title": "Memory Utilization"
      }
    },
    {
      "type": "metric",
      "properties": {
        "metrics": [
          ["AWS/ApplicationELB", "HealthyHostCount", "TargetGroup", $TARGET_GROUP_NAME_JSON],
          [".", "UnHealthyHostCount", "TargetGroup", $TARGET_GROUP_NAME_JSON]
        ],
        "period": 300,
        "stat": "Average",
        "region": "us-east-2",
        "title": "Container Health"
      }
    },
    {
      "type": "metric",
      "properties": {
        "metrics": [
          ["AWS/ApplicationELB", "RequestCount", "TargetGroup", $TARGET_GROUP_ARN_JSON]
        ],
        "period": 300,
        "stat": "Sum",
        "region": "us-east-2",
        "title": "Request Count"
      }
    },
    {
      "type": "metric",
      "properties": {
        "metrics": [
          ["AWS/ApplicationELB", "TargetResponseTime", "TargetGroup", $TARGET_GROUP_NAME_JSON]
        ],
        "period": 300,
        "stat": "Average",
        "region": "us-east-2",
        "title": "Response Time"
      }
    }
  ]
}
EOF

echo "dashboard.json generated for local validation."
cat dashboard.json
# Skipping aws cloudwatch put-dashboard for local test
