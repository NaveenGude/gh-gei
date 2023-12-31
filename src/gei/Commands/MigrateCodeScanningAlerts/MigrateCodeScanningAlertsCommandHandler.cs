﻿using System;
using System.Threading.Tasks;
using OctoshiftCLI.Commands;
using OctoshiftCLI.Services;

namespace OctoshiftCLI.GithubEnterpriseImporter.Commands.MigrateCodeScanningAlerts;

public class MigrateCodeScanningAlertsCommandHandler : ICommandHandler<MigrateCodeScanningAlertsCommandArgs>
{
    private readonly OctoLogger _log;
    private readonly CodeScanningAlertService _codeScanningAlertService;

    public MigrateCodeScanningAlertsCommandHandler(OctoLogger log, CodeScanningAlertService codeScanningAlertService)
    {
        _log = log;
        _codeScanningAlertService = codeScanningAlertService;
    }

    public async Task Handle(MigrateCodeScanningAlertsCommandArgs args)
    {
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        _log.LogInformation("Migrating Repo Code Scanning Alerts...");

        await _codeScanningAlertService.MigrateCodeScanningAlerts(
            args.SourceOrg,
            args.SourceRepo,
            args.TargetOrg,
            args.TargetRepo,
            args.DryRun);

        if (!args.DryRun)
        {
            _log.LogSuccess($"Code scanning alerts successfully migrated.");
        }
    }
}
