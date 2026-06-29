using Karify.Application.Models.Interface;
using Karify.Application.Models.Interface.Repository;
using Karify.Application.Models.Interface.Service;
using Karify.Application.Models.Karify.EnviarConstancia;
using Karify.Application.Models.Karify.GuardarResultados;
using Karify.Application.Models.Karify.ObtenerTesis;
using Karify.Application.Models.Services.GoogleService;
using Karify.Application.Models.UNPRG;
using Microsoft.Extensions.Logging;

namespace Karify.Application.Models.Services
{
    public class ProyectoService : IProyectoService
    {
        private readonly IProyectoRepository _proyectoRepository;
        private readonly IUnprgExternalService _unprgExternalService;
        private readonly IGoogleService _googleService;
        private readonly IConstanciaService _constanciaService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger<ProyectoService> _logger;

        private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
        {
            "los", "las", "una", "uno", "para", "por", "con", "que", "del",
            "este", "esta", "estos", "estas", "son", "fue", "han", "mas",
            "como", "pero", "tambien", "sobre", "entre", "sistema", "usando",
            "utilizando", "implementacion", "presenta", "trabajo", "desarrollo"
        };

        public ProyectoService(
            ILogger<ProyectoService> logger,
            IProyectoRepository proyectoRepository,
            IUnprgExternalService unprgExternalService,
            IGoogleService googleService,
            IConstanciaService constanciaService,
            IDateTimeService dateTimeService)
        {
            this._logger = logger;
            this._proyectoRepository = proyectoRepository;
            this._unprgExternalService = unprgExternalService;
            this._googleService = googleService;
            this._constanciaService = constanciaService;
            this._dateTimeService = dateTimeService;
        }

        public async Task<bool> Execute()
        {
            var proyectos = (await _proyectoRepository.GetProyectoPorRevision()).ToList();

            _logger.LogInformation("Se obtuvieron {Count} proyectos en revisión", proyectos.Count);

            if (proyectos.Count > 0)
                await CompararResultados(proyectos);

            return true;
        }

        private async Task CompararResultados(List<ObtenerTesisResponse> proyectos)
        {
            foreach (var proyecto in proyectos)
            {
                _logger.LogInformation("Iniciando comparación para proyecto: {Nombre}", proyecto.Nombre);

                var tesisRepositorio = await _unprgExternalService.ObtenerTesisEscuela(proyecto.IdEscuela);

                if (tesisRepositorio.Count == 0)
                {
                    _logger.LogWarning("Sin tesis en repositorio para escuela {IdEscuela}", proyecto.IdEscuela);
                    continue;
                }

                var (mejorTesis, porcentaje) = ObtenerMejorCoincidencia(proyecto, tesisRepositorio);

                _logger.LogInformation(
                    "Proyecto: {Nombre} | Tesis similar: {TesisId} - '{TesisTitulo}' | Similitud: {Porcentaje}%",
                    proyecto.Nombre, mejorTesis.Id, mejorTesis.Titulo ?? "N/A", porcentaje);

                var resultado = await _proyectoRepository.GuardarResultadoSimilitud(new GuardarResultadosCommand
                {
                    IdProyecto = proyecto.Id,
                    PorcentajeSimilitud = porcentaje,
                    FechaProcesamiento = this._dateTimeService.HoraLocal(),
                    DOI = mejorTesis.Doi ?? string.Empty,
                });

                if (resultado.Mensaje.Equals("OK"))
                    await EnviarCorreo(porcentaje, proyecto, mejorTesis);
            }
        }

        private (Tesis mejorTesis, double porcentaje) ObtenerMejorCoincidencia(
            ObtenerTesisResponse proyecto,
            List<Tesis> tesisRepositorio)
        {
            var inputTokens = Tokenizar($"{proyecto.Nombre} {proyecto.Nombre} {proyecto.Descripcion}");

            var corpus = new List<List<string>> { inputTokens };
            corpus.AddRange(tesisRepositorio.Select(t =>
                Tokenizar($"{t.Titulo} {t.Titulo} {t.Resumen} {string.Join(" ", t.PalabrasClave)}")));

            var vectores = CalcularTfIdf(corpus);
            var vectorInput = vectores[0];

            Tesis mejorTesis = new();
            double mejorScore = -1;

            for (int i = 0; i < tesisRepositorio.Count; i++)
            {
                double score = CosineSimilitud(vectorInput, vectores[i + 1]);
                if (score > mejorScore)
                {
                    mejorScore = score;
                    mejorTesis = tesisRepositorio[i];
                }
            }

            return (mejorTesis, Math.Round(mejorScore * 100, 2));
        }

        private List<string> Tokenizar(string texto)
        {
            var normalizado = texto
                .ToLowerInvariant()
                .Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => char.IsLetterOrDigit(c) || c == ' ')
                .Aggregate(new System.Text.StringBuilder(), (sb, c) => sb.Append(c))
                .ToString();

            return normalizado
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length > 2 && !Stopwords.Contains(t))
                .ToList();
        }

        private static List<Dictionary<string, double>> CalcularTfIdf(List<List<string>> corpus)
        {
            int N = corpus.Count;

            var tfs = corpus.Select(tokens =>
            {
                var freq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var t in tokens)
                    freq[t] = freq.GetValueOrDefault(t) + 1;
                return freq;
            }).ToList();

            var df = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var freq in tfs)
                foreach (var term in freq.Keys)
                    df[term] = df.GetValueOrDefault(term) + 1;

            return tfs.Select(freq =>
            {
                int total = freq.Values.Sum();
                return freq.ToDictionary(
                    kvp => kvp.Key,
                    kvp =>
                    {
                        double tf = (double)kvp.Value / total;
                        double idf = Math.Log((N + 1.0) / (df.GetValueOrDefault(kvp.Key) + 1.0));
                        return tf * idf;
                    },
                    StringComparer.OrdinalIgnoreCase);
            }).ToList();
        }

        private static double CosineSimilitud(Dictionary<string, double> a, Dictionary<string, double> b)
        {
            double dot = 0, magA = 0, magB = 0;

            foreach (var (term, valA) in a)
            {
                dot += valA * b.GetValueOrDefault(term);
                magA += valA * valA;
            }

            foreach (var valB in b.Values)
                magB += valB * valB;

            return (magA == 0 || magB == 0) ? 0 : dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
        }

        private async Task EnviarCorreo(double porcentaje, ObtenerTesisResponse proyecto, Tesis mejorTesis)
        {
            if (porcentaje >= 40)
            {
                await _googleService.EnvioSolicitudRechazado(new EnviarEvaluacionErronea
                {
                    NombreAlumno = proyecto.NombreAlumno,
                    ApellidoPaterno = proyecto.ApellidoPaterno,
                    ApellidoMaterno = proyecto.ApellidoMaterno,
                    CorreoAlumno = proyecto.Correo,
                    NombreProyecto = proyecto.Nombre,
                    DescripcionProyecto = proyecto.Descripcion,
                    DOI = mejorTesis.Doi ?? string.Empty,
                    NombreProyectoResultado = mejorTesis.Titulo ?? string.Empty,
                    PorcentajeSimilitud = porcentaje,
                });
            }
            else
            {
                var generarConstancia = await this.GenerarConstancia(new EnviarConstanciaCommand()
                {
                    IdProyecto = proyecto.Id
                });

                await _googleService.EnvioSolicitudAprobacion(new EnviarEvaluacionExitosa
                {
                    NombreAlumno = proyecto.NombreAlumno,
                    ApellidoPaterno = proyecto.ApellidoPaterno,
                    ApellidoMaterno = proyecto.ApellidoMaterno,
                    CorreoAlumno = proyecto.Correo,
                    NombreProyecto = proyecto.Nombre,
                    DescripcionProyecto = proyecto.Descripcion,
                    NombreArchivoPdf = generarConstancia.NombreArchivo,
                    PdfBase64 = generarConstancia.Base64
                });
            }
        }

        private async Task<Constancia> GenerarConstancia(EnviarConstanciaCommand request)
        {
            this._logger.LogInformation("Iniciando handler de envio de constancia");
            var datosProyecto = await this._proyectoRepository.ObtenerDatosConstancia(request);
            datosProyecto.NombresAlumnos = (await this._proyectoRepository.ObtenerAlumnosPorProyecto(request.IdProyecto)).ToList();
            var constancia = this._constanciaService.GenerarConstancia(datosProyecto);
            var response = await this._proyectoRepository.GuardarConstancia(new GuardarConstancia()
            {
                IdProyecto = request.IdProyecto,
                NombreConstancia = constancia.NombreArchivo,
                Base64 = constancia.Base64,
                Guid = datosProyecto.CodigoConstancia
            });
            this._logger.LogInformation("Finalizando handler de envio de constancia");
            return constancia;
        }
    }
}