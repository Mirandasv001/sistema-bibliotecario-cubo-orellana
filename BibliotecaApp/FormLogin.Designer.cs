namespace BibliotecaApp
{
    partial class FormLogin
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelFondo = new Panel();
            panelCard = new Panel();
            pictureBoxLogo = new PictureBox();
            lblTitulo = new Label();
            lblSubtitulo = new Label();
            lineSeparador = new Panel();
            lblSeguridad = new Label();
            lblUsuario = new Label();
            txtUsuario = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            btnIngresar = new Button();
            btnCerrar = new Button();
            btnMinimizar = new Button();
            btnMaximizar = new Button();
            btnCerrarVentana = new Button();
            SuspendLayout();
            panelFondo.SuspendLayout();
            panelCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).BeginInit();

            // ── panelFondo (fondo pantalla completa) ──
            panelFondo.Dock = DockStyle.Fill;
            panelFondo.BackColor = Color.FromArgb(22, 32, 50);
            panelFondo.Controls.Add(panelCard);
            panelFondo.Controls.Add(btnCerrarVentana);
            panelFondo.Controls.Add(btnMaximizar);
            panelFondo.Controls.Add(btnMinimizar);

            // ── panelCard (tarjeta centrada) ──
            panelCard.BackColor = Color.White;
            panelCard.Size = new Size(440, 560);
            panelCard.Anchor = AnchorStyles.None;
            panelCard.Controls.Add(pictureBoxLogo);
            panelCard.Controls.Add(lblTitulo);
            panelCard.Controls.Add(lblSubtitulo);
            panelCard.Controls.Add(lineSeparador);
            panelCard.Controls.Add(lblSeguridad);
            panelCard.Controls.Add(lblUsuario);
            panelCard.Controls.Add(txtUsuario);
            panelCard.Controls.Add(lblPassword);
            panelCard.Controls.Add(txtPassword);
            panelCard.Controls.Add(btnIngresar);
            panelCard.Controls.Add(btnCerrar);

            // ── pictureBoxLogo ──
            pictureBoxLogo.Size = new Size(140, 100);
            pictureBoxLogo.Location = new Point(150, 30);
            pictureBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxLogo.BackColor = Color.Transparent;
            pictureBoxLogo.BorderStyle = BorderStyle.None;

            // ── lblTitulo ──
            lblTitulo.AutoSize = false;
            lblTitulo.Size = new Size(380, 32);
            lblTitulo.Location = new Point(30, 145);
            lblTitulo.Text = "Sistema de Gestión Bibliotecaria";
            lblTitulo.Font = new Font(EstiloUI.FuenteBase, 17F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(22, 32, 50);
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;

            // ── lblSubtitulo ──
            lblSubtitulo.AutoSize = false;
            lblSubtitulo.Size = new Size(380, 20);
            lblSubtitulo.Location = new Point(30, 178);
            lblSubtitulo.Text = "Centros Urbanos de Bienestar y Oportunidades";
            lblSubtitulo.Font = new Font(EstiloUI.FuenteBase, 9.5F, FontStyle.Regular);
            lblSubtitulo.ForeColor = Color.FromArgb(100, 110, 130);
            lblSubtitulo.TextAlign = ContentAlignment.MiddleCenter;

            // ── lineSeparador ──
            lineSeparador.BackColor = Color.FromArgb(220, 225, 235);
            lineSeparador.Size = new Size(320, 1);
            lineSeparador.Location = new Point(60, 212);

            // ── lblSeguridad ──
            lblSeguridad.AutoSize = false;
            lblSeguridad.Size = new Size(380, 20);
            lblSeguridad.Location = new Point(30, 224);
            lblSeguridad.Text = "Acceso exclusivo para personal autorizado";
            lblSeguridad.Font = new Font(EstiloUI.FuenteBase, 8.5F, FontStyle.Italic);
            lblSeguridad.ForeColor = Color.FromArgb(85, 85, 85);
            lblSeguridad.TextAlign = ContentAlignment.MiddleCenter;

            // ── lblUsuario ──
            lblUsuario.AutoSize = true;
            lblUsuario.Location = new Point(55, 262);
            lblUsuario.Text = "Usuario";
            lblUsuario.Font = new Font(EstiloUI.FuenteBase, 9F, FontStyle.Bold);
            lblUsuario.ForeColor = Color.FromArgb(50, 58, 72);

            // ── txtUsuario ──
            txtUsuario.Location = new Point(55, 286);
            txtUsuario.Size = new Size(330, 36);
            txtUsuario.Font = new Font(EstiloUI.FuenteBase, 11F);
            txtUsuario.BackColor = Color.FromArgb(248, 249, 252);
            txtUsuario.ForeColor = Color.FromArgb(40, 46, 58);
            txtUsuario.BorderStyle = BorderStyle.FixedSingle;

            // ── lblPassword ──
            lblPassword.AutoSize = true;
            lblPassword.Location = new Point(55, 334);
            lblPassword.Text = "Contraseña";
            lblPassword.Font = new Font(EstiloUI.FuenteBase, 9F, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(50, 58, 72);

            // ── txtPassword ──
            txtPassword.Location = new Point(55, 358);
            txtPassword.Size = new Size(330, 36);
            txtPassword.Font = new Font(EstiloUI.FuenteBase, 11F);
            txtPassword.BackColor = Color.FromArgb(248, 249, 252);
            txtPassword.ForeColor = Color.FromArgb(40, 46, 58);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.UseSystemPasswordChar = true;

            // ── btnIngresar ──
            btnIngresar.Location = new Point(55, 414);
            btnIngresar.Size = new Size(330, 46);
            btnIngresar.Text = "INGRESAR";
            btnIngresar.FlatStyle = FlatStyle.Flat;
            btnIngresar.FlatAppearance.BorderSize = 0;
            btnIngresar.BackColor = Color.FromArgb(59, 111, 216);
            btnIngresar.ForeColor = Color.White;
            btnIngresar.Font = new Font(EstiloUI.FuenteBase, 11F, FontStyle.Bold);
            btnIngresar.Cursor = Cursors.Hand;
            btnIngresar.Click += btnIngresar_Click;

            // ── btnCerrar (enlace "Cerrar sistema" dentro de la tarjeta) ──
            btnCerrar.FlatStyle = FlatStyle.Flat;
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.BackColor = Color.Transparent;
            btnCerrar.ForeColor = Color.FromArgb(130, 140, 155);
            btnCerrar.Font = new Font(EstiloUI.FuenteBase, 8.5F, FontStyle.Underline);
            btnCerrar.Cursor = Cursors.Hand;
            btnCerrar.Size = new Size(330, 28);
            btnCerrar.Location = new Point(55, 475);
            btnCerrar.Text = "Cerrar sistema";
            btnCerrar.Click += (_, _) => Application.Exit();

            // ── btnMinimizar (control de ventana) ──
            btnMinimizar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMinimizar.FlatStyle = FlatStyle.Flat;
            btnMinimizar.FlatAppearance.BorderSize = 0;
            btnMinimizar.BackColor = Color.Transparent;
            btnMinimizar.ForeColor = Color.FromArgb(204, 204, 204);
            btnMinimizar.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnMinimizar.Size = new Size(40, 30);
            btnMinimizar.Location = new Point(1120, 0);
            btnMinimizar.Text = "─";
            btnMinimizar.Cursor = Cursors.Hand;
            btnMinimizar.Click += btnMinimizar_Click;

            // ── btnMaximizar (control de ventana) ──
            btnMaximizar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMaximizar.FlatStyle = FlatStyle.Flat;
            btnMaximizar.FlatAppearance.BorderSize = 0;
            btnMaximizar.BackColor = Color.Transparent;
            btnMaximizar.ForeColor = Color.FromArgb(204, 204, 204);
            btnMaximizar.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnMaximizar.Size = new Size(40, 30);
            btnMaximizar.Location = new Point(1160, 0);
            btnMaximizar.Text = "◻";
            btnMaximizar.Cursor = Cursors.Hand;
            btnMaximizar.Click += btnMaximizar_Click;

            // ── btnCerrarVentana (control de ventana) ──
            btnCerrarVentana.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCerrarVentana.FlatStyle = FlatStyle.Flat;
            btnCerrarVentana.FlatAppearance.BorderSize = 0;
            btnCerrarVentana.BackColor = Color.Transparent;
            btnCerrarVentana.ForeColor = Color.FromArgb(204, 204, 204);
            btnCerrarVentana.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnCerrarVentana.Size = new Size(40, 30);
            btnCerrarVentana.Location = new Point(1200, 0);
            btnCerrarVentana.Text = "X";
            btnCerrarVentana.Cursor = Cursors.Hand;
            btnCerrarVentana.Click += btnCerrarVentana_Click;

            // ── FormLogin ──
            AcceptButton = btnIngresar;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 750);
            Controls.Add(panelFondo);
            BackColor = Color.FromArgb(22, 32, 50);
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(0, 0);
            Text = "Biblioteca CUBO - Acceso";

            panelCard.ResumeLayout(false);
            panelCard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).EndInit();
            ResumeLayout(false);
        }

        private Panel panelFondo;
        private Panel panelCard;
        private PictureBox pictureBoxLogo;
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Label lblSeguridad;
        private Label lblUsuario;
        private Label lblPassword;
        private TextBox txtUsuario;
        private TextBox txtPassword;
        private Button btnIngresar;
        private Button btnCerrar;
        private Button btnMinimizar;
        private Button btnMaximizar;
        private Button btnCerrarVentana;
        private Panel lineSeparador;
    }
}
