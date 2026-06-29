using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karify.Application.Models.Karify.EnviarConstancia
{
    public class GuardarConstancia
    {
        public int IdProyecto { get; set; }
        public string NombreConstancia { get; set; }
        public string Base64 { get; set; }
        public string Guid { get; set; }
    }
}
