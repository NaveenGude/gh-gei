﻿using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Threading.Tasks;
using OctoshiftCLI.Extensions;

namespace OctoshiftCLI.AdoToGithub.Commands
{
    public class MigrateRepoCommand : Command
    {
        private readonly OctoLogger _log;
        private readonly GithubApiFactory _githubApiFactory;
        private readonly EnvironmentVariableProvider _environmentVariableProvider;

        public MigrateRepoCommand(OctoLogger log, GithubApiFactory githubApiFactory, EnvironmentVariableProvider environmentVariableProvider) : base(
            name: "migrate-repo",
            description: "Invokes the GitHub API's to migrate the repo and all PR data" +
                         Environment.NewLine +
                         "Note: Expects ADO_PAT and GH_PAT env variables or --ado-pat and --github-pat options to be set.")
        {
            _log = log;
            _githubApiFactory = githubApiFactory;
            _environmentVariableProvider = environmentVariableProvider;

            var adoOrg = new Option<string>("--ado-org")
            {
                IsRequired = true
            };
            var adoTeamProject = new Option<string>("--ado-team-project")
            {
                IsRequired = true
            };
            var adoRepo = new Option<string>("--ado-repo")
            {
                IsRequired = true
            };
            var githubOrg = new Option<string>("--github-org")
            {
                IsRequired = true
            };
            var githubRepo = new Option<string>("--github-repo")
            {
                IsRequired = true
            };
            var targetRepoVisibility = new Option<string>("--target-repo-visibility")
            {
                IsRequired = false,
                Description = "Defaults to private. Valid values are public, private, internal"
            };
            var wait = new Option<bool>("--wait")
            {
                IsRequired = false,
                Description = "Synchronously waits for the repo migration to finish."
            };
            var adoPat = new Option<string>("--ado-pat")
            {
                IsRequired = false
            };
            var githubPat = new Option<string>("--github-pat")
            {
                IsRequired = false
            };
            var verbose = new Option<bool>("--verbose")
            {
                IsRequired = false
            };

            AddOption(adoOrg);
            AddOption(adoTeamProject);
            AddOption(adoRepo);
            AddOption(githubOrg);
            AddOption(githubRepo);
            AddOption(targetRepoVisibility);
            AddOption(wait);
            AddOption(adoPat);
            AddOption(githubPat);
            AddOption(verbose);

            Handler = CommandHandler.Create<string, string, string, string, string, string, bool, string, string, bool>(Invoke);
        }

        public async Task Invoke(string adoOrg, string adoTeamProject, string adoRepo, string githubOrg, string githubRepo, string targetRepoVisibility = null, bool wait = false, string adoPat = null, string githubPat = null, bool verbose = false)
        {
            _log.Verbose = verbose;

            _log.LogInformation("Migrating Repo...");
            _log.LogInformation($"ADO ORG: {adoOrg}");
            _log.LogInformation($"ADO TEAM PROJECT: {adoTeamProject}");
            _log.LogInformation($"ADO REPO: {adoRepo}");
            _log.LogInformation($"GITHUB ORG: {githubOrg}");
            _log.LogInformation($"GITHUB REPO: {githubRepo}");
            
            if (targetRepoVisibility.HasValue())
            {
                _log.LogInformation($"TARGET REPO VISIBILITY: {targetRepoVisibility}");
            }
            if (wait)
            {
                _log.LogInformation("WAIT: true");
            }
            if (adoPat is not null)
            {
                _log.LogInformation("ADO PAT: ***");
            }
            if (githubPat is not null)
            {
                _log.LogInformation("GITHUB PAT: ***");
            }

            githubPat ??= _environmentVariableProvider.GithubPersonalAccessToken();
            var githubApi = _githubApiFactory.Create(targetPersonalAccessToken: githubPat);

            var adoRepoUrl = GetAdoRepoUrl(adoOrg, adoTeamProject, adoRepo);

            adoPat ??= _environmentVariableProvider.AdoPersonalAccessToken();
            var githubOrgId = await githubApi.GetOrganizationId(githubOrg);
            var migrationSourceId = await githubApi.CreateAdoMigrationSource(githubOrgId, null);

            string migrationId;

            try
            {
                migrationId = await githubApi.StartMigration(migrationSourceId, adoRepoUrl, githubOrgId, githubRepo, adoPat, githubPat, null, null, false, targetRepoVisibility, false);
            }
            catch (OctoshiftCliException ex)
            {
                if (ex.Message == $"A repository called {githubOrg}/{githubRepo} already exists")
                {
                    _log.LogWarning($"The Org '{githubOrg}' already contains a repository with the name '{githubRepo}'. No operation will be performed");
                    return;
                }

                throw;
            }

            if (!wait)
            {
                _log.LogInformation($"A repository migration (ID: {migrationId}) was successfully queued.");
                return;
            }

            var (migrationState, _, failureReason) = await githubApi.GetMigration(migrationId);

            while (RepositoryMigrationStatus.IsPending(migrationState))
            {
                _log.LogInformation($"Migration in progress (ID: {migrationId}). State: {migrationState}. Waiting 10 seconds...");
                await Task.Delay(10000);
                (migrationState, _, failureReason) = await githubApi.GetMigration(migrationId);
            }

            if (RepositoryMigrationStatus.IsFailed(migrationState))
            {
                _log.LogError($"Migration Failed. Migration ID: {migrationId}");
                throw new OctoshiftCliException(failureReason);
            }

            _log.LogSuccess($"Migration completed (ID: {migrationId})! State: {migrationState}");
        }

        private string GetAdoRepoUrl(string org, string project, string repo) => $"https://dev.azure.com/{org}/{project}/_git/{repo}".Replace(" ", "%20");
    }
}
