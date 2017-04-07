using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Photon.Webhooks.Turnbased.Config;
using static Photon.Webhooks.Turnbased.PushNotifications.HubMessage;

namespace Photon.Webhooks.Turnbased.PushNotifications
{
    public class AzureHubNotification : INotification
    {
        private readonly ILogger<AzureHubNotification> _logger;
        private readonly ConnectionStrings _connectionStrings;
        //private readonly TopicDescription td = new TopicDescription("turnbasednotify");

        public AzureHubNotification(ILogger<AzureHubNotification> logger, IOptions<ConnectionStrings> connectionStrings)
        {
            _logger = logger;
            _connectionStrings = connectionStrings.Value;
            CreateTopic(); 
        }

        private void CreateTopic()
        {
            try
            {
                //td.MaxSizeInMegabytes = 5120;
                //td.DefaultMessageTimeToLive = new TimeSpan(0 , 1, 0);
                //TODO: fix error 401 when creating a topic
                var namespaceManager =
                    NamespaceManager.CreateFromConnectionString(_connectionStrings.NotificationHubConnectionString);
                if (!namespaceManager.TopicExists("topic"))
                {
                    namespaceManager.CreateTopic("topic");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Execption caught creating service bus topic - {e}");
            }

        }


        public async Task SendMessage(Dictionary<string, string> notificationContent, string username, string usertag, string target, string appid)
        {
            TopicClient client;
            try
            {
                client = TopicClient.CreateFromConnectionString(_connectionStrings.NotificationHubConnectionString, "topic");
            }
            catch (Exception e)
            {
                _logger.LogError($"Execption caught creating service bus topic client - {e}");
                return;
            }

            var json = WrapMessage(notificationContent, username, usertag, target, appid);

            var message = new BrokeredMessage(json);
            await client.SendAsync(message);
        }
    }
}
