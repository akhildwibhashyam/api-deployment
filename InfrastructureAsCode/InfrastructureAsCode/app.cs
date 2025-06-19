using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using InfrastructureAsCode.Stacks;

namespace InfrastructureAsCode
{
    sealed class AppEntryPoint
    {
        public static void Main(string[] args)
        {
            var app = new App();

            // Get environment and region from context or environment variables
            var environment = app.Node.TryGetContext("env")?.ToString() ?? System.Environment.GetEnvironmentVariable("DEPLOY_ENV") ?? "dev";
            var region = app.Node.TryGetContext("region")?.ToString() ?? System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION") ?? "us-east-2";
            var env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = region,
            };

            // Use env as suffix for resource names
            var repoSuffix = environment;
            // 1. Network Stack
            var networkStack = new NetworkStack(app, $"NetworkStack-{environment}", new StackProps { Env = env });

            // 2. Database Stack (DynamoDB)
            var databaseStack = new DatabaseStack(app, $"DatabaseStack-{environment}", new StackProps { Env = env });

            // 3. Container Registry Stack (ECR)
            var uniqueId = app.Node.TryGetContext("uniqueId")?.ToString() ?? System.Environment.GetEnvironmentVariable("GITHUB_RUN_ID") ?? System.Environment.GetEnvironmentVariable("UNIQUE_ID") ?? "";
            var containerRegistryStack = new ContainerRegistryStack(app, $"ContainerRegistryStack-{environment}", new StackProps { Env = env });

            // 4. ECS Fargate Service Stack
            var vpc = networkStack.Vpc as Vpc;
            if (vpc == null)
            {
                throw new InvalidCastException("networkStack.Vpc is not of type Vpc. Please ensure NetworkStack creates a Vpc, not just IVpc.");
            }
            // Get image tag from environment variable or fallback to "latest"
            var imageTag = System.Environment.GetEnvironmentVariable("ECR_IMAGE_TAG") ?? "latest";
            // Use the repository name from the ECR stack for reference in the pipeline
            var repoName = containerRegistryStack.Repository.RepositoryName;
            // Pass uniqueId to ECSFargateServiceStack for full synchronization
            var ecsServiceStack = new ECSFargateServiceStack(
                app,
                $"ECSFargateServiceStack-{environment}",
                vpc,
                containerRegistryStack.Repository,
                imageTag,
                new StackProps { Env = env }
            );
            app.Synth();
        }
    }
}