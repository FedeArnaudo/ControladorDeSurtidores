﻿using System;
using System.Collections.Generic;
using System.Data;
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
    /// Lógica de interacción para VerDespacho.xaml
    /// </summary>
    public partial class VerDespacho : Window
    {
        public VerDespacho()
        {
            InitializeComponent();
            DataTable result = ConectorSQLite.dt_query("SELECT * FROM despachos ORDER BY fecha DESC");
            //List<InfoDespacho> infoDespachos = TablaDespachos.InstanciaDespachos.InfoDespachos;
            //DataGridDatos.ItemsSource = infoDespachos;
            DataGridDatos.ItemsSource = result.AsDataView();
        }
        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
