﻿using System.Collections.Generic;

namespace Lowadi.Models
{
    public class MyHorse
    {
        public IList<Horses> Horses { get; set; }
        /// <summary>
        /// Страницы
        /// </summary>
        public Page Page { get; set; }
        public int Count { get; set; }
    }
}