﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lowadi;
using Lowadi.Interface;
using Lowadi.Interface.Methods;
using Lowadi.Methods;
using Lowadi.Models;
using Lowadi.Models.Ksk;
using Lowadi.Models.Shop;
using Lowadi.Models.Type;
using Lowadi.Models.Type.Shops;

namespace Lowadi_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Work();
            Console.ReadKey();
        }

        static async void Work()
        {
            var openData = File.ReadAllText("data.txt").Split(':');

            ILowadiApi lowApi = new LowadiApi();
            var isLogin = await lowApi.Login(openData[0], openData[1]);
            if (isLogin.Errors.Count > 0)
            {
                Console.WriteLine("не удалось авторизоваться");
                return;
            }

            // var data = await lowApi.Shop.GetInformation(new List<ItemsType>() {
            //     ItemsType.CompoundFeed,
            //     ItemsType.Apple,
            //     ItemsType.SeedsPass,
            //     ItemsType.Carrot,
            //     ItemsType.Fertilizer_1,
            //     ItemsType.Fertilizer_2
            // });

            // var buy = await lowApi.Shop.Buy(new ShopData() { Id = ItemsType.Apple, Nombre = 1, });
            // var sell = await lowApi.Shop.Sale(new ShopData() { Id = ItemsType.Apple, Nombre = 15, });
            var info = await lowApi.Shop.GetInformation(new List<ItemsType>() {
                ItemsType.Apple,
                ItemsType.Carrot,
                ItemsType.Fertilizer_1,
                ItemsType.Fertilizer_2,
                ItemsType.Flax,
                ItemsType.Forage,
                ItemsType.Iron,
                ItemsType.Leather,
                ItemsType.Manure,
                ItemsType.Oats,
                ItemsType.Sand,
                ItemsType.Straw,
                ItemsType.Wheat,
                ItemsType.Wood,
                ItemsType.CompoundFeed,
                ItemsType.SeedsAlfalfa,
                ItemsType.SeedsApple,
                ItemsType.SeedsCarrot,
                ItemsType.SeedsFlax,
                ItemsType.SeedsOat,
                ItemsType.SeedsPass,
                ItemsType.SeedsWheat
            });
            var ss = info.ToList();


            var buy_info = await lowApi.Shop.Buy(new ShopData() { Id = ItemsType.Iron, Nombre = 1 });

            // for (int i = 0; i < 30; i++)
            // {
            //     var data = await lowApi.Horse.Sale.GetHorses();
            //     if(data.Count == 0)
            //         break;
            //
            //     foreach (var horse in data)
            //     {
            //         var test = await lowApi.Horse.Sale.DoAcheter(horse.LinkBuy);
            //         if (test.Error.Errors != null)
            //             break;
            //         LowadiApi.Equus -= horse.Price;
            //         Console.WriteLine(LowadiApi.Equus);
            //     }
            // }

            // var factories = await lowApi.Horse.GetFactory();
            // var page = await lowApi.Horse.GetHorse(factories.ToList()[0].Id);

            // var horse = page.Horses.First(x=>x.Id == 52763688);
            // await lowApi.Horse.GetHorseInfo(68512390);
            // var doSuckle = await lowApi.Horse.DoSuckle();
            // doSuckle = await lowApi.Horse.DoCentreMission(56381122);
            // doSuckle = await lowApi.Horse.DoEat();
            // doSuckle = await lowApi.Horse.DoDrink();
            // doSuckle = await lowApi.Horse.DoStroke();
            // doSuckle = await lowApi.Horse.DoGroom();
            // doSuckle = await lowApi.Horse.DoEatTreat();
            // doSuckle = await lowApi.Horse.DoNight();
            // doSuckle = await lowApi.Horse.DoAge();
            // var trainig = await lowApi.Horse.DoTraining(TrainingType.Endurance, value: 2);

            // var ksk = await lowApi.Horse.Ksk.CentreInscription(new Inscription() {
            // Fourrage = false,
            // }, 68589785);
            // var testx = await lowApi.Horse.Ksk.DoCentreInscription();

            Console.WriteLine("OK");
        }
    }
}