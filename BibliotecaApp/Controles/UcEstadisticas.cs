using System.Data;
using Microsoft.Data.Sqlite;
using System.Drawing.Drawing2D;

namespace BibliotecaApp
{
    /// <summary>
    /// Módulo de Estadísticas: muestra gráfica de barras de visitas por género
    /// con filtros de rango de fechas sobre la tabla ControlUsuariosSala.
    /// Implementación 100% nativa sin dependencias externas (pintado GDI+).
    /// </summary>
    public partial class UcEstadisticas : UserControl
    {
        // Controles de la interfaz (sin readonly para permitir asignación en ConfigurarControles)
        private Panel _panelFiltros;
        private DateTimePicker _dtpDesde;
        private DateTimePicker _dtpHasta;
        private Button _btnFiltrar;

        private Panel _panelResumen;

        // Labels (se asignan vía 'out' en CrearTarjetaResumen)
        private Label _lblTotalVisitasTitulo;
        private Label _lblTotalVisitasValor;
        private Label _lblMasculinoTitulo;
        private Label _lblMasculinoValor;
        private Label _lblFemeninoTitulo;
        private Label _lblFemeninoValor;

        // Panel para la gráfica custom (pintado en Paint)
        private Panel _pnlGrafica;

        // Datos para el pintado (no readonly, se modifican en CargarDatos)
        private int _valorMasculino = 0;
        private int _valorFemenino = 0;
        private int _valorTotal = 0;

        // Contadores de la consulta actual (se reinician en cada CargarDatos)
        private int totalMasculino = 0;
        private int totalFemenino = 0;
        private int totalVisitas = 0;

        public UcEstadisticas()
        {
            InitializeComponent();
            ConfigurarControles();

            // ============================================================
            // LIMPIAR LABELS DE INICIO: forzar textos en "0" al arrancar
            // ============================================================
            _lblTotalVisitasValor.Text = "0";
            _lblMasculinoValor.Text = "0";
            _lblFemeninoValor.Text = "0";
        }

        private void InitializeComponent()
        {
            this.BackColor = EstiloUI.FondoClaro;
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);
        }

        private void ConfigurarControles()
        {
            // =================================================================
            // PANEL SUPERIOR - FILTROS
            // =================================================================
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
                Font = new Font(EstiloUI.FuenteBase, 10F),
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
                Font = new Font(EstiloUI.FuenteBase, 10F),
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

            // =================================================================
            // PANEL DE RESUMEN - TARJETAS
            // =================================================================
            _panelResumen = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 0, 10)
            };

            // Tarjeta Total Visitas
            var cardTotal = CrearTarjetaResumen("Total Visitas", "0", EstiloUI.Acento, out _lblTotalVisitasTitulo, out _lblTotalVisitasValor);
            cardTotal.Location = new Point(0, 0);

            // Tarjeta Masculino
            var cardMasc = CrearTarjetaResumen("Masculino", "0", Color.FromArgb(52, 152, 219), out _lblMasculinoTitulo, out _lblMasculinoValor);
            cardMasc.Location = new Point(280, 0);

            // Tarjeta Femenino
            var cardFem = CrearTarjetaResumen("Femenino", "0", Color.FromArgb(231, 76, 60), out _lblFemeninoTitulo, out _lblFemeninoValor);
            cardFem.Location = new Point(560, 0);

            _panelResumen.Controls.AddRange(new Control[] { cardTotal, cardMasc, cardFem });
            this.Controls.Add(_panelResumen);

            // =================================================================
            // PANEL GRÁFICA - PINTADO NATIVO GDI+
            // =================================================================
            _pnlGrafica = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = EstiloUI.Blanco,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 10, 0, 0),
                Padding = new Padding(30, 20, 30, 40) // Espacio para ejes y etiquetas
            };
            _pnlGrafica.Paint += PnlGrafica_Paint;
            this.Controls.Add(_pnlGrafica);

            // Orden Z: Gráfica abajo, luego panelResumen, luego panelFiltros arriba
            _pnlGrafica.SendToBack();
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

            lblTitulo = new Label
            {
                Text = titulo,
                Font = new Font(EstiloUI.FuenteBase, 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 130, 140),
                AutoSize = true,
                Location = new Point(15, 15)
            };

            lblValor = new Label
            {
                Text = valorInicial,
                Font = new Font(EstiloUI.FuenteBase, 28F, FontStyle.Bold),
                ForeColor = EstiloUI.TextoOscuro,
                AutoSize = true,
                Location = new Point(15, 40)
            };

            panel.Controls.AddRange(new Control[] { lblTitulo, lblValor });

            // Efecto hover sutil
            panel.MouseEnter += (_, _) => panel.BackColor = EstiloUI.HoverSecundario;
            panel.MouseLeave += (_, _) => panel.BackColor = EstiloUI.Blanco;

            return panel;
        }

        /// <summary>
        /// Carga los datos desde la BD y actualiza tarjetas y gráfica.
        /// Método público para ser llamado desde Form1 al mostrar la vista.
        /// </summary>
        public void CargarDatos()
        {
            // ============================================================
            // PASO 1: REINICIAR VARIABLES (lo primero, obligatorio)
            // ============================================================
            totalMasculino = 0;
            totalFemenino = 0;
            totalVisitas = 0;

            // Formato estricto yyyy-MM-dd para parámetros
            string fechaDesde = _dtpDesde.Value.ToString("yyyy-MM-dd");
            string fechaHasta = _dtpHasta.Value.ToString("yyyy-MM-dd");

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();

                // ============================================================
                // PASO 2: CONSULTA SQL CON FILTRO DE FECHAS
                // ============================================================
                comando.CommandText = @"
                    SELECT Genero, COUNT(*)
                    FROM ControlUsuariosSala
                    WHERE Fecha BETWEEN $desde AND $hasta
                    GROUP BY Genero;";

                comando.Parameters.AddWithValue("$desde", fechaDesde);
                comando.Parameters.AddWithValue("$hasta", fechaHasta);

                // ============================================================
                // PASO 3: LEER Y ACUMULAR (if/else sobre columna "Genero")
                // ============================================================
                using var lector = comando.ExecuteReader();
                while (lector.Read())
                {
                    string genero = lector.GetString(0);
                    int cantidad = lector.GetInt32(1);

                    if (genero.Equals("Masculino", StringComparison.OrdinalIgnoreCase))
                        totalMasculino += cantidad;
                    else if (genero.Equals("Femenino", StringComparison.OrdinalIgnoreCase))
                        totalFemenino += cantidad;
                }

                // Total general = suma de ambos
                totalVisitas = totalMasculino + totalFemenino;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar estadísticas: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // ============================================================
            // PASO 4: ASIGNAR A CAMPOS DE INSTANCIA, ACTUALIZAR LABELS Y DIBUJAR
            // ============================================================
            _valorMasculino = totalMasculino;
            _valorFemenino = totalFemenino;
            _valorTotal = totalVisitas;

            _lblTotalVisitasValor.Text = totalVisitas.ToString("N0");
            _lblMasculinoValor.Text = totalMasculino.ToString("N0");
            _lblFemeninoValor.Text = totalFemenino.ToString("N0");

            // Forzar repintado de la gráfica desde cero
            _pnlGrafica.Invalidate();
        }

        /// <summary>
        /// Evento Paint: dibuja las barras, valores y etiquetas nativamente con escalado correcto.
        /// </summary>
        private void PnlGrafica_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var clientRect = _pnlGrafica.ClientRectangle;
            int paddingLeft = _pnlGrafica.Padding.Left;
            int paddingRight = _pnlGrafica.Padding.Right;
            int paddingTop = _pnlGrafica.Padding.Top;
            int paddingBottom = _pnlGrafica.Padding.Bottom;

            int areaWidth = clientRect.Width - paddingLeft - paddingRight;
            int areaHeight = clientRect.Height - paddingTop - paddingBottom;
            int baseY = clientRect.Height - paddingBottom; // Línea base (eje X)

            // Colores
            Color colorMasc = Color.FromArgb(52, 152, 219);   // Azul
            Color colorFem = Color.FromArgb(231, 76, 60);      // Rojo coral
            Color colorEje = Color.FromArgb(180, 180, 180);
            Color colorTexto = EstiloUI.TextoOscuro;
            Color colorTextoSecundario = Color.FromArgb(120, 130, 140);

            // Fuentes
            using var fontValor = new Font(EstiloUI.FuenteBase, 11F, FontStyle.Bold);
            using var fontCategoria = new Font(EstiloUI.FuenteBase, 10F, FontStyle.Bold);
            using var fontTitulo = new Font(EstiloUI.FuenteBase, 12F, FontStyle.Bold);
            using var fontNoDatos = new Font(EstiloUI.FuenteBase, 11F, FontStyle.Italic);

            // Pinceles
            using var brushTexto = new SolidBrush(colorTexto);
            using var brushTextoSec = new SolidBrush(colorTextoSecundario);
            using var brushMasc = new SolidBrush(colorMasc);
            using var brushFem = new SolidBrush(colorFem);

            // Pluma para eje
            using var penEje = new Pen(colorEje, 1);

            // Título de la gráfica
            string tituloGrafica = "Visitas a Sala por Género";
            SizeF szTitulo = g.MeasureString(tituloGrafica, fontTitulo);
            g.DrawString(tituloGrafica, fontTitulo, brushTexto,
                paddingLeft + (areaWidth - szTitulo.Width) / 2, paddingTop - 5);

            // ---- CASO SIN DATOS (evita dibujar gráfica fantasma) ----
            if (totalMasculino == 0 && totalFemenino == 0)
            {
                string msg = "No hay registros";
                SizeF szMsg = g.MeasureString(msg, fontNoDatos);
                g.DrawString(msg, fontNoDatos, brushTextoSec,
                    paddingLeft + (areaWidth - szMsg.Width) / 2,
                    paddingTop + (areaHeight - szMsg.Height) / 2);
                return;
            }

            // ---- CONFIGURACIÓN DE BARRAS ----
            int numBarras = 2;
            int espacioEntreBarras = 40;
            int anchoBarra = Math.Min(80, (areaWidth - espacioEntreBarras * (numBarras + 1)) / numBarras);
            int inicioX = paddingLeft + (areaWidth - (anchoBarra * numBarras + espacioEntreBarras * (numBarras - 1))) / 2;

            int maxValor = Math.Max(_valorMasculino, _valorFemenino);

            // Margen superior estricto (espacio para el número encima de la barra más alta)
            const int margenSuperior = 40;
            int maxAlturaBarra = areaHeight - margenSuperior;
            if (maxAlturaBarra < 10) maxAlturaBarra = 10; // seguridad

            float escalaY = maxValor > 0 ? (float)maxAlturaBarra / maxValor : 1f;

            // Dibujar eje X (línea base)
            g.DrawLine(penEje, paddingLeft, baseY, paddingLeft + areaWidth, baseY);

            // Datos de las barras
            var barras = new[]
            {
                new { Label = "Masculino", Valor = _valorMasculino, Brush = brushMasc, X = inicioX },
                new { Label = "Femenino", Valor = _valorFemenino, Brush = brushFem, X = inicioX + anchoBarra + espacioEntreBarras }
            };

            foreach (var barra in barras)
            {
                // Altura proporcional, nunca supera maxAlturaBarra
                int alturaBarra = (int)(barra.Valor * escalaY);
                if (alturaBarra > maxAlturaBarra) alturaBarra = maxAlturaBarra;

                int topY = baseY - alturaBarra;

                // Rectángulo de la barra
                var rectBarra = new Rectangle(barra.X, topY, anchoBarra, alturaBarra);

                // Dibujar barra con esquinas redondeadas arriba
                using (var path = GetRoundedRectPath(rectBarra, 6, true))
                {
                    g.FillPath(barra.Brush, path);
                }

                // Dibujar valor encima de la barra (flotando 4px sobre la esquina redondeada)
                if (barra.Valor > 0)
                {
                    string valorStr = barra.Valor.ToString("N0");
                    SizeF szValor = g.MeasureString(valorStr, fontValor);
                    float textY = topY - szValor.Height - 4;
                    // Seguridad: nunca por encima del paddingTop
                    if (textY < paddingTop) textY = paddingTop;
                    g.DrawString(valorStr, fontValor, brushTexto,
                        barra.X + (anchoBarra - szValor.Width) / 2,
                        textY);
                }

                // Dibujar etiqueta de categoría debajo del eje
                SizeF szCat = g.MeasureString(barra.Label, fontCategoria);
                g.DrawString(barra.Label, fontCategoria, brushTexto,
                    barra.X + (anchoBarra - szCat.Width) / 2,
                    baseY + 8);
            }
        }

        /// <summary>
        /// Crea un GraphicsPath con esquinas redondeadas solo en la parte superior.
        /// </summary>
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
                // Esquina inferior derecha (recta)
                path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
                // Esquina inferior izquierda (recta)
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