// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameCreateController.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Photon.Turnbased;
using Photon.Turnbased.DataAccess;
using ServiceStack.Logging;

namespace Photon.Webhooks.Turnbased.Controllers
{
    using System.Web.Http;
    using Models;
    using Newtonsoft.Json;

    public class GameCreateController : Controller
    {
        //TODO: Class contains static singleton methods, means that logger can't be used in methods. 
        //TODO: Maybe just return the message and log or change to no static? Will need to change caller as well

        private readonly ILogger<GameCreateController> _logger;

        #region Public Methods and Operators

        public GameCreateController(ILogger<GameCreateController> logger)
        {
            _logger = logger;
        }
        public dynamic Post(GameCreateRequest request, string appId)
        {

            string message;
            if (!IsValid(request, out message))
            {
                var errorResponse = new ErrorResponse { Message = message };
                _logger.LogError($"{Request.GetUri()} - {JsonConvert.SerializeObject(errorResponse)}");
                return errorResponse;
            }
            
            dynamic response;
            if (!string.IsNullOrEmpty(request.Type) && request.Type == "Load")
            {
                response = GameLoad(request, appId);
            }
            else
            {
                response = GameCreate(request, appId);
            }

            _logger.LogInformation($"{Request.GetUri()} - {JsonConvert.SerializeObject(response)}");
            return response;
        }

        private static dynamic GameCreate(GameCreateRequest request, string appId)
        {
            dynamic response;
            if (DataSources.DataAccess.StateExists(appId, request.GameId))
            {
                response = new ErrorResponse { Message = "Game already exists." };
                return response;
            }

            if (request.CreateOptions == null)
            {
                DataSources.DataAccess.StateSet(appId, request.GameId, string.Empty);
            }
            else
            {
                DataSources.DataAccess.StateSet(appId, request.GameId, (string)JsonConvert.SerializeObject(request.CreateOptions));
            }

            response = new OkResponse();
            //_logger.LogInformation($"{Request.GetUri()} - {JsonConvert.SerializeObject(response)}");
            return response;
        }

        public static dynamic GameLoad(GameCreateRequest request, string appId)
        {
            dynamic response;
            string stateJson = string.Empty;
            stateJson = DataSources.DataAccess.StateGet(appId, request.GameId);

            if (!string.IsNullOrEmpty(stateJson))
            {
                response = new GameLoadResponse { State = JsonConvert.DeserializeObject(stateJson) };
                return response;
            }
            //TBD - check how deleteIfEmpty works with createifnot exists
            if (stateJson == string.Empty)
            {
                DataSources.DataAccess.StateDelete(appId, request.GameId);
                
                //_logger.LogInformation($"Deleted empty state, app id {appId}, gameId {request.GameId}");
        
            }

            if (request.CreateIfNotExists)
            {
                response = new OkResponse();
                //_logger.LogInformation($"{Request.GetUri()} - {JsonConvert.SerializeObject(response)}");
                return response;
            }

            response = new ErrorResponse { Message = "GameId not Found." };
            //_logger.LogError($"{Request.GetUri()} - {JsonConvert.SerializeObject(response)}");
            return response;
        }
        private static bool IsValid(GameCreateRequest request, out string message)
        {
            if (string.IsNullOrEmpty(request.GameId))
            {
                message = "Missing GameId.";
                return false;
            }

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