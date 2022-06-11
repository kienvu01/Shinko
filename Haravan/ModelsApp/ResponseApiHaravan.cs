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
    public class ResponseApiHaravan
    {
        public String status { get ; set ; }
        public String message { get ; set ; }

        public string data { get; set ; }
        public ResponseApiHaravan( String status , String message , string data)
        {
            this.status = status;
            this.message = message;
            this.data = data;
        }
    }
}
