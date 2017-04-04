// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameEventController.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Photon.Webhooks.Turnbased.Controllers
{
    using System.Web.Http;
    using Models;
    using Newtonsoft.Json;


    public class GameEventController : Controller
    {
        private readonly ILogger<GameEventController> _logger;

        public GameEventController(ILogger<GameEventController> logger)
        {
            _logger = logger;
        }
        #region Public Methods and Operators

        public dynamic Post(GameEventRequest request, string appId)
        {
            var okResponse = new OkResponse();
            _logger.LogInformation($"{Request.GetUri()} - {JsonConvert.SerializeObject(okResponse)}");
            return okResponse;
        }

        #endregion
    }
}