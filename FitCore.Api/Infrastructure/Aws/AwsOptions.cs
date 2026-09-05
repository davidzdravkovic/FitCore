namespace FitCore.Api.Infrastructure.Aws;

public class AwsOptions
{
    public const string SectionName = "Aws";

    public string Region { get; set; } = "eu-central-1";
    public string AccessKeyId { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
}
