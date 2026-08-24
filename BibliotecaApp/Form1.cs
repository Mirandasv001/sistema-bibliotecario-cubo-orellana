namespace BibliotecaApp
{
    /// <summary>
    /// Ventana principal (dashboard): menú lateral + panel central donde se
    /// instancia el apartado (UserControl) seleccionado.
    /// </summary>
    public partial class Form1 : Form
    {
        public Form1()
        {
            // Refuerzo de codificación para cualquier salida de diagnóstico por consola.
            // NOTA: esto NO afecta al renderizado de labels (WinForms ya es Unicode);
            // la corrección definitiva está en el encoding de los archivos fuente y del CSV.
            try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { /* sin consola adjunta */ }

            InitializeComponent();
        }

        // ------------------------------------------------------------------
        //  Inicio
        // ------------------------------------------------------------------
        private void Form1_Load(object sender, EventArgs e)
        {
            CargarLogo();
            MostrarApartadoSala();
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

        /// <summary>
        /// Limpia el panel central y carga el UserControl correspondiente.
        /// Se libera (Dispose) el apartado anterior en lugar de solo Controls.Clear()
        /// para no dejar controles huérfanos retenidos por sus event handlers.
        /// </summary>
        private void MostrarApartado(Func<UserControl> crearApartado, Button botonActivo)
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
        }

        private void ResaltarBoton(Button botonActivo)
        {
            foreach (var boton in new[] { btnSala, btnInventario, btnPrestamos })
            {
                bool activo = ReferenceEquals(boton, botonActivo);
                boton.BackColor = activo ? EstiloUI.HoverOscuro : EstiloUI.FondoOscuro;
                boton.ForeColor = activo ? Color.White : Color.WhiteSmoke;
            }
        }
    }
}
