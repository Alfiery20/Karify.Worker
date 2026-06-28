using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Application.Models.Karify
{
    public class ObtenerTesisResponse
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int IdEscuela { get; set; }
        public string Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
