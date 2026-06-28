using Karify.Application.Models.Interface.Service;

namespace Karify.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IProyectoService _proyectoService;

        public Worker(ILogger<Worker> logger, IProyectoService proyectoService)
        {
            this._logger = logger;
            this._proyectoService = proyectoService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int TiempoMinutos = 1*60* 1000; // 5 minutos en milisegundos
            while (!stoppingToken.IsCancellationRequested)
            {
                await this._proyectoService.Execute();
                await Task.Delay(TiempoMinutos, stoppingToken);
            }
        }
    }
}
