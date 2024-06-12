using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CheckBox = System.Windows.Controls.CheckBox;

namespace SurtidoresInfo
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class VerConfig : Window
    {
        public VerConfig()
        {
            InitializeComponent();
            Icon = new BitmapImage(new Uri("pack://application:,,,/SurtidoresInfo;component/LogoSiges24x24.ico"));
            //Loaded += new RoutedEventHandler(ConfiguracionForm_Load);
        }
        private void ConfiguracionForm_Load(object sender, EventArgs e)
        {
            if (Configuracion.existeConfiguracion())
            {
                Configuracion.InfoConfig infoConfig = Configuracion.leerConfiguracion();

                TextBoxPryNuevo.Text = infoConfig.ProyNuevoRuta;
                TextBoxIP.Text = infoConfig.IpControlador;
                ComboBoxTipo.Text = infoConfig.TipoControlador;
                if (infoConfig.Protocolo == 16)
                    CheckBoxProtocol16.IsChecked = true;
                else
                    CheckBoxProtocol32.IsChecked = true;

                StatusForm statusForm = new StatusForm();
                statusForm.Owner = this;
                Hide();
                statusForm.ShowDialog();
                statusForm.Show();
            }
        }
        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            Configuracion.InfoConfig infoConfig = new Configuracion.InfoConfig();
            bool completado = false;

            if (TextBoxPryNuevo.Text != "")
            {
                infoConfig.ProyNuevoRuta = TextBoxPryNuevo.Text;
                if (TextBoxIP.Text != "")
                {
                    infoConfig.IpControlador = TextBoxIP.Text;
                    if (ComboBoxTipo.Text != null && ComboBoxTipo.Text != "")
                    {
                        infoConfig.TipoControlador = ComboBoxTipo.Text;
                        if (CheckBoxProtocol16.IsChecked != false)
                        {
                            infoConfig.Protocolo = 16;
                            completado = true;
                        }
                        else if (CheckBoxProtocol32.IsChecked != false)
                        {
                            infoConfig.Protocolo = 32;
                            completado = true;
                        }
                        else
                        {
                            _ = System.Windows.MessageBox.Show("Debe ingresar el Protocolo de la Estacion");
                        }

                        if (completado && Configuracion.guardarConfiguracion(infoConfig))
                        {
                            _ = System.Windows.MessageBox.Show("Configuracón guardada correctamente",
                                "Exito!", (MessageBoxButton)MessageBoxButtons.OK, (MessageBoxImage)MessageBoxIcon.Information);
                            Close();
                        }
                    }
                    else
                    {
                        _ = System.Windows.MessageBox.Show("Debe seleccionar un Tipo de Controlador");
                    }
                }
                else
                {
                    _ = System.Windows.MessageBox.Show("Debe ingresar la IP del Controlador");
                }
            }
            else
            {
                _ = System.Windows.MessageBox.Show("Debe ingresar la ruta del proyecto");
            }

            /*if (!TextBoxIP.Text.Equals("") && (CheckBoxProtocol16.IsChecked == true || CheckBoxProtocol32.IsChecked == true))
            {
                string protocolo = CheckBoxProtocol16.IsChecked.ToString();
                if (CheckBoxProtocol32.IsChecked == true)
                {
                    protocolo = CheckBoxProtocol32.IsChecked.ToString();
                }
                if (Controlador.init(TextBoxIP.Text, ComboBoxTipo.Text, protocolo))
                {
                    _ = MessageBox.Show("Los datos se han cargado correctamente");
                    VerSurtidores verSurtidores = new VerSurtidores();
                    verSurtidores.Show();
                    Close();
                }
            }
            else if(TextBoxIP.Text.Equals(""))
            {
                _ = MessageBox.Show("Ingrese una IP");
            }
            else
            {
                _ = MessageBox.Show("Debe elegir un protocolo");
            }*/
        }

        private void CheckBoxProtocol16_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBoxProtocol16 = (CheckBox)sender;
            CheckBoxProtocol16.IsChecked = true;
            CheckBoxProtocol32.IsChecked = false;
        }

        private void CheckBoxProtocol16_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBoxProtocol16 = (CheckBox)sender;
            CheckBoxProtocol16.IsChecked = false;
            CheckBoxProtocol32.IsChecked = true;
        }

        private void CheckBoxProtocol32_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBoxProtocol32 = (CheckBox)sender;
            CheckBoxProtocol32.IsChecked = true;
            CheckBoxProtocol16.IsChecked = false;
        }

        private void CheckBoxProtocol32_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox CheckBoxProtocol32 = (CheckBox)sender;
            CheckBoxProtocol32.IsChecked = false;
            CheckBoxProtocol16.IsChecked = true;
        }

        private void btnRutaPN_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.MyComputer, // Carpeta raíz (opcional)
                Description = "Selecciona una carpeta" // Descripción del diálogo (opcional)
            };

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = folderBrowserDialog.SelectedPath;
                TextBoxPryNuevo.Text = folderPath;
            }
        }
    }
}
