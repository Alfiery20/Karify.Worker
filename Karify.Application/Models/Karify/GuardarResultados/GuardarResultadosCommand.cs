using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Application.Models.Karify.GuardarResultados
{
    public class GuardarResultadosCommand
    {
        public int IdProyecto { get; set; }
        public string DOI { get; set; }
        public double PorcentajeSimilitud { get; set; }
    }
}
