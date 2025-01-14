﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using Lowadi.Interface;
using Lowadi.Interface.Methods;
using Lowadi.Methods;
using Lowadi.Models;
using Lowadi.Models.Type;
using Lowadi.Others;

namespace Lowadi
{
    public class LowadiApi : ServerData, ILowadiApi
    {
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public static string UserName { get; set; }

        /// <summary>
        /// Экю
        /// </summary>
        public static int Equus { get; set; }

        /// <summary>
        /// Пропуск
        /// </summary>
        public static int Pass { get; set; }

        /// <summary>
        /// Лошади
        /// </summary>
        public IHorse Horse { get; set; }

        /// <summary>
        /// Продажа лошадей
        /// </summary>
        public IHorseSale HorseSale { get; set; }

        /// <summary>
        /// Магазин
        /// </summary>
        public IShop Shop { get; set; }

        /// <summary>
        /// Язык
        /// </summary>
        public static Server Server { get; private set; }

        private Request _request { get; set; }

        public LowadiApi(ServerType serverType = ServerType.RU)
        {
            Server = Servers.First(x => x.ServerType == serverType);
            _request = new Request();
        }

        public async Task<ErrorModels> Login(string userName, string password)
        {
            IAuth auth = new Auth(_request, Server);
            var authData = await auth.Oauth(userName, password);

            UserName = userName;

            Horse = new Horse(_request, Server.Link);
            HorseSale = new HorseSale(_request, Server.Link);
            Shop = new Shop(_request, Server);

            return JsonConvert.Deserialize<ErrorModels>(authData);
        }

        internal static void GetBalance(string page)
        {
            var doc = new HtmlParser().ParseDocument(page);
            string equus = doc.GetElementById("reserve").GetAttribute("data-amount");
            string pass = doc.GetElementById("pass").GetAttribute("data-amount");

            Equus = int.Parse(equus);
            Pass = int.Parse(pass);
        }
    }
}