using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Flower
    {
        [Key] // primary key
        public int FlowerID { get; set; }
        public string FlowerName { get; set; }

        [Required(ErrorMessage = "Please key in your Flower Produced Date!")]
        [DataType(DataType.Date)]
        [Display(Name = "Flower Produce Date")]
        public DateTime FlowerProducedDate { get; set; }

        [Required(ErrorMessage = "Please select your Flower Type!")]
        [Display(Name = "Flower Type")]
        public string FlowerType { get; set; }

        [Required(ErrorMessage = "Please key in your Flower Price!")]
        [Display(Name = "Flower Price")]
        [Range(1, 1000, ErrorMessage = "Please select the amount between 1 to 1000")]
        public decimal FlowerPrice { get; set; }

        [Display(Name = "Flower Image")]
        public string FlowerURL { get; set; }

        public string FlowerS3Key { get; set; }
    }
}
