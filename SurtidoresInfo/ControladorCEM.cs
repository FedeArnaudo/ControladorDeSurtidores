﻿using System;
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
            Estacion estacion = conectorCEM.ConfiguracionDeLaEstacion();
            try
            {
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
        public override void GrabarCierre()
        {
            ConectorSQLite.query("DELETE FROM cierreBandera");
            CierreDeTurno cierreDeTurno = conectorCEM.InfoTurnoEnCurso();
            try
            {
                string query = "INSERT INTO cierres (monto_contado, volumen_contado, monto_YPFruta, volumen_YPFruta) VALUES ({0})";
                string valores = String.Format(
                    "'{0}','{1}','{2}','{3}'",
                    cierreDeTurno.TotalesMediosDePago[0].TotalMonto.Trim().Substring(1),
                    cierreDeTurno.TotalesMediosDePago[0].TotalVolumen.Trim(),
                    cierreDeTurno.TotalesMediosDePago[3].TotalMonto.Trim(),
                    cierreDeTurno.TotalesMediosDePago[3].TotalVolumen.Trim());

                query = String.Format(query, valores);

                ConectorSQLite.query(query);

                // Traer ID del cierre para poder referenciar los detalles
                query = "SELECT max(id) FROM cierres";

                DataTable table = ConectorSQLite.dt_query(query);

                int id = Convert.ToInt32(table.Rows[0][0]);

                // Grabar cierresxproducto
                query = "INSERT INTO cierresxProducto (id, producto, monto, volumen) VALUES (" + id + ", {0})";
                for (int i = 0; i < cierreDeTurno.TotalesPorProductosPorNivelesPorPeriodo[0][0].Count; i++)
                {
                    string aux =
                        (i + 1).ToString() + "," +
                        "'" + cierreDeTurno.TotalesPorProductosPorNivelesPorPeriodo[0][0][i].TotalMonto.Trim() + "'," +
                        "'" + cierreDeTurno.TotalesPorProductosPorNivelesPorPeriodo[0][0][i].TotalVolumen.Trim() + "'";

                    aux = String.Format(query, aux);

                    ConectorSQLite.query(aux);
                }

                Estacion estacion = Estacion.InstanciaEstacion;
                // Grabar cierresxmanguera
                query = "INSERT INTO cierresxManguera (id, surtidor, manguera, monto, volumen) VALUES (" + id + ", {0})";
                int contador = 0;
                for (int i = 0; i < estacion.numeroDeSurtidores; i++)
                {
                    List<Surtidor> surtidores = estacion.nivelesDePrecio[0];
                    for (int j = 0; j < surtidores[i].tipoDeSurtidor; j++)
                    {
                        string aux =
                            (i + 1).ToString() + "," +
                            (j + 1).ToString() + "," +
                            "'" + cierreDeTurno.TotalPorMangueras[contador].TotalVntasMonto.Trim() + "'," +
                            "'" + cierreDeTurno.TotalPorMangueras[contador].TotalVntasVolumen.Trim() + "'";

                        contador++;

                        aux = String.Format(query, aux);

                        ConectorSQLite.query(aux);
                    }
                }
                ConectorSQLite.query("DELETE FROM despachos");
            }
            catch (Exception e)
            {
                throw new Exception($"Error en el metodo GrabarCierre. Excepcion: {e.Message}");
            }
        }
        /*
         * Metodo para procesar la informacion que traer el CEM y la guarda en la base
         */
        public override void GrabarDespachos()
        {
            for (int i = 1; i < Estacion.InstanciaEstacion.numeroDeSurtidores + 1; i++)
            {
                Despacho despacho = conectorCEM.InfoDeSurtidor(i);
                try
                {
                    //List<InfoDespacho> infoDespachos = TablaDespachos.InstanciaDespachos.InfoDespachos;

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
                        // Verificamos YPF en Ruta
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
                        //TablaDespachos.InstanciaDespachos.InfoDespachos.Add(infoDespacho);

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

                    if (despacho.idVentaAnterior == null || despacho.idVentaAnterior == "")
                    {
                        continue;
                    }

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
                        // Verificamos YPF en Ruta
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
                        //TablaDespachos.InstanciaDespachos.InfoDespachos.Add(infoDespacho);
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
                    DataTable cantidadDeFilas = ConectorSQLite.dt_query("SELECT * FROM despachos");
                    if (cantidadDeFilas.Rows.Count >= 50)
                    {
                        _ = ConectorSQLite.query(@"DELETE FROM despachos WHERE id IN(SELECT id FROM despachos ORDER BY fecha LIMIT 40)");
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Error en el metodo GrabarDespachos. Excepcion: {e.Message}");
                }
            }
        }

        public override void GrabarTanques()
        {
            List<Tanque> tanques = conectorCEM.InfoDeTanques(Estacion.InstanciaEstacion.tanques.Count);
            try
            {
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
