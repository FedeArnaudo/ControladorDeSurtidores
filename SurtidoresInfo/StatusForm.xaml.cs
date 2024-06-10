using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SurtidoresInfo
{
    /// <summary>
    /// Lógica de interacción para StatusForm.xaml
    /// </summary>
    public partial class StatusForm : Window
    {
        public StatusForm()
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/SurtidoresInfo;component/LogoSiges.ico"));
            Loaded += new RoutedEventHandler(StatusForm_Load);
        }
        private void StatusForm_Load(object sender, EventArgs e)
        {
            if (!Configuracion.existeConfiguracion())
            {
                VerConfig verConfig = new VerConfig
                {
                    Owner = this
                };
                verConfig.Closed += VerConfig_Closed;
                verConfig.Show();
                Hide();

                IsEnabled = false;
            }
            else
            {
                init();
            }
        }

        private void init()
        {
            if (!Controlador.Init(Configuracion.leerConfiguracion()))
            {

                // TODO: Borrar archivo config para que no abra de vuelta.

                //btnCerrar_Click(null, null); // Cerrar
            }
        }

        private void VerConfig_Closed(object sender, EventArgs e)
        {
            IsEnabled = true;
            Show();
            _ = Activate();
            init();
        }
        private void btnVerDespacho_Click(object sender, RoutedEventArgs e)
        {
            VerDespacho verDespacho = new VerDespacho();
            verDespacho.Show();
        }

        private void btnCambiarConfig_Click(object sender, RoutedEventArgs e)
        {
            VerConfig verConfig = new VerConfig();
            verConfig.Show();
        }

        private void btnVerConfigEstacion_Click(object sender, RoutedEventArgs e)
        {
            VerSurtidores verSurtidores = new VerSurtidores();
            verSurtidores.Show();
        }
    }
}
