using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Doggo2.Repositories.Utils
{
    public class ReaderUtils 
    {

        // static means you can call the method without the instance of the class
        public static string GetNullableString(SqlDataReader reader, string columnName)
        {
            if (!reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return reader.GetString(reader.GetOrdinal(columnName));
            }
            else
            {
                return null;
            }
        }

        //internal static string GetNullableString(int v)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
