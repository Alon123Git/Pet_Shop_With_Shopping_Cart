using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        [Required]
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        [ValidateNever]
        public OrderHeader orderHeader { get; set; }
        [Required]
        public int AnimalId { get; set; }
        [ForeignKey("AnimalId")]
        [ValidateNever]
        public Animal animal { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}