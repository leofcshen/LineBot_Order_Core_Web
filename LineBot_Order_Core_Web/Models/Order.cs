using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LineBot_Order_Core_Web.Models
{
    public class Order
    {
        public enum Rice
        {
            白飯 = 1,
            紫米 = 2
        }

        public enum RiceQuantity
        {
            正常 = 1,
            減半 = 2
        }
        /// <summary>
        /// 點餐人
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 餐點
        /// </summary>
        public string meal { get; set; }
        /// <summary>
        /// 飯類
        /// </summary>
        public Rice rice { get; set; }
        /// <summary>
        /// 飯量
        /// </summary>
        public RiceQuantity riceQuantity { get; set; }
        
        public int Price { get; set; }
        /// <summary>
        /// 訂餐清單
        /// </summary>
        public static List<Order> lisOrder = new List<Order>();
        public Order() { }
        public Order(string _name, string _meal, Rice _rice, RiceQuantity _riceQuantity, int _price)
        {
            name = _name;
            meal = _meal;
            rice = _rice;
            riceQuantity = _riceQuantity;
            Price = _price;

            lisOrder.Add(this);
        }
        /// <summary>
        /// 開團狀態
        /// </summary>
        public static bool isStart { get; set; } = false;

        /// <summary>
        /// 回傳訂餐清單
        /// </summary>
        /// <returns></returns>
        public static string PrintOrderList()
        {
            if (lisOrder.Count() == 0)
                return "訂餐清單為空。";

            string str = string.Empty;
            foreach (var item in lisOrder)
                str += $"{item.name}：{item.meal}_{item.rice}_{item.riceQuantity}\n";

            return str;            
        }
    }
}
