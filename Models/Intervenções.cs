using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaulosAutoWithAPI.Models
{
    public class Intervenções
    {
        public List<Intervenção> intervencoes { get; set; }
        public string[] descricaoIntervencao { get; set; }
        public bool finalizada { get; set; }
    }
}
