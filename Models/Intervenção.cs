using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PaulosAutoWithAPI.Models
{
    public class Intervenção
    {
        public DateTime data { get; set; }
        public float horas { get; set; }
    }
}
