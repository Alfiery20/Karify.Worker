using Karify.Application.Models.Interface.Repository;
using Karify.Application.Models.Interface.Service;
using Microsoft.Extensions.Logging;

namespace Karify.Application.Models.Services
{
    public class ProyectoService : IProyectoService
    {
        private readonly IProyectoRepository _proyectoRepository;
        private readonly ILogger _logger;

        public ProyectoService(
            ILogger<ProyectoService> logger,
            IProyectoRepository proyectoRepository)
        {
            this._logger = logger;
            this._proyectoRepository = proyectoRepository;
        }

        public async Task<bool> Execute()
        {
            this._logger.LogInformation("Prueba de funcionamiento");
            return true;
        }
    }
}
