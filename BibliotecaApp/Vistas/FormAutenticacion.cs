using System;
using System.Drawing;
using System.Windows.Forms;

namespace BibliotecaApp
{
    public class FormAutenticacion : Form
    {
        private readonly string _usuarioEsperado;
        private readonly string _claveEsperada;
        private TextBox _txtUsuario;
        private TextBox _txtClave;

        public bool Autenticado { get; private set; }

        public FormAutenticacion(string usuarioEsperado, string claveEsperada)
        {
            _usuarioEsperado = usuarioEsperado;
            _claveEsperada = claveEsperada;
            InitializeComponent();

            if (IconoAplicacion.Obtener() is Icon icoLogo) Icon = icoLogo;
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
            if (_txtUsuario.Text != _usuarioEsperado || _txtClave.Text != _claveEsperada)
            {
                MessageBox.Show("Credenciales incorrectas. Acción cancelada.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _txtClave.Clear();
                _txtClave.Focus();
                this.DialogResult = DialogResult.None; // Evita cerrar el formulario
            }
            else
            {
                this.DialogResult = DialogResult.OK;
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