using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PaulosAutoWithAPI.Models
{
    public class Equipamento
    {
        public string brand { get; set; }
        public string model { get; set; }
        public string serialNumber { get; set; }
        public string plate { get; set; }
        public float currentHours { get; set; }
        public string image { get; set; }
    }
}

