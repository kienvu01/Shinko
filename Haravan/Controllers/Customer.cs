using Haravan.Filter;
using Haravan.FuncLib;
using Haravan.Model;
using Haravan.ModelsApp;
using log4net;
using Microsoft.AspNetCore.Authorization;
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
    [Route("Customer")]
    [ApiController]
    public class Customer : ControllerBase
    {
        private readonly IConfiguration _config;


        public Customer(IConfiguration config)
        {
            _config = config;
        }
        [HttpPost]
        [Route("UpdateAllCustomer")]
        public async Task<IActionResult> DongBo_Customer([FromBody] API_Data_Customer_UpdateCustomer data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);

                string phone = "";
                foreach(string s in data.phone)
                {
                    phone += $"{s},";
                }
                phone += "0 ";

                DateTime mydate = Convert.ToDateTime(data.toDate);
                mydate = Convert.ToDateTime(data.toDate).AddDays(1);
                data.toDate = mydate.ToString("yyyy/MM/dd");
                Customers cus = new Customers(_config);
                ResponseData res = await cus.UpdateAllCustomer(data.fromDate, data.toDate,phone);
                if (res.status == "ok") return Ok(res);
                else
                {
                    ILog log = Logger.GetLog(typeof(Customer));

                    log.Error(res.message);
                    return StatusCode(500);
                }
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost]
        [Route("AddNewCustomer")]
        public async  Task<IActionResult> AddNewCustomer([FromBody] Res_NewCustomer data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);

                Customers cus = new Customers(_config);
                ResponseData res = await cus.CreatNewCustomer(data);

                return Ok(res);
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(200, e.Message);
            }
        }
        [HttpPost]
        [Route("Test")]
        public async Task<IActionResult> Test ([FromBody] Object data)
        {
            try
            {
                bool check = Library.CheckAuthentication(_config, this);
                if (!check) return StatusCode(401);
                string s = data.ToString();
                JObject obj = JObject.Parse(s);
                Customers cus = new Customers(_config);
                cus.UpdateCustomer(obj);

                return Ok();
            }
            catch (Exception e)
            {
                ILog log = Logger.GetLog(typeof(Customer));
                log.Error(Request.Path);
                return StatusCode(200, e.Message);
            }
        }
    }
}
