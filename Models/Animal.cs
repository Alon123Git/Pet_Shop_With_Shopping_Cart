using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Animal
    {
        [Key]
        [HiddenInput]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Animal name")]
        public string AnimalName { get; set; }
        [Required]
        public int Age { get; set; }
        public string ShortDescription { get; set; }
        [Display(Name = "Image")]
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal PriceBeforeDiscount { get; set; }
        
    }
}