﻿
namespace OctoshiftCLI.Commands.WaitForMigration;

public class WaitForMigrationCommandArgs
{
    public string MigrationId { get; set; }
    public string GithubPat { get; set; }
    public bool Verbose { get; set; }
}
