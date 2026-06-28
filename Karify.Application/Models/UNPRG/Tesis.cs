using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Application.Models.UNPRG
{
    public class Tesis
    {
        public int Id { get; set; }
        public string Doi { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Resumen { get; set; } = string.Empty;
        public DateTime FechaPublicacion { get; set; }
        public string Idioma { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
        public string UrlPdf { get; set; } = string.Empty;
        public string TipoAcceso { get; set; } = string.Empty;
        public string TipoTesis { get; set; } = string.Empty;
        public string AreaOcde { get; set; } = string.Empty;

        public List<string> PalabrasClave { get; set; } = new();

        public Escuela Escuela { get; set; } = new();

        public List<Autor> Autores { get; set; } = new();
    }
}
