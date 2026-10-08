using System.Data;
using Microsoft.Data.Sqlite;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace BibliotecaApp
{
    /// <summary>
    /// Módulo de Estadísticas: muestra gráfica de barras de visitas por género
    /// con filtros de rango de fechas sobre la tabla ControlUsuariosSala.
    /// Implementación 100% nativa sin dependencias externas (pintado GDI+).
    /// W-12: Soporte dinámico de géneros (Dictionary) — no hardcodea Masculino/Femenino.
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
        private Label _lblGenero1Titulo;
        private Label _lblGenero1Valor;
        private Label _lblGenero2Titulo;
        private Label _lblGenero2Valor;

        // Panel para la gráfica custom (pintado en Paint)
        private Panel _pnlGrafica;

        // Datos para el pintado (no readonly, se modifican en CargarDatos)
        private int _valorGenero1 = 0;
        private int _valorGenero2 = 0;
        private int _valorTotal = 0;
        private string _nombreGenero1 = "Masculino";
        private string _nombreGenero2 = "Femenino";

        // Colores por género (configurables)
        private static readonly Color ColorGenero1 = Color.FromArgb(52, 152, 219);  // Azul
        private static readonly Color ColorGenero2 = Color.FromArgb(231, 76, 60);   // Rojo
        private static readonly Color ColorTotal = Color.FromArgb(27, 43, 66);      // Azul oscuro

        public UcEstadisticas()
        {
            InitializeComponent();
            ConfigurarControles();

            // ============================================================
            // LIMPIAR LABELS DE INICIO: forzar textos en "0" al arrancar
            // ============================================================
            _lblTotalVisitasValor.Text = "0";
            _lblGenero1Valor.Text = "0";
            _lblGenero2Valor.Text = "0";
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.AliceBlue;
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
            var cardTotal = CrearTarjetaResumen("Total Visitas", "0", ColorTotal, out _lblTotalVisitasTitulo, out _lblTotalVisitasValor);
            cardTotal.Location = new Point(0, 0);

            // Tarjeta Género 1 (dinámico)
            var cardGenero1 = CrearTarjetaResumen(_nombreGenero1, "0", ColorGenero1, out _lblGenero1Titulo, out _lblGenero1Valor);
            cardGenero1.Location = new Point(280, 0);

            // Tarjeta Género 2 (dinámico)
            var cardGenero2 = CrearTarjetaResumen(_nombreGenero2, "0", ColorGenero2, out _lblGenero2Titulo, out _lblGenero2Valor);
            cardGenero2.Location = new Point(560, 0);

            _panelResumen.Controls.AddRange(new Control[] { cardTotal, cardGenero1, cardGenero2 });
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
        /// W-12: Usa Dictionary para soportar cualquier género dinámicamente.
        /// </summary>
        public void CargarDatos()
        {
            // ============================================================
            // PASO 1: REINICIAR VARIABLES (lo primero, obligatorio)
            // ============================================================
            var generos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int totalVisitas = 0;

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
                // PASO 3: LEER Y ACUMULAR (Dictionary dinámico — W-12)
                // ============================================================
                using var lector = comando.ExecuteReader();
                while (lector.Read())
                {
                    string genero = lector.GetString(0);
                    int cantidad = lector.GetInt32(1);

                    if (generos.ContainsKey(genero))
                        generos[genero] += cantidad;
                    else
                        generos[genero] = cantidad;

                    totalVisitas += cantidad;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar estadísticas: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // ============================================================
            // PASO 4: ASIGNAR TOP 2 GÉNEROS + TOTAL
            // ============================================================
            // Ordenar por cantidad descendente y tomar top 2
            var topGeneros = generos
                .OrderByDescending(kvp => kvp.Value)
                .Take(2)
                .ToList();

            // Asignar primer género
            if (topGeneros.Count > 0)
            {
                _nombreGenero1 = topGeneros[0].Key;
                _valorGenero1 = topGeneros[0].Value;
            }
            else
            {
                _nombreGenero1 = "Masculino";
                _valorGenero1 = 0;
            }

            // Asignar segundo género
            if (topGeneros.Count > 1)
            {
                _nombreGenero2 = topGeneros[1].Key;
                _valorGenero2 = topGeneros[1].Value;
            }
            else
            {
                _nombreGenero2 = "Femenino";
                _valorGenero2 = 0;
            }

            _valorTotal = totalVisitas;

            // Actualizar labels
            _lblTotalVisitasValor.Text = totalVisitas.ToString("N0");
            _lblGenero1Titulo.Text = _nombreGenero1;
            _lblGenero1Valor.Text = _valorGenero1.ToString("N0");
            _lblGenero2Titulo.Text = _nombreGenero2;
            _lblGenero2Valor.Text = _valorGenero2.ToString("N0");

            // Forzar repintado de la gráfica desde cero
            _pnlGrafica.Invalidate();
        }

        /// <summary>
        /// Evento Paint: dibuja las barras, valores y etiquetas nativamente con escalado correcto.
        /// W-12: Barras dinámicas — Total + Top 2 géneros detectados.
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

            // Colores (usar estáticos definidos en la clase)
            Color colorTotal = ColorTotal;
            Color colorGenero1 = ColorGenero1;
            Color colorGenero2 = ColorGenero2;
            Color colorEje = Color.FromArgb(180, 180, 180);
            Color colorGrid = Color.FromArgb(230, 230, 230);
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
            using var brushTotal = new SolidBrush(colorTotal);
            using var brushGenero1 = new SolidBrush(colorGenero1);
            using var brushGenero2 = new SolidBrush(colorGenero2);

            // Plumas
            using var penEje = new Pen(colorEje, 1);
            using var penGrid = new Pen(colorGrid, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };

            // Título de la gráfica
            string tituloGrafica = "Visitas a Sala por Género";
            SizeF szTitulo = g.MeasureString(tituloGrafica, fontTitulo);
            g.DrawString(tituloGrafica, fontTitulo, brushTexto,
                paddingLeft + (areaWidth - szTitulo.Width) / 2, paddingTop - 5);

            // ---- CASO SIN DATOS (evita dibujar gráfica fantasma) ----
            if (_valorGenero1 == 0 && _valorGenero2 == 0)
            {
                string msg = "No hay registros";
                SizeF szMsg = g.MeasureString(msg, fontNoDatos);
                g.DrawString(msg, fontNoDatos, brushTextoSec,
                    paddingLeft + (areaWidth - szMsg.Width) / 2,
                    paddingTop + (areaHeight - szMsg.Height) / 2);
                return;
            }

            // ---- CÁLCULO DE ESCALA BASADO EN TOTAL ----
            int total = _valorTotal; // Usar el total real
            int maxValor = Math.Max(total, Math.Max(_valorGenero1, _valorGenero2));

            // Espacio reservado arriba para el texto del valor (altura de fuente + gap)
            int espacioTextoArriba = (int)Math.Ceiling(g.MeasureString("0", fontValor).Height) + 8; // ~19 + 8 = ~27px
            const int margenSeguridad = 8; // margen extra de seguridad visual
            int margenSuperiorTotal = espacioTextoArriba + margenSeguridad; // ~35px total

            // Altura máxima real que puede tener una barra (respetando margen superior)
            int maxAlturaBarra = areaHeight - margenSuperiorTotal;
            if (maxAlturaBarra < 10) maxAlturaBarra = 10; // seguridad

            float escalaY = maxValor > 0 ? (float)maxAlturaBarra / maxValor : 1f;

            // Y mínimo que puede alcanzar el tope de una barra (respeta paddingTop + margenSuperiorTotal)
            int minTopY = paddingTop + margenSuperiorTotal;

            // ---- GRIDLINES HORIZONTALES (fondo profesional) ----
            int numGridLines = 4;
            for (int i = 1; i <= numGridLines; i++)
            {
                float y = baseY - (maxAlturaBarra * i / (float)numGridLines);
                g.DrawLine(penGrid, paddingLeft, y, paddingLeft + areaWidth, y);
            }

            // Dibujar eje X (línea base)
            g.DrawLine(penEje, paddingLeft, baseY, paddingLeft + areaWidth, baseY);

            // ---- CONFIGURACIÓN DE 3 BARRAS CENTRADAS ----
            int numBarras = 3;
            int espacioEntreBarras = 30;
            int anchoBarra = Math.Min(70, (areaWidth - espacioEntreBarras * (numBarras + 1)) / numBarras);
            int inicioX = paddingLeft + (areaWidth - (anchoBarra * numBarras + espacioEntreBarras * (numBarras - 1))) / 2;

            // Datos de las 3 barras: Total, Género 1, Género 2 (dinámicos — W-12)
            var barras = new[]
            {
                new { Label = "Total", Valor = total, Brush = brushTotal, X = inicioX },
                new { Label = _nombreGenero1, Valor = _valorGenero1, Brush = brushGenero1, X = inicioX + anchoBarra + espacioEntreBarras },
                new { Label = _nombreGenero2, Valor = _valorGenero2, Brush = brushGenero2, X = inicioX + 2 * (anchoBarra + espacioEntreBarras) }
            };

            foreach (var barra in barras)
            {
                // Altura proporcional, nunca supera maxAlturaBarra
                int alturaBarra = (int)(barra.Valor * escalaY);
                if (alturaBarra > maxAlturaBarra) alturaBarra = maxAlturaBarra;

                int topY = baseY - alturaBarra;
                // Garantía dura: la barra nunca sube más allá de minTopY (respeta margen para texto)
                if (topY < minTopY) topY = minTopY;

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
                    // Con minTopY calculado correctamente, esto nunca debería ser < paddingTop
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