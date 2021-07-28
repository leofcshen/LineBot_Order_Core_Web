using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LineBot_Order_Core_Web.Models
{
    public class LocalHusband
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string name { get; set; }
        public List<HasMenuItem> hasMenuItem { get; set; }
                
        /// <summary>
        /// 判斷抓本日菜單關鍵字用的字典
        /// </summary>
        public static readonly Dictionary<string, string> _dWeekdayToChinese = new Dictionary<string, string>
        {
            { "Monday", "週一菜單" },
            { "Tuesday", "週二菜單" },
            { "Wednesday", "週三菜單" },
            { "Thusday", "週四菜單" },
            { "Friady", "週五菜單" },
        };
        /// <summary>
        /// 抓本日菜單更新 todayMenu
        /// </summary>        
        public static bool UpdateTodayMenu()
        {
            var date = (int)DateTime.Now.DayOfWeek;

            if (date > 0 && date < 6) // 一到五才有菜單
            {
                HtmlWeb webClient = new HtmlWeb();
                HtmlDocument doc = webClient.Load("https://localhusband.oddle.me/zh_TW/"); //載入網址資料
                var node = doc.DocumentNode.SelectSingleNode("//script[contains(.,'\"@context\"')]"); // 抓節點
                JObject a = JObject.Parse(node.InnerText);
                var b = a["hasMenu"]["hasMenuSection"];
                //var lisLocalHusband = JsonConvert.DeserializeObject<List<LocalHusband>>(b.ToString());
                var lisLocalHusband = JsonConvert.DeserializeObject<List<LocalHusband>>(b.ToString());

                string weekDay = DateTime.Now.DayOfWeek.ToString();
                string weekDayChinese = (_dWeekdayToChinese.ContainsKey(weekDay)) ? _dWeekdayToChinese[weekDay] : string.Empty;

                foreach (var menu in lisLocalHusband)
                {
                    if (menu.name.Contains(weekDayChinese)) // 抓出今天的菜單
                    {                        
                        Menu.todayMenu.Clear(); // 濾掉筷子、素食、輕食沙拉
                        int count = 1;
                        foreach (var item in menu.hasMenuItem)
                        {
                            if (item.name != "筷子" && !item.name.Contains("素食餐盒") && !item.name.Contains("輕食沙拉"))
                            {
                                Menu m = new Menu(count, item);                                
                                count++;
                                Menu.todayMenu.Add(m);
                            }
                        }
                        break;
                    }
                }
                return true;
            }
            else
                return false;
        }
        
    }
    public class Offers
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public int price { get; set; }
        public string priceCurrency { get; set; }
    }

    public class HasMenuItem
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Offers offers { get; set; }
        // 放過濾後的菜單
        
    }

    public class Menu
    {
        /// <summary>
        /// 品項索引
        /// </summary>
        public int Number { get; set; }
        public HasMenuItem hasMenuItem { get; set; }
        public Menu(int _number, HasMenuItem _hasMenuItem)
        {
            Number = _number;
            hasMenuItem = _hasMenuItem;
        }
        public static List<Menu> todayMenu { get; set; } = new List<Menu>();
        /// <summary>
        /// 本日菜單字串
        /// </summary>        
        public static string GetTodayMenuList()
        {
            string menuList = string.Empty;
            foreach (var meal in Menu.todayMenu)
                menuList += $"{meal.Number}.{meal.hasMenuItem.name}_{meal.hasMenuItem.offers.price}\n";
            return menuList;
        }
    }
}
