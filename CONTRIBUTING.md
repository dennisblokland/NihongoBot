# Contributing to NihongoBot

Thank you for your interest in contributing to NihongoBot! This document provides guidelines for contributing to the project.

## Development Setup

Please refer to the [README.md](README.md) for instructions on setting up the development environment.

## Contribution Workflow

1. **Fork the repository** and create your feature branch from `main`.
2. **Make your changes** following the project's coding conventions.
3. **Write tests** for your changes if applicable.
4. **Ensure all tests pass** by running `dotnet test`.
5. **Submit a pull request** to the `main` branch.

## Code Style

This project follows standard C# coding conventions:
- Use tab indentation (size 4)
- Private fields prefixed with underscore: `_fieldName`
- Explicit type declarations preferred over `var`
- Follow the existing patterns in the codebase

## Testing

Before submitting your pull request:
- Run `dotnet restore` to restore dependencies
- Run `dotnet build` to ensure the project builds successfully
- Run `dotnet test` to ensure all tests pass

## Deployment Process

### Continuous Integration (CI)

All pull requests and pushes to the `main` branch trigger the CI pipeline which:
- Builds the project
- Runs all tests
- Validates code quality

### Continuous Deployment (CD)

The deployment process has been designed with **manual approval controls** for production releases:

#### Build Stage (Automatic)
When code is pushed to the `main` branch, the following steps run automatically:
1. **Checkout**: Downloads the latest code
2. **Setup**: Configures .NET environment and required tools
3. **Build**: Compiles the project and runs tests
4. **Generate Artifacts**: Creates deployment files (docker-compose.yaml)
5. **Upload**: Stores deployment artifacts for the approval stage

#### Approval Stage (Manual)
After the build stage completes successfully:
- **A manual approval is required** before deployment can proceed
- Only authorized maintainers can approve deployments
- The approval request will be visible in the GitHub Actions interface
- Approvers can review the changes and deployment artifacts before approval

#### Deploy Stage (Automatic after approval)
Once approved, the deployment stage:
1. **Downloads**: Retrieves the approved deployment artifacts
2. **Deploys**: Copies files and restarts the containerized application
3. **Verifies**: Ensures the new version is running

### Who Can Approve Deployments?

Deployment approvals can only be granted by:
- Repository maintainers
- Users with `Write` or `Admin` permissions on the repository
- Users explicitly added to the production environment reviewers list

### Viewing Deployment Status

You can monitor deployment status by:
1. Going to the **Actions** tab in the GitHub repository
2. Selecting the relevant workflow run
3. Viewing the progress of each stage
4. Seeing pending approvals in the deployment timeline

### Emergency Deployments

For urgent fixes, maintainers can:
- Use the **workflow_dispatch** trigger to manually start a deployment
- The same approval process applies for consistency and security

## Pull Request Guidelines

- Provide a clear description of the changes
- Reference any related issues
- Include screenshots for UI changes
- Ensure your branch is up to date with `main`

## Reporting Issues

When reporting bugs or requesting features:
- Use the GitHub issue templates when available
- Provide clear reproduction steps for bugs
- Include relevant logs or error messages

## Questions?

If you have questions about contributing, feel free to:
- Open an issue for discussion
- Reach out to the maintainers

Thank you for contributing to NihongoBot! 🎌