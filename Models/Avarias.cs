using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaulosAutoWithAPI.Models
{
    public class Avarias
    {
        public Dictionary<Avaria,string> listTodasAvarias { get; set; }

        public List<DateTime> listDates { get; set; }
    }
}
