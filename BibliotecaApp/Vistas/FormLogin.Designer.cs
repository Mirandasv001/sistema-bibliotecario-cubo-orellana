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
            pictureBoxLogo = new PictureBox();
            lblTitulo = new Label();
            lblUsuario = new Label();
            txtUsuario = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            btnTogglePassword = new Button();
            btnIngresar = new Button();
            lnkCerrarSistema = new LinkLabel();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).BeginInit();
            SuspendLayout();
            // 
            // pictureBoxLogo
            // 
            pictureBoxLogo.BackColor = Color.Transparent;
            pictureBoxLogo.Location = new Point(50, 30);
            pictureBoxLogo.Name = "pictureBoxLogo";
            pictureBoxLogo.Size = new Size(300, 120);
            pictureBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxLogo.TabIndex = 0;
            pictureBoxLogo.TabStop = false;
            // 
            // lblTitulo
            // 
            lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(50, 58, 72);
            lblTitulo.Location = new Point(40, 160);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(320, 30);
            lblTitulo.TabIndex = 1;
            lblTitulo.Text = "Sistema Bibliotecario CUBO";
            lblTitulo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblUsuario
            // 
            lblUsuario.AutoSize = true;
            lblUsuario.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUsuario.ForeColor = Color.FromArgb(50, 58, 72);
            lblUsuario.Location = new Point(70, 210);
            lblUsuario.Name = "lblUsuario";
            lblUsuario.Size = new Size(49, 15);
            lblUsuario.TabIndex = 2;
            lblUsuario.Text = "Usuario";
            // 
            // txtUsuario
            // 
            txtUsuario.BackColor = Color.White;
            txtUsuario.BorderStyle = BorderStyle.FixedSingle;
            txtUsuario.Font = new Font("Segoe UI", 10F);
            txtUsuario.ForeColor = Color.FromArgb(40, 46, 58);
            txtUsuario.Location = new Point(70, 235);
            txtUsuario.Name = "txtUsuario";
            txtUsuario.Size = new Size(260, 25);
            txtUsuario.TabIndex = 3;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(50, 58, 72);
            lblPassword.Location = new Point(70, 280);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(69, 15);
            lblPassword.TabIndex = 4;
            lblPassword.Text = "Contraseña";
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.White;
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Font = new Font("Segoe UI", 10F);
            txtPassword.ForeColor = Color.FromArgb(40, 46, 58);
            txtPassword.Location = new Point(70, 305);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(260, 25);
            txtPassword.TabIndex = 5;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnTogglePassword
            // 
            btnTogglePassword.BackColor = Color.White;
            btnTogglePassword.Cursor = Cursors.Hand;
            btnTogglePassword.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            btnTogglePassword.FlatStyle = FlatStyle.Flat;
            btnTogglePassword.Font = new Font("Segoe UI", 10F);
            btnTogglePassword.ForeColor = Color.FromArgb(100, 100, 100);
            btnTogglePassword.Location = new Point(336, 305);
            btnTogglePassword.Name = "btnTogglePassword";
            btnTogglePassword.Size = new Size(40, 27);
            btnTogglePassword.TabIndex = 6;
            btnTogglePassword.Text = "👁";
            btnTogglePassword.UseVisualStyleBackColor = false;
            btnTogglePassword.Click += btnTogglePassword_Click;
            // 
            // btnIngresar
            // 
            btnIngresar.BackColor = Color.FromArgb(41, 128, 185);
            btnIngresar.Cursor = Cursors.Hand;
            btnIngresar.FlatAppearance.BorderSize = 0;
            btnIngresar.FlatStyle = FlatStyle.Flat;
            btnIngresar.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnIngresar.ForeColor = Color.White;
            btnIngresar.Location = new Point(70, 370);
            btnIngresar.Name = "btnIngresar";
            btnIngresar.Size = new Size(260, 40);
            btnIngresar.TabIndex = 7;
            btnIngresar.Text = "INICIAR SESIÓN";
            btnIngresar.UseVisualStyleBackColor = false;
            btnIngresar.Click += btnIngresar_Click;
            // 
            // lnkCerrarSistema
            // 
            lnkCerrarSistema.ActiveLinkColor = Color.FromArgb(41, 128, 185);
            lnkCerrarSistema.Font = new Font("Segoe UI", 9F);
            lnkCerrarSistema.LinkColor = Color.FromArgb(100, 110, 130);
            lnkCerrarSistema.Location = new Point(40, 425);
            lnkCerrarSistema.Name = "lnkCerrarSistema";
            lnkCerrarSistema.Size = new Size(320, 25);
            lnkCerrarSistema.TabIndex = 8;
            lnkCerrarSistema.TabStop = true;
            lnkCerrarSistema.Text = "Cerrar sistema";
            lnkCerrarSistema.TextAlign = ContentAlignment.MiddleCenter;
            lnkCerrarSistema.VisitedLinkColor = Color.FromArgb(100, 110, 130);
            lnkCerrarSistema.LinkClicked += lnkCerrarSistema_LinkClicked;
            // 
            // FormLogin
            // 
            AcceptButton = btnIngresar;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(400, 540);
            Controls.Add(pictureBoxLogo);
            Controls.Add(lblTitulo);
            Controls.Add(lblUsuario);
            Controls.Add(txtUsuario);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(btnTogglePassword);
            Controls.Add(btnIngresar);
            Controls.Add(lnkCerrarSistema);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Biblioteca CUBO - Acceso";
            ((System.ComponentModel.ISupportInitialize)pictureBoxLogo).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private PictureBox pictureBoxLogo;
        private Label lblTitulo;
        private Label lblUsuario;
        private TextBox txtUsuario;
        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnTogglePassword;
        private Button btnIngresar;
        private LinkLabel lnkCerrarSistema;
    }
}