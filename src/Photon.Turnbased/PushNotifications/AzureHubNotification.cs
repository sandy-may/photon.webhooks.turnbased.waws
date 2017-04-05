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
        internal static ILogger<AzureHubNotification> Logger { get; private set; }
        internal static ConnectionStrings ConnectionStrings { get; private set; }
        private const string Topic = "turnbasedNotify";

        public AzureHubNotification(ILogger<AzureHubNotification> logger, IOptions<ConnectionStrings> connectionStrings)
        {
            Logger = logger;
            ConnectionStrings = connectionStrings.Value;
            CreateTopic(); 
        }

        private static async void CreateTopic()
        {
            try
            {
                var namespaceManager =
                    NamespaceManager.CreateFromConnectionString(ConnectionStrings.NotificationHubConnectionString);
                if (!await namespaceManager.TopicExistsAsync(Topic))
                {
                    await namespaceManager.CreateTopicAsync(Topic);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Execption caught creating service bus topic - {e}");
            }

        }

        public async Task SendMessage(Dictionary<string, string> notificationContent, string username, string usertag, string target, string appid)
        {
            TopicClient client;
            try
            {
                client = TopicClient.CreateFromConnectionString(ConnectionStrings.NotificationHubConnectionString, Topic);
            }
            catch (Exception e)
            {
                Logger.LogError($"Execption caught creating service bus topic client - {e}");
                return;
            }

            var json = WrapMessage(notificationContent, username, usertag, target, appid);

            var message = new BrokeredMessage(json);
            await client.SendAsync(message);
        }
    }
}
