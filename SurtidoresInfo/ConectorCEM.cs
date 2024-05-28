using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurtidoresInfo
{
    public class ConectorCEM
    {
        private readonly byte separador = 0x7E;
        private readonly string nombreDelPipe = "CEM44POSPIPE";
        private readonly string ipControlador;
        private readonly int protocolo;

        public ConectorCEM()
        {
            ipControlador = Configuracion.leerConfiguracion().IpControlador;
            protocolo = Configuracion.leerConfiguracion().Protocolo;
        }
        public Estacion ConfiguracionDeLaEstacion()
        {
            Estacion estacionTemp = Estacion.InstanciaEstacion;

            byte[] mensaje = protocolo == 16 ? (new byte[] { 0x65 }) : (new byte[] { 0xB5 });
            int confirmacion = 0;
            int surtidores = 1;
            int islas = 2;          // Esto no se usa, guarda el valor 0
            int tanques = 3;
            int productos = 4;

            //Traigo las descripciones de los productos de la tabla combus
            List<string[]> combus = TraerDescripciones();

            //byte[] respuesta = EnviarComando(mensaje);

            ///
            ///Uso este comando para leer respuestas guardadas
            ///
            byte[] respuesta = LeerArchivo("ConfigEstacion");

            if (respuesta[confirmacion] != 0x0)
            {
                throw new Exception("No se recibió mensaje de confirmación al solicitar info de la estación");
            }

            estacionTemp.numeroDeSurtidores = respuesta[surtidores];
            _ = respuesta[islas];

            estacionTemp.numeroDeTanques = respuesta[tanques];

            estacionTemp.numeroDeProductos = respuesta[productos];

            int posicion = productos + 1;
            List<Producto> tempProductos = new List<Producto>();
            for (int i = 0; i < estacionTemp.numeroDeProductos; i++)
            {
                Producto producto = new Producto
                {
                    numeroDeProducto = LeerCampoVariable(respuesta, ref posicion),
                    precioUnitario = LeerCampoVariable(respuesta, ref posicion)
                };
                DescartarCampoVariable(respuesta, ref posicion);

                foreach (string[] s in combus)
                {
                    if (s[1].Equals(producto.precioUnitario))
                    {
                        producto.descripcion = s[0];
                        break;
                    }
                }
                tempProductos.Add(producto);
            }
            estacionTemp.productos = tempProductos;

            List<Surtidor> tempSurtidores = new List<Surtidor>();
            for (int i = 0; i < estacionTemp.numeroDeSurtidores; i++)
            {
                Surtidor surtidor = new Surtidor
                {
                    nivelDeSurtidor = respuesta[posicion]
                };
                posicion++;
                surtidor.tipoDeSurtidor = respuesta[posicion] + 1;
                estacionTemp.numeroDeMangerasTotales += respuesta[posicion] + 1;
                posicion++;

                for (int j = 0; j < surtidor.tipoDeSurtidor; j++)
                {
                    Manguera manguera = new Manguera
                    {
                        numeroDeManquera = j + 1
                    };
                    string productoH = "0" + respuesta[posicion];
                    posicion++;
                    foreach (Producto producto in tempProductos)
                    {
                        if (producto.numeroDeProducto.Equals(productoH))
                        {
                            manguera.producto = producto;
                            break;
                        }
                    }
                    surtidor.mangueras.Add(manguera);
                }
                surtidor.numeroDeSurtidor = i + 1;
                tempSurtidores.Add(surtidor);
            }
            estacionTemp.nivelesDePrecio.Add(tempSurtidores);

            for (int i = 0; i < estacionTemp.numeroDeTanques; i++)
            {
                Tanque tanque = new Tanque
                {
                    NumeroDeTanque = i + 1,
                    ProductoTanque = respuesta[posicion]
                };
                posicion++;
                estacionTemp.tanques.Add(tanque);
            }
            return estacionTemp;
        }
        public Despacho InformacionDeSurtidor(int numeroDeSurtidor)
        {
            byte[] mensaje = protocolo == 16 ? (new byte[] { 0x70 }) : (new byte[] { 0xC0 });
            int confirmacion = 0;
            int status = 1;
            int nro_venta = 2;
            int codigo_producto = 3;

            Despacho despachoTemp = new Despacho();
            try
            {
                //byte[] respuesta = EnviarComando(new byte[] { (byte)(mensaje[0] + Convert.ToByte(numeroDeSurtidor)) });
                //Uso este comando para leer respuestas guardadas
                byte[] respuesta = LeerArchivo("despacho-" + numeroDeSurtidor);
                if (respuesta[confirmacion] != 0x0)
                {
                    throw new Exception("No se recibió mensaje de confirmación al solicitar info del surtidor");
                }

                // Proceso ultima venta
                byte statusUltimaVenta = respuesta[status];

                bool despachando = false;
                bool detenido = false;

                switch (statusUltimaVenta)
                {
                    case 0x01:
                        despachoTemp.statusUltimaVenta = Despacho.ESTADO_SURTIDOR.DISPONIBLE;
                        break;
                    case 0x02:
                        despachoTemp.statusUltimaVenta = Despacho.ESTADO_SURTIDOR.EN_SOLICITUD;
                        break;
                    case 0x03:
                        despachoTemp.statusUltimaVenta = Despacho.ESTADO_SURTIDOR.DESPACHANDO;
                        despachando = true;
                        break;
                    case 0x04:
                        despachoTemp.statusUltimaVenta = Despacho.ESTADO_SURTIDOR.AUTORIZADO;
                        break;
                    case 0x05:
                        despachoTemp.statusUltimaVenta = Despacho.ESTADO_SURTIDOR.VENTA_FINALIZADA_IMPAGA;
                        break;
                    case 0x08:
                        despachoTemp.statusUltimaVenta = Despacho.ESTADO_SURTIDOR.DEFECTUOSO;
                        break;
                    case 0x09:
                        despachoTemp.statusUltimaVenta = Despacho.ESTADO_SURTIDOR.ANULADO;
                        break;
                    case 0x0A:
                        despachoTemp.statusUltimaVenta = Despacho.ESTADO_SURTIDOR.DETENIDO;
                        detenido = true;
                        break;
                    default:
                        break;
                }

                int posicion = codigo_producto + 1;

                if (despachando || detenido)
                {
                    despachoTemp.nroUltimaVenta = 0;
                    despachoTemp.productoUltimaVenta = 0;
                    despachoTemp.montoUltimaVenta = "";
                    despachoTemp.volumenUltimaVenta = "";
                    despachoTemp.ppuUltimaVenta = "";
                    despachoTemp.ultimaVentaFacturada = false;
                    despachoTemp.idUltimaVenta = null;
                    posicion = status + 1;
                }
                else
                {
                    despachoTemp.nroUltimaVenta = respuesta[nro_venta];
                    despachoTemp.productoUltimaVenta = respuesta[codigo_producto];
                    despachoTemp.montoUltimaVenta = LeerCampoVariable(respuesta, ref posicion);
                    despachoTemp.volumenUltimaVenta = LeerCampoVariable(respuesta, ref posicion);
                    despachoTemp.ppuUltimaVenta = LeerCampoVariable(respuesta, ref posicion);
                    despachoTemp.ultimaVentaFacturada = Convert.ToBoolean(respuesta[posicion]);
                    posicion++;
                    despachoTemp.idUltimaVenta = LeerCampoVariable(respuesta, ref posicion);
                }

                //Proceso venta anterior
                byte statusVentaAnterior = respuesta[posicion];
                switch (statusVentaAnterior)
                {
                    case 0x01:
                        despachoTemp.statusVentaAnterior = Despacho.ESTADO_SURTIDOR.DISPONIBLE;
                        break;
                    case 0x05:
                        despachoTemp.statusVentaAnterior = Despacho.ESTADO_SURTIDOR.VENTA_FINALIZADA_IMPAGA;
                        break;
                    default:
                        break;
                }
                posicion++;

                despachoTemp.nroVentaAnterior = respuesta[posicion];
                posicion++;

                despachoTemp.productoVentaAnterior = respuesta[posicion];
                posicion++;

                despachoTemp.montoVentaAnterior = LeerCampoVariable(respuesta, ref posicion);
                despachoTemp.volumenVentaAnterios = LeerCampoVariable(respuesta, ref posicion);
                despachoTemp.ppuVentaAnterior = LeerCampoVariable(respuesta, ref posicion);
                despachoTemp.ventaAnteriorFacturada = Convert.ToBoolean(respuesta[posicion]);
                posicion++;

                despachoTemp.idVentaAnterior = despachando || detenido ? "" : LeerCampoVariable(respuesta, ref posicion);
                //tablaDespachos.despachos.Add(tempDespacho);
            }
            catch (Exception e)
            {
                throw new Exception("Error al obtener informacion del despacho" + e.Message);
            }
            return despachoTemp;
        }
        public List<Tanque> InformacionDeTanque(int cantidadDeTanques)
        {
            byte[] mensaje = protocolo == 16 ? (new byte[] { 0x70 }) : (new byte[] { 0xC0 });
            int confirmacion = 0;
            try
            {
                //byte[] respuesta = EnviarComando(new byte[] { (byte)(mensaje[0] + Convert.ToByte()) });

                ///
                ///Uso este comando para leer respuestas guardadas
                ///
                byte[] respuesta = LeerArchivo("infoTanques");

                if (respuesta[confirmacion] != 0x0)
                {
                    throw new Exception("No se recibió mensaje de confirmación al solicitar info del surtidor");
                }

                int posicion = confirmacion + 1;

                for (int i = 0; i < cantidadDeTanques; i++)
                {

                    foreach (Tanque tanque in Estacion.InstanciaEstacion.tanques)
                    {
                        if (tanque.NumeroDeTanque == (i + 1))
                        {
                            tanque.NumeroDeTanque = (i + 1);
                            tanque.VolumenProductoT = LeerCampoVariable(respuesta, ref posicion);
                            tanque.VolumenAguaT = LeerCampoVariable(respuesta, ref posicion);
                            tanque.VolumenVacioT = LeerCampoVariable(respuesta, ref posicion);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error al obtener informacion del tanque" + e.Message);
            }
            return Estacion.InstanciaEstacion.tanques;
        }
        public void CierreDeTurno()
        {

        }
        private byte[] EnviarComando(byte[] comando)
        {
            byte[] buffer;
            string ip = ipControlador;
            try
            {
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(ip, nombreDelPipe))
                {
                    pipeClient.Connect();

                    pipeClient.Write(comando, 0, comando.Length);

                    buffer = new byte[pipeClient.OutBufferSize];

                    _ = pipeClient.Read(buffer, 0, buffer.Length);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error al enviar el comando. Excepción: " + e.Message);
            }
            return buffer;
        }
        /*
         * Metodo para leer los campos variables, por ejemplo precios o cantidades.
         * El metodo para frenar la iteracion, es un valor conocido, proporcionado por el fabricante
         * denominado como "separador".
         */
        private string LeerCampoVariable(byte[] data, ref int pos)
        {
            string ret = "";
            ret += Encoding.ASCII.GetString(new byte[] { data[pos] });
            int i = pos + 1;
            while (data[i] != separador)
            {
                ret += Encoding.ASCII.GetString(new byte[] { data[i] });
                i++;
            }
            i++;
            pos = i;
            return ret;
        }
        /*
         * Metodo para saltearse los valores que no son utilizados en la respuesta del CEM.
         * Al finalizar el proceso del metodo, el valor de la posicion queda seteada para
         * el siguiente dato a procesar.
         */
        private void DescartarCampoVariable(byte[] data, ref int pos)
        {
            while (data[pos] != separador)
            {
                pos++;
            }
            pos++;
        }
        /*
         * Se utiliza para testear las respuestas reales del Cem-44
         * se lee un .txt que contiene las respuestas y las guarda en un byte,
         * para simular la respuesta.
         */
        private byte[] LeerArchivo(string nombreArchivo)
        {
            byte[] respuesta = null;
            // Obtener la ruta del directorio donde se ejecuta el programa
            string directorioEjecucion = AppDomain.CurrentDomain.BaseDirectory;

            // Combinar la ruta del directorio con el nombre del archivo
            string rutaArchivo = Path.Combine(directorioEjecucion, nombreArchivo + ".txt");

            // Verificar si el archivo existe
            if (File.Exists(rutaArchivo))
            {
                // Leer todas las líneas del archivo
                string[] lines = File.ReadAllLines(rutaArchivo);

                // Arreglo para almacenar todos los bytes
                byte[] byteArray = new byte[lines.Length * 4]; // Se asume que cada fila tiene 4 valores numéricos (4 bytes cada uno)

                // Índice para rastrear la posición en el arreglo de bytes
                int index = 0;

                // Procesar cada línea del archivo
                foreach (string line in lines)
                {
                    // Dividir la línea en valores numéricos individuales
                    string[] numericValues = line.Split(',');

                    // Convertir cada valor numérico en un byte y agregarlos al arreglo de bytes
                    foreach (string value in numericValues)
                    {
                        byteArray[index++] = Convert.ToByte(value.Trim());
                    }
                }
                respuesta = byteArray;
            }
            return respuesta;
        }
        /*
         *  Metodo para obtener las descripciones de los combustibles, de la tabla combus
         */
        private List<string[]> TraerDescripciones()
        {
            List<string[]> datos = new List<string[]>();
            string rutaDatos = Configuracion.leerConfiguracion().ProyNuevoRuta + @"\tablas";
            string connectionString = @"Driver={Driver para o Microsoft Visual FoxPro};SourceType=DBF;" + $@"Dbq={rutaDatos}\";

            // Definir la consulta SQL
            string query = "SELECT Desc, Importe FROM Combus";

            try
            {
                // Crear una conexión a la base de datos
                using (OdbcConnection connection = new OdbcConnection(connectionString))
                {
                    // Abrir la conexión
                    connection.Open();

                    // Crear un comando SQL con la consulta y la conexión
                    using (OdbcCommand command = new OdbcCommand(query, connection))
                    {
                        // Crear un lector de datos
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            Console.WriteLine();
                            // Leer y mostrar los datos
                            while (reader.Read())
                            {
                                // Acceder a las columnas por índice o nombre
                                string columna1 = reader.GetString(0);// Suponiendo que el segundo campo es un entero
                                float columna2 = reader.GetFloat(1);

                                datos.Add(item: new string[] { columna1.Trim(), columna2.ToString("0.000").Replace(",", ".") });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejar la excepción
                Console.WriteLine($"Error al acceder a la tabla Combus. Excepcion: {ex.Message}");
            }
            return datos;
        }
    }
}
