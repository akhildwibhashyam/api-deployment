using Amazon.CDK;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.CloudWatch;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.CloudWatch.Actions;
using Constructs;

namespace InfrastructureAsCode.Stacks
{
    public class ECSFargateServiceStack : Stack
    {
        // Fix: Make FargateService nullable to satisfy the compiler for the default constructor
        public ApplicationLoadBalancedFargateService? FargateService { get; private set; }

        public ECSFargateServiceStack(Construct scope, string id, Vpc vpc, Repository ecrRepo, string imageTag, StackProps? props = null)
            : base(scope, id, props)
        {
            // Create CloudWatch Log Group for Fargate service
            var logGroup = new LogGroup(this, "ProductManagementLogs", new LogGroupProps
            {
                LogGroupName = $"/ecs/product-management-{id}",
                Retention = RetentionDays.ONE_WEEK,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // Create SNS Topic for Alarms
            var alarmTopic = new Topic(this, "ProductManagementAlarms", new TopicProps
            {
                TopicName = $"product-management-alarms-{id.ToLower()}"
            });

            // Add email subscription to the alarm topic if email is provided
            var notificationEmail = System.Environment.GetEnvironmentVariable("NOTIFICATION_EMAIL");
            if (!string.IsNullOrEmpty(notificationEmail))
            {
                alarmTopic.AddSubscription(new EmailSubscription(notificationEmail));
            }

            // Create ECS Cluster in the given VPC
            var cluster = new Cluster(this, "ProductManagementCluster", new ClusterProps
            {
                Vpc = vpc,
                ContainerInsightsV2 = ContainerInsights.ENABLED // Enable Container Insights v2
            });

            // Create Fargate service with Application Load Balancer
            FargateService = new ApplicationLoadBalancedFargateService(this, "ProductManagementFargateService", new ApplicationLoadBalancedFargateServiceProps
            {
                Cluster = cluster,
                Cpu = 512,
                MemoryLimitMiB = 1024,
                DesiredCount = 2,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromEcrRepository(ecrRepo, imageTag),
                    ContainerPort = 80,
                    Environment = new Dictionary<string, string>
                    {
                        { "ASPNETCORE_ENVIRONMENT", "Production" },
                        { "DYNAMODB_TABLE_NAME", "Products" },
                        { "ASPNETCORE_URLS", "http://+:80" }
                    },
                    LogDriver = LogDriver.AwsLogs(new AwsLogDriverProps
                    {
                        LogGroup = logGroup,
                        StreamPrefix = "product-management"
                    })
                },
                PublicLoadBalancer = true,
                ListenerPort = 80,
                HealthCheckGracePeriod = Duration.Seconds(60),
            });

            // Set ALB health check path to /health for the target group
            FargateService.TargetGroup.ConfigureHealthCheck(new Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck
            {
                Path = "/health",
                HealthyHttpCodes = "200"
            });

            // Create CloudWatch Alarms

            // 1. CPU Usage Alarm
            var cpuAlarm = new Alarm(this, "CpuUsageAlarm", new AlarmProps
            {
                Metric = FargateService.Service.MetricCpuUtilization(),
                Threshold = 80,
                EvaluationPeriods = 3,
                DatapointsToAlarm = 2,
                ComparisonOperator = ComparisonOperator.GREATER_THAN_OR_EQUAL_TO_THRESHOLD,
                TreatMissingData = TreatMissingData.BREACHING,
                AlarmDescription = "Alert when CPU usage is high",
                AlarmName = $"{id}-high-cpu-usage"
            });
            cpuAlarm.AddAlarmAction(new SnsAction(alarmTopic));

            // 2. Memory Usage Alarm
            var memoryAlarm = new Alarm(this, "MemoryUsageAlarm", new AlarmProps
            {
                Metric = FargateService.Service.MetricMemoryUtilization(),
                Threshold = 80,
                EvaluationPeriods = 3,
                DatapointsToAlarm = 2,
                ComparisonOperator = ComparisonOperator.GREATER_THAN_OR_EQUAL_TO_THRESHOLD,
                TreatMissingData = TreatMissingData.BREACHING,
                AlarmDescription = "Alert when memory usage is high",
                AlarmName = $"{id}-high-memory-usage"
            });
            memoryAlarm.AddAlarmAction(new SnsAction(alarmTopic));

            // 3. Service Health Alarm (based on unhealthy hosts)
            var healthAlarm = new Alarm(this, "ServiceHealthAlarm", new AlarmProps
            {
                Metric = FargateService.TargetGroup.Metrics.UnhealthyHostCount(),
                Threshold = 1,
                EvaluationPeriods = 2,
                DatapointsToAlarm = 2,
                ComparisonOperator = ComparisonOperator.GREATER_THAN_OR_EQUAL_TO_THRESHOLD,
                TreatMissingData = TreatMissingData.BREACHING,
                AlarmDescription = "Alert when there are unhealthy hosts",
                AlarmName = $"{id}-unhealthy-hosts"
            });
            healthAlarm.AddAlarmAction(new SnsAction(alarmTopic));

            // Grant DynamoDB permissions to the Fargate task role
            var taskRole = FargateService.TaskDefinition.TaskRole;
            var productsTableArn = "arn:aws:dynamodb:" + Stack.Of(this).Region + ":" + Stack.Of(this).Account + ":table/Products";
            taskRole.AttachInlinePolicy(new Amazon.CDK.AWS.IAM.Policy(this, "ProductsTableAccessPolicy", new PolicyProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[]
                        {
                            "dynamodb:DescribeTable",
                            "dynamodb:Query",
                            "dynamodb:Scan",
                            "dynamodb:GetItem",
                            "dynamodb:PutItem",
                            "dynamodb:UpdateItem",
                            "dynamodb:DeleteItem"
                        },
                        Resources = new[] { productsTableArn }
                    })
                }
            }));

            // Output ECS Cluster Name
            new CfnOutput(this, "ClusterName", new CfnOutputProps
            {
                Value = cluster.ClusterName
            });

            // Output ECS Service Name
            new CfnOutput(this, "ServiceName", new CfnOutputProps
            {
                Value = FargateService.Service.ServiceName
            });

            // Output Target Group ARN
            new CfnOutput(this, "TargetGroupArn", new CfnOutputProps
            {
                Value = FargateService.TargetGroup.TargetGroupArn
            });
        }
    }
}