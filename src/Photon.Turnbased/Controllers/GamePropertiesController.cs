// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GamePropertiesController.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Photon.Turnbased;
using Photon.Turnbased.DataAccess;
using ServiceStack.Host;
using ServiceStack.Logging;

namespace Photon.Webhooks.Turnbased.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using Models;
    using Newtonsoft.Json;
    using Photon.Webhooks.Turnbased.PushNotifications;
    using ServiceStack.Text;
    
    public class GamePropertiesController : Controller
    {
        private readonly ILogger<GamePropertiesController> _logger;

        //TODO: turn this into Azure Push Notifications
        private readonly PushWoosh pushWoosh;

        #region Public Methods and Operators

        public GamePropertiesController(ILogger<GamePropertiesController> logger)
        {
            //TODO: Remove all the stuff about pushwoosh
            _logger = logger;
            pushWoosh = new PushWoosh(_logger);
        }
        public dynamic Post(GamePropertiesRequest request, string appId)
        {
            string message;
            if (!IsValid(request, out message))
            {
                var errorResponse = new ErrorResponse { Message = message };
                _logger.LogError($"{Request.GetUri()} - {JsonConvert.SerializeObject(errorResponse)}");
                return errorResponse;
            }

            if (request.State != null)
            {
                var state = (string)JsonConvert.SerializeObject(request.State);
                DataSources.DataAccess.StateSet(appId, request.GameId, state);

                var properties = request.Properties;
                object actorNrNext = null;
                properties?.TryGetValue("turn", out actorNrNext);
                var userNextInTurn = string.Empty;
                foreach (var actor in request.State.ActorList)
                {
                    if (actorNrNext != null)
                    {
                        if (actor.ActorNr == actorNrNext)
                        {
                            userNextInTurn = (string)actor.UserId;
                        }
                    }
                    DataSources.DataAccess.GameInsert(appId, (string)actor.UserId, request.GameId, (int)actor.ActorNr);
                }

                if (!string.IsNullOrEmpty(userNextInTurn))
                {
                    var notificationContent = new Dictionary<string, string>
                                                  {
                                                      { "en", "{USERNAME} finished. It's your turn." },
                                                      { "de", "{USERNAME} hat seinen Zug gemacht. Du bist dran." },
                                                  };
                    pushWoosh.RequestPushNotification(notificationContent, request.Username, "UID2", userNextInTurn, appId);
                }
            }

            var response = new OkResponse();
            _logger.LogInformation($"{Request.GetUri()} - {JsonConvert.SerializeObject(response)}");
            return response;
        }


        private static bool IsValid(GamePropertiesRequest request, out string message)
        {
            if (string.IsNullOrEmpty(request.GameId))
            {
                message = "Missing GameId.";
                return false;
            }

            message = "";
            return true;
        }

        #endregion
    }
}