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
        private System.Collections.Generic.Dictionary<string, UserControl> _vistasCacheadas = new System.Collections.Generic.Dictionary<string, UserControl>();

        private UcControlSala? _vistaSala;
        private UcInventario? _vistaInventario;
        private UcPrestamosExternos? _vistaPrestamos;
        private UcAlertas? _vistaAlertas;

        public Form1()
        {
            // Refuerzo de codificación para cualquier salida de diagnóstico por consola.
            // NOTA: esto NO afecta al renderizado de labels (WinForms ya es Unicode);
            // la corrección definitiva está en el encoding de los archivos fuente y del CSV.
            try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { /* sin consola adjunta */ }

            InitializeComponent();

            // Configuración de UI dinámica
            ConfigurarBotonMenu(btnSala, "Control de Sala");
            ConfigurarBotonMenu(btnInventario, "Inventario");
            ConfigurarBotonMenu(btnPrestamos, "Préstamos Externos");
            ConfigurarBotonMenu(btnAlertas, "Alertas de Vencidos");
            ConfigurarBotonMenu(btnGuiaUso, "Guía de Uso");

            // Suscripción de eventos de navegación
            btnSala.Click += (_, _) => MostrarApartadoSala();
            btnPrestamos.Click += (_, _) => MostrarApartadoPrestamos();
            btnInventario.Click += (_, _) => MostrarApartadoInventario();
            btnAlertas.Click += (_, _) => MostrarApartadoAlertas();
            btnAlertas.Paint += btnAlertas_Paint;
            btnGuiaUso.Click += (_, _) => btnGuiaUso_Click();

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

        /// <summary>Carga el logo CUBO desde la carpeta Recursos sin bloquear el archivo.</summary>
        private void CargarLogo()
        {
            try
            {
                string rutaLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recursos", "Logo de inicio.jpg");
                if (File.Exists(rutaLogo))
                {
                    pictureBoxLogo.Image = Image.FromFile(rutaLogo);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error de UI - Logo no encontrado: {ex.Message}");
            }
        }

        // ------------------------------------------------------------------
        //  Navegación entre apartados
        // ------------------------------------------------------------------
<<<<<<< HEAD
        private void MostrarApartadoSala() =>
            MostrarApartado(() => new UcControlSala(), btnSala);

        private void MostrarApartadoInventario() =>
            MostrarApartado(() => new UcInventario(), btnInventario);

        private void MostrarApartadoPrestamos() =>
            MostrarApartado(() => new UcPrestamosExternos(), btnPrestamos);

        private void MostrarApartadoAlertas() =>
            MostrarApartado(() => new UcAlertas(), btnAlertas);

        private UserControl MostrarApartado(Func<UserControl> crearApartado, Button botonActivo)
        {
            ResaltarBoton(botonActivo);
            string claveVista = botonActivo.Name;

            if (!_vistasCacheadas.ContainsKey(claveVista))
            {
                UserControl nuevaVista = crearApartado();
                nuevaVista.Dock = DockStyle.Fill;
                panelContenedor.Controls.Add(nuevaVista);
                _vistasCacheadas[claveVista] = nuevaVista;
=======
        private void MostrarApartadoSala()
        {
            if (_vistaSala == null)
            {
                _vistaSala = new UcControlSala();
                _vistaSala.Dock = DockStyle.Fill;
                panelContenedor.Controls.Add(_vistaSala);
>>>>>>> abf6834d8d92c564587b6ab3ad8e614b672c9d42
            }
            OcultarVistas();
            _vistaSala.BringToFront();
            _vistaSala.Show();
            ResaltarBoton(btnSala);
        }

<<<<<<< HEAD
            _vistasCacheadas[claveVista].BringToFront();
            ActualizarVista(_vistasCacheadas[claveVista]);
            return _vistasCacheadas[claveVista];
        }

        private static void ActualizarVista(UserControl vista)
        {
            switch (vista)
            {
                case UcAlertas alertas:
                    alertas.Actualizar();
                    break;
                case UcInventario inventario:
                    inventario.Actualizar();
                    break;
                case UcPrestamosExternos prestamos:
                    prestamos.Actualizar();
                    break;
            }
        }

        /// <summary>
        /// Sincroniza las vistas dependientes inmediatamente después de un
        /// cambio de préstamo, sin esperar al siguiente tick del temporizador.
        /// </summary>
        public void NotificarCambioPrestamos()
        {
            ActualizarContadorAlertas();

            if (_vistasCacheadas.TryGetValue(btnAlertas.Name, out var alertas))
                ActualizarVista(alertas);

            if (_vistasCacheadas.TryGetValue(btnInventario.Name, out var inventario))
                ActualizarVista(inventario);
=======
        private void MostrarApartadoInventario()
        {
            if (_vistaInventario == null)
            {
                _vistaInventario = new UcInventario();
                _vistaInventario.Dock = DockStyle.Fill;
                panelContenedor.Controls.Add(_vistaInventario);
            }
            OcultarVistas();
            _vistaInventario.BringToFront();
            _vistaInventario.Show();
            ResaltarBoton(btnInventario);
        }

        private void MostrarApartadoPrestamos()
        {
            if (_vistaPrestamos == null)
            {
                _vistaPrestamos = new UcPrestamosExternos();
                _vistaPrestamos.Dock = DockStyle.Fill;
                panelContenedor.Controls.Add(_vistaPrestamos);
            }
            OcultarVistas();
            _vistaPrestamos.BringToFront();
            _vistaPrestamos.Show();
            ResaltarBoton(btnPrestamos);
        }

        private void MostrarApartadoAlertas()
        {
            if (_vistaAlertas == null)
            {
                _vistaAlertas = new UcAlertas();
                _vistaAlertas.Dock = DockStyle.Fill;
                panelContenedor.Controls.Add(_vistaAlertas);
            }
            OcultarVistas();
            _vistaAlertas.BringToFront();
            _vistaAlertas.Show();
            ResaltarBoton(btnAlertas);
        }

        private void OcultarVistas()
        {
            _vistaSala?.Hide();
            _vistaInventario?.Hide();
            _vistaPrestamos?.Hide();
            _vistaAlertas?.Hide();
>>>>>>> abf6834d8d92c564587b6ab3ad8e614b672c9d42
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
            MostrarApartadoPrestamos();
            if (_vistaPrestamos != null)
                _vistaPrestamos.CargarDesdeInventario(codigo, titulo);
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

        private void ConfigurarBotonMenu(Button boton, string texto)
        {
            boton.Dock = DockStyle.Top;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = EstiloUI.HoverOscuro;
            boton.BackColor = EstiloUI.FondoOscuro;
            boton.ForeColor = Color.White;
            boton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            boton.Height = 50;
            boton.TextAlign = ContentAlignment.MiddleLeft;
            boton.Padding = new Padding(15, 0, 0, 0);
            boton.Cursor = Cursors.Hand;
            boton.Text = texto;
            boton.UseVisualStyleBackColor = false;
        }

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
                    WHERE EstadoLibro IN ('Pendiente', 'Renovado')
                      AND date(FechaEntrega) < date('now', 'localtime');";

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

        // ====================================================================
        //  CERRAR SESIÓN
        // ====================================================================

        private void btnCerrarSesion_Click(object? sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show(
                "¿Estás seguro que deseas cerrar la sesión actual?",
                "Cerrar Sesión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                Application.Restart();
            }
        }

        private void lblNombreApp_Click(object sender, EventArgs e)
        {

        }

        private void pictureBoxLogo_Click(object sender, EventArgs e)
        {

        }
    }
}
