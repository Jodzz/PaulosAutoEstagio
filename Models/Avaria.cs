using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PaulosAutoWithAPI.Models
{
    public class Avaria
    {
        public string email { get; set; }
        public string cliente { get; set; }
        public DateTime dataRegisto { get; set; }
        public string descricao { get; set; }
        public string[] imagens { get; set; }
        public string[] videos { get; set; }
    }
}
