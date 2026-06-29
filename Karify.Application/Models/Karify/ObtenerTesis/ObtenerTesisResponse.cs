using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Application.Models.Karify.ObtenerTesis
{
    public class ObtenerTesisResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int IdEscuela { get; set; }
        public string Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string NombreAlumno { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Correo { get; set; }
    }
}
