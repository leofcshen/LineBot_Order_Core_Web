using HtmlAgilityPack;
using isRock.LineBot;
using LineBot_Order_Core_Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static LineBot_Order_Core_Web.Models.Order;
using static LineBot_Order_Core_Web.Models.User;

namespace LineBot_Order_Core_Web.Controllers
{
    [Route("api/linebot")]
    [ApiController]
    public class LineBotController : ControllerBase
    {
        readonly ILogger<LineBotController> _log;
        private readonly IConfiguration _config;
        private readonly LineBotConfig _lineBotConfig;
        /// <summary>
        /// 操作失敗，請輸入 \"!cmd\" 檢查指令格式，找不出問題請洽管理員。
        /// </summary>
        private readonly string strCmdFail = "操作失敗，指令格式錯誤或沒有權限\n請至 https://linebotorder.rovingwind.synology.me 檢查指令格式\n找不出問題請洽管理員。";

        public LineBotController(ILogger<LineBotController> log, IConfiguration config, LineBotConfig lineBotConfig)
        {
            _log = log;
            _config = config;
            _lineBotConfig = lineBotConfig;
        }

        [HttpPost]
        public async Task<IActionResult> Post(dynamic request)
        {
            // 接到的資料打 log
            string strRequest = Convert.ToString(request);
            _log.LogInformation(strRequest);
            var channelAccessToken = _config.GetValue<string>("LineBot:Channel_access_token"); // 抓 Config
            var receivedMessage = JsonConvert.DeserializeObject<LineReceivedMessage>(strRequest); // 收到訊息解成物件

            #region 讓 Line Devloper 官網打測試不要報錯
            if (receivedMessage.events.Count == 0)
            {
                return Ok(request);
            }
            #endregion

            #region 讀取 UserList.json 到 lisUser
            string strUserList = System.IO.File.ReadAllText("UserList.json");
            JObject objUserList = JObject.Parse(strUserList);
            JArray arrayUserList = (JArray)objUserList["User"];
            IList<User> lisUser = arrayUserList.ToObject<List<User>>();
            #endregion

            #region 解析 request 資料，設定必要參數。
            string type = receivedMessage.events[0].type; // 取得類型 (User 加入或解除封鎖時為 follow，封鎖為 unfollow)
            string messageType = receivedMessage.events[0].message.type; // 取得訊息類型
            string userId = receivedMessage.events[0].source.userId; // 取得使用者 ID
            string message = receivedMessage.events[0].message.text; // 取得文字訊息
            var replyToken = receivedMessage.events[0].replyToken; // 取得回應者 Token
            var q = lisUser.Where(x => x.UserID == userId).ToList(); // 查詢 UserId
            bool isRegistered = (q.Count != 0); // 是否已註冊
            string idName = q.Count == 1 ? $"({ q[0].EmployeeID}){q[0].UserName}" : string.Empty; // 組裝 (工號)姓名 以利同名辨識
            #endregion

            try
            {
                switch (type)
                {
                    #region 用戶加入歡迎訊息
                    case "follow": // 用戶加入發送訊息                        
                        if (isRegistered) // 判斷有沒有註冊，因為有可能是舊用戶封鎖後再解除 type 也會是 follow
                            message = $"Hi {q[0].UserName}({q[0].EmployeeID})，歡迎使用訂餐機器人(測試版)，您已註冊過帳號。\n輸入 \"!cmd\" 以查看指令。";
                        else
                            message = "Hi，歡迎使用訂餐機器人(測試版)，您尚未註冊帳號，請輸入 \"!register 工號 姓名\" 進行註冊，姓名如有英文同名請加姓氏以利辨識(例如 \"Leo_Shen\"，注意中間不可有空格。)";                        
                        break;
                    #endregion
                    
                    #region 自動回訊
                    case "message":
                        switch (messageType)
                        {
                            case "sticker": // 貼圖訊息
                                type = "sticker";
                                break;

                            case "image": // 圖片訊息
                                type = "image";
                                break;

                            case "text": // 文字訊息
                                string[] arrayMessage = message.Split(" "); // 分隔指令及參數
                                if (isRegistered) // 已註冊
                                {
                                    if (arrayMessage[0] == "!register") // 註冊過不能再註冊
                                        message = $"無效指令，您已註冊過帳號\n工號：{q[0].EmployeeID}\n使用者名稱：{q[0].UserName}";
                                    else
                                    {
                                        switch (arrayMessage[0]) // 判斷指令
                                        {
                                            #region user 功能

                                            #region !rename 修改個人姓名
                                            case "!rename":
                                                if (arrayMessage.Length == 2) // 檢查格式
                                                {
                                                    var u = objUserList["User"];
                                                    foreach (var i in u)
                                                    {
                                                        if (i["UserID"].ToString() == q[0].UserID)
                                                            i["UserName"] = arrayMessage[1];
                                                    }
                                                    string strConvert = Convert.ToString(objUserList); // 轉回 string
                                                    System.IO.File.WriteAllText("UserList.json", strConvert); // 存檔
                                                    message = $"修改成功\n您的新名字為 {arrayMessage[1]}";
                                                }
                                                break;
                                            #endregion

                                            #region !info 查詢個人資訊
                                            case "!info":
                                                message = $"您的使用者資料如下：\n工號：{q[0].EmployeeID}\n使用者名稱：{q[0].UserName}\n權限：{q[0].Role}";
                                                break;
                                            #endregion

                                            #region !menu 查菜單
                                            case "!menu":
                                                {
                                                    if (Menu.todayMenu.Count != 0)
                                                    {
                                                        string menuList = Menu.GetTodayMenuList();
                                                        message = $"{menuList}";
                                                    }
                                                    else
                                                        message = "目前無菜單";
                                                }
                                                break;
                                            #endregion

                                            #region !order 點餐
                                            case "!order":
                                                if (isStart)
                                                {
                                                    if (arrayMessage.Length == 4) // 檢查格式
                                                    {
                                                        var isOrdered = lisOrder.Where(x => x.name == idName).ToList();
                                                        if (isOrdered.Count == 1) // 查訂單數訂過不能再訂
                                                            message = "你已經訂過了，如要改單請先 !delete 再重訂。";
                                                        else // 加訂單
                                                        {
                                                            if (!Enum.IsDefined(typeof(Rice), int.Parse(arrayMessage[2])) || !Enum.IsDefined(typeof(RiceQuantity), int.Parse(arrayMessage[3])))
                                                                throw new ArgumentOutOfRangeException(); // 檢查 Enum 值有沒有超出範圍
                                                            if (Menu.todayMenu[int.Parse(arrayMessage[1]) - 1] != null)
                                                            {
                                                                string meal = Menu.todayMenu[int.Parse(arrayMessage[1]) - 1].hasMenuItem.name;
                                                                int price = Menu.todayMenu[int.Parse(arrayMessage[1]) - 1].hasMenuItem.offers.price;
                                                                Order order = new Order(idName, meal, (Rice)int.Parse(arrayMessage[2]), (RiceQuantity)int.Parse(arrayMessage[3]),price);
                                                                message = $"訂餐成功：\n{idName} {meal} {(Rice)int.Parse(arrayMessage[2])} {(RiceQuantity)int.Parse(arrayMessage[3])}_{price}";
                                                            }
                                                            else
                                                                message = $"訂餐失敗：請檢查品項是否超出範圍。";

                                                        }
                                                    }
                                                }
                                                else
                                                    message = "目前沒開團哦~";
                                                break;
                                            #endregion

                                            #region !list 查看點餐清單
                                            case "!list":
                                                message = $"目前訂餐清單：\n{Order.PrintOrderList()}";
                                                break;
                                            #endregion

                                            #region !delete 刪除個人的訂餐
                                            case "!delete":
                                                foreach (var order in lisOrder)
                                                {
                                                    if (order.name == idName)
                                                    {
                                                        lisOrder.Remove(order);
                                                        message = "已刪除您的訂單。";
                                                        break;
                                                    }
                                                    else
                                                        message = "操作失敗，沒有您的訂單資料。";
                                                }
                                                break;
                                            #endregion

                                            #region !time 查看系統時間                              
                                            case "!time":
                                                message = $"現在時間 {DateTime.Now}";
                                                break;
                                            #endregion

                                            #region !users 查看使用者清單
                                            case "!users":
                                                var users = lisUser.ToList();
                                                message = $"目前使用者：\n";
                                                foreach (var user in users)
                                                    if (user.Enable == "1")
                                                        message += $"({user.EmployeeID}){user.UserName} {user.Role}\n";
                                                break;
                                            #endregion

                                            #endregion

                                            #region admin 功能

                                            #region 測試
                                            //case "haha":
                                            //    var ButtonsTemplateMsg = new isRock.LineBot.ButtonsTemplate();
                                            //    ButtonsTemplateMsg.altText = "無法顯示時的替代文字";
                                            //    ButtonsTemplateMsg.thumbnailImageUrl = new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/a/a6/Anonymous_emblem.svg/160px-Anonymous_emblem.svg.png");
                                            //    ButtonsTemplateMsg.text = "請訂餐";
                                            //    ButtonsTemplateMsg.title = "詢問";

                                            //    var action = new List<isRock.LineBot.TemplateActionBase>();
                                            //    action.Add(new isRock.LineBot.PostbackAction()
                                            //    { label = "A餐", data = "product=2" });
                                            //    action.Add(new isRock.LineBot.PostbackAction()
                                            //    { label = "B餐", data = "product=2" });
                                            //    action.Add(new isRock.LineBot.PostbackAction()
                                            //    { label = "C餐", data = "product=2" });
                                            //    action.Add(new isRock.LineBot.PostbackAction()
                                            //    { label = "C餐", data = "product=2" });


                                            //    ButtonsTemplateMsg.actions = action;

                                            //    isRock.LineBot.Utility.ReplyTemplateMessage(
                                            //        replyToken,
                                            //        ButtonsTemplateMsg, _lineBotConfig.Channel_access_token);

                                            //    break;

                                            //case "haha":
                                            //    var item = new isRock.LineBot.RichMenu.RichMenuItem();
                                            //    item.name = "no name";
                                            //    item.chatBarText = "1快捷選單";
                                            //    // 建立左方按鈕區塊
                                            //    var leftButton = new isRock.LineBot.RichMenu.Area();
                                            //    leftButton.bounds.x = 0;
                                            //    leftButton.bounds.y = 0;
                                            //    leftButton.bounds.width = 460;
                                            //    leftButton.bounds.height = 1686;
                                            //    leftButton.action = new MessageAction() { label = "左", text = "/左" };

                                            //    var rightButton = new isRock.LineBot.RichMenu.Area();
                                            //    rightButton.bounds.x = 2040;
                                            //    rightButton.bounds.y = 0;
                                            //    rightButton.bounds.width = 2040 +460;
                                            //    rightButton.bounds.height = 1686;
                                            //    rightButton.action = new MessageAction() { label = "右", text = "/右" };
                                            //    item.areas.Add(leftButton);
                                            //    item.areas.Add(rightButton);

                                            //    var menu = isRock.LineBot.Utility.CreateRichMenu(item,
                                            //        new Uri("http://arock.blob.core.windows.net/blogdata201902/test01.png"),
                                            //        _lineBotConfig.Channel_access_token);
                                            //    isRock.LineBot.Utility.SetDefaultRichMenu(menu.richMenuId, _lineBotConfig.Channel_access_token);
                                            //    isRock.LineBot.Utility.CancelDefaultRichMenu(_lineBotConfig.Channel_access_token);

                                            //    break;
                                            #endregion

                                            #region !notify 發送通知
                                            case "!notify":
                                                if (q[0].Role == "admin" && arrayMessage.Length == 2)
                                                {
                                                    IList<LineNotifyToken> lisToken = LineNotifyToken.GetNotifyTokenList();
                                                    foreach (var token in lisToken)
                                                    {
                                                        // 取消連動會報錯 catch 掉
                                                        try { isRock.LineNotify.Utility.SendNotify(token.Token, $"\n{arrayMessage[1]}"); }
                                                        catch { }
                                                    }
                                                    message = "發送通知成功。";
                                                }
                                                break;
                                            #endregion

                                            #region !send 發送訊息
                                            case "!send":
                                                if (arrayMessage.Length == 2 && q[0].Role == "admin")
                                                {
                                                    using (var client = new HttpClient())
                                                    {
                                                        client.BaseAddress = new Uri("http://localhost:50172/");
                                                        HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, $"api/linebot/SendMessage?message={arrayMessage[1]}");
                                                        var a = client.Send(requestMessage);
                                                        message = a.StatusCode == HttpStatusCode.OK ? $"發送訊息 \"{arrayMessage[1]}\" 成功。" : $"發送訊息 \"{arrayMessage[1]}\" 失敗。";
                                                    }
                                                }
                                                break;
                                            #endregion

                                            #region !start 開團
                                            case "!start":
                                                if (q[0].Role == "admin")
                                                {
                                                    if (Order.isStart) // 檢查是不是開團中
                                                    {
                                                        message = $"操作失敗，目前有進行中的開團還沒結單。";
                                                    }
                                                    else
                                                    {                                                        
                                                        if (LocalHusband.UpdateTodayMenu()) // 抓菜單
                                                        {
                                                            lisOrder.Clear();
                                                            Order.isStart = true; // 設成開團中                                                            
                                                            string menuList = Menu.GetTodayMenuList();
                                                            IList<LineNotifyToken> lisToken = LineNotifyToken.GetNotifyTokenList();

                                                            #region 發送開團通知
                                                            foreach (var token in lisToken)
                                                            {
                                                                // 取消連動會報錯 catch 掉
                                                                try { isRock.LineNotify.Utility.SendNotify(token.Token, $"已開團，請用 Line Bot 訂餐。\n{menuList}"); }
                                                                catch { }
                                                            }
                                                            #endregion

                                                            message = $"開團成功，已發送開團通知。";
                                                        }
                                                        else
                                                            message = $"開團失敗，六日無菜單";
                                                    }
                                                }

                                                break;
                                            #endregion

                                            #region !end 結單
                                            case "!end":
                                                if (q[0].Role == "admin")
                                                {
                                                    if (Order.isStart) // 檢查是不是開團中
                                                    {
                                                        Order.isStart = false;
                                                        IList<LineNotifyToken> lisToken = LineNotifyToken.GetNotifyTokenList();

                                                        string orderList = Order.PrintOrderList();

                                                        #region 發送結單通知
                                                        foreach (var token in lisToken)
                                                        {
                                                            // 取消連動會報錯 catch 掉
                                                            try { isRock.LineNotify.Utility.SendNotify(token.Token, $"訂餐已結單，訂餐清單：\n{orderList}"); }
                                                            catch { }
                                                        }
                                                        #endregion

                                                        message = $"結單成功，已發送通知。";                                                        
                                                    }
                                                    else
                                                        message = $"操作失敗，目前沒有進行中的開團。";
                                                }
                                                break;
                                            #endregion

                                            #region !isStart 查詢/修改開團狀態
                                            case "!isStart":
                                                if (q[0].Role == "admin")                                                    
                                                    if (arrayMessage.Length == 2) // 修改開團狀態
                                                    {
                                                        Order.isStart = Boolean.Parse(arrayMessage[1]);
                                                        message = $"操作成功，開團狀態已調為 {Order.isStart}";
                                                    }
                                                    else if (arrayMessage.Length == 1) // 查詢開團狀態
                                                        message = isStart.ToString();
                                                break;
                                            #endregion


                                            #region !clear 清除所有點餐清單
                                            case "!clearAll":
                                                if (arrayMessage.Length == 2 && q[0].Role == "admin")
                                                {
                                                    Order.lisOrder.Clear();
                                                    message = $"訂單已清空，目前訂餐清單：\n{Order.PrintOrderList()}";
                                                }
                                                else
                                                    message = $"請輸入 \"!clear yes\" 以清空訂單。";
                                                break;
                                            #endregion

                                            #region !disable 停用帳號
                                            case "!disable":
                                                if (arrayMessage.Length == 2 && q[0].Role == "admin") // 檢查格式及權限
                                                {
                                                    var u = objUserList["User"];
                                                    foreach (var i in u)
                                                    {
                                                        if (i["EmployeeID"].ToString() == arrayMessage[1])
                                                            i["Enable"] = "0";
                                                    }
                                                    string strConvert = Convert.ToString(objUserList);
                                                    System.IO.File.WriteAllText("UserList.json", strConvert);
                                                    message = $"禁用成功\n工號 {arrayMessage[1]} 已禁用";
                                                }
                                                break;
                                            #endregion

                                            #region !enable 啟用帳號
                                            case "!enable":
                                                if (arrayMessage.Length == 2 && q[0].Role == "admin") // 檢查格式及權限
                                                {
                                                    var u = objUserList["User"];
                                                    foreach (var i in u)
                                                    {
                                                        if (i["EmployeeID"].ToString() == arrayMessage[1])
                                                            i["Enable"] = "1";
                                                    }
                                                    string strConvert = Convert.ToString(objUserList);
                                                    System.IO.File.WriteAllText("UserList.json", strConvert);
                                                    message = $"啟用成功\n工號 {arrayMessage[1]} 已啟用";
                                                }
                                                break;
                                            #endregion

                                            #region !role 修改權限
                                            case "!role":
                                                if (arrayMessage.Length == 3 && q[0].Role == "admin") // 檢查格式及權限
                                                {
                                                    if (arrayMessage[1] == "11084") // 防止我自己的被改掉
                                                        message = "禁止修改管理員權限";
                                                    else
                                                    {
                                                        var u = objUserList["User"];
                                                        foreach (var i in u)
                                                        {
                                                            if (i["EmployeeID"].ToString() == arrayMessage[1])
                                                                i["Role"] = arrayMessage[2];
                                                        }
                                                        string strConvert = Convert.ToString(objUserList);
                                                        System.IO.File.WriteAllText("UserList.json", strConvert);
                                                        message = $"設定權限成功\n工號 {arrayMessage[1]} 權限已調為 {arrayMessage[2]}";
                                                    }
                                                }
                                                break;
                                            #endregion

                                            #endregion


                                            //case "!pic":
                                            //    Uri aa= "https://www.cnet.com/a/img/-qQkzFVyOPEoBRS7K5kKS0GFDvk=/940x0/2020/04/16/7d6d8ed2-e10c-4f91-b2dd-74fae951c6d8/bazaart-edit-app.jpg");
                                            //    isRock.LineBot.Bot bot = new isRock.LineBot.Bot(_lineBotConfig.Channel_access_token);
                                            //    isRock.LineBot.ImageMessage imageMessage = new isRock.LineBot.ImageMessage()

                                            //    break;

                                            default:
                                                message = strCmdFail;
                                                break;
                                        }
                                    }
                                }
                                else // 未註冊
                                {
                                    #region 未註冊
                                    if (arrayMessage[0] == "!register")
                                    {
                                        if (arrayMessage.Length == 3) // 檢查格式
                                        {
                                            var user = new User(userId, arrayMessage[1], arrayMessage[2]); // 產生 User 物件
                                            string strUser = JsonConvert.SerializeObject(user); // 序列化 User 物件
                                            JObject jobjUser = JObject.Parse(strUser); // 轉成 JObject
                                            arrayUserList.Add(jobjUser); // 加入 JObject 到 JArray
                                            string strConvert = Convert.ToString(objUserList); // 轉回 string
                                            System.IO.File.WriteAllText("UserList.json", strConvert); // 存檔

                                            message = $"註冊成功\n工號：{arrayMessage[1]}\n姓名：{arrayMessage[2]}";
                                        }
                                        else
                                            message = $"註冊失敗，請檢查格式及參數，注意參數裡不可有空格。";
                                    }
                                    else
                                        message = "尚未註冊，請輸入 \"!register 工號 姓名\" 註冊";
                                    #endregion
                                }
                                break;
                        }
                        break;
                        #endregion
                }

                return Ok(request);
            }
            catch (Exception ex)
            {
                #region 打 Error Log
                _log.LogError(Convert.ToString(ex));
                message = strCmdFail;
                #endregion

                return Ok(request);
            }
            finally
            {
                #region 發送自動訊息
                Random r = new Random();
                var replyMessage = new
                {
                    replyToken = replyToken,
                    messages = (type == "sticker") ? new object[] { new
                        {
                            type = type,
                            packageId = "789",
                            stickerId = r.Next(10855,10895).ToString()
                        } } : new object[] { new
                        {
                            type = "text",
                            text = message != null ? message : strCmdFail
                        }
                        }
                };

                string s = JsonConvert.SerializeObject(replyMessage); // 轉為 JSON 字串
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                WebClient webClient = new WebClient();
                webClient.Headers.Clear();
                webClient.Headers.Add("Content-Type", "application/json");
                webClient.Headers.Add("Authorization", "Bearer " + channelAccessToken);
                webClient.UploadData("https://api.line.me/v2/bot/message/reply", bytes);
                #endregion
            }
        }

        [Route("SendMessage")]
        [HttpGet]
        public IActionResult SendMessage(string message)
        {
            try
            {
                _log.LogInformation($"SendMessage with message:\"{message}\"");
                isRock.LineBot.Bot bot = new isRock.LineBot.Bot(_lineBotConfig.Channel_access_token);
                bot.PushMessage("U3b9c40da296eb6261877ae2281548b56", message);
                return Ok();
            }
            catch (Exception ex)
            {
                _log.LogError("SendMessage Fail，Error Message:" + ex);
                return NotFound();
            }
        }
        /// <summary>
        /// 發送 Line 訊息
        /// </summary>
        /// <param name="imgUrl"></param>
        /// <returns></returns>
        [Route("SendPhoto")]
        [HttpGet]
        public IActionResult SendPhoto(Uri imgUrl)
        {
            isRock.LineBot.Bot bot = new isRock.LineBot.Bot(_lineBotConfig.Channel_access_token);
            bot.PushMessage("U3b9c40da296eb6261877ae2281548b56", imgUrl);
            return Ok();
        }

        [Route("SendNotify")]
        [HttpGet]
        public IActionResult SendNotify(string _notify)
        {
            var ret = isRock.LineNotify.Utility.GetTokenFromCode("", "VzlPlwx33JtB3uX6uVkaYB", "pOKzaRzMTUkLVygyuet1rgVUN2xSOWmhAurlqmIgR6Y", "https://c76b48a962c3.ngrok.io");
            isRock.LineNotify.Utility.SendNotify(ret.access_token, _notify);

            return Ok();
        }
    }
}
