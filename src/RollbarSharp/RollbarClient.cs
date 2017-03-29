﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RollbarSharp.Builders;
using RollbarSharp.Serialization;
using System.Net.Http;

namespace RollbarSharp
{
    /// <summary>
    /// The Rollbar client. This is where applications will interact
    /// with Rollbar. There shouldn't be any need for them to deal
    /// with any objects aside from this
    /// </summary>
    public class RollbarClient : IRollbarClient
    {
        /// <summary>
        /// Signature for the handler fired when the request is complete
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        public delegate void RequestCompletedEventHandler(object source, RequestCompletedEventArgs args);

        public delegate void RequestSendingEventHandler(object source, RequestStartingEventArgs args);

        public Configuration Configuration { get; protected set; }

        /// <summary>
        /// Builds Rollbar requests from <see cref="Exception"/>s or text messages
        /// </summary>
        /// <remarks>This only builds the body of the request, not the whole notice payload</remarks>
        public DataModelBuilder NoticeBuilder { get; protected set; }

        /// <summary>
        /// Fires just before sending the final JSON payload to Rollbar
        /// </summary>
        public event RequestSendingEventHandler RequestStarting;

        /// <summary>
        /// Fires when we've received a response from Rollbar
        /// </summary>
        public event RequestCompletedEventHandler RequestCompleted;

        // Use same HttpClient instance for all requests (see https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/)
        private HttpClient httpClient = new HttpClient();

        public RollbarClient(Configuration configuration)
        {
            Configuration = configuration;
            NoticeBuilder = new DataModelBuilder(Configuration);
        }

        /// <summary>
        /// Creates a new RollbarClient using the given access token
        /// and all default <see cref="Configuration"/> values
        /// </summary>
        /// <param name="accessToken"></param>
        public RollbarClient(string accessToken)
            : this(new Configuration(accessToken))
        {
        }

        /// <summary>
        /// Creates a new RollbarClient using configuration values from app/web.config
        /// </summary>
        public RollbarClient()
            : this(Configuration.CreateFromAppConfig())
        {
        }

        /// <summary>
        /// Sends an exception using the "critical" level
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="title"></param>
        /// <param name="modelAction"></param>
        /// <param name="userParam"></param>
        public Task SendCriticalException(Exception ex, string title = null, Action<DataModel> modelAction = null, object userParam = null)
        {
            return SendException(ex, title, "critical", modelAction);
        }

        /// <summary>
        /// Sends an exception using the "error" level
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="title"></param>
        /// <param name="modelAction"></param>
        /// <param name="userParam"></param>
        public Task SendErrorException(Exception ex, string title = null, Action<DataModel> modelAction = null, object userParam = null)
        {
            return SendException(ex, title, "error", modelAction);
        }

        /// <summary>
        /// Sents an exception using the "warning" level
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="title"></param>
        /// <param name="modelAction"></param>
        /// <param name="userParam"></param>
        public Task SendWarningException(Exception ex, string title = null, Action<DataModel> modelAction = null, object userParam = null)
        {
           return SendException(ex, title, "warning", modelAction);
        }

        /// <summary>
        /// Sends the given <see cref="Exception"/> to Rollbar including
        /// the stack trace. 
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="title"></param>
        /// <param name="level">Default is "error". "critical" and "warning" may also make sense to use.</param>
        /// <param name="modelAction"></param>
        /// <param name="userParam"></param>
        public Task SendException(Exception ex, string title = null, string level = "error", Action<DataModel> modelAction = null, object userParam = null)
        {
            var notice = NoticeBuilder.CreateExceptionNotice(ex, title, level);
            if (modelAction != null)
            {
                modelAction(notice);
            }
            return Send(notice, userParam);
        }

        /// <summary>
        /// Sends a text notice using the "critical" level
        /// </summary>
        /// <param name="message"></param>
        /// <param name="customData"></param>
        /// <param name="modelAction"></param>
        /// <param name="userParam"></param>
        public Task SendCriticalMessage(string message, IDictionary<string, object> customData = null, Action<DataModel> modelAction = null, object userParam = null)
        {
            return SendMessage(message, "critical", customData, modelAction, userParam);
        }

        /// <summary>
        /// Sents a text notice using the "error" level
        /// </summary>
        /// <param name="message"></param>
        /// <param name="customData"></param>
        /// <param name="modelAction"></param>
        /// <param name="userParam"></param>
        public Task SendErrorMessage(string message, IDictionary<string, object> customData = null, Action<DataModel> modelAction = null, object userParam = null)
        {
            return SendMessage(message, "error", customData, modelAction, userParam);
        }

        /// <summary>
        /// Sends a text notice using the "warning" level
        /// </summary>
        /// <param name="message"></param>
        /// <param name="customData"></param>
        /// <param name="modelAction"></param>
        /// <param name="userParam"></param>
        public Task SendWarningMessage(string message, IDictionary<string, object> customData = null, Action<DataModel> modelAction = null, object userParam = null)
        {
            return SendMessage(message, "warning", customData, modelAction, userParam);
        }

        /// <summary>
        /// Sends a text notice using the "info" level
        /// </summary>
        /// <param name="message"></param>
        /// <param name="customData"></param>
        /// <param name="modelAction"></param>
        /// <param name="userParam"></param>
        public Task SendInfoMessage(string message, IDictionary<string, object> customData = null, Action<DataModel> modelAction = null, object userParam = null)
        {
            return SendMessage(message, "info", customData, modelAction, userParam);
        }

        /// <summary>
        /// Sends a text notice using the "debug" level
        /// </summary>
        /// <param name="message"></param>
        /// <param name="customData"></param>
        /// <param name="modelAction"></param>
        /// <param name="userParam"></param>
        public Task SendDebugMessage(string message, IDictionary<string, object> customData = null, Action<DataModel> modelAction = null, object userParam = null)
        {
            return SendMessage(message, "debug", customData, modelAction);
        }

        /// <summary>
        /// Sents a text notice using the given level of severity
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        /// <param name="customData"></param>
        /// <param name="modelAction"></param>
        /// <param name="userParam"></param>
        public Task SendMessage(string message, string level, IDictionary<string, object> customData = null, Action<DataModel> modelAction = null, object userParam = null)
        {
            var notice = NoticeBuilder.CreateMessageNotice(message, level, customData);
            if (modelAction != null)
            {
                modelAction(notice);
            }
            return Send(notice, userParam);
        }

        public Task Send(DataModel data, object userParam)
        {
            var payload = new PayloadModel(Configuration.AccessToken, data);
            return HttpPost(payload, userParam);
        }

        /// <summary>
        /// Serialize the given object for transmission
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data, Configuration.JsonSettings);
        }

        protected Task HttpPost(PayloadModel payload, object userParam)
        {
            var payloadString = Serialize(payload);
            return HttpPost(payloadString, userParam);
        }

        protected Task HttpPost(string payload, object userParam)
        {
            return Task.Factory.StartNew(async () => await HttpPostAsync(payload, userParam));
        }

        protected async Task HttpPostAsync(string payload, object userParam)
        {
            OnRequestStarting(payload, userParam);

            try
            {
                var payloadBytes = Encoding.UTF8.GetBytes(payload);
                var httpResponse = await httpClient.PostAsync(this.Configuration.Endpoint, new ByteArrayContent(payloadBytes));
                var response = await httpResponse.Content.ReadAsStringAsync();

                OnRequestCompleted(new Result((int)httpResponse.StatusCode, response, userParam));
            }
            catch (Exception err)
            {
                OnRequestCompleted(new Result(0, err.Message, userParam));
            }
        }
        
        protected void OnRequestStarting(string payload, object userParam)
        {
            if (RequestStarting == null)
                return;

            RequestStarting(this, new RequestStartingEventArgs(payload, userParam));
        }

        protected void OnRequestCompleted(WebResponse response, object userParam)
        {
            var responseCode = (int) ((HttpWebResponse) response).StatusCode;
            string responseText;

            using (var stream = response.GetResponseStream())
            {
                if (stream == null)
                    responseText = string.Empty;
                else
                {
                    using (var reader = new StreamReader(stream))
                    {
                        responseText = reader.ReadToEnd();
                    }
                }
            }
            
            var result = new Result(responseCode, responseText, userParam);
            OnRequestCompleted(result);
        }

        protected void OnRequestCompleted(Result result)
        {
            if (RequestCompleted == null)
                return;

            var args = new RequestCompletedEventArgs(result);
            RequestCompleted(this, args);
        }
    }
}
