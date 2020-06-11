using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace PaulosAutoWithAPI.Models
{
    public class DetalhesEquipamento
    {
        public Equipamento equipamento { get; set; }

        public List<Avaria> listaAvarias { get; set; }

        public List<Intervenções> listaIntervencoes { get; set; }

        public List<Horas> listaHoras { get; set; }

        public List<DateTime> listaDates { get; set; }

        public int resultadoPost { get; set; }
        public bool acessoHoras { get; set; }
        public bool acessoHist { get; set; }
    }
}
