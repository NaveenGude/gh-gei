﻿using System.IO;

namespace OctoshiftCLI.BbsToGithub.Commands.GenerateScript;

public class GenerateScriptCommandArgs
{
    public string BbsServerUrl { get; set; }
    public string GithubOrg { get; set; }
    public string BbsUsername { get; set; }
    public string BbsPassword { get; set; }
    public string BbsProject { get; set; }
    public string BbsSharedHome { get; set; }
    public string ArchiveDownloadHost { get; set; }
    public string SshUser { get; set; }
    public string SshPrivateKey { get; set; }
    public int SshPort { get; set; }
    public string SmbUser { get; set; }
    public string SmbDomain { get; set; }
    public FileInfo Output { get; set; }
    public bool Kerberos { get; set; }
    public bool Verbose { get; set; }
    public string AwsBucketName { get; set; }
    public string AwsRegion { get; set; }
    public bool KeepArchive { get; set; }
    public bool NoSslVerify { get; set; }
}
