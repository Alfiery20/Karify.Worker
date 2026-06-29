using Karify.Application.Models.Interface.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Infrastructure.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime HoraLocal()
        {
            var horaActualPacifico = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"));
            var fechaActual = horaActualPacifico;
            return fechaActual.DateTime;
        }

        public DateTime HoraActual()
        {
            return DateTime.UtcNow;
        }
    }
}
