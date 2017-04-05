// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetGameListController.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Photon.Turnbased;
using Photon.Turnbased.DataAccess;
using Photon.Webhooks.Turnbased.DataAccess;
using ServiceStack.Logging;

namespace Photon.Webhooks.Turnbased.Controllers
{
    using System.Collections.Generic;
    using System.Web.Http;
    using Models;
    using Newtonsoft.Json;
    using ServiceStack.Text;

    public class GetGameListController : Controller
    {
        private readonly ILogger<GetGameListController> _logger;
        private readonly IDataAccess _dataAccess;

        #region Public Methods and Operators

        public GetGameListController(ILogger<GetGameListController> logger, IDataAccess dataAccess)
        {
            _logger = logger;
            _dataAccess = dataAccess;
        }
        public dynamic Post(GetGameListRequest request, string appId)
        {
            string message;
            if (!IsValid(request, out message))
            {
                var errorResponse = new ErrorResponse { Message = message };
                _logger.LogError($"{Request.GetUri()} - {JsonConvert.SerializeObject(errorResponse)}");
                return errorResponse;
            }

            var list = new Dictionary<string, object>();

            foreach (var pair in _dataAccess.GameGetAll(appId, request.UserId))
            {
                // exists - save result in list
                //if (DataSources.DataAccess.StateExists(appId, pair.Key))
                var stateJson = DataSources.DataAccess.StateGet(appId, pair.Key);
                if (stateJson != null)
                {
                    dynamic customProperties = null;
                    if (stateJson != string.Empty)
                    {
                        var state = JsonConvert.DeserializeObject<dynamic>(stateJson);
                        customProperties = state.CustomProperties;
                    }

                    var gameListItem = new GameListItem(){ ActorNr = int.Parse(pair.Value), Properties = customProperties };

                    list.Add(pair.Key, gameListItem);
                }
                // not exists - delete
                else
                {
                    _dataAccess.GameDelete(appId, request.UserId, pair.Key);
                }
            }

            var getGameListResponse = new GetGameListResponse { Data = list };
            _logger.LogInformation($"{Request.GetUri()} - {JsonConvert.SerializeObject(getGameListResponse)}");
            return getGameListResponse;
        }

        private static bool IsValid(GetGameListRequest request, out string message)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                message = "Missing UserId.";
                return false;
            }

            message = "";
            return true;
        }
        #endregion
    }
}