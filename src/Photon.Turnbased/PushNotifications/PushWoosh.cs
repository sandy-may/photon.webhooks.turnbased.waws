using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Extensions.Logging;

namespace Photon.Webhooks.Turnbased.PushNotifications
{
    using System.Configuration;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using Newtonsoft.Json;

    public class PushWoosh
    {
        private readonly ILogger _logger;

        // clients require 'application' and 'appname' 
        // server requires 'application' and 'auth'
        //TODO: this whole class using azure notification hub
        private static readonly string application = "";//ConfigurationManager.AppSettings["PushWooshApplication"];
        private static readonly string auth = "";//ConfigurationManager.AppSettings["PushWooshAuth"];

        private static readonly Uri uri = new Uri("https://cp.pushwoosh.com/json/1.3/createMessage");

        public PushWoosh(ILogger logger)
        {
            _logger = logger;
        }
        public void RequestPushNotification(Dictionary<string, string> notificationContent, string username, string usertag, string target, string appid)
        {
            if (string.IsNullOrEmpty(application) || string.IsNullOrEmpty(auth))
            {
                _logger.LogWarning("PushWoosh is not configured. Skipping push notification.");
                return;
            }

            var conditions = new List<IList<string>>
                                 {
                                     new[] { usertag, "EQ", target },
                                     new[] { "PhotonAppId", "EQ", appid }
                                 };

            var content = new Dictionary<string, string>();
            foreach (var item in notificationContent)
            {
                content[item.Key] = item.Value.Replace("{USERNAME}", username);
            }

            var notifications = new List<PushWooshNotification>
                                    {
                                        new PushWooshNotification
                                            {
                                                send_date = "now",
                                                ignore_user_timezone = true,
                                                content = content,
                                            }
                                    };
            var request = new Dictionary<string, PushWooshRequest>
                              {
                                  {
                                      "request",
                                      new PushWooshRequest
                                          {
                                              application = application,
                                              auth = auth,
                                              notifications = notifications,
                                              conditions = conditions,
                                          }
                                  }
                              };

            var jsonRequest = JsonConvert.SerializeObject(request);
            PostRequest(uri, jsonRequest);
        }

        private void PostRequest(Uri uri, string requestBody)
        {
            ThreadPool.QueueUserWorkItem(o =>
                    {
                        _logger.LogInformation($"PushWoosh Request {requestBody}");
                        try
                        {
                            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
                            request.Method = "POST";
                            request.ContentLength = requestBody.Length;
                            request.KeepAlive = true;
                            request.Proxy = null;

                            using (var stream = new StreamWriter(request.GetRequestStream()))
                            {
                                stream.Write(requestBody);
                            }

                            byte[] buffer = new byte[4096];
                            using (var ms = new MemoryStream())
                            using (var response = request.GetResponse() as HttpWebResponse)
                            {
                                if (response != null)
                                {
                                    var respStream = response.GetResponseStream();

                                    if (respStream != null)
                                    {
                                        var read = respStream.Read(buffer, 0, buffer.Length);

                                        while (read > 0)
                                        {
                                            ms.Write(buffer, 0, read);
                                            read = respStream.Read(buffer, 0, buffer.Length);
                                        }

                                        // get the data of the stream
                                        var data = ms.ToArray();

                                        _logger.LogInformation($"PushWoosh Response: {Encoding.UTF8.GetString(data, 0 , data.Length)}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.Message);
                        }
                    });
        }
    }

    public class PushWooshNotification
    {
        public string send_date;
        public bool ignore_user_timezone;
        public Dictionary<string, string> content;
    }

    public class PushWooshRequest
    {
        public string application;
        public string auth;
        public List<PushWooshNotification> notifications;
        public List<IList<string>> conditions;
    }
}