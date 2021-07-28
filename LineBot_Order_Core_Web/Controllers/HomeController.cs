using LineBot_Order_Core_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LineBot_Order_Core_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LineNotifyConfig _lineNotifyConfig;

        public HomeController(ILogger<HomeController> logger, LineNotifyConfig lineNotifyConfig)
        {
            _logger = logger;
            _lineNotifyConfig = lineNotifyConfig;
        }

        public IActionResult Index()
        {           
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        public IActionResult NotifyTokenSuccess()
        {
            return View();
        }
        public IActionResult OrderList()
        {
            return View(Order.lisOrder);
        }
        public IActionResult UserList()
        {
            #region 讀取 UserList.json 到 lisUser
            string strUserList = System.IO.File.ReadAllText("UserList.json");
            JObject objUserList = JObject.Parse(strUserList);
            JArray arrayUserList = (JArray)objUserList["User"];
            IList<User> lisUser = arrayUserList.ToObject<List<User>>();
            #endregion

            return View(lisUser);
        }
    }
}
