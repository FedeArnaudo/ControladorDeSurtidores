using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurtidoresInfo
{
    class Configuracion
    {
        public class InfoConfig
        {
            public InfoConfig()
            {
                ProyNuevoRuta = "";
                IpControlador = "";
                TipoControlador = "";
                Protocolo = 0;
            }
            public string ProyNuevoRuta { get; set; }
            public string IpControlador { get; set; }
            public string TipoControlador { get; set; }
            public int Protocolo { get; set; }
        }
        private static readonly string configFile = Environment.CurrentDirectory + "/config.ini";
        static public InfoConfig leerConfiguracion()
        {
            InfoConfig infoConfig;
            try
            {
                StreamReader reader;
                try
                {
                    reader = new StreamReader(configFile);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error al leer archivo de configuración. Excepción: " + e.Message);
                    return null;
                }

                infoConfig = new InfoConfig
                {
                    ProyNuevoRuta = reader.ReadLine().Trim(),
                    IpControlador = reader.ReadLine().Trim(),
                    TipoControlador = reader.ReadLine(),
                    Protocolo = Convert.ToInt32(reader.ReadLine())
                };

                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al leer archivo de configuración. Formato incorrecto. Excepción: " + e.Message);
                return null;
            }
            return infoConfig;
        }
        static public bool guardarConfiguracion(InfoConfig infoConfig)
        {
            try
            {
                //Crea el archivo config.ini
                using (StreamWriter outputFile = new StreamWriter(configFile, false))
                {
                    outputFile.WriteLine(infoConfig.ProyNuevoRuta.Trim());
                    outputFile.WriteLine(infoConfig.IpControlador.Trim());
                    outputFile.WriteLine((infoConfig.TipoControlador).ToString());
                    outputFile.WriteLine(infoConfig.Protocolo.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al guardar la configuración. Excepción: " + e.Message);
                return false;
            }
            return true;
        }
        static public bool existeConfiguracion()
        {
            if (File.Exists(configFile))
            {
                return true;
            }
            return false;
        }

    }
}
