﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyBook.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Category Name")]
        [MaxLength(30)]
        public string? Name { get; set; }
        [Display(Name = "Display Order")]
        [Range(1 , 100 , ErrorMessage = "Must Enter Number Between 1 And 100")]
        public int DisplayOrder { get; set; }
    }
}
