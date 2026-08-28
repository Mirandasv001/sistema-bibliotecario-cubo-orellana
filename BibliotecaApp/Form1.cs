namespace BibliotecaApp
{
    /// <summary>
    /// Ventana principal (dashboard): menú lateral + panel central donde se
    /// instancia el apartado (UserControl) seleccionado.
    /// </summary>
    public partial class Form1 : Form
    {
        private readonly System.Windows.Forms.Timer _timerAlertas;
        private int _conteoMorosos = 0;

        public Form1()
        {
            // Refuerzo de codificación para cualquier salida de diagnóstico por consola.
            // NOTA: esto NO afecta al renderizado de labels (WinForms ya es Unicode);
            // la corrección definitiva está en el encoding de los archivos fuente y del CSV.
            try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { /* sin consola adjunta */ }

            InitializeComponent();

            // Timer de notificaciones: consulta los morosos al arrancar y cada intervalo.
            _timerAlertas = new System.Windows.Forms.Timer { Interval = 30000 };
            _timerAlertas.Tick += (_, _) => ActualizarContadorAlertas();
        }

        // ------------------------------------------------------------------
        //  Inicio
        // ------------------------------------------------------------------
        private void Form1_Load(object sender, EventArgs e)
        {
            CargarLogo();
            MostrarApartadoSala();

            // Consulta inicial del contador de morosos y arranque del refill periódico.
            ActualizarContadorAlertas();
            _timerAlertas.Start();
        }

        /// <summary>Carga el logo CUBO desde el directorio de la aplicación sin bloquear el archivo.</summary>
        private void CargarLogo()
        {
            try
            {
                string[] candidatos = { "images (2).jpg", "logo.png", "logo.jpg", "logo.jpeg" };

                string? ruta = candidatos
                    .Select(n => Path.Combine(AppContext.BaseDirectory, n))
                    .FirstOrDefault(File.Exists);

                if (ruta == null) return;

                byte[] bytes = File.ReadAllBytes(ruta);
                pictureBoxLogo.Image = Image.FromStream(new MemoryStream(bytes));
            }
            catch
            {
                // Si no hay imagen disponible, el menú funciona igualmente.
            }
        }

        // ------------------------------------------------------------------
        //  Navegación entre apartados
        // ------------------------------------------------------------------
        private void MostrarApartadoSala() =>
            MostrarApartado(() => new UcControlSala(), btnSala);

        private void MostrarApartadoInventario() =>
            MostrarApartado(() => new UcInventario(), btnInventario);

        private void MostrarApartadoPrestamos() =>
            MostrarApartado(() => new UcPrestamosExternos(), btnPrestamos);

        private void MostrarApartadoAlertas() =>
            MostrarApartado(() => new UcAlertas(), btnAlertas);

        /// <summary>
        /// Limpia el panel central y carga el UserControl correspondiente.
        /// Se libera (Dispose) el apartado anterior en lugar de solo Controls.Clear()
        /// para no dejar controles huérfanos retenidos por sus event handlers.
        /// Devuelve el control creado para poder inyectarle datos a continuación.
        /// </summary>
        private UserControl MostrarApartado(Func<UserControl> crearApartado, Button botonActivo)
        {
            ResaltarBoton(botonActivo);

            foreach (var anterior in panelContenedor.Controls.Cast<Control>().ToArray())
            {
                panelContenedor.Controls.Remove(anterior);
                anterior.Dispose();
            }

            UserControl apartado = crearApartado();
            apartado.Dock = DockStyle.Fill;
            panelContenedor.Controls.Add(apartado);
            return apartado;
        }

        // ------------------------------------------------------------------
        //  Flujo ágil: Inventario (doble clic) → Préstamos Externos
        // ------------------------------------------------------------------

        /// <summary>
        /// Método público invocado por UcInventario al hacer doble clic en un
        /// ejemplar disponible. Cambia la vista activa a Préstamos Externos y
        /// le inyecta el Código y el Título para agilizar el registro.
        /// </summary>
        public void CargarPrestamoDesdeInventario(string codigo, string titulo)
        {
            UserControl apartado = MostrarApartado(() => new UcPrestamosExternos(), btnPrestamos);
            if (apartado is UcPrestamosExternos prestamos)
                prestamos.CargarDesdeInventario(codigo, titulo);
        }

        /// <summary>Muestra el manual rápido de uso de la aplicación.</summary>
        private void btnGuiaUso_Click()
        {
            const string guia =
                "\U0001F4D6 CÓMO REGISTRAR UN PRÉSTAMO:\n" +
                "1. Vaya a la pestaña 'Inventario'.\n" +
                "2. Busque el libro deseado y haga DOBLE CLIC sobre él.\n" +
                "3. El sistema lo llevará automáticamente a 'Préstamos Externos' con el libro ya cargado.\n" +
                "4. Llene los datos del usuario y haga clic en 'Registrar Préstamo'.\n\n" +
                "\U0001F504 CÓMO RENOVAR O DEVOLVER:\n" +
                "1. En la tabla inferior de 'Préstamos Externos', seleccione el préstamo activo.\n" +
                "2. Llene la fecha y personal correspondiente en la sección de Devolución/Renovación.\n" +
                "3. Haga clic en el botón de la acción deseada.";

            MessageBox.Show(guia, "Guía de Uso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //Botones importantes

        private void ResaltarBoton(Button botonActivo)
        {
            foreach (var boton in new[] { btnSala, btnInventario, btnPrestamos, btnAlertas })
            {
                bool activo = ReferenceEquals(boton, botonActivo);
                boton.BackColor = activo ? EstiloUI.HoverOscuro : EstiloUI.FondoOscuro;
                boton.ForeColor = activo ? Color.White : Color.WhiteSmoke;
            }
        }

        // ====================================================================
        //  NOTIFICACIONES DE PRÉSTAMOS VENCIDOS (bolita roja / badge)
        // ====================================================================

        /// <summary>
        /// Consulta cuántos préstamos están vencidos (EstadoLibro = 'Pendiente'
        /// y pasada la FechaEntrega esperada) y actualiza el badge del botón.
        /// </summary>
        private void ActualizarContadorAlertas()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    SELECT COUNT(*)
                    FROM PrestamosExternos
                    WHERE EstadoLibro = 'Pendiente'
                      AND julianday(FechaEntrega) < julianday('now');";

                long total = (long)(comando.ExecuteScalar() ?? 0);
                int nuevo = (int)Math.Min(total, 99);

                if (nuevo != _conteoMorosos)
                {
                    _conteoMorosos = nuevo;
                    // Fuerza el repintado del botón para dibujar u ocultar la bolita.
                    btnAlertas.Invalidate();
                }
            }
            catch (Microsoft.Data.Sqlite.SqliteException)
            {
                // Si la BD no está disponible, se silencia; la próxima consulta reintenta.
            }
            catch (Exception)
            {
                // Nunca debe tumbar la aplicación por fallos en la consulta de notificaciones.
            }
        }

        /// <summary>Dibuja la "bolita roja" con el número de morosos sobre btnAlertas.</summary>
        private void btnAlertas_Paint(object? sender, PaintEventArgs e)
        {
            if (_conteoMorosos <= 0) return;

            const int badgeSize = 20;
            int x = btnAlertas.Width - badgeSize - 10;
            int y = 9;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (var brocha = new SolidBrush(Color.Crimson))
                e.Graphics.FillEllipse(brocha, x, y, badgeSize, badgeSize);

            string texto = _conteoMorosos > 99 ? "99+" : _conteoMorosos.ToString();
            using var fuente = new Font(EstiloUI.FuenteBase, 8.5F, FontStyle.Bold);
            using var formato = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            using var pincelTexto = new SolidBrush(Color.White);
            e.Graphics.DrawString(texto, fuente, pincelTexto,
                new RectangleF(x, y, badgeSize, badgeSize), formato);
        }
    }
}
