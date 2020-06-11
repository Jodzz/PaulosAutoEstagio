using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PaulosAutoWithAPI.Models
{
    public class Documento
    {
        public string diarioObra { get; set; }
        public int numeroObra { get; set; }
        public int ano { get; set; }
        public string diario { get; set; }
        public int numero { get; set; }
        public string fileName { get; set; }
        public string pathToDoc { get; set; }
    }
}
