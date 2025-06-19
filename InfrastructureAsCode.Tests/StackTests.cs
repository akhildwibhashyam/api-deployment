using Amazon.CDK;
using Amazon.CDK.Assertions;

namespace InfrastructureAsCode.Tests;

public class NetworkStackTests
{
    [Fact]
    public void NetworkStack_CreatesVpc()
    {
        // ARRANGE
        var app = new App();
        var stack = new NetworkStack(app, "TestNetworkStack");
        var template = Template.FromStack(stack);

        // ASSERT
        // Verify VPC is created
        template.ResourceCountIs("AWS::EC2::VPC", 1);
        // Verify VPC has correct CIDR
        template.HasResourceProperties("AWS::EC2::VPC", new Dictionary<string, object>
        {
            ["CidrBlock"] = Match.StringLikeRegexp("10.*"),
            ["EnableDnsHostnames"] = true,
            ["EnableDnsSupport"] = true,
            ["InstanceTenancy"] = "default",
        });
        // Verify subnets are created
        template.ResourceCountIs("AWS::EC2::Subnet", 4); // 2 public subnets, 2 private subnets
        // Verify NAT Gateway is created
        template.ResourceCountIs("AWS::EC2::NatGateway", 1);
    }
}

public class DatabaseStackTests
{
    [Fact]
    public void DatabaseStack_CreatesDynamoDbTable()
    {
        // ARRANGE
        var app = new App();
        var stack = new DatabaseStack(app, "TestDatabaseStack");
        var template = Template.FromStack(stack);

        // ASSERT
        // Verify DynamoDB table is created
        template.ResourceCountIs("AWS::DynamoDB::Table", 1);
        template.HasResourceProperties("AWS::DynamoDB::Table", new Dictionary<string, object>
        {
            ["BillingMode"] = "PAY_PER_REQUEST",
            ["KeySchema"] = Match.ArrayWith(new[]
            {
                new Dictionary<string, object>
                {
                    ["AttributeName"] = "Id",
                    ["KeyType"] = "HASH"
                }
            })
        });
    }
}

public class ECSFargateServiceStackTests
{
    [Fact]
    public void ECSFargateService_CreatesECSClusterAndService()
    {
        // ARRANGE
        var app = new App();
        // Setup dependencies needed for ECSFargateServiceStack
        var networkStack = new NetworkStack(app, "TestNetworkStack");
        var containerRegistryStack = new ContainerRegistryStack(app, "TestContainerRegistryStack");
        var stack = new ECSFargateServiceStack(app, "TestECSFargateStack", networkStack.Vpc, 
            containerRegistryStack.Repository, "latest");
        var template = Template.FromStack(stack);

        // ASSERT
        // Verify ECS Cluster is created
        template.ResourceCountIs("AWS::ECS::Cluster", 1);
        
        // Verify ECS Service is created
        template.HasResourceProperties("AWS::ECS::Service", new Dictionary<string, object>
        {
            ["LaunchType"] = "FARGATE",
            ["DesiredCount"] = 2,
            ["ServiceName"] = Match.StringLikeRegexp(".*FargateService.*")
        });

        // Verify ALB is created
        template.ResourceCountIs("AWS::ElasticLoadBalancingV2::LoadBalancer", 1);
        template.ResourceCountIs("AWS::ElasticLoadBalancingV2::TargetGroup", 1);
        
        // Verify CloudWatch Log Group is created
        template.ResourceCountIs("AWS::Logs::LogGroup", 1);

        // Verify CloudWatch Alarms are created
        template.ResourceCountIs("AWS::CloudWatch::Alarm", 3); // CPU, Memory, and Service Health alarms
    }
}
