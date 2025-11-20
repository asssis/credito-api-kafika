using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CreditApi.Services
{
    public class NoopMessagePublisher : IMessagePublisher
    {
        private readonly ILogger<NoopMessagePublisher> _logger;
        public NoopMessagePublisher(ILogger<NoopMessagePublisher> logger) => _logger = logger;

        public Task PublishAsync(string topic, string message)
        {
            _logger?.LogInformation("Noop publish called. Topic: {Topic} Message length: {Len}", topic, message?.Length ?? 0);
            return Task.CompletedTask;
        }
    }
}
