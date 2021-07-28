using LineBot_Order_Core_Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LineBot_Order_Core_Web.Views.Home
{
    public class LoginController : Controller
    {        
        public IActionResult Login()
        {
            return PartialView();
        }
        [HttpPost]
        public IActionResult Login(Login login)
        {
            if (login.txtPassword == "airiti")
            {                
                return RedirectToAction("Index", "Home");
            }
            else
                return PartialView();
        }

        [HttpPost]
        public JsonResult checklogin([FromBody] Login login)
        {
            string data = (login.txtPassword == "airiti") ? "通關碼正確" : "通關碼錯誤";            
            return Json(data);
        }
    }
}
