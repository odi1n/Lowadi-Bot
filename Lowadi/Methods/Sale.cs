﻿using System;
using System;
using System.Text;
using System.Web;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Lowadi.Models;
using Lowadi.Models.Type;
using Lowadi.Others;
using Newtonsoft.Json;
using JsonConvert = Lowadi.Others.JsonConvert;

namespace Lowadi.Methods
{
    public class Sale
    {
        private Request _request;

        private const string _pageLink = "https://www.lowadi.com/marche/vente/index";
        private const string _pageBuy = "https://www.lowadi.com/marche/vente/prive/doAcheter";

        public Sale(Request request)
        {
            this._request = request;
        }

        public async Task<List<Corrals>> GetHorses(TypeSale typeSale = TypeSale.Reserved, int page = 0)
        {
            string prive = "";
            if (typeSale == TypeSale.Auctions) prive = "enchere";
            if (typeSale == TypeSale.Straight) prive = "modificationDate";
            if (typeSale == TypeSale.Reserved) prive = "prive";

            Dictionary<string, string> param = new Dictionary<string, string>()
            {
                ["type"] = prive,
                ["tri"] = "modificationDate",
                ["sens"] = "DESC",
                ["page"] = page.ToString(),
            };

            using (var response = await _request.GetAsync(_pageLink, param))
            {
                string content = await response.Content.ReadAsStringAsync();
                LowadiApi.GetBalance(content);
                return ParseHorse(content);
            }
        }

        private List<Corrals> ParseHorse(string pageData)
        {
            var doc = new HtmlParser().ParseDocument(pageData);
            List<Corrals> corralsList = new List<Corrals>();

            foreach (var element in doc.Body.QuerySelectorAll("#vente-chevaux>table>tbody>tr"))
            {
                var sex = element.QuerySelector("td:nth-child(5)>table>tbody>tr:nth-child(1)>td>img")
                    .GetAttribute("alt");
                var name = element.QuerySelector("td:nth-child(5)>table>tbody>tr:nth-child(2)>td>a").Text();
                var skills = element.QuerySelector("td:nth-child(7)>span>span>span:nth-child(1)>div").Text();
                var genetics = element.QuerySelector("td:nth-child(7)>span>span>span:nth-child(2)>span").Text();
                var date = element.QuerySelector("td:nth-child(9)>small").Text();
                var price = element.QuerySelector("td:nth-child(10)>div>div>div>span>strong").Text();
                string linkBuy = element.QuerySelector("td:nth-child(10)>div>div>script").Text();
                linkBuy = Regex.Match(linkBuy, "'params': '(.*?)'}").Groups[1].ToString();

                corralsList.Add(new Corrals()
                {
                    Sex = sex == "male" ? Sex.Male : Sex.Male,
                    Name = name,
                    Skills = Int32.Parse(skills),
                    Genetics = Int32.Parse(genetics),
                    Date = DateTime.Parse(date),
                    Price = Int32.Parse(price),
                    LinkBuy = HttpUtility.UrlDecode(linkBuy)
                });
            }

            return corralsList;
        }

        public async Task<BuyHorses> Buy(string linkBuy)
        {
            using (var response = await _request.PostAsync(_pageBuy, linkBuy))
            {
                string content = await response.Content.ReadAsStringAsync();
                return new BuyHorses()
                {
                    Buy = JsonConvert.Deserialize<BuyH>(content),
                    Error = JsonConvert.Deserialize<ErrorModels>(content),
                };
            }
        }

        public class BuyHorses
        {
            public BuyH Buy { get; set; }
            public ErrorModels Error { get; set; }
        }

        public partial class BuyH
        {
            [JsonProperty("externalRedirection")] public long ExternalRedirection { get; set; }

            [JsonProperty("redirection")] public string Redirection { get; set; }
        }

        public enum TypeSale
        {
            Auctions,
            Straight,
            Reserved,
        }
    }
}