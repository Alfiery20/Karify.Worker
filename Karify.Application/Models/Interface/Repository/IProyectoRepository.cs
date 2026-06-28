using Karify.Application.Models.Karify;
using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Application.Models.Interface.Repository
{
    public interface IProyectoRepository
    {
        Task<IEnumerable<ObtenerTesisResponse>> GetProyectoPorRevision();
    }
}
