// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameLoadController.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Photon.Webhooks.Turnbased.Controllers
{
    using System.Web.Http;
    using Newtonsoft.Json;
    using Models;

    public class GameLoadController : Controller
    {
        private readonly ILogger<GameLoadController> _logger;

        #region Public Methods and Operators

        public GameLoadController(ILogger<GameLoadController> logger)
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

            dynamic response = GameCreateController.GameLoad(request, appId);
            _logger.LogInformation($"{Request.GetUri()} - {JsonConvert.SerializeObject(response)}");
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