using Karify.Application.Models.Interface;
using Karify.Application.Models.Interface.Repository;
using Karify.Application.Models.Interface.Service;
using Karify.Application.Models.Karify;
using Microsoft.Extensions.Logging;

namespace Karify.Application.Models.Services
{
    public class ProyectoService : IProyectoService
    {
        private readonly IProyectoRepository _proyectoRepository;
        private readonly IUnprgExternalService _unprgExternalService;
        private readonly ILogger _logger;

        public ProyectoService(
            ILogger<ProyectoService> logger,
            IProyectoRepository proyectoRepository,
            IUnprgExternalService unprgExternalService)
        {
            this._logger = logger;
            this._proyectoRepository = proyectoRepository;
            this._unprgExternalService = unprgExternalService;
        }

        public async Task<bool> Execute()
        {
            this._logger.LogInformation("Prueba de funcionamiento");
            var response = await this._proyectoRepository.GetProyectoPorRevision();
            if (response.Count() > 0)
            {
                this.CompararResultados(response.ToList());
            }
            this._logger.LogInformation($"Se obtuvo {response.Count()} proyectos en revisión");
            return true;
        }

        public async Task CompararResultados(List<ObtenerTesisResponse> proyectos)
        {
            foreach (var proyecto in proyectos)
            {
                this._logger.LogInformation("Iniciando comparación para proyecto: {Nombre}", proyecto.Nombre);

                // Llamado a la API mock por escuela
                var tesisRepositorio = await this._unprgExternalService.ObtenerTesisEscuela(proyecto.IdEscuela);

                if (tesisRepositorio.Count == 0)
                {
                    this._logger.LogWarning("No se encontraron tesis en el repositorio para la escuela {IdEscuela}", proyecto.IdEscuela);
                    continue;
                }

                // Armar el input con los datos del proyecto
                var inputTokens = Tokenizar($"{proyecto.Nombre} {proyecto.Nombre} {proyecto.Descripcion}");

                // Construir corpus: input primero, luego cada tesis del repositorio
                var corpus = new List<List<string>> { inputTokens };
                corpus.AddRange(tesisRepositorio.Select(t =>
                    Tokenizar($"{t.Titulo} {t.Titulo} {t.Resumen} {string.Join(" ", t.PalabrasClave)}")));

                var vectores = CalcularTfIdf(corpus);
                var vectorInput = vectores[0];

                // Calcular similitud contra cada tesis
                UNPRG.Tesis? mejorTesis = null;
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

                var porcentaje = Math.Round(mejorScore * 100, 2);

                this._logger.LogInformation(
                    "Proyecto: {Nombre} | Tesis más similar: Id: {TesisId} - '{TesisTitulo}' | Similitud: {Porcentaje}%",
                    proyecto.Nombre, mejorTesis.Id, mejorTesis?.Titulo ?? "N/A", porcentaje);
            }
        }

        private List<string> Tokenizar(string texto)
        {
            return texto
                .ToLowerInvariant()
                .Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => char.IsLetterOrDigit(c) || c == ' ')
                .Aggregate(new System.Text.StringBuilder(), (sb, c) => sb.Append(c))
                .ToString()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length > 2 && !Stopwords.Contains(t))
                .ToList();
        }

        private List<Dictionary<string, double>> CalcularTfIdf(List<List<string>> corpus)
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
                var vec = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                foreach (var (term, count) in freq)
                {
                    double tf = (double)count / total;
                    double idf = Math.Log((N + 1.0) / (df.GetValueOrDefault(term) + 1.0));
                    vec[term] = tf * idf;
                }
                return vec;
            }).ToList();
        }

        private double CosineSimilitud(Dictionary<string, double> a, Dictionary<string, double> b)
        {
            double dot = 0, magA = 0, magB = 0;
            foreach (var (term, valA) in a)
            {
                dot += valA * b.GetValueOrDefault(term);
                magA += valA * valA;
            }
            foreach (var valB in b.Values)
                magB += valB * valB;

            if (magA == 0 || magB == 0) return 0;
            return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
        }

        private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
        {
            "los", "las", "una", "uno", "para", "por", "con", "que", "del",
            "este", "esta", "estos", "estas", "son", "fue", "han", "mas",
            "como", "pero", "tambien", "sobre", "entre", "sistema", "usando",
            "utilizando", "implementacion", "presenta", "trabajo", "desarrollo"
        };
    }
}
