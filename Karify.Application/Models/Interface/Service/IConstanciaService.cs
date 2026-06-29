using Karify.Application.Models.Karify.EnviarConstancia;
using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Application.Models.Interface.Service
{
    public interface IConstanciaService
    {
        Constancia GenerarConstancia(ObtenerDatosConstancia request);
    }
}
