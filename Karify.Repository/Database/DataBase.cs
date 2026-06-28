using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Karify.Repository.Database
{
    public class DataBase
    {
        private readonly IConfiguration _configuration;

        public DataBase(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public SqlConnection CreateConnection()
        {
            var conexionBd = _configuration.GetConnectionString("Karify");
            return new SqlConnection(conexionBd);
        }
    }
}
