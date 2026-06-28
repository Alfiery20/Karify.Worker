using Dapper;
using Karify.Application.Models.Interface.Repository;
using Karify.Application.Models.Karify;
using Karify.Repository.Database;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Karify.Repository.Repository
{
    public class ProyectoRepository : IProyectoRepository
    {
        private readonly DataBase dataBase;

        public ProyectoRepository(DataBase dataBase)
        {
            this.dataBase = dataBase;
        }

        public async Task<IEnumerable<ObtenerTesisResponse>> GetProyectoPorRevision()
        {
            using (var cnx = this.dataBase.CreateConnection())
            {
                using (var reader = await cnx.ExecuteReaderAsync(
                    "[dbo].[usp_ObtenerProyectoPorRevision]",
                    commandType: CommandType.StoredProcedure))
                {
                    List<ObtenerTesisResponse> response = new();
                    while (reader.Read())
                    {
                        response.Add(new ObtenerTesisResponse()
                        {
                            Id = Convert.IsDBNull(reader["ID"]) ? 0 : Convert.ToInt32(reader["ID"].ToString()),
                            Nombre = Convert.IsDBNull(reader["NOMBRE"]) ? string.Empty : reader["NOMBRE"].ToString(),
                            Descripcion= Convert.IsDBNull(reader["DESCRIPCION"]) ? string.Empty : reader["DESCRIPCION"].ToString(),
                            IdEscuela = Convert.IsDBNull(reader["ID_ESCUELA"]) ? 0 : Convert.ToInt32(reader["ID_ESCUELA"].ToString()),
                            Estado = Convert.IsDBNull(reader["ESTADO"]) ? string.Empty : reader["ESTADO"].ToString(),
                            FechaRegistro= Convert.IsDBNull(reader["FECHA_REGISTRO"]) ? DateTime.Now : Convert.ToDateTime(reader["FECHA_REGISTRO"].ToString())
                        });
                    }
                    return response;
                }
            }
        }

    }
}
