using System.Data;
using Microsoft.Data.Sqlite;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace BibliotecaApp
{
    /// <summary>
    /// Módulo de Estadísticas: DOS gráficas GDI+ nativas con un filtro de
    /// rango de fechas compartido (arriba, gobernando ambas):
    /// izquierda = Control de Sala (ControlUsuariosSala.Fecha),
    /// derecha = Préstamos Externos (PrestamosExternos.FechaPrestamo).
    /// Cada mitad tiene sus 3 tarjetas de resumen (Total + top 2 géneros).
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

        // Panel de resumen: 6 tarjetas (3 Sala a la izquierda + 3 Préstamos a la derecha)
        private Panel _panelResumen;

        // Labels tarjetas Sala (se asignan vía 'out' en CrearTarjetaResumen)
        private Label _lblTotalVisitasTitulo;
        private Label _lblTotalVisitasValor;
        private Label _lblGenero1Titulo;
        private Label _lblGenero1Valor;
        private Label _lblGenero2Titulo;
        private Label _lblGenero2Valor;

        // Labels tarjetas Préstamos Externos (mitad derecha)
        private Label _lblTotalPrestamosTitulo = null!;
        private Label _lblTotalPrestamosValor = null!;
        private Label _lblPrestGenero1Titulo = null!;
        private Label _lblPrestGenero1Valor = null!;
        private Label _lblPrestGenero2Titulo = null!;
        private Label _lblPrestGenero2Valor = null!;

        // Paneles de gráfica: mitad izquierda (Sala) y mitad derecha (Préstamos)
        private Panel _pnlContenedorGraficas = null!;
        private Panel _pnlGraficaSala = null!;
        private Panel _pnlGraficaPrestamos = null!;

        // Referencias a las 6 tarjetas (para UbicarTarjetas según ancho disponible)
        private Panel[] _tarjetas = Array.Empty<Panel>();

        // Padding compartido por los dos paneles (única fuente para DibujarGraficaBarras)
        private const int PadIzq = 30, PadArr = 20, PadDer = 30, PadAba = 40;

        // Datos para el pintado (no readonly, se modifican en CargarDatos)
        private int _valorGenero1 = 0;
        private int _valorGenero2 = 0;
        private int _valorTotal = 0;
        private string _nombreGenero1 = "Masculino";
        private string _nombreGenero2 = "Femenino";

        // Datos Préstamos Externos (mitad derecha)
        private int _valorPrestTotal = 0;
        private int _valorPrestGenero1 = 0;
        private int _valorPrestGenero2 = 0;
        private string _nombrePrestGenero1 = "Masculino";
        private string _nombrePrestGenero2 = "Femenino";

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

            // --- Mitad IZQUIERDA: Control de Sala (3 tarjetas) ---
            var cardTotal = CrearTarjetaResumen("Total Visitas", "0", ColorTotal, out _lblTotalVisitasTitulo, out _lblTotalVisitasValor);
            var cardGenero1 = CrearTarjetaResumen(_nombreGenero1, "0", ColorGenero1, out _lblGenero1Titulo, out _lblGenero1Valor);
            var cardGenero2 = CrearTarjetaResumen(_nombreGenero2, "0", ColorGenero2, out _lblGenero2Titulo, out _lblGenero2Valor);

            // --- Mitad DERECHA: Préstamos Externos (3 tarjetas nuevas) ---
            var cardPrestTotal = CrearTarjetaResumen("Total Préstamos", "0", ColorTotal, out _lblTotalPrestamosTitulo, out _lblTotalPrestamosValor);
            var cardPrestGenero1 = CrearTarjetaResumen(_nombrePrestGenero1, "0", ColorGenero1, out _lblPrestGenero1Titulo, out _lblPrestGenero1Valor);
            var cardPrestGenero2 = CrearTarjetaResumen(_nombrePrestGenero2, "0", ColorGenero2, out _lblPrestGenero2Titulo, out _lblPrestGenero2Valor);

            // Orden: 3 primeras = Sala (izquierda), 3 siguientes = Préstamos (derecha)
            _tarjetas = new[] { cardTotal, cardGenero1, cardGenero2,
                                 cardPrestTotal, cardPrestGenero1, cardPrestGenero2 };

            _panelResumen.Controls.AddRange(_tarjetas);
            _panelResumen.Resize += (_, _) => UbicarTarjetas(); // tarjetas más pequeñas: 3 por mitad
            this.Controls.Add(_panelResumen);

            // =================================================================
            // PANEL GRÁFICA - DOS MITADES (Sala | Préstamos), PINTADO NATIVO GDI+
            // =================================================================
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
            _pnlContenedorGraficas.Resize += (_, _) => DistribuirGraficas(); // mitades iguales (Width / 2)

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
        /// Reparte las 6 tarjetas: 3 de Sala en la mitad izquierda y
        /// 3 de Préstamos en la mitad derecha (ancho dinámico, sin tocar Designer).
        /// </summary>
        private void UbicarTarjetas()
        {
            int mitad = _panelResumen.ClientSize.Width / 2;
            if (mitad < 150) return; // aún sin tamaño final (se reintentará en Resize)

            const int separacion = 10;
            int anchoTarjeta = (mitad - separacion * 4) / 3;
            if (anchoTarjeta < 90) anchoTarjeta = 90;

            for (int i = 0; i < _tarjetas.Length; i++)
            {
                int bloque = i / 3;          // 0 = Sala (izquierda), 1 = Préstamos (derecha)
                int dentro = i % 3;
                int x = bloque * mitad + separacion + dentro * (anchoTarjeta + separacion);
                _tarjetas[i].SetBounds(x, 0, anchoTarjeta, 100);
            }
        }

        /// <summary>
        /// Divide el área de gráfica en dos mitades iguales (Width / 2):
        /// izquierda = Control de Sala, derecha = Préstamos Externos.
        /// </summary>
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

        /// <summary>
        /// Carga los datos de AMBOS módulos con el mismo rango de fechas
        /// y actualiza las 6 tarjetas + las 2 gráficas (GDI+).
        /// Método público para ser llamado desde Form1 al mostrar la vista.
        /// W-12: Usa Dictionary para soportar cualquier género dinámicamente.
        /// </summary>
        public void CargarDatos()
        {
            // ============================================================
            // PASO 1: REINICIAR ACUMULADORES (lo primero, obligatorio)
            // ============================================================
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

                // ============================================================
                // PASO 2: CONSULTA 1 — Control de Sala (columna Fecha)
                // ============================================================
                comando.CommandText = @"
                    SELECT Genero, COUNT(*)
                    FROM ControlUsuariosSala
                    WHERE Fecha BETWEEN $desde AND $hasta
                    GROUP BY Genero;";

                using (var lector = comando.ExecuteReader())
                    AcumularPorGenero(lector, generosSala, ref totalVisitas);

                // ============================================================
                // PASO 3: CONSULTA 2 — Préstamos Externos (columna FechaPrestamo)
                // ============================================================
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

            // ============================================================
            // PASO 4: TOP 2 GÉNEROS + TOTAL — CONTROL DE SALA
            // ============================================================
            var topSala = generosSala.OrderByDescending(kvp => kvp.Value).Take(2).ToList();
            _nombreGenero1 = topSala.Count > 0 ? topSala[0].Key : "Masculino";
            _valorGenero1 = topSala.Count > 0 ? topSala[0].Value : 0;
            _nombreGenero2 = topSala.Count > 1 ? topSala[1].Key : "Femenino";
            _valorGenero2 = topSala.Count > 1 ? topSala[1].Value : 0;
            _valorTotal = totalVisitas;

            // ============================================================
            // PASO 5: TOP 2 GÉNEROS + TOTAL — PRÉSTAMOS EXTERNOS
            // ============================================================
            var topPrestamos = generosPrestamos.OrderByDescending(kvp => kvp.Value).Take(2).ToList();
            _nombrePrestGenero1 = topPrestamos.Count > 0 ? topPrestamos[0].Key : "Masculino";
            _valorPrestGenero1 = topPrestamos.Count > 0 ? topPrestamos[0].Value : 0;
            _nombrePrestGenero2 = topPrestamos.Count > 1 ? topPrestamos[1].Key : "Femenino";
            _valorPrestGenero2 = topPrestamos.Count > 1 ? topPrestamos[1].Value : 0;
            _valorPrestTotal = totalPrestamos;

            // ============================================================
            // PASO 6: ACTUALIZAR LAS 6 TARJETAS
            // ============================================================
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

            // Forzar repintado de AMBAS gráficas desde cero
            _pnlGraficaSala.Invalidate();
            _pnlGraficaPrestamos.Invalidate();
        }

        /// <summary>
        /// Lee un GROUP BY Genero (columna 0 = género, columna 1 = COUNT)
        /// y acumula por género + total. Géneros vacíos/nulos → "(Sin dato)".
        /// </summary>
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

        /// <summary>
        /// Paint de la gráfica izquierda (Control de Sala).
        /// </summary>
        private void PnlGraficaSala_Paint(object? sender, PaintEventArgs e)
        {
            DibujarGraficaBarras(e.Graphics, DatosBarrasSala(), "Visitas a Sala por Género",
                _pnlGraficaSala.Width, _pnlGraficaSala.Height);
        }

        /// <summary>
        /// Paint de la gráfica derecha (Préstamos Externos).
        /// </summary>
        private void PnlGraficaPrestamos_Paint(object? sender, PaintEventArgs e)
        {
            DibujarGraficaBarras(e.Graphics, DatosBarrasPrestamos(), "Préstamos Externos por Género",
                _pnlGraficaPrestamos.Width, _pnlGraficaPrestamos.Height);
        }

        /// <summary>
        /// Barras de la gráfica de Sala: Total + top 2 géneros detectados.
        /// </summary>
        private Dictionary<string, int> DatosBarrasSala()
        {
            var datos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            datos["Total"] = _valorTotal;
            if (!datos.ContainsKey(_nombreGenero1)) datos[_nombreGenero1] = _valorGenero1;
            if (!datos.ContainsKey(_nombreGenero2)) datos[_nombreGenero2] = _valorGenero2;
            return datos;
        }

        /// <summary>
        /// Barras de la gráfica de Préstamos Externos: Total + top 2 géneros detectados.
        /// </summary>
        private Dictionary<string, int> DatosBarrasPrestamos()
        {
            var datos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            datos["Total"] = _valorPrestTotal;
            if (!datos.ContainsKey(_nombrePrestGenero1)) datos[_nombrePrestGenero1] = _valorPrestGenero1;
            if (!datos.ContainsKey(_nombrePrestGenero2)) datos[_nombrePrestGenero2] = _valorPrestGenero2;
            return datos;
        }

        /// <summary>
        /// Método GDI+ maestro: dibuja barras redondeadas con escalado correcto a
        /// partir de un Dictionary genérico (una barra por entrada, en orden).
        /// Mantiene AntiAlias, gridlines, ejes y colores de EstiloUI (W-12).
        /// </summary>
        private void DibujarGraficaBarras(Graphics g, Dictionary<string, int> datos, string titulo, int ancho, int alto)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var entradas = datos.ToList(); // orden de inserción: Total primero, luego géneros

            int areaWidth = ancho - PadIzq - PadDer;
            int areaHeight = alto - PadArr - PadAba;
            int baseY = alto - PadAba; // Línea base (eje X)
            if (areaWidth < 40) areaWidth = 40;
            if (areaHeight < 40) areaHeight = 40;

            // Colores
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

            // Plumas
            using var penEje = new Pen(colorEje, 1);
            using var penGrid = new Pen(colorGrid, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };

            // Título de la gráfica
            SizeF szTitulo = g.MeasureString(titulo, fontTitulo);
            g.DrawString(titulo, fontTitulo, brushTexto,
                PadIzq + (areaWidth - szTitulo.Width) / 2, PadArr - 5);

            // ---- CASO SIN DATOS (evita dibujar gráfica fantasma) ----
            if (entradas.Count == 0 || entradas.All(kv => kv.Value == 0))
            {
                string msg = "No hay registros";
                SizeF szMsg = g.MeasureString(msg, fontNoDatos);
                g.DrawString(msg, fontNoDatos, brushTextoSec,
                    PadIzq + (areaWidth - szMsg.Width) / 2,
                    PadArr + (areaHeight - szMsg.Height) / 2);
                return;
            }

            // ---- CÁLCULO DE ESCALA (máximo entre todas las entradas) ----
            int maxValor = entradas.Max(kv => kv.Value);

            // Espacio reservado arriba para el texto del valor (altura de fuente + gap)
            int espacioTextoArriba = (int)Math.Ceiling(g.MeasureString("0", fontValor).Height) + 8;
            const int margenSeguridad = 8;
            int margenSuperiorTotal = espacioTextoArriba + margenSeguridad;

            // Altura máxima real que puede tener una barra (respetando margen superior)
            int maxAlturaBarra = areaHeight - margenSuperiorTotal;
            if (maxAlturaBarra < 10) maxAlturaBarra = 10;

            float escalaY = maxValor > 0 ? (float)maxAlturaBarra / maxValor : 1f;

            // Y mínimo que puede alcanzar el tope de una barra
            int minTopY = PadArr + margenSuperiorTotal;

            // ---- GRIDLINES HORIZONTALES (fondo profesional) ----
            int numGridLines = 4;
            for (int i = 1; i <= numGridLines; i++)
            {
                float y = baseY - (maxAlturaBarra * i / (float)numGridLines);
                g.DrawLine(penGrid, PadIzq, y, PadIzq + areaWidth, y);
            }

            // Dibujar eje X (línea base)
            g.DrawLine(penEje, PadIzq, baseY, PadIzq + areaWidth, baseY);

            // ---- BARRAS DINÁMICAS (una por entrada del diccionario) ----
            int numBarras = entradas.Count;
            int espacioEntreBarras = 30;
            int anchoBarra = Math.Min(70, (areaWidth - espacioEntreBarras * (numBarras + 1)) / numBarras);
            if (anchoBarra < 10) anchoBarra = 10;
            int inicioX = PadIzq + (areaWidth - (anchoBarra * numBarras + espacioEntreBarras * (numBarras - 1))) / 2;

            for (int i = 0; i < numBarras; i++)
            {
                string etiqueta = entradas[i].Key;
                int valor = entradas[i].Value;

                // 1ª barra = Total (azul oscuro); los géneros alternan azul/rojo
                Color colorBarra = i == 0 ? ColorTotal : (i % 2 == 1 ? ColorGenero1 : ColorGenero2);
                using var brushBarra = new SolidBrush(colorBarra);

                int xBarra = inicioX + i * (anchoBarra + espacioEntreBarras);

                // Altura proporcional, nunca supera maxAlturaBarra
                int alturaBarra = (int)(valor * escalaY);
                if (alturaBarra > maxAlturaBarra) alturaBarra = maxAlturaBarra;

                int topY = baseY - alturaBarra;
                // Garantía dura: la barra nunca sube más allá de minTopY (respeta margen para texto)
                if (topY < minTopY) topY = minTopY;

                var rectBarra = new Rectangle(xBarra, topY, anchoBarra, alturaBarra);

                // Dibujar barra con esquinas redondeadas arriba (mismo estilo nativo)
                using (var path = GetRoundedRectPath(rectBarra, 6, true))
                {
                    g.FillPath(brushBarra, path);
                }

                // Dibujar valor encima de la barra (flotando 4px sobre la esquina redondeada)
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