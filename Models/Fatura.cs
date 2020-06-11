using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PaulosAutoWithAPI.Models
{
    public class Fatura
    {
        public string diario { get; set; }
        public int ano { get; set; }
        public int number { get; set; }
        public int docNumber { get; set; }
        public DateTime issueDate { get; set; }
        public DateTime dueDate { get; set; }
        public float totalAmount { get; set; }
        public float paidAmount { get; set; }
        public string docURL { get; set; }
}
}