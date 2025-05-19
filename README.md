# Azure DevOps Pull Request Reader

This is a simple .NET 8 console application that uses the Azure DevOps REST API to read last month's pull requests 
from a Team Projects's repositories. It retrieves the pull request details and prints them in a readable format.

## Prerequisites

1. Create a Personal Access Token (PAT) in Azure DevOps with the necessary permissions to read pull requests.
2. In secrets.json, add the following
```json
{
  "PersonalAccessToken": "<the generated PAT>",
  "TeamProjectName": "<your team project name>"
}
```
3. In appSettings.json, add the following
```json
{
  "AzureDevOpsServer": "<your Azure DevOps server URL>"
}
```

## Expanding Functionality
- Add more filters to the pull request query (e.g., by author, status, etc.).
- Implement pagination to handle large numbers of pull requests.
- Add command-line arguments to allow users to specify the organization, project, and repository.
- Add command-line arguments to allow users to specify the date range for pull requests.