using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karify.Application.Models.Karify.EnviarConstancia
{
    public class ObtenerDatosConstancia
    {
        public int Id { get; set; }
        public string NombreProyecto { get; set; } = string.Empty;
        public List<EnviarConstanciaAlumno> NombresAlumnos { get; set; } = [];
        public string ProfesorAsesor { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string CodigoConstancia { get; set; } = Guid.NewGuid().ToString("N").ToUpper()[..12];
    }
}
