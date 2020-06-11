using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaulosAutoWithAPI.Models
{
    public class Cliente
    {
        public int id { get; set; }
        public string email { get; set; }
        public string contaCliente { get; set; }
        public int numeroCliente { get; set; }
        public string[] permissoes { get; set; }
        public string[] equipamentos { get; set; }
    }
}
