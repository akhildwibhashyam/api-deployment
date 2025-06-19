## Product Management System API Deployment

### 1. Introduction

This document covers the Product Management System’s architecture, prerequisites, deployment instructions, and a theoretical explanation for each service used in the pipeline. Our solution is designed to be modular, scalable, secure, and reproducible across different environments and AWS regions.

### 2. Prerequisites and Setup
- An AWS account with appropriate permissions
- An AWS CLI configured with credentials
- An AWS Elastic Container Registry (ECR) repository
- Git repository with application code
- Docker installed
- Node.js and npm installed
- CDK installed globally

### 3. Deployment Instructions
- Check out the code
- Build the .NET application
- Publish the application
- Push Docker image to ECR
- Deploy AWS resources with CDK

### 4. Architectural Explanation
- Docker: Containerize our application for ease of delivery and scaling
- ECR: Stores the container images safely and efficiently
- AWS CDK: IaC tool used to deploy AWS resources alongside the application
- IAM and Security: Managed by following AWS best practices

### 5. Evaluation Criteria
- Reproducibility: Deployment is automated and can be rerun
- Functionality: The application performs identically across environments
- Architecture Quality: The architecture is designed following industry standards
- Code Quality: The CDK code is well-structured and easy to follow
- Configuration Management: Managed through secrets and environment variables
- Documentation: Detailed explanations aid understanding and future maintenance

### 6. Limitations and Improvement Ideas
- Currently, there is a manual triggering process for pipeline
- Improvement to enable automatic deploy upon code merges
- Support for multiple environments (DEV, STAGING, PROD) with separate configuration