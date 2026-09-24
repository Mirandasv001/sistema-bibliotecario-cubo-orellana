using System.ComponentModel;

namespace BibliotecaApp
{
    partial class Form1
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

        #region Código generado por el Diseñador de Windows Forms

        private void InitializeComponent()
        {
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Form1));
            panelMenu = new Panel();
            panelNav = new Panel();
            btnEstadisticas = new Button();
            btnGuiaUso = new Button();
            btnAlertas = new Button();
            btnPrestamos = new Button();
            btnInventario = new Button();
            btnSala = new Button();
            btnCerrarSesion = new Button();
            lblVersion = new Label();
            panelLogo = new Panel();
            pictureBoxLogo = new PictureBox();
            lblNombreApp = new Label();
            panelContenedor = new Panel();
            panelMenu.SuspendLayout();
            panelNav.SuspendLayout();
            panelLogo.SuspendLayout();
            ((ISupportInitialize)pictureBoxLogo).BeginInit();
            SuspendLayout();
            // 
            // panelMenu
            // 
            panelMenu.BackColor = Color.FromArgb(27, 36, 55);
            panelMenu.Controls.Add(panelNav);
            panelMenu.Controls.Add(btnCerrarSesion);
            panelMenu.Controls.Add(lblVersion);
            panelMenu.Controls.Add(panelLogo);
            panelMenu.Dock = DockStyle.Left;
            panelMenu.Location = new Point(0, 0);
            panelMenu.Name = "panelMenu";
            panelMenu.Size = new Size(220, 720);
            panelMenu.TabIndex = 1;
            // 
            // panelNav
            // 
            panelNav.BackColor = Color.FromArgb(27, 36, 55);
            panelNav.Controls.Add(btnEstadisticas);
            panelNav.Controls.Add(btnGuiaUso);
            panelNav.Controls.Add(btnAlertas);
            panelNav.Controls.Add(btnPrestamos);
            panelNav.Controls.Add(btnInventario);
            panelNav.Controls.Add(btnSala);
            panelNav.Dock = DockStyle.Fill;
            panelNav.Location = new Point(0, 180);
            panelNav.Name = "panelNav";
            panelNav.Padding = new Padding(0, 20, 0, 0);
            panelNav.Size = new Size(220, 452);
            panelNav.TabIndex = 0;
            // 
            // btnEstadisticas
            // 
            btnEstadisticas.Location = new Point(0, 0);
            btnEstadisticas.Name = "btnEstadisticas";
            btnEstadisticas.Size = new Size(75, 23);
            btnEstadisticas.TabIndex = 0;
            // 
            // btnGuiaUso
            // 
            btnGuiaUso.Location = new Point(0, 0);
            btnGuiaUso.Name = "btnGuiaUso";
            btnGuiaUso.Size = new Size(75, 23);
            btnGuiaUso.TabIndex = 1;
            // 
            // btnAlertas
            // 
            btnAlertas.Location = new Point(0, 0);
            btnAlertas.Name = "btnAlertas";
            btnAlertas.Size = new Size(75, 23);
            btnAlertas.TabIndex = 1;
            // 
            // btnPrestamos
            // 
            btnPrestamos.Location = new Point(0, 0);
            btnPrestamos.Name = "btnPrestamos";
            btnPrestamos.Size = new Size(75, 23);
            btnPrestamos.TabIndex = 2;
            // 
            // btnInventario
            // 
            btnInventario.Location = new Point(0, 0);
            btnInventario.Name = "btnInventario";
            btnInventario.Size = new Size(75, 23);
            btnInventario.TabIndex = 3;
            // 
            // btnSala
            // 
            btnSala.Location = new Point(0, 0);
            btnSala.Name = "btnSala";
            btnSala.Size = new Size(75, 23);
            btnSala.TabIndex = 4;
            // 
            // btnCerrarSesion
            // 
            btnCerrarSesion.BackColor = Color.FromArgb(27, 36, 55);
            btnCerrarSesion.Cursor = Cursors.Hand;
            btnCerrarSesion.Dock = DockStyle.Bottom;
            btnCerrarSesion.FlatAppearance.BorderSize = 0;
            btnCerrarSesion.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 30, 30);
            btnCerrarSesion.FlatStyle = FlatStyle.Flat;
            btnCerrarSesion.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCerrarSesion.ForeColor = Color.White;
            btnCerrarSesion.Location = new Point(0, 632);
            btnCerrarSesion.Name = "btnCerrarSesion";
            btnCerrarSesion.Padding = new Padding(15, 0, 0, 0);
            btnCerrarSesion.Size = new Size(220, 48);
            btnCerrarSesion.TabIndex = 1;
            btnCerrarSesion.Text = "Cerrar Sesión";
            btnCerrarSesion.TextAlign = ContentAlignment.MiddleLeft;
            btnCerrarSesion.UseVisualStyleBackColor = false;
            btnCerrarSesion.Click += btnCerrarSesion_Click;
            // 
            // lblVersion
            // 
            lblVersion.Dock = DockStyle.Bottom;
            lblVersion.Font = new Font("Segoe UI", 8.5F);
            lblVersion.ForeColor = Color.FromArgb(140, 150, 170);
            lblVersion.Location = new Point(0, 680);
            lblVersion.Name = "lblVersion";
            lblVersion.Padding = new Padding(16, 8, 8, 12);
            lblVersion.Size = new Size(220, 40);
            lblVersion.TabIndex = 2;
            lblVersion.Text = "Sitio del Niño © 2026";
            lblVersion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelLogo
            // 
            panelLogo.BackColor = Color.FromArgb(27, 36, 55);
            panelLogo.Controls.Add(pictureBoxLogo);
            panelLogo.Controls.Add(lblNombreApp);
            panelLogo.Dock = DockStyle.Top;
            panelLogo.Location = new Point(0, 0);
            panelLogo.Name = "panelLogo";
            panelLogo.Size = new Size(220, 180);
            panelLogo.TabIndex = 3;
            // 
            // pictureBoxLogo
            // 
            pictureBoxLogo.BackColor = Color.White;
            pictureBoxLogo.Image = (Image)resources.GetObject("pictureBoxLogo.Image");
            pictureBoxLogo.Location = new Point(6, 3);
            pictureBoxLogo.Name = "pictureBoxLogo";
            pictureBoxLogo.Size = new Size(211, 132);
            pictureBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxLogo.TabIndex = 0;
            pictureBoxLogo.TabStop = false;
            pictureBoxLogo.Click += pictureBoxLogo_Click;
            // 
            // lblNombreApp
            // 
            lblNombreApp.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblNombreApp.ForeColor = Color.White;
            lblNombreApp.Location = new Point(12, 131);
            lblNombreApp.Name = "lblNombreApp";
            lblNombreApp.Size = new Size(180, 46);
            lblNombreApp.TabIndex = 3;
            lblNombreApp.Text = "BIBLIOTECA ";
            lblNombreApp.TextAlign = ContentAlignment.MiddleCenter;
            lblNombreApp.Click += lblNombreApp_Click;
            // 
            // panelContenedor
            // 
            panelContenedor.BackColor = Color.FromArgb(244, 246, 250);
            panelContenedor.Dock = DockStyle.Fill;
            panelContenedor.Location = new Point(220, 0);
            panelContenedor.Name = "panelContenedor";
            panelContenedor.Padding = new Padding(10, 8, 10, 10);
            panelContenedor.Size = new Size(1060, 720);
            panelContenedor.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1280, 720);
            Controls.Add(panelContenedor);
            Controls.Add(panelMenu);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(1150, 660);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Biblioteca CUBO — Sistema de Gestión";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            panelMenu.ResumeLayout(false);
            panelNav.ResumeLayout(false);
            panelLogo.ResumeLayout(false);
            ((ISupportInitialize)pictureBoxLogo).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelMenu;
        private Panel panelLogo;
        private PictureBox pictureBoxLogo;
        private Label lblNombreApp;
        private Panel panelNav;
        private Button btnSala;
        private Button btnPrestamos;
        private Button btnInventario;
        private Button btnAlertas;
        private Button btnGuiaUso;
        private Button btnEstadisticas;
        private Label lblVersion;
        private Button btnCerrarSesion;
        private Panel panelContenedor;
    }
}
