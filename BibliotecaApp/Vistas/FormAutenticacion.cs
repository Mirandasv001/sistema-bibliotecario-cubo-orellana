using System;
using System.Drawing;
using System.Windows.Forms;

namespace BibliotecaApp
{
    /// <summary>
    /// Formulario de autenticación reutilizable para acciones críticas que requieren
    /// credenciales de administrador. Diseñado para ser consistente con la UI del sistema.
    /// </summary>
    public class FormAutenticacion : Form
    {
        private readonly string _rolRequerido;
        private TextBox _txtUsuario;
        private TextBox _txtClave;
        private Button _btnAceptar;

        /// <summary>
        /// Indica si la autenticación fue exitosa.
        /// </summary>
        public bool Autenticado { get; private set; }

        /// <summary>
        /// Crea una instancia del formulario de autenticación.
        /// </summary>
        /// <param name="rolRequerido">Rol requerido para la acción ("Admin" o "Operador").</param>
        /// <param name="mensaje">Mensaje opcional a mostrar al usuario.</param>
        public FormAutenticacion(string rolRequerido = "Admin", string mensaje = "Se requiere autenticación de administrador para realizar esta acción.")
        {
            _rolRequerido = rolRequerido;
            InitializeComponent();
            ConfigurarMensaje(mensaje);
        }

        private void InitializeComponent()
        {
            this.Text = "Autenticación Requerida";
            this.Size = new Size(360, 320);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = EstiloUI.FondoClaro;
            this.KeyPreview = true;
            this.KeyDown += FormAutenticacion_KeyDown;

            // Etiqueta informativa
            var lblInfo = EstiloUI.CrearEtiqueta("Ingrese credenciales de administrador para continuar.");
            lblInfo.Location = new Point(20, 20);
            lblInfo.AutoSize = true;
            lblInfo.MaximumSize = new Size(300, 40);

            // Usuario
            var lblUsuario = EstiloUI.CrearEtiqueta("Usuario:");
            lblUsuario.Location = new Point(20, 70);
            lblUsuario.AutoSize = true;

            _txtUsuario = new TextBox
            {
                Location = new Point(20, 95),
                Width = 300,
                Font = new Font(EstiloUI.FuenteBase, 10F)
            };
            EstiloUI.EstilizarEntrada(_txtUsuario);

            // Contraseña
            var lblClave = EstiloUI.CrearEtiqueta("Contraseña:");
            lblClave.Location = new Point(20, 130);
            lblClave.AutoSize = true;

            _txtClave = new TextBox
            {
                Location = new Point(20, 155),
                Width = 300,
                Font = new Font(EstiloUI.FuenteBase, 10F),
                PasswordChar = '•',
                UseSystemPasswordChar = true
            };
            EstiloUI.EstilizarEntrada(_txtClave);

            // Panel inferior para botones (alineación profesional)
            var panelBotones = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 55,
                Padding = new Padding(15, 10, 15, 10),
                BackColor = Color.Transparent
            };

            // Botón Cancelar (se agrega primero por RightToLeft)
            var btnCancelar = new Button
            {
                Text = "Cancelar",
                Size = new Size(100, 35),
                DialogResult = DialogResult.Cancel,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            EstiloUI.EstilizarBotonSecundario(btnCancelar);
            btnCancelar.Margin = new Padding(0, 0, 10, 0);

            // Botón Aceptar (Confirmar)
            _btnAceptar = new Button
            {
                Text = "Aceptar",
                Size = new Size(100, 35),
                DialogResult = DialogResult.OK,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            EstiloUI.EstilizarBotonPrimario(_btnAceptar);
            _btnAceptar.Click += BtnAceptar_Click;

            // Agregar botones al panel (orden: Cancelar, luego Aceptar por RightToLeft)
            panelBotones.Controls.Add(btnCancelar);
            panelBotones.Controls.Add(_btnAceptar);

            // Agregar controles al formulario
            this.Controls.Add(panelBotones);
            this.Controls.Add(lblInfo);
            this.Controls.Add(_txtUsuario);
            this.Controls.Add(_txtClave);

            this.AcceptButton = _btnAceptar;
            this.CancelButton = btnCancelar;
        }

        private void ConfigurarMensaje(string mensaje)
        {
            // El mensaje se muestra en la primera etiqueta
            if (Controls.Count > 0 && Controls[0] is Label lbl)
            {
                lbl.Text = mensaje;
            }
        }

        private void BtnAceptar_Click(object? sender, EventArgs e)
        {
            string usuario = _txtUsuario.Text.Trim();
            string clave = _txtClave.Text;

            bool esValido = false;

            // Validar según el rol requerido
            if (_rolRequerido == "Admin")
            {
                // Credenciales de administrador
                if (usuario == "AdminCubo" && clave == "Admin123$")
                {
                    esValido = true;
                }
            }
            else if (_rolRequerido == "Operador")
            {
                // Credenciales de operador
                if (usuario == "UserCubo" && clave == "1234$")
                {
                    esValido = true;
                }
            }

            if (!esValido)
            {
                MessageBox.Show("Credenciales incorrectas. Operación denegada.",
                    "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _txtClave.Clear();
                _txtClave.Focus();
                this.DialogResult = DialogResult.None; // Evita cerrar el formulario
            }
            else
            {
                this.Autenticado = true;
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