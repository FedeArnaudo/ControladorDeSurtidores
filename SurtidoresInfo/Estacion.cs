using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurtidoresInfo
{
    /*public class Estacion
    {
        public readonly int NUM_MAX_NIVELES = 5;
        public Estacion()
        {
            nivelesDePrecio = new List<List<Surtidor>>();
            tanques = new List<Tanque>();
        }
        // Instancia de Singleton
        public static Estacion instanciaEstacion { get; } = new Estacion();
        public int numeroDeSurtidores { get; set; }
        public int numeroDeTanques { get; set; }
        public int numeroDeProductos { get; set; }
        public int numeroDeMangerasTotales { get; set; }
        public List<List<Surtidor>> nivelesDePrecio { get; set; }
        public List<Tanque> tanques { get; set; }
        public List<ConfigEstacion> verSurtidores()
        {
            List<ConfigEstacion> infoSurtidors = new List<ConfigEstacion>();
            List<Surtidor> tempSurtidores = nivelesDePrecio[0];
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
                    else if(manguera.numeroDeManquera == 3)
                    {
                        letra = "C";
                    }
                    else if(manguera.numeroDeManquera == 4)
                    {
                        letra = "D";
                    }
                    infoSurtidors.Add(new ConfigEstacion(surtidor.numeroDeSurtidor.ToString(), letra, manguera.producto.precioUnitario, manguera.producto.descripcion));
                }
            }
            return infoSurtidors;
        }
    }

    public class ConfigEstacion
    {
        public ConfigEstacion(string surt, string mang, string prod, string desc)
        {
            Surt = surt;
            Mang = mang;
            Prod = prod;
            Desc = desc;
        }
        public string Surt { get; set; }
        public string Mang { get; set; }
        public string Prod { get; set; }
        public string Desc { get; set; }
    }

    public class Surtidor
    {
        public Surtidor()
        {
            mangueras = new List<Manguera>();
        }
        public int nivelDeSurtidor { get; set; }  // indica al nivel de precio al que trabaja
        public int tipoDeSurtidor { get; set; }   // indica la cantidad de mangueras que tiene ese surtidor
        public List<Manguera> mangueras { get; set; }
        public int numeroDeSurtidor { get; set; }

        class SurtidorPorNivel
        {
            public string numeroDeNivel { get; set; }
            public string numeroDeSurtidor { get; set; }
            public string tipoDeSurtidor { get; set; } // indica la cantidad de mangueras que tiene ese surtidor
        }
    }

    public class Tanque
    {
        public Tanque()
        {

        }
        public int numeroDeTanque { get; set; }
        public int prod { get; set; }
        public Producto producto { get; set; }
    }

    /**
     * Es el "Tipo Surtidor X" y me dice la cantidad de mangueras
     */
    /*
    public class Manguera
    {
        public Manguera()
        {

        }
        public int numeroDeManquera { get; set; }
        public Producto producto { get; set; }
    }

    public class Producto
    {
        public Producto()
        {
            descripcion = "";
        }

        public string numeroDeProducto { get; set; }
        public string precioUnitario { get; set; }
        public string descripcion { get; set; }
    }*/
}
