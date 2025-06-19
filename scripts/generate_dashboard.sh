#!/bin/bash

# Check if jq is installed, install if not
if ! command -v jq &> /dev/null; then
  echo "jq not found, installing..."
  sudo apt-get update && sudo apt-get install -y jq
fi

# Create dashboard for the service
DASHBOARD_NAME="product-management-prod-15741561983"
CLUSTER_NAME=$(aws cloudformation describe-stacks --stack-name ECSFargateServiceStack-prod --query 'Stacks[0].Outputs[?OutputKey==`ClusterName`].OutputValue' --output text)
SERVICE_NAME=$(aws cloudformation describe-stacks --stack-name ECSFargateServiceStack-prod --query 'Stacks[0].Outputs[?OutputKey==`ServiceName`].OutputValue' --output text)
TARGET_GROUP_ARN=$(aws cloudformation describe-stacks --stack-name ECSFargateServiceStack-prod --query 'Stacks[0].Outputs[?OutputKey==`TargetGroupArn`].OutputValue' --output text)
# Extract Target Group Name from ARN
TARGET_GROUP_NAME=$(basename "${TARGET_GROUP_ARN}")

# Debug output for variable values
echo "CLUSTER_NAME=$CLUSTER_NAME"
echo "SERVICE_NAME=$SERVICE_NAME"
echo "TARGET_GROUP_ARN=$TARGET_GROUP_ARN"
echo "TARGET_GROUP_NAME=$TARGET_GROUP_NAME"

# Validate variables
if [[ -z "$CLUSTER_NAME" || -z "$SERVICE_NAME" || -z "$TARGET_GROUP_NAME" || -z "$TARGET_GROUP_ARN" ]]; then
  echo "::error::One or more required CloudWatch dimension values are empty."
  exit 1
fi

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

aws cloudwatch put-dashboard --dashboard-name ${DASHBOARD_NAME} --dashboard-body file://dashboard.json
echo "Created/Updated CloudWatch Dashboard: ${DASHBOARD_NAME}"