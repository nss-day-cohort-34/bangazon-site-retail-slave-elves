using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon.Models.OrderViewModels
{
    public class OrderCloseViewModel
    {
        public List<SelectListItem> PaymentTypes { get; set; } = new List<SelectListItem>();
        public Order Order { get; set; }
    }
}
