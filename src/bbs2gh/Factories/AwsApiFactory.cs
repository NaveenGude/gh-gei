using OctoshiftCLI.Services;

namespace OctoshiftCLI.BbsToGithub.Factories;

public class AwsApiFactory
{
    private readonly EnvironmentVariableProvider _environmentVariableProvider;

    public AwsApiFactory(EnvironmentVariableProvider environmentVariableProvider)
    {
        _environmentVariableProvider = environmentVariableProvider;
    }

    public virtual AwsApi Create(string awsRegion = null, string awsAccessKeyId = null, string awsSecretAccessKey = null, string awsSessionToken = null)
    {
        awsAccessKeyId ??= _environmentVariableProvider.AwsAccessKeyId();
        awsSecretAccessKey ??= _environmentVariableProvider.AwsSecretAccessKey();
        awsSessionToken ??= _environmentVariableProvider.AwsSessionToken(false);
        awsRegion ??= _environmentVariableProvider.AwsRegion();

        return new AwsApi(awsAccessKeyId, awsSecretAccessKey, awsRegion, awsSessionToken);
    }
}
