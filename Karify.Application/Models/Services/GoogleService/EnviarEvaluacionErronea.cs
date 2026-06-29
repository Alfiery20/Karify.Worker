using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Application.Models.Services.GoogleService
{
    public class EnviarEvaluacionErronea
    {
        public string NombreAlumno { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string NombreProyecto { get; set; }
        public string DescripcionProyecto { get; set; }
        public string NombreProyectoResultado { get; set; }
        public string DOI { get; set; }
        public double PorcentajeSimilitud { get; set; }
        public string CorreoAlumno { get; set; }
    }
}
