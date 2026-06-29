using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Application.Models.Interface.Service
{
    public interface IDateTimeService
    {
        public DateTime HoraLocal();
        public DateTime HoraActual();
    }
}
