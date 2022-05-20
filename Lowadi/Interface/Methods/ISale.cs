﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Lowadi.Models;
using Lowadi.Models.Type;

namespace Lowadi.Interface.Methods
{
    public interface ISale
    {
        Task<ICollection<Corrals>> GetHorses(TypeSale typeSale = TypeSale.Reserved, int page = 0);
        Task<BuyHorse> Buy(string linkBuy);
    }
}