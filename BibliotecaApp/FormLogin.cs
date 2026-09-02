namespace BibliotecaApp
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
            AplicarEstilos();
            CargarLogo();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            CentrarTarjeta();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CentrarTarjeta();
        }

        private void CentrarTarjeta()
        {
            panelCard.Location = new Point(
                (panelFondo.Width - panelCard.Width) / 2,
                (panelFondo.Height - panelCard.Height) / 2);
        }

        private void AplicarEstilos()
        {
            EstiloUI.EstilizarEntrada(txtUsuario);
            EstiloUI.EstilizarEntrada(txtPassword);
            EstiloUI.EstablecerPlaceholder(txtUsuario, "Ingrese su usuario");
            EstiloUI.EstablecerPlaceholder(txtPassword, "Ingrese su contraseña");

            btnIngresar.MouseEnter += (_, _) =>
                btnIngresar.BackColor = Color.FromArgb(75, 125, 230);
            btnIngresar.MouseLeave += (_, _) =>
                btnIngresar.BackColor = Color.FromArgb(59, 111, 216);

            btnCerrar.MouseEnter += (_, _) =>
                btnCerrar.ForeColor = Color.FromArgb(59, 111, 216);
            btnCerrar.MouseLeave += (_, _) =>
                btnCerrar.ForeColor = Color.FromArgb(130, 140, 155);

            // Controles de ventana: hover effects
            btnMinimizar.MouseEnter += (_, _) =>
                btnMinimizar.BackColor = Color.FromArgb(50, 60, 80);
            btnMinimizar.MouseLeave += (_, _) =>
                btnMinimizar.BackColor = Color.Transparent;

            btnMaximizar.MouseEnter += (_, _) =>
                btnMaximizar.BackColor = Color.FromArgb(50, 60, 80);
            btnMaximizar.MouseLeave += (_, _) =>
                btnMaximizar.BackColor = Color.Transparent;

            btnCerrarVentana.MouseEnter += (_, _) =>
            {
                btnCerrarVentana.BackColor = Color.FromArgb(180, 40, 40);
                btnCerrarVentana.ForeColor = Color.White;
            };
            btnCerrarVentana.MouseLeave += (_, _) =>
            {
                btnCerrarVentana.BackColor = Color.Transparent;
                btnCerrarVentana.ForeColor = Color.FromArgb(204, 204, 204);
            };
        }

        private void CargarLogo()
        {
            try
            {
                string nombreArchivo = "Logo de inicio";
                string[] extensiones = { ".jpg", ".png", ".jpeg" };
                string[] candidatos = extensiones.Select(ext => nombreArchivo + ext).ToArray();

                string? ruta = BuscarArchivoLogo(candidatos);
                if (ruta == null) return;

                byte[] bytes = File.ReadAllBytes(ruta);
                using var stream = new MemoryStream(bytes);
                pictureBoxLogo.Image = Image.FromStream(stream);
            }
            catch
            {
                // Si no se carga el logo, la interfaz funciona igualmente.
            }
        }

        private static string? BuscarArchivoLogo(string[] nombresArchivos)
        {
            // 1. Buscar en Application.StartupPath (bin\Debug o publicación)
            string? ruta = BuscarEnDirectorio(AppDomain.CurrentDomain.BaseDirectory, nombresArchivos);
            if (ruta != null) return ruta;

            // 2. Subir en el árbol de directorios hasta 5 niveles hacia la raíz del proyecto
            DirectoryInfo? dir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
            int niveles = 0;

            while (dir != null && niveles < 5)
            {
                ruta = BuscarEnDirectorio(dir.FullName, nombresArchivos);
                if (ruta != null) return ruta;

                dir = Directory.GetParent(dir.FullName);
                niveles++;
            }

            return null;
        }

        private static string? BuscarEnDirectorio(string directorio, string[] nombresArchivos)
        {
            foreach (string nombre in nombresArchivos)
            {
                string rutaCompleta = Path.Combine(directorio, nombre);
                if (File.Exists(rutaCompleta))
                    return rutaCompleta;
            }
            return null;
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            if (txtUsuario.Text == "UserCubo" && txtPassword.Text == "1234$")
            {
                Form1 form1 = new Form1();
                form1.FormClosed += (_, _) => Application.Exit();
                form1.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show(
                    "Usuario o contraseña incorrectos.",
                    "Error de autenticación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        // ── Controles de ventana personalizados ──

        private void btnMinimizar_Click(object? sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnMaximizar_Click(object? sender, EventArgs e)
        {
            this.WindowState = this.WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        }

        private void btnCerrarVentana_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
