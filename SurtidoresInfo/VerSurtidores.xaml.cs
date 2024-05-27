using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Lógica de interacción para VerSurtidores.xaml
    /// </summary>
    public partial class VerSurtidores : Window
    {
        public VerSurtidores()
        {
            InitializeComponent();
            List<ConfigEstacion> infoSurtidors = MostrarConfiguracion();
            DataGridDatos.ItemsSource = infoSurtidors;
        }
        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private List<ConfigEstacion> MostrarConfiguracion()
        {
            Estacion estacion = Estacion.InstanciaEstacion;
            List<ConfigEstacion> infoSurtidores = new List<ConfigEstacion>();
            List<Surtidor> tempSurtidores = estacion.nivelesDePrecio[0];
            foreach (Surtidor surtidor in tempSurtidores)
            {
                List<Manguera> tempManguera = surtidor.mangueras;
                foreach (Manguera manguera in tempManguera)
                {
                    string letra = "A";
                    if (manguera.numeroDeManquera == 2)
                    {
                        letra = "B";
                    }
                    else if (manguera.numeroDeManquera == 3)
                    {
                        letra = "C";
                    }
                    else if (manguera.numeroDeManquera == 4)
                    {
                        letra = "D";
                    }
                    infoSurtidores.Add(new ConfigEstacion(surtidor.numeroDeSurtidor.ToString(), letra, manguera.producto.numeroDeProducto, manguera.producto.precioUnitario, manguera.producto.descripcion));
                }
            }
            return infoSurtidores;
        }
    }
}
