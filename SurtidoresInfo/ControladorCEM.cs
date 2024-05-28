using System;
using System.Collections.Generic;
using System.Data;

namespace SurtidoresInfo
{
    internal class ControladorCEM : Controlador
    {
        private readonly ConectorCEM conectorCEM;
        public ControladorCEM()
        {
            conectorCEM = new ConectorCEM();
            GrabarConfigEstacion();
            GrabarTanques();
        }
        public override void GrabarConfigEstacion()
        {
            try
            {
                Estacion estacion = conectorCEM.ConfiguracionDeLaEstacion();
                List<Surtidor> tempSurtidores = estacion.nivelesDePrecio[0];
                foreach (Surtidor surtidor in tempSurtidores)
                {
                    string campos = "IdSurtidor,Manguera,Producto,Precio,DescProd";
                    List<Manguera> tempManguera = surtidor.mangueras;
                    foreach (Manguera manguera in tempManguera)
                    {
                        string letra = null;
                        switch (manguera.numeroDeManquera)
                        {
                            case 1:
                                letra = "A";
                                break;
                            case 2:
                                letra = "B";
                                break;
                            case 3:
                                letra = "C";
                                break;
                            case 4:
                                letra = "D";
                                break;
                        }
                        string rows = string.Format("{0},'{1}','{2}','{3}','{4}'",
                            surtidor.numeroDeSurtidor,
                            letra,
                            manguera.producto.numeroDeProducto,
                            manguera.producto.precioUnitario.ToString(),
                            manguera.producto.descripcion);

                        DataTable tabla = ConectorSQLite.dt_query("SELECT * FROM Surtidores WHERE IdSurtidor = " + surtidor.numeroDeSurtidor + " AND Manguera = '" + letra + "'");

                        _ = tabla.Rows.Count == 0
                            ? ConectorSQLite.query(string.Format("INSERT INTO Surtidores ({0}) VALUES ({1})", campos, rows))
                            : ConectorSQLite.query(string.Format("UPDATE Surtidores SET Producto = ('{0}'), Precio = ('{1}'), DescProd = ('{2}') WHERE IdSurtidor = ({3}) AND Manguera = ('{4}')",
                                manguera.producto.numeroDeProducto,
                                manguera.producto.precioUnitario.ToString(),
                                manguera.producto.descripcion,
                                surtidor.numeroDeSurtidor,
                                letra));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error en el metodo GrabarConfigEstacion. Excepcion: {e.Message}");
            }
        }
        /*
         * Metodo para obtener la informacion de los despachos
         */
        public override void GrabarDespachos()
        {
            try
            {
                for (int i = 1; i < Estacion.InstanciaEstacion.numeroDeSurtidores + 1; i++)
                {
                    Despacho despacho = conectorCEM.InformacionDeSurtidor(i);
                    List<InfoDespacho> infoDespachos = TablaDespachos.InstanciaDespachos.InfoDespachos;

                    if (despacho.nroUltimaVenta == 0 || despacho.idUltimaVenta == null || despacho.idUltimaVenta == "")
                    {
                        continue;
                    }

                    DataTable tabla = ConectorSQLite.dt_query("SELECT * FROM despachos WHERE id = '" + despacho.idUltimaVenta + "' AND surtidor = " + i);

                    /// Procesamiento de la ultima venta
                    if (tabla.Rows.Count == 0)
                    {
                        InfoDespacho infoDespacho = new InfoDespacho
                        {
                            ID = despacho.idUltimaVenta,
                            Surtidor = i,
                            Producto = "",
                            Monto = despacho.montoUltimaVenta,
                            Volumen = despacho.volumenUltimaVenta,
                            PPU = despacho.ppuUltimaVenta,
                            Facturado = Convert.ToInt32(despacho.ultimaVentaFacturada),
                            YPFRuta = 0,
                            Desc = ""
                        };

                        foreach (Producto p in Estacion.InstanciaEstacion.productos)
                        {
                            if (p.precioUnitario == infoDespacho.PPU)
                            {
                                if (p.numeroPorDespacho == null || p.numeroPorDespacho == "")
                                {
                                    p.numeroPorDespacho = despacho.productoUltimaVenta.ToString();
                                }
                                infoDespacho.Producto = p.numeroDeProducto;
                                infoDespacho.Desc = p.descripcion;
                                if (despacho.ultimaVentaFacturada)
                                {
                                    infoDespacho.YPFRuta = 1;
                                }
                                break;
                            }
                        }
                        if (infoDespacho.Producto == "")
                        {
                            foreach (Producto p in Estacion.InstanciaEstacion.productos)
                            {
                                if (p.numeroPorDespacho != null && p.numeroPorDespacho != "" && p.numeroPorDespacho == despacho.productoUltimaVenta.ToString())
                                {
                                    infoDespacho.Producto = p.numeroPorDespacho;
                                    infoDespacho.Desc = p.descripcion;
                                }
                                else
                                {
                                    infoDespacho.Producto = despacho.productoUltimaVenta.ToString();
                                }
                                infoDespacho.YPFRuta = 1;
                            }
                        }
                        TablaDespachos.InstanciaDespachos.InfoDespachos.Add(infoDespacho);

                        /// Agregar a Base de Datos
                        string campos = "id,surtidor,producto,monto,volumen,PPU,facturado,YPFruta,DesProd";
                        string rows = string.Format("'{0}',{1},'{2}','{3}','{4}','{5}',{6},{7},'{8}'",
                            infoDespacho.ID,
                            infoDespacho.Surtidor,
                            infoDespacho.Producto,
                            infoDespacho.Monto,
                            infoDespacho.Volumen,
                            infoDespacho.PPU,
                            infoDespacho.Facturado,
                            infoDespacho.YPFRuta,
                            infoDespacho.Desc);
                        _ = ConectorSQLite.query(string.Format("INSERT INTO despachos ({0}) VALUES ({1})", campos, rows));
                    }

                    tabla = ConectorSQLite.dt_query("SELECT * FROM despachos WHERE id = '" + despacho.idVentaAnterior + "' AND surtidor = " + i);

                    /// Procesamiento de la venta anterior
                    if (tabla.Rows.Count == 0)
                    {
                        InfoDespacho infoDespacho = new InfoDespacho
                        {
                            ID = despacho.idVentaAnterior,
                            Surtidor = i,
                            Producto = "",
                            Monto = despacho.montoVentaAnterior,
                            Volumen = despacho.volumenVentaAnterios,
                            PPU = despacho.ppuVentaAnterior,
                            YPFRuta = 0,
                            Desc = ""
                        };
                        foreach (Producto p in Estacion.InstanciaEstacion.productos)
                        {
                            if (p.precioUnitario == infoDespacho.PPU)
                            {
                                if (p.numeroPorDespacho == null || p.numeroPorDespacho == "")
                                {
                                    p.numeroPorDespacho = despacho.productoVentaAnterior.ToString();
                                }
                                infoDespacho.Producto = p.numeroDeProducto;
                                infoDespacho.Desc = p.descripcion;
                                if (despacho.ventaAnteriorFacturada)
                                {
                                    infoDespacho.YPFRuta = 1;
                                }
                                break;
                            }
                        }
                        if (infoDespacho.Producto == "")
                        {
                            foreach (Producto p in Estacion.InstanciaEstacion.productos)
                            {
                                if (p.numeroPorDespacho != null && p.numeroPorDespacho != "" && p.numeroPorDespacho == despacho.productoVentaAnterior.ToString())
                                {
                                    infoDespacho.Producto = p.numeroPorDespacho;
                                    infoDespacho.Desc = p.descripcion;
                                }
                                else
                                {
                                    infoDespacho.Producto = despacho.productoVentaAnterior.ToString();
                                }
                                infoDespacho.YPFRuta = 1;
                            }
                        }
                        TablaDespachos.InstanciaDespachos.InfoDespachos.Add(infoDespacho);
                        /// Agregar a Base de Datos
                        string campos = "id,surtidor,producto,monto,volumen,PPU,facturado,YPFruta,DesProd";
                        string rows = string.Format("'{0}',{1},'{2}','{3}','{4}','{5}',{6},{7},'{8}'",
                            infoDespacho.ID,
                            infoDespacho.Surtidor,
                            infoDespacho.Producto,
                            infoDespacho.Monto,
                            infoDespacho.Volumen,
                            infoDespacho.PPU,
                            infoDespacho.Facturado,
                            infoDespacho.YPFRuta,
                            infoDespacho.Desc);
                        _ = ConectorSQLite.query(string.Format("INSERT INTO despachos ({0}) VALUES ({1})", campos, rows));
                    }

                    if (TablaDespachos.InstanciaDespachos.InfoDespachos.Count == 50)
                    {
                        for (int filas = 0; filas < 10; filas++)
                        {
                            TablaDespachos.InstanciaDespachos.InfoDespachos.RemoveAt(0);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Error en el metodo GrabarDespachos. Excepcion: {e.Message}");
            }
        }

        public override void GrabarTanques()
        {
            try{
                List<Tanque> tanques = conectorCEM.InformacionDeTanque(Estacion.InstanciaEstacion.tanques.Count);

                for (int i = 0; i < tanques.Count; i++)
                {
                    int res = ConectorSQLite.query("UPDATE Tanques SET volumen = '" + tanques[i].VolumenProductoT + 
                        "" + "', total = '" + (Convert.ToDouble(tanques[i].VolumenProductoT) + Convert.ToDouble(tanques[i].VolumenVacioT) + Convert.ToDouble(tanques[i].VolumenAguaT)).ToString() +
                        "" + "' WHERE id = " + tanques[i].NumeroDeTanque);

                    if (res == 0)
                    {
                        string campos = "id,volumen,total";
                        string rows = string.Format("{0},'{1}','{2}'",
                            tanques[i].NumeroDeTanque,
                            tanques[i].VolumenProductoT,
                            (Convert.ToDouble(tanques[i].VolumenProductoT) + Convert.ToDouble(tanques[i].VolumenVacioT) + Convert.ToDouble(tanques[i].VolumenAguaT)).ToString());
                        _ = ConectorSQLite.query(string.Format("INSERT INTO Tanques ({0}) VALUES ({1})", campos, rows));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error en el método traer tanques. Excepcion: " + e.Message);
            }
        }
    }
}
