using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Crypto;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Signatures;
using Karify.Application.Models.Interface.Service;
using Karify.Application.Models.Karify.EnviarConstancia;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Pkcs;


namespace Karify.Infrastructure.Services
{
    public class ConstanciaService : IConstanciaService
    {
        private readonly string _certPath;
        private readonly string _certPassword;
        private readonly string _logoPath;

        private static readonly DeviceRgb ColorVerde = new(0, 100, 0);
        private static readonly DeviceRgb ColorAmarillo = new(255, 215, 0);
        private static readonly DeviceRgb ColorGrisClaro = new(245, 245, 245);
        private static readonly DeviceRgb ColorTextoGris = new(80, 80, 80);

        public ConstanciaService(IConfiguration configuration)
        {
            this._certPath = configuration["Certificados:CertPath"]!;
            this._certPassword = configuration["Certificados:CertPassword"]!;
            this._logoPath = configuration["Certificados:LogoPath"]!;
        }

        public Constancia GenerarConstancia(ObtenerDatosConstancia request)
        {
            using var memoryStream = new MemoryStream();

            // 1. Generar PDF sin firma primero
            GenerarContenido(memoryStream, request);
            var pdfBytes = memoryStream.ToArray();

            // 2. Firmar el PDF
            var pdfFirmado = FirmarPdf(pdfBytes);

            return new Constancia
            {
                Base64 = Convert.ToBase64String(pdfFirmado),
                NombreArchivo = $"Constancia_{request.NombreProyecto.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf"
            };
        }

        // ---------------------------------------------------------------
        // GENERACIÓN DE CONTENIDO
        // ---------------------------------------------------------------
        private void GenerarContenido(Stream stream, ObtenerDatosConstancia r)
        {
            var writer = new PdfWriter(stream, new WriterProperties().UseSmartMode());
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf, PageSize.A4);
            document.SetMargins(60, 55, 60, 55);

            var fontBold = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var fontRegular = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
            var fontItalic = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_OBLIQUE);

            // ---------------------------------------------------------------
            // CABECERA: Logo + Datos institucionales
            // ---------------------------------------------------------------
            var logoData = ImageDataFactory.Create(_logoPath);
            var logo = new iText.Layout.Element.Image(logoData).SetWidth(70).SetHeight(70);

            var headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 3 }))
                .UseAllAvailableWidth()
                .SetMarginBottom(20);

            headerTable.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(logo));

            headerTable.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph("UNIVERSIDAD NACIONAL PEDRO RUIZ GALLO")
                    .SetFont(fontBold).SetFontSize(14).SetFontColor(ColorVerde)
                    .SetMarginBottom(2))
                .Add(new Paragraph("VICERRECTORADO ACADÉMICO")
                    .SetFont(fontRegular).SetFontSize(9).SetFontColor(ColorTextoGris))
                .Add(new Paragraph("DIRECCIÓN DE SERVICIOS ACADÉMICOS")
                    .SetFont(fontRegular).SetFontSize(9).SetFontColor(ColorTextoGris)));

            document.Add(headerTable);

            // Línea separadora amarilla
            document.Add(new LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.SolidLine(2f))
                .SetStrokeColor(ColorAmarillo)
                .SetMarginBottom(2));

            // Línea separadora verde delgada
            document.Add(new LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.SolidLine(0.5f))
                .SetStrokeColor(ColorVerde)
                .SetMarginBottom(30));

            // ---------------------------------------------------------------
            // TÍTULO
            // ---------------------------------------------------------------
            document.Add(new Paragraph("CONSTANCIA DE EVALUACIÓN ANTIPLAGIO")
                .SetFont(fontBold)
                .SetFontSize(16)
                .SetFontColor(ColorVerde)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(4));

            document.Add(new Paragraph($"N° {r.CodigoConstancia}")
                .SetFont(fontRegular)
                .SetFontSize(10)
                .SetFontColor(ColorTextoGris)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(30));

            // ---------------------------------------------------------------
            // CUERPO
            // ---------------------------------------------------------------
            document.Add(new Paragraph("La Dirección de Servicios Académicos que suscribe, hace constar que el siguiente proyecto de investigación ha sido evaluado mediante el Sistema de Detección de Similitud Karify:")
                .SetFont(fontRegular)
                .SetFontSize(11)
                .SetFontColor(ColorTextoGris)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetMarginBottom(20));

            // Tabla de datos
            var datosTable = new Table(UnitValue.CreatePercentArray(new float[] { 2, 5 }))
                .UseAllAvailableWidth()
                .SetMarginBottom(20);

            void AgregarFila(string label, string valor)
            {
                datosTable.AddCell(new Cell()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(new iText.Layout.Borders.SolidBorder(ColorGrisClaro, 0.5f))
                    .SetPaddingTop(8).SetPaddingBottom(8)
                    .Add(new Paragraph(label)
                        .SetFont(fontBold).SetFontSize(10).SetFontColor(ColorVerde)));

                datosTable.AddCell(new Cell()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(new iText.Layout.Borders.SolidBorder(ColorGrisClaro, 0.5f))
                    .SetPaddingTop(8).SetPaddingBottom(8)
                    .Add(new Paragraph(valor)
                        .SetFont(fontRegular).SetFontSize(10).SetFontColor(ColorTextoGris)));
            }

            AgregarFila("Proyecto:", r.NombreProyecto);
            AgregarFila("Alumno(s):", string.Join("\n", r.NombresAlumnos.Select(a => $"{a.Nombre} — DNI: {a.NumeroDocumento}")));
            AgregarFila("Profesor Asesor:", r.ProfesorAsesor);
            AgregarFila("Fecha:", r.Fecha.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-PE")));

            document.Add(datosTable);

            // Cuadro de resultado
            document.Add(new Paragraph("El proyecto cumple con los estándares de originalidad establecidos por la institución y ha sido APROBADO para continuar con el proceso académico correspondiente.")
                .SetFont(fontRegular)
                .SetFontSize(10)
                .SetFontColor(ColorConstants.WHITE)
                .SetBackgroundColor(ColorVerde)
                .SetPadding(14)
                .SetBorderRadius(new BorderRadius(4))
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetMarginBottom(10));

            document.Add(new Paragraph("Se expide la presente a petición del interesado para los fines que estime conveniente.")
                .SetFont(fontItalic)
                .SetFontSize(10)
                .SetFontColor(ColorTextoGris)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetMarginBottom(30));

            document.Add(new Paragraph($"Lambayeque, {r.Fecha.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-PE"))}")
                .SetFont(fontRegular)
                .SetFontSize(10)
                .SetFontColor(ColorTextoGris)
                .SetMarginBottom(50));

            // ---------------------------------------------------------------
            // PIE: Firma y sello
            // ---------------------------------------------------------------
            var tablaFirma = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1 }))
                .UseAllAvailableWidth()
                .SetMarginBottom(30);

            tablaFirma.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBorderTop(new iText.Layout.Borders.SolidBorder(ColorVerde, 1))
                .SetPaddingTop(8)
                .Add(new Paragraph("Sistema Karify — UNPRG")
                    .SetFont(fontBold).SetFontSize(9).SetFontColor(ColorVerde)
                    .SetTextAlignment(TextAlignment.CENTER))
                .Add(new Paragraph("Dirección de Servicios Académicos")
                    .SetFont(fontRegular).SetFontSize(8).SetFontColor(ColorTextoGris)
                    .SetTextAlignment(TextAlignment.CENTER)));

            tablaFirma.AddCell(new Cell()
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetBorderTop(new iText.Layout.Borders.SolidBorder(ColorVerde, 1))
                .SetPaddingTop(8)
                .Add(new Paragraph($"Código: {r.CodigoConstancia}")
                    .SetFont(fontRegular).SetFontSize(9).SetFontColor(ColorTextoGris)
                    .SetTextAlignment(TextAlignment.CENTER))
                .Add(new Paragraph($"Emitido: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .SetFont(fontRegular).SetFontSize(9).SetFontColor(ColorTextoGris)
                    .SetTextAlignment(TextAlignment.CENTER)));

            document.Add(tablaFirma);

            // Línea separadora final
            document.Add(new LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.SolidLine(0.5f))
                .SetStrokeColor(ColorVerde)
                .SetMarginBottom(8));

            // Datos de contacto en pie
            var pieTable = new Table(UnitValue.CreatePercentArray(new float[] { 1, 1, 1 }))
                .UseAllAvailableWidth();

            void AgregarPie(string texto)
            {
                pieTable.AddCell(new Cell()
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .Add(new Paragraph(texto)
                        .SetFont(fontRegular).SetFontSize(7).SetFontColor(ColorTextoGris)
                        .SetTextAlignment(TextAlignment.CENTER)));
            }

            AgregarPie("mesadepartes_dsa@unprg.edu.pe");
            AgregarPie("Juan XXIII 391");
            AgregarPie("Ciudad Universitaria — Lambayeque");

            document.Add(pieTable);

            // Nota firma digital
            document.Add(new Paragraph("Este documento contiene una firma digital institucional. Cualquier modificación invalidará su autenticidad.")
                .SetFont(fontItalic)
                .SetFontSize(7)
                .SetFontColor(ColorTextoGris)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(6));

            document.Close();
        }

        private static Paragraph CampoInfo(string label, string valor, PdfFont bold, PdfFont regular)
        {
            return new Paragraph()
                .Add(new Text($"{label} ").SetFont(bold).SetFontSize(11).SetFontColor(ColorVerde))
                .Add(new Text(valor).SetFont(regular).SetFontSize(11))
                .SetMarginBottom(8);
        }

        // ---------------------------------------------------------------
        // FIRMA DIGITAL
        // ---------------------------------------------------------------
        private byte[] FirmarPdf(byte[] pdfBytes)
        {
            using var inputStream = new MemoryStream(pdfBytes);
            using var outputStream = new MemoryStream();

            var reader = new PdfReader(inputStream);
            var signer = new PdfSigner(reader, outputStream, new StampingProperties());

            signer.SetSignerProperties(new SignerProperties().SetFieldName("KarifyFirmaInstitucional"));

            // Cargar certificado y clave privada desde .pfx
            var pfxBytes = File.ReadAllBytes(_certPath);
            var store = new Pkcs12StoreBuilder().Build();
            store.Load(new MemoryStream(pfxBytes), _certPassword.ToCharArray());

            string alias = store.Aliases.Cast<string>().First(a => store.IsKeyEntry(a));

            var privateKey = new PrivateKeyBC(store.GetKey(alias).Key);
            var chain = store.GetCertificateChain(alias)
                             .Select(c => new X509CertificateBC(c.Certificate))
                             .Cast<IX509Certificate>()
                             .ToArray();

            IExternalSignature firma = new PrivateKeySignature(privateKey, DigestAlgorithms.SHA256);

            signer.SignDetached(
                new BouncyCastleDigest(),
                firma,
                chain,
                null,
                null,
                null,
                0,
                PdfSigner.CryptoStandard.CMS
            );

            return outputStream.ToArray();
        }
    }
}
