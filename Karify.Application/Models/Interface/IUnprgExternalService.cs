using Karify.Application.Models.UNPRG;
using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Application.Models.Interface
{
    public interface IUnprgExternalService
    {
        Task<List<Tesis>> ObtenerTesisGeneral();
        Task<List<Tesis>> ObtenerTesisFacultad(int idFacultad);
        Task<List<Tesis>> ObtenerTesisEscuela(int idEscuela); 
        Task<Tesis> ObtenerTesis(int idTesis);
    }
}
