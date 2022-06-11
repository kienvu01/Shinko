using Haravan.FuncLib;
using Haravan.ModelsApp;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Haravan.Controllers
{
    [Route("Voucher")]
    [ApiController]
    public class Voucher : ControllerBase
    {
        private readonly IConfiguration _config;


        public Voucher(IConfiguration config)
        {
            _config = config;
        }
        [HttpPost]
        [Route("CheckDisCount")]
        public async Task<IActionResult> CheckVoucherExits([FromBody] Object data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);

                ModelsApp.Discounts o = new Discounts(_config);
                ResponseData res = await o.CheckDisCount_test(data);

                return Ok(res);
  
                
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Voucher));

                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }
    }
}
