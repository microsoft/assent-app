// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.CFS.Approvals.SupportService.API.Controllers.api.v1
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.CFS.Approvals.SupportServices.Helper.Interface;
    using Microsoft.CFS.Approvals.SupportServices.Helper.ServiceHelper;

    /// <summary>
    /// The Service Bus Monitoring Controller
    /// </summary>
    [Route("api/v1/ServiceBusMonitoring")]
    [ApiController]
    public class ServiceBusMonitoringController : ControllerBase
    {
        /// <summary>
        /// The service bus helper
        /// </summary>
        private readonly IServiceBusHelper _serviceBusHelper;
        private readonly ConfigurationHelper _configurationHelper;
        private readonly string _environment;
        public ServiceBusMonitoringController(
            IServiceBusHelper serviceBusHelper,
            ConfigurationHelper configurationHelper,
            IActionContextAccessor actionContextAccessor)
        {
            _environment = actionContextAccessor?.ActionContext?.RouteData?.Values["env"]?.ToString();
            _serviceBusHelper = serviceBusHelper;
            _configurationHelper = configurationHelper;
        }

        /// <summary>
        /// Get list of document numbers for given subscription , topic and message type
        /// </summary>
        /// <param name="subscriptionName"></param>
        /// <param name="topicName"></param>
        /// <param name="messageType">Active/Dead letter</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{env}")]
        public Dictionary<string, ArrayList> Get(string subscriptionName, string topicName, string messageType)
        {

            Dictionary<string, ArrayList> docCollection = _serviceBusHelper.PeekServiceBusMessage(topicName, subscriptionName, messageType);
            return docCollection;
        }

        /// <summary>
        /// Get list of topics for connection string
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTopic/{env}")]
        public List<string> Get()
            {

            List<string> topicCollection = _serviceBusHelper.GetTopics().Where(t => _configurationHelper.appSettings[_environment].ServiceBusTopics.Contains(t)).ToList();
            return topicCollection;
        }


        /// <summary>
        /// Get subscriptions for the selected topic
        /// </summary>
        /// <param name="topicName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSubscription/{env}")]
        public List<string> Get(string TopicName)
        {

            List<string> subscriptionCollection = _serviceBusHelper.GetSubscriptions(TopicName);
            return subscriptionCollection;
        }
    }
}
