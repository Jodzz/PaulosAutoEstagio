using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace PaulosAutoWithAPI.Models
{
    public class Faturacao
    {
        public List<Fatura> listaFaturas { get; set; }
        public List<Documento> listaDocumentos { get; set; }
    }
}