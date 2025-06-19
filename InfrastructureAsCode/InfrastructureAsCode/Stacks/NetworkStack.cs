using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Constructs;

namespace InfrastructureAsCode.Stacks
{
    public class NetworkStack : Stack
    {
        public IVpc Vpc { get; private set; }

        public NetworkStack(Construct scope, string id, StackProps? props = null)
            : base(scope, id, props)
        {
            // Create a VPC with Public and Private Subnets
            Vpc = new Vpc(this, "ProductManagementVpc", new VpcProps
            {
                MaxAzs = 2,
                NatGateways = 1,
                SubnetConfiguration = new[]
                {
                    new SubnetConfiguration
                    {
                        SubnetType = SubnetType.PUBLIC,
                        Name = "Public",
                        CidrMask = 24
                    },
                    new SubnetConfiguration
                    {
                        SubnetType = SubnetType.PRIVATE_WITH_EGRESS,
                        Name = "Private",
                        CidrMask = 24
                    }
                }
            });
        }
    }
}