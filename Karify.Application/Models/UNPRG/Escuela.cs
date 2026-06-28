using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Application.Models.UNPRG
{
    public class Escuela
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public Facultad Facultad { get; set; } = new();
    }
}
