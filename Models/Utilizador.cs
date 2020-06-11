using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaulosAutoWithAPI.Models
{
    public class Utilizador
    {
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string[] roles { get; set; }
        public string[] equipamentos { get; set; }
        public string token { get; set; }
    }
}
