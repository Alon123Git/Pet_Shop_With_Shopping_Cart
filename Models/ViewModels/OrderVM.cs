using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ViewModels
{
    public class OrderVM
    {
        public IEnumerable<OrderHeader> orderHeader { get; set; }
        public OrderHeader orderHeaderSingle { get; set; }
        public IEnumerable<OrderDetail> orderDetail { get; set; }
    }
}