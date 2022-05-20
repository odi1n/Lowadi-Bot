﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Lowadi.Interface.Methods;
using Lowadi.Models;
using Lowadi.Models.Type;
using Lowadi.Others;

namespace Lowadi.Methods
{
    public class Horse : IHorse
    {
        public ISale Sale { get; set; }
        private Request _request;
        private string _dataPageHorseInfo { get; set; }

        private const string _pageMain = "https://www.lowadi.com";
        private const string _pageFactory = _pageMain + "/elevage/chevaux/?elevage=all-horses";
        private const string _pageGetHorses = _pageMain + "/elevage/chevaux/searchHorse";

        private const string _pageHorseInfo = _pageMain + "/elevage/chevaux/cheval?id=";
        private const string _pageDoSuckle = _pageMain + "/elevage/chevaux/doSuckle";
        private const string _pageDoDrink = _pageMain + "/elevage/chevaux/doDrink";
        private const string _pageDoStroke = _pageMain + "/elevage/chevaux/doStroke";
        private const string _pageDoGroom = _pageMain + "/elevage/chevaux/doGroom";
        private const string _pageDoEatTreat = _pageMain + "/elevage/chevaux/doEatTreat";
        private const string _pageDoPlay = _pageMain + "/elevage/chevaux/doPlay";
        private const string _pageDoEat = _pageMain + "/elevage/chevaux/doEat";
        private const string _pageDoNight = _pageMain + "/elevage/chevaux/doNight";
        private const string _pageDoAge = _pageMain + "/elevage/chevaux/doAge";

        private const string _pageDoCentreMission = _pageMain + "/elevage/chevaux/doCentreMission";

        public Horse(Request request)
        {
            this._request = request;
            this.Sale = new Sale(request);
        }

        /// <summary>
        /// Получить все заводы
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<Factory>> GetFactory()
        {
            List<Factory> factories = new List<Factory>();
            using (var response = await _request.GetAsync(_pageFactory))
            {
                string content = await response.Content.ReadAsStringAsync();
                var doc = new HtmlParser().ParseDocument(content);
                foreach (var element in doc.QuerySelectorAll("a.tab-action-select.tab-action"))
                {
                    if (element.GetAttribute("href") == null)
                        continue;

                    string name = element.Text();
                    string id = element.GetAttribute("href");
                    id = Regex.Match(id, "#tab-(.*)").Groups[1].Value;

                    factories.Add(new Factory() { Name = name, Id = int.Parse(id == "all-horses" ? "0" : id), });
                }
            }

            return factories;
        }

        /// <summary>
        /// Получить всех лошадей
        /// </summary>
        /// <param name="idFactory">Номер завода</param>
        /// <returns></returns>
        public async Task<MyHorse> GetAllHorse(int idFactory)
        {
            MyHorse myHorse = new MyHorse() { Horses = new List<Horses>() };
            int pageNumber = 1;

            for (int i = 0; i < pageNumber + 1; i++)
            {
                Dictionary<string, string> param = new Dictionary<string, string>() {
                    ["go"] = "1",
                    ["startingPage"] = i.ToString(),
                    ["id"] = idFactory.ToString(),
                    ["chevalType"] = "",
                    ["chevalEspece"] = "any-all",
                    ["unicorn"] = "2",
                    ["pegasus"] = "2",
                };

                using (var response = await _request.PostAsync(_pageGetHorses, param))
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var doc = new HtmlParser().ParseDocument(content);

                    string min = "0";
                    string max = "0";
                    if (doc.QuerySelector(".pageNumbering>ul>li.page") != null)
                    {
                        min = doc.QuerySelectorAll(".pageNumbering>ul>li.page>a").First().GetAttribute("data-page");
                        max = doc.QuerySelectorAll(".pageNumbering>ul>li.page>a").Last().GetAttribute("data-page");
                    }

                    if (myHorse.Page == null)
                    {
                        pageNumber = int.Parse(max);
                        myHorse.Page = new Page() { Min = int.Parse(min), Max = int.Parse(max) };
                    }

                    foreach (IElement element in doc.QuerySelectorAll("li.damier-cell>div"))
                    {
                        string id = element.QuerySelector("div>a").GetAttribute("href");
                        id = Regex.Match(id, "id=(.*)").Groups[1].Value;
                        string name = element.QuerySelector("div>ul>li>a").Text();
                        string factory = element.QuerySelector("div>ul>li>a.affixe") == null
                            ? null
                            : element.QuerySelector("ul>li>a.affixe").Text();
                        var sleep = element.QuerySelector("div.cheval-status>span>img") != null;

                        myHorse.Horses.Add(new Horses() {
                            Id = int.Parse(id), Name = name, Factory = factory, IsSleep = sleep
                        });
                    }
                }
            }

            return myHorse;
        }

        /// <summary>
        /// Получить информацию о лошади
        /// </summary>
        /// <param name="idHorse">Id Лошади</param>
        /// <returns></returns>
        public async Task GetHorseInfo(int idHorse)
        {
            using (var response = await _request.GetAsync(_pageHorseInfo + idHorse))
                _dataPageHorseInfo = await response.Content.ReadAsStringAsync();
        }

        private Dictionary<string, string> ParsInput(string pageInfo, string key, int index = 2, int max = 15)
        {
            IHtmlDocument document = new HtmlParser().ParseDocument(pageInfo);
            Dictionary<string, string> param = new Dictionary<string, string>();

            for (int i = index; index < max; i++)
            {
                if (document.QuerySelector($"#{key}>input:nth-child({i})") == null)
                    break;

                string name = document.QuerySelector($"#{key}>input:nth-child({i})").GetAttribute("name")
                    .ToLower();
                string cleareAll = Regex.Match(name, key + "(.*)").Groups[1].Value.ToLower();

                string value1 = document.QuerySelector($"#{key}>input:nth-child({i})").GetAttribute("value")
                    .ToLower();

                param.Add(cleareAll == "" ? name : cleareAll, value1);
            }

            return param;
        }

        private Dictionary<string, string> ParsInput(string pageInfo, string id, string key, int value)
        {
            IHtmlDocument document = new HtmlParser().ParseDocument(pageInfo);
            Dictionary<string, string> param = new Dictionary<string, string>();

            if (document.GetElementById(id) == null)
                return param;

            string name = document.GetElementById(id).GetAttribute("name").ToLower();
            string cleareAll = Regex.Match(name, key + "(.*)").Groups[1].Value.ToLower();

            param.Add(cleareAll == "" ? name : cleareAll, value.ToString());
            return param;
        }

        private int ParsData(string pageInfo, string selector)
        {
            IHtmlDocument document = new HtmlParser().ParseDocument(_dataPageHorseInfo);
            if (document.QuerySelector(selector) == null)
                return 0;

            var count = document.QuerySelector(selector).Text().Replace(" ", "");
            return int.Parse(count);
        }

        /// <summary>
        /// Уход - дать молока
        /// </summary>
        /// <returns></returns>
        public async Task<ActionInfo> DoSuckle()
        {
            var param = ParsInput(_dataPageHorseInfo, "form-do-suckle");
            if (param.Count == 0)
                return null;

            using (var response = await _request.PostAsync(_pageDoSuckle, param))
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.Deserialize<ActionInfo>(content);
            }
        }

        /// <summary>
        /// Уход - Кормить
        /// </summary>
        /// <returns></returns>
        public async Task<ActionInfo> DoEat(int forageCount = 0, int oatsCount = 0)
        {
            Dictionary<string, string> param = ParsInput(_dataPageHorseInfo, "feeding");

            if (param.Count == 0)
                return null;

            if (forageCount == 0)
                forageCount = ParsData(_dataPageHorseInfo, ".section-fourrage.section-fourrage-target");
            if (oatsCount == 0)
                oatsCount = ParsData(_dataPageHorseInfo, ".section-avoine.section-avoine-target");

            var forage = ParsInput(_dataPageHorseInfo, "haySlider-sliderHidden", "feeding", forageCount);
            if (forage.Count > 0)
                param.Add(forage.First().Key, forage.First().Value);

            var oats = ParsInput(_dataPageHorseInfo, "oatsSlider-sliderHidden", "feeding", oatsCount);
            if (oats.Count > 0)
                param.Add(oats.First().Key, oats.First().Value);

            using (HttpResponseMessage response = await _request.PostAsync(_pageDoEat, param))
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.Deserialize<ActionInfo>(content);
            }
        }

        /// <summary>
        /// Уход - дать воды
        /// </summary>
        /// <returns></returns>
        public async Task<ActionInfo> DoDrink()
        {
            var param = ParsInput(_dataPageHorseInfo, "form-do-drink");
            if (param.Count == 0)
                return null;

            using (var response = await _request.PostAsync(_pageDoDrink, param))
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.Deserialize<ActionInfo>(content);
            }
        }

        /// <summary>
        /// Уход - Ласкать
        /// </summary>
        /// <returns></returns>
        public async Task<ActionInfo> DoStroke()
        {
            var param = ParsInput(_dataPageHorseInfo, "form-do-stroke");
            if (param.Count == 0)
                return null;

            using (var response = await _request.PostAsync(_pageDoStroke, param))
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.Deserialize<ActionInfo>(content);
            }
        }

        /// <summary>
        /// Уход - Чистить
        /// </summary>
        /// <returns></returns>
        public async Task<ActionInfo> DoGroom()
        {
            var param = ParsInput(_dataPageHorseInfo, "form-do-groom");
            if (param.Count == 0)
                return null;

            using (var response = await _request.PostAsync(_pageDoGroom, param))
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.Deserialize<ActionInfo>(content);
            }
        }

        /// <summary>
        /// Уход - Морковь
        /// </summary>
        /// <returns></returns>
        public async Task<ActionInfo> DoEatTreat()
        {
            var param = ParsInput(_dataPageHorseInfo, "form-do-eat-treat-carotte");
            if (param.Count == 0)
                return null;

            using (var response = await _request.PostAsync(_pageDoEatTreat, param))
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.Deserialize<ActionInfo>(content);
            }
        }

        /// <summary>
        /// Уход - Играть
        /// </summary>
        /// <returns></returns>
        public async Task<ActionInfo> DoPlay()
        {
            var param = ParsInput(_dataPageHorseInfo, "formCenterPlay");
            if (param.Count == 0)
                return null;

            using (var response = await _request.PostAsync(_pageDoPlay, param))
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.Deserialize<ActionInfo>(content);
            }
        }

        /// <summary>
        /// Уход - Отправить спать
        /// </summary>
        /// <returns></returns>
        public async Task<ActionInfo> DoNight()
        {
            var param = ParsInput(_dataPageHorseInfo, "form-do-night");
            if (param.Count == 0)
                return null;

            using (var response = await _request.PostAsync(_pageDoNight, param))
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.Deserialize<ActionInfo>(content);
            }
        }

        /// <summary>
        /// Уход - Вырастить
        /// </summary>
        /// <returns></returns>
        public async Task<RedirectInfo> DoAge()
        {
            var param = ParsInput(_dataPageHorseInfo, "age");
            if (param.Count == 0)
                return null;

            using (var response = await _request.PostAsync(_pageDoAge, param))
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.Deserialize<RedirectInfo>(content);
            }
        }

        /// <summary>
        /// Выполнить миссию
        /// </summary>
        /// <returns></returns>
        public async Task<ActionInfo> DoCentreMission(int idHorse)
        {
            Dictionary<string, string> param = new Dictionary<string, string>() { ["id"] = idHorse.ToString() };

            using (var response = await _request.PostAsync(_pageDoCentreMission, param))
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.Deserialize<ActionInfo>(content);
            }
        }

        /// <summary>
        /// Выполнить прогулки
        /// </summary>
        /// <param name="walk">Тип прогулки</param>
        /// <param name="value">На сколько сделать прогулку</param>
        /// <returns></returns>
        public async Task<ActionInfo> DoWalk(Walk walk, int value = 1)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            if (walk == Walk.Foret)
            {
                param = ParsInput(_dataPageHorseInfo, "formbaladeForet");
                if (param.Count == 0)
                    return null;

                var parsInput = ParsInput(_dataPageHorseInfo, "walkforetSlider-sliderHidden", "formbaladeForet", value);
                param.Add(parsInput.First().Key, parsInput.First().Value);
            }
            else if (walk == Walk.Montagne)
            {
                param = ParsInput(_dataPageHorseInfo, "formbaladeMontagne");
                if (param.Count == 0)
                    return null;

                var parsInput = ParsInput(_dataPageHorseInfo, "walkmontagneSlider-sliderHidden", "formbaladeMontagne",
                    value);
                param.Add(parsInput.First().Key, parsInput.First().Value);
            }

            using (HttpResponseMessage response = await _request.PostAsync(_pageDoCentreMission, param))
            {
                string content = await response.Content.ReadAsStringAsync();
                return JsonConvert.Deserialize<ActionInfo>(content);
            }
        }
    }
}