using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
            AsignarIconoAplicacion();
            CargarLogo();
        }

        /// <summary>
        /// Asigna el icono institucional CUBO a la barra de título con fallback seguro.
        /// </summary>
        private void AsignarIconoAplicacion()
        {
            try
            {
                // Prioridad 1: extraer el icono embebido en el .exe (ApplicationIcon del csproj).
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                try
                {
                    // Prioridad 2: cargar desde Recursos\app.ico junto al .exe.
                    string rutaIcono = Path.Combine(AppContext.BaseDirectory, "Recursos", "app.ico");
                    if (File.Exists(rutaIcono))
                        this.Icon = new Icon(rutaIcono);
                }
                catch
                {
                    // Fallback: icono por defecto del sistema (no crashea).
                    this.Icon = SystemIcons.Application;
                }
            }
        }

        private void CargarLogo()
        {
            try
            {
                // junto al .exe; con MemoryStream para no dejar el archivo bloqueado
                string rutaLogo = Path.Combine(AppContext.BaseDirectory, "Recursos", "Logo de inicio.jpg");
                if (File.Exists(rutaLogo))
                {
                    byte[] imageBytes = File.ReadAllBytes(rutaLogo);
                    using var ms = new MemoryStream(imageBytes);
                    // Deep copy: el Bitmap copia los píxeles al heap y no depende del
                    // stream, que se libera al salir del using sin bloquear el .jpg.
                    using var imgOriginal = Image.FromStream(ms);
                    pictureBoxLogo.Image = new Bitmap(imgOriginal);
                }
            }
            catch
            {
                // Si no se carga el logo, la interfaz funciona igualmente.
            }
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
                // Obtenemos el hash almacenado y el rol
                cmd.CommandText = "SELECT Contrasena, Rol FROM Usuarios WHERE Usuario = @usuario;";
                cmd.Parameters.AddWithValue("@usuario", usuario);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string storedHash = reader.GetString(0);
                    string rol = reader.GetString(1);

                    // Verificar contraseña (soporta hash PBKDF2 y legacy texto plano)
                    bool passwordValid;
                    if (PasswordHasher.IsLegacyHash(storedHash))
                    {
                        // Migración silenciosa: texto plano -> hash
                        passwordValid = (storedHash == password);
                        if (passwordValid)
                        {
                            // Actualizar a hash seguro
                            using var updateCmd = conexion.CreateCommand();
                            updateCmd.CommandText = "UPDATE Usuarios SET Contrasena = @newhash WHERE Usuario = @usuario;";
                            updateCmd.Parameters.AddWithValue("@newhash", PasswordHasher.Hash(password));
                            updateCmd.Parameters.AddWithValue("@usuario", usuario);
                            updateCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        passwordValid = PasswordHasher.Verify(password, storedHash);
                    }

                    if (passwordValid)
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