using Karify.Application.Models.UNPRG;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Karify.Infrastructure.Services
{
    public class UnprgExternalService
    {
        private readonly HttpClient _http;

        public UnprgExternalService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("MockApi");
        }

        public async Task<List<Tesis>> ObtenerTesisGeneral()
        {
            var response = await _http.GetAsync("UNPRG");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Tesis>>(json) ?? new List<Tesis>();
        }

        public async Task<List<Tesis>> ObtenerTesisFacultad(int idFacultad)
        {
            var response = await _http.GetAsync($"UNPRG/facultad/${idFacultad}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Tesis>>(json) ?? new List<Tesis>();
        }

        public async Task<List<Tesis>> ObtenerTesisEscuela(int idEscuela)
        {
            var response = await _http.GetAsync($"UNPRG/escuela/${idEscuela}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Tesis>>(json) ?? new List<Tesis>();
        }

        public async Task<List<Tesis>> ObtenerTesis(int idTesis)
        {
            var response = await _http.GetAsync($"UNPRG/${idTesis}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Tesis>>(json) ?? new List<Tesis>();
        }
    }
}
