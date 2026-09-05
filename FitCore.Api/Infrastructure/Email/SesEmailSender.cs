using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using FitCore.Api.Infrastructure.Aws;
using Microsoft.Extensions.Options;

namespace FitCore.Api.Infrastructure.Email;

public class SesEmailSender(
    IOptions<AwsOptions> awsOptions,
    IOptions<EmailOptions> emailOptions,
    ILogger<SesEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var aws = awsOptions.Value;
        var from = emailOptions.Value.From;

        if (string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("Email:From is not configured.");

        if (string.IsNullOrWhiteSpace(aws.AccessKeyId) ||
            string.IsNullOrWhiteSpace(aws.SecretAccessKey))
            throw new InvalidOperationException("Aws access keys are not configured.");

        using var client = new AmazonSimpleEmailServiceClient(
            aws.AccessKeyId,
            aws.SecretAccessKey,
            RegionEndpoint.GetBySystemName(aws.Region));

        var request = new SendEmailRequest
        {
            Source = from,
            Destination = new Destination
            {
                ToAddresses = [to]
            },
            Message = new Message
            {
                Subject = new Content(subject),
                Body = new Body
                {
                    Html = new Content(htmlBody)
                }
            }
        };

        var response = await client.SendEmailAsync(request, cancellationToken);
        logger.LogInformation("SES email sent to {To}. MessageId={MessageId}", to, response.MessageId);
    }
}
