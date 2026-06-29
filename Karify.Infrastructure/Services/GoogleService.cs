using Karify.Application.Models.Interface.Service;
using Karify.Application.Models.Services.GoogleService;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;

namespace Karify.Infrastructure.Services
{
    public class GoogleService : IGoogleService
    {
        private readonly IConfiguration _configuration;
        private readonly string servidor;
        private readonly string correo;
        private readonly string clave;
        private readonly int puertoTLS;
        public GoogleService(IConfiguration configuration)
        {
            this._configuration = configuration;
            this.servidor = this._configuration["GmailCredential:Servidor"] ?? "";
            this.correo = this._configuration["GmailCredential:Correo"] ?? "";
            this.clave = this._configuration["GmailCredential:Clave"] ?? "";
            this.puertoTLS = Convert.ToInt32(this._configuration["GmailCredential:PuertoTLS"]);
        }
        
        public async Task EnvioSolicitudAprobacion(EnviarEvaluacionExitosa envioCorreo)
        {
            string
                Servidor = this.servidor,
                Correo = this.correo,
                Clave = this.clave;
            int PuertoTLS = this.puertoTLS;
            var smtp = new SmtpClient(Servidor, PuertoTLS)
            {
                Credentials = new NetworkCredential(Correo, Clave),
                EnableSsl = true
            };
            var htmlBody = await File.ReadAllTextAsync("Templates/ProyectoAprobado.html");

            htmlBody = htmlBody
                    .Replace("{{NombreAlumno}}", envioCorreo.NombreAlumno)
                    .Replace("{{ApellidoPaterno}}", envioCorreo.ApellidoPaterno)
                    .Replace("{{ApellidoMaterno}}", envioCorreo.ApellidoMaterno)
                    .Replace("{{NombreProyecto}}", envioCorreo.NombreProyecto)
                    .Replace("{{DescripcionProyecto}}", envioCorreo.DescripcionProyecto);
            var mail = new MailMessage
            {
                From = new MailAddress(Correo, "Sistema de Gestión de Proyectos"),

                Subject = "Aprobación de anteproyecto",
                Body = htmlBody,
                IsBodyHtml = true
            };

            mail.To.Add(envioCorreo.CorreoAlumno);

            await smtp.SendMailAsync(mail);
        }

        public async Task EnvioSolicitudRechazado(EnviarEvaluacionErronea envioCorreo)
        {
            string
                Servidor = this.servidor,
                Correo = this.correo,
                Clave = this.clave;
            int PuertoTLS = this.puertoTLS;
            var smtp = new SmtpClient(Servidor, PuertoTLS)
            {
                Credentials = new NetworkCredential(Correo, Clave),
                EnableSsl = true
            };
            var htmlBody = await File.ReadAllTextAsync("Templates/ProyectoRechazado.html");

            htmlBody = htmlBody
                    .Replace("{{NombreAlumno}}", envioCorreo.NombreAlumno)
                    .Replace("{{ApellidoPaterno}}", envioCorreo.ApellidoPaterno)
                    .Replace("{{ApellidoMaterno}}", envioCorreo.ApellidoMaterno)
                    .Replace("{{NombreProyecto}}", envioCorreo.NombreProyecto)
                    .Replace("{{DescripcionProyecto}}", envioCorreo.DescripcionProyecto)
                    .Replace("{{NombreProyectoResultado}}", envioCorreo.NombreProyectoResultado)
                    .Replace("{{DOI}}", envioCorreo.DOI)
                    .Replace("{{PorcentajeSimilitud}}", envioCorreo.PorcentajeSimilitud.ToString("0.00"));
            var mail = new MailMessage
            {
                From = new MailAddress(Correo, "Sistema de Gestión de Proyectos"),

                Subject = "Rechazo de anteproyecto",
                Body = htmlBody,
                IsBodyHtml = true
            };

            mail.To.Add(envioCorreo.CorreoAlumno);

            await smtp.SendMailAsync(mail);
        }
    }
}
