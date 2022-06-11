using Haravan.FuncLib;
using Haravan.Model;
using Haravan.ModelsApp;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Haravan.Controllers
{
    [Route("webhooks")]
    [ApiController]
    public class Webhooks : ControllerBase
    {
        private readonly IConfiguration _config;


        public Webhooks(IConfiguration config )
        {
            _config = config;
        }
        [HttpGet]
        [Route("")]
        public IActionResult Get()
        {
            try
            {
                ILog log = Logger.GetLog(typeof(Webhooks));
                var VerifyToken = _config.GetValue<string>("config_Haravan:webhook:hrVerifyToken");
                string verify_token = Request.Query["hub.verify_token"];
                Object obj = new{};
                if (VerifyToken !=null && verify_token != null && VerifyToken == verify_token)
                {
                    string challenge = Request.Query["hub.challenge"];
                    if (challenge == null) challenge = "";
                    log.Info(Request.Path);
                    return Ok(challenge);
                }
                else
                {
                    log.Error(Request.Path);
                    return StatusCode(401); 
                }
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Webhooks));
                log.Error(Request.Path);
                return StatusCode(200,e.Message);
            }
        }

        [HttpPost]
        [Route("App")]
        public IActionResult WebhookHaravan([FromBody] Object val)
        {
            try
            {

                var app_secret = _config.GetValue<string>("config_Haravan:app_secret");
                ILog log = Logger.GetLog(typeof(Webhooks));

                string topic = Request.Headers["x-haravan-topic"];
                ApiLog.log(topic, Request.Headers, val, "send ok", "");
                if (topic != null)
                {
                    string s123 = val.ToString();
                    var topicname = _config.GetValue<string>("topic_haravan:" + topic);
                    dynamic re = Haravan.BLL.CallFunc1.CallFuncbyTopic( topicname, val);

                    return Ok();
                }
                else
                {
                    return StatusCode(200);
                }
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Webhooks));
                log.Error(e.Message);
                return StatusCode(200, e.Message);
            }
        }

        [HttpPost]
        [Route("App2")]
        public IActionResult WebhookHaravan2([FromBody] Object val)
        {
            try
            {

                var app_secret = _config.GetValue<string>("config_Haravan:app_secret");
                ILog log = Logger.GetLog(typeof(Webhooks));

                string topic = Request.Headers["x-haravan-topic"];
                ApiLog.log(topic, Request.Headers, val, "send ok", "");
                if (topic != null)
                {
                    string s123 = val.ToString();
                    var topicname = _config.GetValue<string>("topic_haravan:" + topic);
                    dynamic re = Haravan.BLL.CallFunc2.CallFuncbyTopic(topicname, val);

                    return Ok();
                }
                else
                {
                    return StatusCode(200);
                }
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Webhooks));
                log.Error(e.Message);
                return StatusCode(200, e.Message);
            }
        }
        [HttpPost]
        [Route("App3")]
        public IActionResult WebhookHaravan3([FromBody] Object val)
        {
            try
            {

                var app_secret = _config.GetValue<string>("config_Haravan:app_secret");
                ILog log = Logger.GetLog(typeof(Webhooks));

                string topic = Request.Headers["x-haravan-topic"];
                ApiLog.log(topic, Request.Headers, val, "send ok", "");
                if (topic != null)
                {
                    string s123 = val.ToString();
                    var topicname = _config.GetValue<string>("topic_haravan:" + topic);
                    dynamic re = Haravan.BLL.CallFunc3.CallFuncbyTopic(topicname, val);

                    return Ok();
                }
                else
                {
                    return StatusCode(200);
                }
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Webhooks));
                log.Error(e.Message);
                return StatusCode(200, e.Message);
            }
        }

        [HttpPost]
        [Route("ShinkoWebHook")]
        public IActionResult WebhokWP([FromBody] Object val)
        {
            try
            {
                ILog log = Logger.GetLog(typeof(Webhooks));
                string source = Request.Headers["X-WC-Webhook-Source"];
                string topic = Request.Headers["X-WC-Webhook-Topic"];
                string webhooksId = Request.Headers["X-WC-Webhook-ID"];
                ApiLog.logval("test-hook", "");
                ApiLog.log($"WP-{topic}", Request.Headers, val, "send ok", "");
                bool temp = CheckWebHookWP(source, topic, webhooksId);
                if (temp)
                {
                    string data_temp = val.ToString();
                    JObject obj = JObject.Parse(data_temp);
                    if(topic == "order.created") WP_Order.UpdateOrder(obj);
                    if (topic == "order.updated") WP_Order.UpdateOrder(obj);

                }
                return StatusCode(200);
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Webhooks));
                log.Error(e.Message);
                return StatusCode(200, e.Message);
            }
        }
        [HttpGet]
        [Route("WP")]
        public IActionResult WebhokWP_2([FromBody] Object val)
        {
            try
            {
                ILog log = Logger.GetLog(typeof(Webhooks));
                ApiLog.log("WP2", Request.Headers, val, "send ok", "");
                return StatusCode(200);
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Webhooks));
                log.Error(e.Message);
                return StatusCode(200, e.Message);
            }
        }
        public bool CheckWebHookWP(string source,string topic,string id)
        {
            bool check = true;
            try
            {
                var s1 = _config.GetValue<string>($"config_wp:url");
                if (source != s1) return false;
                var ss = _config.GetValue<string>($"wp_id_webhooks:{topic}");
                if (ss != id) return  false;
                return check;
            }
            catch(Exception e)
            {
                return false;
            }
        }
    }
}
