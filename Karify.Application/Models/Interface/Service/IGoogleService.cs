using Karify.Application.Models.Services.GoogleService;
using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Application.Models.Interface.Service
{
    public interface IGoogleService
    {
        Task EnvioSolicitudAprobacion(EnviarEvaluacionExitosa envioCorreo);

        Task EnvioSolicitudRechazado(EnviarEvaluacionErronea envioCorreo);
    }
}
