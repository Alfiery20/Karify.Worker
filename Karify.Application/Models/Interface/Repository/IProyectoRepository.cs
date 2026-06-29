using Karify.Application.Models.Karify.GuardarResultados;
using Karify.Application.Models.Karify.ObtenerTesis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Application.Models.Interface.Repository
{
    public interface IProyectoRepository
    {
        Task<IEnumerable<ObtenerTesisResponse>> GetProyectoPorRevision();
        Task<GuardarResultadosResponse> GuardarResultadoSimilitud(GuardarResultadosCommand command);
    }
}
