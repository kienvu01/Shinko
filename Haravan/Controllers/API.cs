using Haravan.ModelsApp;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Controllers
{
    [Route("")]
    [ApiController]
    public class API : ControllerBase
    {
        [HttpGet]
        [Route("")]
        public IActionResult Get()
        {
            ILog log = Logger.GetLog(typeof(Webhooks));
            log.Info("connect Ok");
            return Ok("ok");
        }

        [HttpPost]
        [Route("")]
        public IActionResult Get2()
        {
            return Ok("ok");
        }
        [HttpPut]
        [Route("")]
        public IActionResult Get3()
        {
            return Ok("ok");
        }
    }
}
