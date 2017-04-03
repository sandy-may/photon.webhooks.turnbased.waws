// ------------------------------------------------------------------------------------------------
//  <copyright file="Global.asax.cs" company="Exit Games GmbH">
//    Copyright (c) Exit Games GmbH.  All rights reserved.
//  </copyright>
// ------------------------------------------------------------------------------------------------

namespace Photon.Webhooks.Turnbased
{
    using System;
    using System.Web.Http;
    using System.Configuration;

    using DataAccess;

    using Microsoft.WindowsAzure.Storage;

    using ServiceStack.Redis;

    public class WebApiApplication : System.Web.HttpApplication
    {
        public static IDataAccess DataAccess;

        public static CloudStorageAccount CloudStorageAccount;


        public static PooledRedisClientManager PooledRedisClientManager;

        protected void Application_Start()
        {
            if (ConfigurationManager.AppSettings["DataAccess"].Equals("Azure", StringComparison.OrdinalIgnoreCase))
            {
                CloudStorageAccount = CloudStorageAccount.Parse(
                                    string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                                        ConfigurationManager.AppSettings["AzureAccountName"],
                                        ConfigurationManager.AppSettings["AzureAccountKey"])
                                    );

                DataAccess = new Azure();
            }
            else if (ConfigurationManager.AppSettings["DataAccess"].Equals("Redis", StringComparison.OrdinalIgnoreCase))
            {
                PooledRedisClientManager = new PooledRedisClientManager(
                  string.IsNullOrEmpty(ConfigurationManager.AppSettings["RedisPassword"]) ?
                      string.Format("{0}:{1}", ConfigurationManager.AppSettings["RedisUrl"], ConfigurationManager.AppSettings["RedisPort"]) :
                      string.Format("{0}@{1}:{2}", ConfigurationManager.AppSettings["RedisPassword"], ConfigurationManager.AppSettings["RedisUrl"], ConfigurationManager.AppSettings["RedisPort"])
                  );

                DataAccess = new Redis();
            }
            else
            {
                DataAccess = new NotImplemented();
            }

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
