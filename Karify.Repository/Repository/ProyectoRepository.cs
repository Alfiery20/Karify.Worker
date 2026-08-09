using Dapper;
using Karify.Application.Models.Interface.Repository;
using Karify.Application.Models.Karify.EnviarConstancia;
using Karify.Application.Models.Karify.GuardarResultados;
using Karify.Application.Models.Karify.ObtenerTesis;
using Karify.Repository.Database;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Karify.Repository.Repository
{
    public class ProyectoRepository : IProyectoRepository
    {
        private readonly DataBase _dataBase;

        public ProyectoRepository(DataBase dataBase)
        {
            this._dataBase = dataBase;
        }

        public async Task<IEnumerable<ObtenerTesisResponse>> GetProyectoPorRevision()
        {
            using (var cnx = this._dataBase.CreateConnection())
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
                            NombreAlumno = Convert.IsDBNull(reader["NOMBRE_ALUMNO"]) ? string.Empty : reader["NOMBRE_ALUMNO"].ToString(),
                            ApellidoPaterno = Convert.IsDBNull(reader["APELLIDO_PATERNO"]) ? string.Empty : reader["APELLIDO_PATERNO"].ToString(),
                            ApellidoMaterno = Convert.IsDBNull(reader["APELLIDO_MATERNO"]) ? string.Empty : reader["APELLIDO_MATERNO"].ToString(),
                            Correo = Convert.IsDBNull(reader["CORREO"]) ? string.Empty : reader["CORREO"].ToString(),
                            Estado = Convert.IsDBNull(reader["ESTADO"]) ? string.Empty : reader["ESTADO"].ToString(),
                            FechaRegistro= Convert.IsDBNull(reader["FECHA_REGISTRO"]) ? DateTime.Now : Convert.ToDateTime(reader["FECHA_REGISTRO"].ToString())
                        });
                    }
                    return response;
                }
            }
        }

        public async Task<GuardarResultadosResponse> GuardarResultadoSimilitud(GuardarResultadosCommand command)
        {
            using (var cnx = this._dataBase.CreateConnection())
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@pidProyecto", command.IdProyecto, DbType.Int32, ParameterDirection.Input);
                parameters.Add("@pDOI", command.DOI, DbType.String, ParameterDirection.Input);
                parameters.Add("@pPorcentaje", command.PorcentajeSimilitud, DbType.Double, ParameterDirection.Input);
                parameters.Add("@pFechaProcesamiento", command.FechaProcesamiento, DbType.DateTime, ParameterDirection.Input);

                parameters.Add("@msj", "", DbType.String, ParameterDirection.Output);

                var reader = await cnx.ExecuteAsync(
                    "[dbo].[usp_GuardarResultadoAnalisis]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

                GuardarResultadosResponse response = new()
                {
                    Mensaje = parameters.Get<string>("@msj"),
                };
                return response;

            }
        }

        public async Task<ObtenerDatosConstancia> ObtenerDatosConstancia(EnviarConstanciaCommand command)
        {
            using (var cnx = _dataBase.CreateConnection())
            {
                ObtenerDatosConstancia response = new();
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@pIdProyecto", command.IdProyecto, DbType.Int32, ParameterDirection.Input);

                using (var reader = await cnx.ExecuteReaderAsync(
                    "[dbo].[usp_ObtenerDatosConstancia]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure))
                {
                    while (reader.Read())
                    {
                        response.Id = Convert.IsDBNull(reader["ID"]) ? 0 : Convert.ToInt32(reader["ID"].ToString());
                        response.NombreProyecto = Convert.IsDBNull(reader["NOMBRE"]) ? "" : reader["NOMBRE"].ToString();
                        response.PorcentajeSimilitud = Convert.IsDBNull(reader["PORCENTAJE_SIMILITUD"]) ? 0 : Convert.ToDouble(reader["PORCENTAJE_SIMILITUD"].ToString());
                        response.ProfesorAsesor = Convert.IsDBNull(reader["NOMBRE_PROFESOR"]) ? "" : reader["NOMBRE_PROFESOR"].ToString();
                        response.Fecha = Convert.IsDBNull(reader["FECHA"]) ? default : Convert.ToDateTime(reader["FECHA"].ToString());

                    }
                }
                return response;
            }
        }

        public async Task<IEnumerable<EnviarConstanciaAlumno>> ObtenerAlumnosPorProyecto(int IdProyecto)
        {
            using (var cnx = _dataBase.CreateConnection())
            {
                List<EnviarConstanciaAlumno> proyectos = new();
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("pIdProyecto", IdProyecto, DbType.Int32, ParameterDirection.Input);

                using (var reader = await cnx.ExecuteReaderAsync(
                    "[dbo].[usp_ObtenerAlumnosPorProyecto]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure))
                {
                    while (reader.Read())
                    {
                        proyectos.Add(new EnviarConstanciaAlumno()
                        {
                            NumeroDocumento = Convert.IsDBNull(reader["NUMERO_DOCUMENTO"]) ? "" : reader["NUMERO_DOCUMENTO"].ToString(),
                            Nombre = Convert.IsDBNull(reader["NOMBRE"]) ? "" : reader["NOMBRE"].ToString()
                        });
                    }
                }
                return proyectos;
            }
        }

        public async Task<EnviarConstanciaCommandDTO> GuardarConstancia(GuardarConstancia command)
        {
            using (var cnx = _dataBase.CreateConnection())
            {
                EnviarConstanciaCommandDTO response = new();
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@pIdProyecto", command.IdProyecto, DbType.Int32, ParameterDirection.Input);
                parameters.Add("@pNombreConstancia", command.NombreConstancia, DbType.String, ParameterDirection.Input);
                parameters.Add("@pBase64", command.Base64, DbType.String, ParameterDirection.Input);
                parameters.Add("@pGuid", command.Guid, DbType.String, ParameterDirection.Input);
                parameters.Add("@msj", "", DbType.String, ParameterDirection.Output);

                using var reader = await cnx.ExecuteReaderAsync(
                    "[dbo].[usp_GuardarConstancia]",
                    param: parameters,
                    commandType: CommandType.StoredProcedure);

                response.Mensaje = parameters.Get<string>("msj");

                return response;
            }
        }
    }
}
