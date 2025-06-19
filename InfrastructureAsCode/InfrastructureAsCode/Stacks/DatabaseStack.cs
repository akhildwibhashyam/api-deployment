using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;

namespace InfrastructureAsCode.Stacks
{
    public class DatabaseStack : Stack
    {
        public Table ProductsTable { get; private set; }

        public DatabaseStack(Construct scope, string id, StackProps? props = null)
            : base(scope, id, props)
        {
            var envSuffix = this.Node.TryGetContext("env")?.ToString() ?? System.Environment.GetEnvironmentVariable("DEPLOY_ENV") ?? "dev";
            ProductsTable = new Table(this, "ProductsTable", new TableProps
            {
                TableName = $"Products-{envSuffix}",
                PartitionKey = new Amazon.CDK.AWS.DynamoDB.Attribute { Name = "Id", Type = AttributeType.STRING },
                BillingMode = Amazon.CDK.AWS.DynamoDB.BillingMode.PAY_PER_REQUEST,
                RemovalPolicy = Amazon.CDK.RemovalPolicy.DESTROY, // For dev/testing only, change for prod
            });
        }
    }
}