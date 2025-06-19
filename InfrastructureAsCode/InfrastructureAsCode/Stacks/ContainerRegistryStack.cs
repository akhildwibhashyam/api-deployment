using Amazon.CDK;
using Amazon.CDK.AWS.ECR;
using Constructs;

namespace InfrastructureAsCode.Stacks
{
    public class ContainerRegistryStack : Stack
    {
        public Repository Repository { get; private set; }

        public ContainerRegistryStack(Construct scope, string id, StackProps? props = null)
            : base(scope, id, props)
        {
            // Use environment as suffix if provided in context/env
            var envSuffix = this.Node.TryGetContext("env")?.ToString() ?? System.Environment.GetEnvironmentVariable("DEPLOY_ENV") ?? "dev";
            var uniqueId = this.Node.TryGetContext("uniqueId")?.ToString() ?? System.Environment.GetEnvironmentVariable("GITHUB_RUN_ID") ?? System.Environment.GetEnvironmentVariable("UNIQUE_ID") ?? "";
            var repoName = $"product-management-system-{envSuffix}";
            if (!string.IsNullOrEmpty(uniqueId))
            {
                repoName += $"-{uniqueId}";
            }

            Repository = new Repository(this, "ProductManagementRepo", new RepositoryProps
            {
                RepositoryName = repoName,
                RemovalPolicy = RemovalPolicy.DESTROY,  // Deletes the repo on stack deletion
            });
        }
    }
}