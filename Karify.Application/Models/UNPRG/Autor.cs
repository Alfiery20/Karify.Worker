using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Application.Models.UNPRG
{
    public class Autor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;

        public string NombreCompleto =>
            $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}";
    }
}
