﻿using System;
using System.Data;
using System.Data.SQLite;

namespace SurtidoresInfo
{
    class ConectorSQLite
    {
        private static readonly string databaseName = "cds.db";
        private static readonly string connectionString = "Data Source='{0}';Version=3;";

        /// <summary>
        /// Método para ejecutar una query que devuelva una dataTable (como un select)
        /// </summary>
        /// <param name="query"> Comando a ejecutar </param>
        /// <returns> Una tabla con la respuesta al comando </returns>
        public static DataTable Dt_query(string query)
        {
            DataTable ret = new DataTable();
            using (SQLiteConnection db = new SQLiteConnection(
                string.Format(connectionString, Configuracion.LeerConfiguracion().ProyNuevoRuta + "\\CDS\\" + databaseName)))
            {
                db.Open();
                using (SQLiteCommand command = new SQLiteCommand(query, db))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        ret.Load(reader);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Método para ejecutar una query que no devuelve una tabla (como un update)
        /// </summary>
        /// <param name="query"> Comando a ejecutar </param>
        /// <returns> Cantidad de registros alterados </returns>
        public static int Query(string query)
        {
            int ret = 0;
            using (SQLiteConnection db = new SQLiteConnection(
                string.Format(connectionString, Configuracion.LeerConfiguracion().ProyNuevoRuta + "\\CDS\\" + databaseName)))
            {
                db.Open();
                using (SQLiteCommand command = new SQLiteCommand(query, db))
                {
                    ret = command.ExecuteNonQuery();
                }
            }
            return ret;
        }
        /// <summary>
        /// Método para obtener el precio de un producto según su ID
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>Precio del producto</returns>
        public static bool ComprobarCierre()
        {
            bool cierreFlag = false;

            string query = $"SELECT EXISTS(SELECT 1 FROM cierreBandera WHERE hacerCierre IS NOT NULL)";

            using (SQLiteConnection db = new SQLiteConnection(string.Format(connectionString, Configuracion.LeerConfiguracion().ProyNuevoRuta + "\\CDS\\" + databaseName)))
            {
                db.Open();
                using (SQLiteCommand command = new SQLiteCommand(query, db))
                {
                    cierreFlag = Convert.ToBoolean(command.ExecuteScalar());
                }
            }
            return cierreFlag;
        }

        /// <summary>
        /// Método para obtener el precio de un producto según su ID
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>Precio del producto</returns>
        public static double ObtenerPrecioProducto(string productId)
        {
            double precio = 0;

            string query = $"SELECT precio FROM productos WHERE ID = '{productId}'";

            using (SQLiteConnection db = new SQLiteConnection(string.Format(connectionString, Configuracion.LeerConfiguracion().ProyNuevoRuta + "\\CDS\\" + databaseName)))
            {
                db.Open();
                using (SQLiteCommand command = new SQLiteCommand(query, db))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        precio = Convert.ToDouble(result);
                    }
                }
            }
            return precio;
        }

        /// <summary>
        /// Método para insertar datos en la tabla "surtidores"
        /// </summary>
        /// <param name="idSurtidor">ID del surtidor</param>
        /// <param name="manguera">Número de manguera</param>
        /// <param name="producto">Nombre del producto</param>
        /// <param name="precio">Precio del producto</param>
        public static void InsertarDatosSurtidores(int idSurtidor, int manguera, string producto, double precio)
        {
            string query = $"INSERT INTO surtidores (IdSurtidor, Manguera, Producto, Precio) VALUES ({idSurtidor}, {manguera}, '{producto}', {precio})";

            using (SQLiteConnection db = new SQLiteConnection(string.Format(connectionString, Configuracion.LeerConfiguracion().ProyNuevoRuta + "\\CDS\\" + databaseName)))
            {
                db.Open();
                using (SQLiteCommand command = new SQLiteCommand(query, db))
                {
                    _ = command.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// Método para verificar si ya existe un registro con los mismos valores en la tabla "surtidores"
        /// </summary>
        public static bool ExisteRegistro(int idSurtidor, int manguera, string producto)
        {
            string query = $"SELECT COUNT(*) FROM surtidores WHERE IdSurtidor = {idSurtidor} AND Manguera = {manguera} AND Producto = '{producto}'";

            using (SQLiteConnection db = new SQLiteConnection(string.Format(connectionString, Configuracion.LeerConfiguracion().ProyNuevoRuta + "\\CDS\\" + databaseName)))
            {
                db.Open();
                using (SQLiteCommand command = new SQLiteCommand(query, db))
                {
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }
    }
}
