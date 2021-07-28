using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LineBot_Order_Core_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LineBot_Order_Core_Web.Controllers
{
    public class LineNotifyController : Controller
    {
        private readonly LineNotifyConfig _lineNotifyConfig;
        readonly ILogger<LineNotifyController> _log;

        public LineNotifyController(ILogger<LineNotifyController> log, LineNotifyConfig lineNotifyConfig)
        {
            _log = log;
            _lineNotifyConfig = lineNotifyConfig;
        }

        // POST: api/LineNotify/SendNotify
        /// <summary>傳送文字通知</summary>
        /// <param name="msg">訊息</param>
        [HttpPost]
        [Route("SendNotify")]
        public async Task<IActionResult> SendNotify([FromBody]MessageModel msg)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_lineNotifyConfig.NotifyUrl);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + msg.Token);

                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("message", msg.Message)
                });

                await client.PostAsync("", form);
            }

            return new EmptyResult();
        }
    }
}
