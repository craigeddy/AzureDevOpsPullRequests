using Chronic.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevOpsPullRequests;

// https://www.nuget.org/packages/Microsoft.TeamFoundationServer.Client/
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

// https://www.nuget.org/packages/Microsoft.VisualStudio.Services.InteractiveClient/
using Microsoft.VisualStudio.Services.Client;

// https://www.nuget.org/packages/Microsoft.VisualStudio.Services.Client/
using Microsoft.VisualStudio.Services.Common;

class Program
{
    static void Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<Program>() // Loads secrets for this assembly
            .Build();

        // Be sure to send in the full collection uri, i.e. http://myserver:8080/tfs/defaultcollection
        // We are using default VssCredentials which uses NTLM against an Azure DevOps Server.  See additional provided
        // Create a connection with PAT for authentication
        var orgUrl = new Uri(config["AzureDevOpsServer"]);
        var personalAccessToken = config["PersonalAccessToken"];
        var connection = new VssConnection(orgUrl, new VssBasicCredential(string.Empty, personalAccessToken));
        
        // Create a client
        var witClient = connection.GetClient<WorkItemTrackingHttpClient>();
        
        var gitHttpClient = connection.GetClient<Microsoft.TeamFoundation.SourceControl.WebApi.GitHttpClient>();
        var teamProjectName = config["TeamProjectName"];
        var gitRepos = gitHttpClient.GetRepositoriesAsync(teamProjectName).Result;
        var parser = new Parser();
        
        // Get counts of pull requests for each repo
        var endDate = parser.Parse("last month").End;
        var startDate = parser.Parse("last month").Start;
        Console.WriteLine($"Completed Pull Requests from {startDate:d} to {endDate:d}");
        Console.WriteLine();
        
        foreach (var repo in gitRepos)
        {
            var pullRequests = gitHttpClient.GetPullRequestsAsync(repo.Id, new GitPullRequestSearchCriteria()
            {
                Status = PullRequestStatus.All,
                MinTime = startDate,
                // new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1, 0, 0, 0),
                MaxTime = endDate,
            }).Result;

            // SHORT CIRCUIT IF NO PULL REQUESTS
            if (pullRequests.Count == 0) continue;
            
            Console.WriteLine($"Project: {repo.Name} - Completed Pull Requests: {pullRequests.Count}");
            foreach (var pullRequest in pullRequests.OrderBy(pr => pr.CreationDate))
            {
                var msg = $"  PR: {pullRequest.PullRequestId} - {pullRequest.Title} by {pullRequest.CreatedBy.DisplayName} - {pullRequest.CreationDate:d}";
                if(args.Contains("--includeUrl"))
                {
                    msg += $" - {repo.WebUrl}/pullrequest/{pullRequest.PullRequestId}";
                }
                    
                Console.WriteLine(msg);
            }
            Console.WriteLine();
        }
        
        // Get 2 levels of query hierarchy items
        List<QueryHierarchyItem> queryHierarchyItems = witClient.GetQueriesAsync(teamProjectName, depth: 2).Result;
    }
}