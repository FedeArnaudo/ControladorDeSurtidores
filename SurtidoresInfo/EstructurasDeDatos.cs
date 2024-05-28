using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurtidoresInfo
{
    public class Estacion
    {
        public readonly int NUM_MAX_NIVELES = 5;
        public Estacion()
        {
            nivelesDePrecio = new List<List<Surtidor>>();
            tanques = new List<Tanque>();
            productos = new List<Producto>();
        }
        // Instancia de Singleton
        public static Estacion InstanciaEstacion { get; } = new Estacion();
        public int numeroDeSurtidores { get; set; }
        public int numeroDeTanques { get; set; }
        public int numeroDeProductos { get; set; }
        public int numeroDeMangerasTotales { get; set; }
        public List<List<Surtidor>> nivelesDePrecio { get; set; }
        public List<Tanque> tanques { get; set; }
        public List<Producto> productos { get; set; }
    }
    public class ConfigEstacion
    {
        public ConfigEstacion(string surt, string mang, string prod, string precio, string desc)
        {
            Surt = surt;
            Mang = mang;
            Prod = prod;
            Prec = precio;
            Desc = desc;
        }
        public string Surt { get; set; }
        public string Mang { get; set; }
        public string Prod { get; set; }
        public string Prec { get; set; }
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
        public int NumeroDeTanque { get; set; }
        public int ProductoTanque { get; set; }
        public string VolumenProductoT { get; set; }
        public string VolumenAguaT { get; set; }
        public string VolumenVacioT { get; set; }
        public Producto Producto { get; set; }
    }
    /**
     * Es el "Tipo Surtidor X" y me dice la cantidad de mangueras
     */
    public class Manguera
    {
        public Manguera()
        { }
        public int numeroDeManquera { get; set; }
        public Producto producto { get; set; }
        public Tanque tanque { get; set; }
    }
    public class Producto
    {
        public Producto()
        {
            descripcion = "";
        }

        public string numeroDeProducto { get; set; }
        public string numeroPorDespacho { get; set; }
        public string precioUnitario { get; set; }
        public string descripcion { get; set; }
    }
    public class Despacho
    {
        public Despacho()
        { }

        public enum ESTADO_SURTIDOR
        {
            DISPONIBLE,
            EN_SOLICITUD,
            DESPACHANDO,
            AUTORIZADO,
            VENTA_FINALIZADA_IMPAGA,
            DEFECTUOSO,
            ANULADO,
            DETENIDO
        }
        public ESTADO_SURTIDOR statusUltimaVenta { get; set; }
        public int nroUltimaVenta { get; set; }
        public int productoUltimaVenta { get; set; }
        public string montoUltimaVenta { get; set; }
        public string volumenUltimaVenta { get; set; }
        public string ppuUltimaVenta { get; set; }
        public bool ultimaVentaFacturada { get; set; }
        public string idUltimaVenta { get; set; }

        public ESTADO_SURTIDOR statusVentaAnterior { get; set; }
        public int nroVentaAnterior { get; set; }
        public int productoVentaAnterior { get; set; }
        public string montoVentaAnterior { get; set; }
        public string volumenVentaAnterios { get; set; }
        public string ppuVentaAnterior { get; set; }
        public bool ventaAnteriorFacturada { get; set; }
        public string idVentaAnterior { get; set; }
    }
    public class InfoDespacho
    {
        public InfoDespacho() { }
        public string ID { get; set; }
        public int Surtidor { get; set; }
        public string Producto { get; set; }
        public string Monto { get; set; }
        public string Volumen { get; set; }
        public string PPU { get; set; }
        public int Facturado { get; set; }
        public int YPFRuta { get; set; }
        public string Desc { get; set; }
    }
    public class TablaDespachos
    {
        public TablaDespachos()
        {
            InfoDespachos = new List<InfoDespacho>();
        }
        public static TablaDespachos InstanciaDespachos { get; } = new TablaDespachos();
        public List<InfoDespacho> InfoDespachos { get; set; }
    }
}
