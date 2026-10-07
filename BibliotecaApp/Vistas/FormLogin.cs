using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
            CargarLogo();
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
            // 1. Buscar en la carpeta Recursos (nueva ubicación)
            string rutaRecursos = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recursos");
            string? ruta = BuscarEnDirectorio(rutaRecursos, nombresArchivos);
            if (ruta != null) return ruta;

            // 2. Buscar en Application.StartupPath (bin\Debug o publicación)
            ruta = BuscarEnDirectorio(AppDomain.CurrentDomain.BaseDirectory, nombresArchivos);
            if (ruta != null) return ruta;

            // 3. Subir en el árbol de directorios hasta 5 niveles hacia la raíz del proyecto
            DirectoryInfo? dir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
            int niveles = 0;

            while (dir != null && niveles < 5)
            {
                ruta = BuscarEnDirectorio(dir.FullName, nombresArchivos);
                if (ruta != null) return ruta;

                // También buscar en Recursos de cada nivel
                string rutaRecursosNivel = Path.Combine(dir.FullName, "Recursos");
                ruta = BuscarEnDirectorio(rutaRecursosNivel, nombresArchivos);
                if (ruta != null) return ruta;

                dir = Directory.GetParent(dir.FullName);
                niveles++;
            }

            return null;
        }

        private static string? BuscarEnDirectorio(string directorio, string[] nombresArchivos)
        {
            if (!Directory.Exists(directorio)) return null;

            foreach (string nombre in nombresArchivos)
            {
                string rutaCompleta = Path.Combine(directorio, nombre);
                if (File.Exists(rutaCompleta))
                    return rutaCompleta;
            }
            return null;
        }

        private void btnTogglePassword_Click(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !txtPassword.UseSystemPasswordChar;
            btnTogglePassword.Text = txtPassword.UseSystemPasswordChar ? "👁" : "🔒";
        }

        private void lnkCerrarSistema_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Application.Exit();
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Ingrese usuario y contraseña.", "Biblioteca CUBO",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsuario.Focus();
                return;
            }

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText = "SELECT Rol FROM Usuarios WHERE Usuario = @usuario AND Contrasena = @contrasena;";
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@contrasena", password);

                var rol = cmd.ExecuteScalar()?.ToString();

                if (rol != null)
                {
                    SesionGlobal.NombreUsuario = usuario;
                    SesionGlobal.Rol = rol;
                    AbrirFormularioPrincipal();
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error al autenticar: {ex.Message}",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirFormularioPrincipal()
        {
            Form1 form1 = new Form1();
            form1.FormClosed += (_, _) => Application.Exit();
            form1.Show();
            this.Hide();
        }
    }
}