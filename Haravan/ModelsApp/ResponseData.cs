using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.ModelsApp
{
    public class ResponseData
    {
        public String status { get ; set ; }
        public String message { get ; set ; }

        public dynamic data { get; set ; }
        public ResponseData( String status , String message , dynamic data)
        {
            this.status = status;
            this.message = message;
            this.data = data;
        }
        public IActionResult Ok(ControllerBase c)
        {
            ILog log = Logger.GetLog(typeof(ResponseData));
            log.Info(c.Request.Path);
            return c.Ok(data);
        }
    }
}
