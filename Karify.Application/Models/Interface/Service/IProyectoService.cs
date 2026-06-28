using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Application.Models.Interface.Service
{
    public interface IProyectoService
    {
        Task<bool> Execute();
    }
}
