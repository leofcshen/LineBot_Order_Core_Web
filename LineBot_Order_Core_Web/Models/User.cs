using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LineBot_Order_Core_Web.Models
{
    public class User
    {
        /// <summary>
        /// Line UserID
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 員工工號
        /// </summary>
        public string EmployeeID { get; set; }
        /// <summary>
        /// 部門
        /// </summary>
        public string Department { get; set; }
        /// <summary>
        /// 員工姓名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 帳號權限 (admin、user)
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// 帳號狀態
        /// </summary>
        public string Enable { get; set; }
        public User(string _userID, string _employeeID, string _userName)
        {
            UserID = _userID;
            EmployeeID = _employeeID;
            Department = "";
            UserName = _userName;
            Role = "user";
            Enable = "1";
        }
    }
}
