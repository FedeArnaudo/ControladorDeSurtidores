using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurtidoresInfo
{
    /// <summary>
    /// Esta clase tiene una sola instancia, se encarga de escribir mensajes de log
    /// en la ruta {executalblePath}/Log.
    /// 
    /// Genera un log por dia y borra los que tengan mas de 2 dias de antiguedad
    /// </summary>
    class Log
    {
        public enum LogType
        {
            t_debug = 0,
            t_info,
            t_warning,
            t_error
        }

        // Singleton implementation
        public static Log Instance { get; } = new Log();

        // Esta variable almacena el tipo de log que esta seteando como maximo.
        // Por ejemplo, si se setea como t_warning, solo los mensajes del tipo t_warning y t_error
        // se van a mostar. Si se pone en t_debug, todos los mensajes se muestran.
        private LogType logLevel = LogType.t_debug;

        private Log() { }

        public void SetLogLevel(LogType level)
        {
            logLevel = level;
        }

        public bool WriteLog(string message, LogType type)
        {
            try
            {
                // Create folder if not exist
                //string path = Environment.CurrentDirectory + "/Log";
                string path = AppDomain.CurrentDomain.BaseDirectory + "/Log";
                string logFile = "log" + DateTime.Now.ToString("dd-MM-yyyy") + ".txt";

                if (!Directory.Exists(path))
                {
                    _ = Directory.CreateDirectory(path);
                }

                // Delete 2 days old log
                string deleteFile = "log" + DateTime.Now.Subtract(new TimeSpan(4, 0, 0, 0)).ToString("dd-MM-yyyy") + ".txt";
                if (File.Exists(Environment.CurrentDirectory + "/Log/" + deleteFile))
                {
                    File.Delete(Environment.CurrentDirectory + "/Log/" + deleteFile);
                }

                // Log message with correspondant log level
                switch (type)
                {
                    case LogType.t_debug:
                        if (logLevel <= type)
                        {
                            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, logFile), true))
                            {
                                outputFile.WriteLine(DateTime.Now.ToString("hh:mm:ss") + "  DEBUG:   " + message);
                            }
                        }
                        break;
                    case LogType.t_info:
                        if (logLevel <= type)
                        {
                            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, logFile), true))
                            {
                                outputFile.WriteLine(DateTime.Now.ToString("hh:mm:ss") + "  INFO:    " + message);
                            }
                        }
                        break;
                    case LogType.t_warning:
                        if (logLevel <= type)
                        {
                            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, logFile), true))
                            {
                                outputFile.WriteLine(DateTime.Now.ToString("hh:mm:ss") + "  WARNING: " + message);
                            }
                        }
                        break;
                    case LogType.t_error:
                        if (logLevel <= type)
                        {
                            using (StreamWriter outputFile = new StreamWriter(Path.Combine(path, logFile), true))
                            {
                                outputFile.WriteLine(DateTime.Now.ToString("hh:mm:ss") + "  ERROR:   " + message);
                            }
                        }
                        break;
                    default:
                        break;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
