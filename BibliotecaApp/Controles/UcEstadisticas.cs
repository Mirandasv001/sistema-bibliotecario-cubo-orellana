using System.Data;
using Microsoft.Data.Sqlite;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace BibliotecaApp
{
    public partial class UcEstadisticas : UserControl
    {
        private Panel _panelFiltros;
        private DateTimePicker _dtpDesde;
        private DateTimePicker _dtpHasta;
        private Button _btnFiltrar;

        private Panel _panelResumen;

        private Label _lblTotalVisitasTitulo;
        private Label _lblTotalVisitasValor;
        private Label _lblGenero1Titulo;
        private Label _lblGenero1Valor;
        private Label _lblGenero2Titulo;
        private Label _lblGenero2Valor;

        private Label _lblTotalPrestamosTitulo = null!;
        private Label _lblTotalPrestamosValor = null!;
        private Label _lblPrestGenero1Titulo = null!;
        private Label _lblPrestGenero1Valor = null!;
        private Label _lblPrestGenero2Titulo = null!;
        private Label _lblPrestGenero2Valor = null!;

        private Panel _pnlContenedorGraficas = null!;
        private Panel _pnlGraficaSala = null!;
        private Panel _pnlGraficaPrestamos = null!;

        private Panel[] _tarjetas = Array.Empty<Panel>();

        private const int PadIzq = 30, PadArr = 20, PadDer = 30, PadAba = 40;

        private int _valorGenero1 = 0;
        private int _valorGenero2 = 0;
        private int _valorTotal = 0;
        private string _nombreGenero1 = "Masculino";
        private string _nombreGenero2 = "Femenino";

        private int _valorPrestTotal = 0;
        private int _valorPrestGenero1 = 0;
        private int _valorPrestGenero2 = 0;
        private string _nombrePrestGenero1 = "Masculino";
        private string _nombrePrestGenero2 = "Femenino";

        private static readonly Color ColorGenero1 = Color.FromArgb(52, 152, 219);  // Azul celeste
        private static readonly Color ColorGenero2 = Color.FromArgb(231, 76, 60);   // Rojo
        private static readonly Color ColorTotal = Color.FromArgb(27, 43, 66);      
        private static Color ColorPorGenero(string? nombre)
        {
            if (string.Equals(nombre, "Total", StringComparison.OrdinalIgnoreCase)) return ColorTotal;
            if (string.Equals(nombre, "Femenino", StringComparison.OrdinalIgnoreCase)) return ColorGenero2;
            return ColorGenero1; // Masculino
        }

        private static void ActualizarAcento(Panel tarjeta, Color color)
        {
            if (tarjeta.Tag is Panel acento) acento.BackColor = color;
        }

        public UcEstadisticas()
        {
            InitializeComponent();
            ConfigurarControles();

            _lblTotalVisitasValor.Text = "0";
            _lblGenero1Valor.Text = "0";
            _lblGenero2Valor.Text = "0";
            _lblTotalPrestamosValor.Text = "0";
            _lblPrestGenero1Valor.Text = "0";
            _lblPrestGenero2Valor.Text = "0";
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.AliceBlue;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);
        }

        private void ConfigurarControles()
        {
            _panelFiltros = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = EstiloUI.Blanco,
                Padding = new Padding(15, 10, 15, 10),
                Margin = new Padding(0, 0, 0, 10)
            };

            var lblDesde = new Label
            {
                Text = "Desde:",
                AutoSize = true,
                Font = EstiloUI.Etiqueta(),
                ForeColor = EstiloUI.TextoOscuro,
                Location = new Point(15, 20)
            };

            _dtpDesde = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = EstiloUI.FuenteNormal,
                Location = new Point(75, 16),
                Width = 130,
                Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)
            };
            EstiloUI.EstilizarEntrada(_dtpDesde);

            var lblHasta = new Label
            {
                Text = "Hasta:",
                AutoSize = true,
                Font = EstiloUI.Etiqueta(),
                ForeColor = EstiloUI.TextoOscuro,
                Location = new Point(220, 20)
            };

            _dtpHasta = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = EstiloUI.FuenteNormal,
                Location = new Point(275, 16),
                Width = 130,
                Value = DateTime.Today
            };
            EstiloUI.EstilizarEntrada(_dtpHasta);

            _btnFiltrar = new Button
            {
                Text = "Filtrar",
                Location = new Point(425, 14),
                Size = new Size(110, 35),
                Font = EstiloUI.BotonPrincipal(),
                Cursor = Cursors.Hand
            };
            EstiloUI.EstilizarBotonPrimario(_btnFiltrar);
            _btnFiltrar.Click += (_, _) => CargarDatos();

            _panelFiltros.Controls.AddRange(new Control[] { lblDesde, _dtpDesde, lblHasta, _dtpHasta, _btnFiltrar });
            this.Controls.Add(_panelFiltros);

            _panelResumen = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 0, 10)
            };

            var cardTotal = CrearTarjetaResumen("Total de lectores en sala", "0", ColorTotal, out _lblTotalVisitasTitulo, out _lblTotalVisitasValor);
            var cardGenero1 = CrearTarjetaResumen(_nombreGenero1, "0", ColorPorGenero(_nombreGenero1), out _lblGenero1Titulo, out _lblGenero1Valor);
            var cardGenero2 = CrearTarjetaResumen(_nombreGenero2, "0", ColorPorGenero(_nombreGenero2), out _lblGenero2Titulo, out _lblGenero2Valor);

            var cardPrestTotal = CrearTarjetaResumen("Total Préstamos", "0", ColorTotal, out _lblTotalPrestamosTitulo, out _lblTotalPrestamosValor);
            var cardPrestGenero1 = CrearTarjetaResumen(_nombrePrestGenero1, "0", ColorPorGenero(_nombrePrestGenero1), out _lblPrestGenero1Titulo, out _lblPrestGenero1Valor);
            var cardPrestGenero2 = CrearTarjetaResumen(_nombrePrestGenero2, "0", ColorPorGenero(_nombrePrestGenero2), out _lblPrestGenero2Titulo, out _lblPrestGenero2Valor);

            _tarjetas = new[] { cardTotal, cardGenero1, cardGenero2,
                                 cardPrestTotal, cardPrestGenero1, cardPrestGenero2 };

            _panelResumen.Controls.AddRange(_tarjetas);
            _panelResumen.Resize += (_, _) => UbicarTarjetas(); // tarjetas más pequeñas: 3 por mitad
            this.Controls.Add(_panelResumen);

            _pnlContenedorGraficas = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 10, 0, 0)
            };

            _pnlGraficaSala = new Panel
            {
                BackColor = EstiloUI.Blanco,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(PadIzq, PadArr, PadDer, PadAba) // espacio para ejes y etiquetas
            };
            _pnlGraficaSala.Paint += PnlGraficaSala_Paint;

            _pnlGraficaPrestamos = new Panel
            {
                BackColor = EstiloUI.Blanco,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(PadIzq, PadArr, PadDer, PadAba)
            };
            _pnlGraficaPrestamos.Paint += PnlGraficaPrestamos_Paint;

            _pnlContenedorGraficas.Controls.AddRange(new Control[] { _pnlGraficaSala, _pnlGraficaPrestamos });
            _pnlContenedorGraficas.Resize += (_, _) => DistribuirGraficas(); 

            this.Controls.Add(_pnlContenedorGraficas);

            // Orden Z: Gráfica abajo, luego panelResumen, luego panelFiltros arriba
            _pnlContenedorGraficas.SendToBack();
            DistribuirGraficas();
        }

        private Panel CrearTarjetaResumen(string titulo, string valorInicial, Color colorAcento, out Label lblTitulo, out Label lblValor)
        {
            var panel = new Panel
            {
                Size = new Size(260, 100),
                BackColor = EstiloUI.Blanco,
                Margin = new Padding(0, 0, 20, 0)
            };

            // Línea de color acento en el borde superior
            var borderTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = colorAcento
            };
            panel.Controls.Add(borderTop);
            panel.Tag = borderTop; // acento accesible para recolorear según el género real

            lblTitulo = new Label
            {
                Text = titulo,
                Font = EstiloUI.FuenteTituloTarjeta,
                ForeColor = Color.FromArgb(120, 130, 140),
                AutoSize = true,
                Location = new Point(15, 15)
            };

            lblValor = new Label
            {
                Text = valorInicial,
                Font = EstiloUI.FuenteCifraGrande,
                ForeColor = EstiloUI.TextoOscuro,
                AutoSize = true,
                Location = new Point(15, 40)
            };

            panel.Controls.AddRange(new Control[] { lblTitulo, lblValor });

            panel.MouseEnter += (_, _) => panel.BackColor = EstiloUI.HoverSecundario;
            panel.MouseLeave += (_, _) => panel.BackColor = EstiloUI.Blanco;

            return panel;
        }

        private void UbicarTarjetas()
        {
            int mitad = _panelResumen.ClientSize.Width / 2;
            if (mitad < 150) return; 

            const int separacion = 10;
            int anchoTarjeta = (mitad - separacion * 4) / 3;
            if (anchoTarjeta < 90) anchoTarjeta = 90;

            for (int i = 0; i < _tarjetas.Length; i++)
            {
                int bloque = i / 3;          
                int dentro = i % 3;
                int x = bloque * mitad + separacion + dentro * (anchoTarjeta + separacion);
                _tarjetas[i].SetBounds(x, 0, anchoTarjeta, 100);
            }
        }

        private void DistribuirGraficas()
        {
            int ancho = _pnlContenedorGraficas.ClientSize.Width;
            int alto = _pnlContenedorGraficas.ClientSize.Height;
            if (ancho <= 0 || alto <= 0) return;

            int mitad = ancho / 2;
            const int separacion = 10;
            _pnlGraficaSala.SetBounds(0, 0, mitad - separacion / 2, alto);
            _pnlGraficaPrestamos.SetBounds(mitad + separacion / 2, 0,
                ancho - mitad - separacion / 2, alto);
        }

        public void CargarDatos()
        {
            var generosSala = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var generosPrestamos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int totalVisitas = 0;
            int totalPrestamos = 0;

            // Formato estricto yyyy-MM-dd para parámetros
            string fechaDesde = _dtpDesde.Value.ToString("yyyy-MM-dd");
            string fechaHasta = _dtpHasta.Value.ToString("yyyy-MM-dd");

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();

                // Mismo rango de fechas para ambas consultas
                comando.Parameters.AddWithValue("$desde", fechaDesde);
                comando.Parameters.AddWithValue("$hasta", fechaHasta);

                comando.CommandText = @"
                    SELECT Genero, COUNT(*)
                    FROM ControlUsuariosSala
                    WHERE Fecha BETWEEN $desde AND $hasta
                    GROUP BY Genero;";

                using (var lector = comando.ExecuteReader())
                    AcumularPorGenero(lector, generosSala, ref totalVisitas);

                comando.CommandText = @"
                    SELECT Genero, COUNT(*)
                    FROM PrestamosExternos
                    WHERE FechaPrestamo BETWEEN $desde AND $hasta
                    GROUP BY Genero;";

                using (var lector = comando.ExecuteReader())
                    AcumularPorGenero(lector, generosPrestamos, ref totalPrestamos);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar estadísticas: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            var topSala = generosSala.OrderByDescending(kvp => kvp.Value).Take(2).ToList();
            _nombreGenero1 = topSala.Count > 0 ? topSala[0].Key : "Masculino";
            _valorGenero1 = topSala.Count > 0 ? topSala[0].Value : 0;
            _nombreGenero2 = topSala.Count > 1 ? topSala[1].Key : "Femenino";
            _valorGenero2 = topSala.Count > 1 ? topSala[1].Value : 0;
            _valorTotal = totalVisitas;

            var topPrestamos = generosPrestamos.OrderByDescending(kvp => kvp.Value).Take(2).ToList();
            _nombrePrestGenero1 = topPrestamos.Count > 0 ? topPrestamos[0].Key : "Masculino";
            _valorPrestGenero1 = topPrestamos.Count > 0 ? topPrestamos[0].Value : 0;
            _nombrePrestGenero2 = topPrestamos.Count > 1 ? topPrestamos[1].Key : "Femenino";
            _valorPrestGenero2 = topPrestamos.Count > 1 ? topPrestamos[1].Value : 0;
            _valorPrestTotal = totalPrestamos;

            _lblTotalVisitasValor.Text = totalVisitas.ToString("N0");
            _lblGenero1Titulo.Text = _nombreGenero1;
            _lblGenero1Valor.Text = _valorGenero1.ToString("N0");
            _lblGenero2Titulo.Text = _nombreGenero2;
            _lblGenero2Valor.Text = _valorGenero2.ToString("N0");

            _lblTotalPrestamosValor.Text = totalPrestamos.ToString("N0");
            _lblPrestGenero1Titulo.Text = _nombrePrestGenero1;
            _lblPrestGenero1Valor.Text = _valorPrestGenero1.ToString("N0");
            _lblPrestGenero2Titulo.Text = _nombrePrestGenero2;
            _lblPrestGenero2Valor.Text = _valorPrestGenero2.ToString("N0");

            // Acento de cada tarjeta con el color FIJO del género que representa
            if (_tarjetas.Length >= 6)
            {
                ActualizarAcento(_tarjetas[0], ColorTotal);
                ActualizarAcento(_tarjetas[1], ColorPorGenero(_nombreGenero1));
                ActualizarAcento(_tarjetas[2], ColorPorGenero(_nombreGenero2));
                ActualizarAcento(_tarjetas[3], ColorTotal);
                ActualizarAcento(_tarjetas[4], ColorPorGenero(_nombrePrestGenero1));
                ActualizarAcento(_tarjetas[5], ColorPorGenero(_nombrePrestGenero2));
            }

            // Forzar repintado de AMBAS gráficas desde cero
            _pnlGraficaSala.Invalidate();
            _pnlGraficaPrestamos.Invalidate();
        }

        private static void AcumularPorGenero(IDataReader lector,
            Dictionary<string, int> acumulado, ref int total)
        {
            while (lector.Read())
            {
                string genero = lector.IsDBNull(0) ? "" : lector.GetString(0).Trim();
                if (genero.Length == 0) genero = "(Sin dato)";

                int cantidad = lector.GetInt32(1);
                acumulado[genero] = acumulado.TryGetValue(genero, out int previo)
                    ? previo + cantidad
                    : cantidad;

                total += cantidad;
            }
        }

        private void PnlGraficaSala_Paint(object? sender, PaintEventArgs e)
        {
            DibujarGraficaBarras(e.Graphics, DatosBarrasSala(), "Visitas a Sala por Género",
                _pnlGraficaSala.Width, _pnlGraficaSala.Height);
        }

        private void PnlGraficaPrestamos_Paint(object? sender, PaintEventArgs e)
        {
            DibujarGraficaBarras(e.Graphics, DatosBarrasPrestamos(), "Préstamos Externos por Género",
                _pnlGraficaPrestamos.Width, _pnlGraficaPrestamos.Height);
        }

        private Dictionary<string, int> DatosBarrasSala()
        {
            var datos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            datos["Total"] = _valorTotal;
            if (!datos.ContainsKey(_nombreGenero1)) datos[_nombreGenero1] = _valorGenero1;
            if (!datos.ContainsKey(_nombreGenero2)) datos[_nombreGenero2] = _valorGenero2;
            return datos;
        }

        private Dictionary<string, int> DatosBarrasPrestamos()
        {
            var datos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            datos["Total"] = _valorPrestTotal;
            if (!datos.ContainsKey(_nombrePrestGenero1)) datos[_nombrePrestGenero1] = _valorPrestGenero1;
            if (!datos.ContainsKey(_nombrePrestGenero2)) datos[_nombrePrestGenero2] = _valorPrestGenero2;
            return datos;
        }

        private void DibujarGraficaBarras(Graphics g, Dictionary<string, int> datos, string titulo, int ancho, int alto)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var entradas = datos.ToList(); 

            int areaWidth = ancho - PadIzq - PadDer;
            int areaHeight = alto - PadArr - PadAba;
            int baseY = alto - PadAba; 
            if (areaWidth < 40) areaWidth = 40;
            if (areaHeight < 40) areaHeight = 40;

            // Colores
            Color colorEje = Color.FromArgb(180, 180, 180);
            Color colorGrid = Color.FromArgb(230, 230, 230);
            Color colorTexto = EstiloUI.TextoOscuro;
            Color colorTextoSecundario = Color.FromArgb(120, 130, 140);

            var fontValor = EstiloUI.FuenteGraficaValor;
            var fontCategoria = EstiloUI.FuenteGraficaCategoria;
            var fontTitulo = EstiloUI.FuenteGraficaTitulo;
            var fontNoDatos = EstiloUI.FuenteSinDatos;

            // Pinceles
            using var brushTexto = new SolidBrush(colorTexto);
            using var brushTextoSec = new SolidBrush(colorTextoSecundario);

            // Plumas
            using var penEje = new Pen(colorEje, 1);
            using var penGrid = new Pen(colorGrid, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };

            // Título de la gráfica
            SizeF szTitulo = g.MeasureString(titulo, fontTitulo);
            g.DrawString(titulo, fontTitulo, brushTexto,
                PadIzq + (areaWidth - szTitulo.Width) / 2, PadArr - 5);

            if (entradas.Count == 0 || entradas.All(kv => kv.Value == 0))
            {
                string msg = "No hay registros";
                SizeF szMsg = g.MeasureString(msg, fontNoDatos);
                g.DrawString(msg, fontNoDatos, brushTextoSec,
                    PadIzq + (areaWidth - szMsg.Width) / 2,
                    PadArr + (areaHeight - szMsg.Height) / 2);
                return;
            }

            int maxValor = entradas.Max(kv => kv.Value);

            int espacioTextoArriba = (int)Math.Ceiling(g.MeasureString("0", fontValor).Height) + 8;
            const int margenSeguridad = 8;
            int margenSuperiorTotal = espacioTextoArriba + margenSeguridad;

            int maxAlturaBarra = areaHeight - margenSuperiorTotal;
            if (maxAlturaBarra < 10) maxAlturaBarra = 10;

            float escalaY = maxValor > 0 ? (float)maxAlturaBarra / maxValor : 1f;

            // Y mínimo que puede alcanzar el tope de una barra
            int minTopY = PadArr + margenSuperiorTotal;

            int numGridLines = 4;
            for (int i = 1; i <= numGridLines; i++)
            {
                float y = baseY - (maxAlturaBarra * i / (float)numGridLines);
                g.DrawLine(penGrid, PadIzq, y, PadIzq + areaWidth, y);
            }

            g.DrawLine(penEje, PadIzq, baseY, PadIzq + areaWidth, baseY);

            int numBarras = entradas.Count;
            int espacioEntreBarras = 30;
            int anchoBarra = Math.Min(70, (areaWidth - espacioEntreBarras * (numBarras + 1)) / numBarras);
            if (anchoBarra < 10) anchoBarra = 10;
            int inicioX = PadIzq + (areaWidth - (anchoBarra * numBarras + espacioEntreBarras * (numBarras - 1))) / 2;

            for (int i = 0; i < numBarras; i++)
            {
                string etiqueta = entradas[i].Key;
                int valor = entradas[i].Value;

                Color colorBarra = ColorPorGenero(etiqueta);
                using var brushBarra = new SolidBrush(colorBarra);

                int xBarra = inicioX + i * (anchoBarra + espacioEntreBarras);

                // Altura proporcional, nunca supera maxAlturaBarra
                int alturaBarra = (int)(valor * escalaY);
                if (alturaBarra > maxAlturaBarra) alturaBarra = maxAlturaBarra;

                int topY = baseY - alturaBarra;
                if (topY < minTopY) topY = minTopY;

                var rectBarra = new Rectangle(xBarra, topY, anchoBarra, alturaBarra);

                using (var path = GetRoundedRectPath(rectBarra, 6, true))
                {
                    g.FillPath(brushBarra, path);
                }

                if (valor > 0)
                {
                    string valorStr = valor.ToString("N0");
                    SizeF szValor = g.MeasureString(valorStr, fontValor);
                    float textY = topY - szValor.Height - 4;
                    if (textY < PadArr) textY = PadArr;
                    g.DrawString(valorStr, fontValor, brushTexto,
                        xBarra + (anchoBarra - szValor.Width) / 2, textY);
                }

                // Dibujar etiqueta de categoría debajo del eje
                SizeF szCat = g.MeasureString(etiqueta, fontCategoria);
                g.DrawString(etiqueta, fontCategoria, brushTexto,
                    xBarra + (anchoBarra - szCat.Width) / 2, baseY + 8);
            }
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius, bool soloSuperior)
        {
            var path = new GraphicsPath();
            int d = radius * 2;

            if (soloSuperior)
            {
                // Esquina superior izquierda
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                // Esquina superior derecha
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
                path.AddLine(rect.X, rect.Bottom, rect.X, rect.Y + radius);
            }            else
            {
                // Todas las esquinas redondeadas
                path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            }

            path.CloseFigure();
            return path;
        }
    }
}