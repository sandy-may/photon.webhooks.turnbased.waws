using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.NotificationHubs;
namespace Photon.Webhooks.Turnbased.PushNotifications
{
    public static class HubMessage
    {
        public static TemplateNotification WrapMessage(Dictionary<string, string> notificationContent, string username, string usertag,
            string target, string appid)
        {
            var conditions = new List<IList<string>>
            {
                new[] {usertag, "EQ", target},
                new[] {"PhotonAppId", "EQ", appid}
            };

            var content = new Dictionary<string, string>();
            foreach (var item in notificationContent)
            {
                content[item.Key] = item.Value.Replace("{USERNAME}", username);
            }

            var notifications = new List<HubNotification>
            {
                new HubNotification
                {
                    SendDate = DateTime.UtcNow.ToShortTimeString(),
                    IgnoreUserTimezone = true,
                    Content = content,
                }
            };
            var request = new Dictionary<string, string>
            {
                {
                    "request",
                    JsonConvert.SerializeObject(new HubRequest
                    {
                        Notifications = notifications,
                        Conditions = conditions,
                    })
                }
            };
            return new TemplateNotification(request);
        }
    }
}
