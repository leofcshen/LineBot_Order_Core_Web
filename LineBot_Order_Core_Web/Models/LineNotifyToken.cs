using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LineBot_Order_Core_Web.Models
{
    public class LineNotifyToken
    {
        public string Token { get; set; }

        /// <summary>
        /// 讀取 TokenList.json 轉成 list
        /// </summary>
        /// <returns></returns>
        public static IList<LineNotifyToken> GetNotifyTokenList()
        {
            string strNotifyTokenList = System.IO.File.ReadAllText("NotifyTokenList.json"); // 讀檔
            JObject jObjTokenList = JObject.Parse(strNotifyTokenList); // 轉成 JObj
            JArray jArrTokenList = (JArray)jObjTokenList["LineNotifyToken"]; // 取 LineNotifyToken 區塊
            return jArrTokenList.ToObject<IList<LineNotifyToken>>();
        }
    }
}
