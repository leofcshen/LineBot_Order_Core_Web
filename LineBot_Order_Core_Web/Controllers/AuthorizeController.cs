using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LineBot_Order_Core_Web.Extensions;
using LineBot_Order_Core_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LineBot_Order_Core_Web.Controllers
{
    [Route("[controller]")]
    public class AuthorizeController : Controller
    {
        private readonly LineNotifyConfig _lineNotifyConfig;
        readonly ILogger<AuthorizeController> _log;        
        
        public AuthorizeController(ILogger<AuthorizeController> log, LineNotifyConfig lineNotifyConfig)
        {
            _log = log;
            _lineNotifyConfig = lineNotifyConfig;
        }

        // GET: api/Authorize
        /// <summary>設定與 Lind Notify 連動</summary>
        [HttpGet]
        public IActionResult GetAuthorize()
        {            
            var uri = Uri.EscapeUriString(
                _lineNotifyConfig.AuthorizeUrl + "?" +
                "response_type=code" +
                "&client_id=" + _lineNotifyConfig.ClientId +
                "&redirect_uri=" + _lineNotifyConfig.CallbackUrl +
                "&scope=notify" +
                "&state=" + _lineNotifyConfig.State
            );
            Response?.Redirect(uri);

            return new EmptyResult();
        }

        // GET: api/Authorize/Callback 綁給 Line Notify
        /// <summary>取得使用者 code</summary>
        /// <param name="code">用來取得 Access Tokens 的 Authorize Code</param>
        /// <param name="state">驗證用。避免 CSRF 攻擊</param>
        /// <param name="error">錯誤訊息</param>
        /// <param name="errorDescription">錯誤描述</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Callback")]
        public async Task<IActionResult> GetCallback(
            [FromQuery] string code,
            [FromQuery] string state,
            [FromQuery] string error,
            [FromQuery][JsonProperty("error_description")] string errorDescription)
        {
            if (!string.IsNullOrEmpty(error))
                return new JsonResult(new
                {
                    error,
                    state,
                    errorDescription
                });

            Response.Redirect(_lineNotifyConfig.SuccessUrl + "?token=" + await FetchToken(code));

            return new EmptyResult();
        }

        /// <summary>取得使用者 Token</summary>
        /// <param name="code">用來取得 Access Tokens 的 Authorize Code</param>
        /// <returns></returns>
        private async Task<string> FetchToken(string code)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 60);
                client.BaseAddress = new Uri(_lineNotifyConfig.TokenUrl);

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", _lineNotifyConfig.CallbackUrl),
                    new KeyValuePair<string, string>("client_id", _lineNotifyConfig.ClientId),
                    new KeyValuePair<string, string>("client_secret", _lineNotifyConfig.ClientSecret)
                });
                var response = await client.PostAsync("", content);
                var data = await response.Content.ReadAsStringAsync();

                var token = JsonConvert.DeserializeObject<JObject>(data)["access_token"].ToString();
                
                #region 新增到 TokenList
                string strNotifyTokenList = System.IO.File.ReadAllText("NotifyTokenList.json"); // 讀檔
                JObject jObjTokenList = JObject.Parse(strNotifyTokenList); // 轉成 JObj
                JArray jArrTokenList = (JArray)jObjTokenList["LineNotifyToken"]; // 取 LineNotifyToken 區塊
                LineNotifyToken userToken = new LineNotifyToken() { Token = token }; // 產生 Token 物件
                string strToken = JsonConvert.SerializeObject(userToken); // 序列化 Token 物件
                JObject jObjToken = JObject.Parse(strToken); // 轉成 JObject
                jArrTokenList.Add(jObjToken); // 新增 Token
                string strConvert = Convert.ToString(jObjTokenList); // 轉回 string
                System.IO.File.WriteAllText("NotifyTokenList.json", strConvert); // 存檔
                #endregion

                // 發測試通知
                isRock.LineNotify.Utility.SendNotify(token, DateTime.Now.ToString() + "這是測試通知。");

                return token;
            }
        }
    }
}
