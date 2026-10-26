using System;
using System.Drawing;
using System.Windows.Forms;

namespace BibliotecaApp
{
    public class FormAutenticacion : Form
    {
        private TextBox _txtUsuario;
        private TextBox _txtClave;

        public bool Autenticado { get; private set; }

        // Diálogo que valida contra la tabla Usuarios (hash PBKDF2) y exige rol Administrador.
        public FormAutenticacion()
        {
            InitializeComponent();

            if (IconoAplicacion.Obtener() is Icon icoLogo) Icon = icoLogo;
        }

        // Punto de entrada único para las acciones destructivas.
        public static bool SolicitarAdmin(IWin32Window? owner)
        {
            using var formulario = new FormAutenticacion();
            return formulario.ShowDialog(owner) == DialogResult.OK && formulario.Autenticado;
        }

        private void InitializeComponent()
        {
            this.Text = "Autenticación requerida";
            this.Size = new Size(340, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = EstiloUI.FondoClaro;
            this.KeyPreview = true;
            this.KeyDown += FormAutenticacion_KeyDown;

            // Usuario
            var lblUsuario = EstiloUI.CrearEtiqueta("Usuario:");
            lblUsuario.Location = new Point(20, 20);
            lblUsuario.AutoSize = true;

            _txtUsuario = new TextBox
            {
                Location = new Point(20, 50),
                Width = 280,
                Font = new Font(EstiloUI.FuenteBase, 10F)
            };
            EstiloUI.EstilizarEntrada(_txtUsuario);

            // Contraseña
            var lblClave = EstiloUI.CrearEtiqueta("Contraseña:");
            lblClave.Location = new Point(20, 95);
            lblClave.AutoSize = true;

            _txtClave = new TextBox
            {
                Location = new Point(20, 125),
                Width = 280,
                Font = new Font(EstiloUI.FuenteBase, 10F),
                PasswordChar = '•'
            };
            EstiloUI.EstilizarEntrada(_txtClave);

            var btnAceptar = new Button
            {
                Text = "Aceptar",
                Location = new Point(110, 185),
                Size = new Size(100, 35),
                DialogResult = DialogResult.OK
            };
            EstiloUI.EstilizarBotonPrimario(btnAceptar);
            btnAceptar.Click += BtnAceptar_Click;

            // Agregar controles al formulario
            this.Controls.AddRange(new Control[] { 
                lblUsuario, _txtUsuario, 
                lblClave, _txtClave, 
                btnAceptar 
            });

            this.AcceptButton = btnAceptar;
        }

        private void BtnAceptar_Click(object? sender, EventArgs e)
        {
            if (!ValidarAdministrador(_txtUsuario.Text.Trim(), _txtClave.Text))
            {
                MessageBox.Show("Credenciales incorrectas o sin privilegios de Administrador. Acción cancelada.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _txtClave.Clear();
                _txtClave.Focus();
                this.DialogResult = DialogResult.None; // Evita cerrar el formulario
            }
            else
            {
                Autenticado = true;
                this.DialogResult = DialogResult.OK;
            }
        }

        // Comprueba el hash PBKDF2 almacenado en la BD y que el rol sea Administrador.
        private static bool ValidarAdministrador(string usuario, string clave)
        {
            if (usuario.Length == 0 || clave.Length == 0) return false;

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText = "SELECT Contrasena, Rol FROM Usuarios WHERE Usuario = @usuario;";
                cmd.Parameters.AddWithValue("@usuario", usuario);

                using var lector = cmd.ExecuteReader();
                if (!lector.Read()) return false;

                string hash = lector.GetString(0);
                string rol = lector.GetString(1);

                bool claveOk = PasswordHasher.IsLegacyHash(hash)
                    ? hash == clave
                    : PasswordHasher.Verify(clave, hash);

                return claveOk && SesionGlobal.EsRolAdministrador(rol);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validando administrador: {ex.Message}");
                return false;
            }
        }

        private void FormAutenticacion_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }
    }
}