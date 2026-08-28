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
            panelMenu = new Panel();
            panelLogo = new Panel();
            pictureBoxLogo = new PictureBox();
            lblNombreApp = new Label();
            panelNav = new Panel();
            btnSala = new Button();
            btnPrestamos = new Button();
            btnInventario = new Button();
            btnAlertas = new Button();
            btnGuiaUso = new Button();
            lblVersion = new Label();
            panelContenedor = new Panel();
            panelMenu.SuspendLayout();
            panelLogo.SuspendLayout();
            ((ISupportInitialize)pictureBoxLogo).BeginInit();
            panelNav.SuspendLayout();
            SuspendLayout();
            // 
            // panelMenu
            // 
            panelMenu.BackColor = EstiloUI.FondoOscuro;
            // Orden de la colección: el último agregado se acopla primero,
            // por eso panelLogo queda al inicio visual y panelNav rellena el resto.
            panelMenu.Controls.Add(panelNav);
            panelMenu.Controls.Add(lblVersion);
            panelMenu.Controls.Add(lblNombreApp);
            panelMenu.Controls.Add(panelLogo);
            panelMenu.Dock = DockStyle.Left;
            panelMenu.Location = new Point(0, 0);
            panelMenu.Name = "panelMenu";
            panelMenu.Size = new Size(220, 720);
            // 
            // panelLogo
            // 
            panelLogo.BackColor = EstiloUI.FondoOscuro;
            panelLogo.Controls.Add(pictureBoxLogo);
            panelLogo.Dock = DockStyle.Top;
            panelLogo.Name = "panelLogo";
            panelLogo.Size = new Size(220, 150);
            // 
            // pictureBoxLogo
            // 
            pictureBoxLogo.BackColor = EstiloUI.FondoOscuro;
            pictureBoxLogo.Dock = DockStyle.Fill;
            pictureBoxLogo.Name = "pictureBoxLogo";
            pictureBoxLogo.Padding = new Padding(14, 12, 14, 4);
            pictureBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;
            // 
            // lblNombreApp
            // 
            lblNombreApp.Dock = DockStyle.Top;
            lblNombreApp.Font = new Font(EstiloUI.FuenteBase, 13F, FontStyle.Bold);
            lblNombreApp.ForeColor = EstiloUI.Blanco;
            lblNombreApp.Name = "lblNombreApp";
            lblNombreApp.Padding = new Padding(0, 2, 0, 10);
            lblNombreApp.Size = new Size(220, 46);
            lblNombreApp.Text = "BIBLIOTECA CUBO";
            lblNombreApp.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelNav
            // 
            panelNav.AutoScroll = false;
            panelNav.BackColor = EstiloUI.FondoOscuro;
            panelNav.Dock = DockStyle.Fill;
            panelNav.Name = "panelNav";
            panelNav.Padding = new Padding(0, 20, 0, 0);
            panelNav.Size = new Size(220, 484);
            // Los botones se agregan en orden inverso para que queden apilados hacia abajo.
            panelNav.Controls.Add(btnGuiaUso);
            panelNav.Controls.Add(btnAlertas);
            panelNav.Controls.Add(btnPrestamos);
            panelNav.Controls.Add(btnInventario);
            panelNav.Controls.Add(btnSala);
            // 
            // Configuración estricta de cada botón de navegación
            // 
            ConfigurarBotonMenu(btnSala, "Control de Sala");
            ConfigurarBotonMenu(btnInventario, "Inventario");
            ConfigurarBotonMenu(btnPrestamos, "Préstamos Externos");
            ConfigurarBotonMenu(btnAlertas, "Alertas de Vencidos");
            ConfigurarBotonMenu(btnGuiaUso, "Guía de Uso");
            btnSala.Click += (_, _) => MostrarApartadoSala();
            btnPrestamos.Click += (_, _) => MostrarApartadoPrestamos();
            btnInventario.Click += (_, _) => MostrarApartadoInventario();
            btnAlertas.Click += (_, _) => MostrarApartadoAlertas();
            btnAlertas.Paint += btnAlertas_Paint;
            btnGuiaUso.Click += (_, _) => btnGuiaUso_Click();
            // 
            // lblVersion
            // 
            lblVersion.Dock = DockStyle.Bottom;
            lblVersion.Font = EstiloUI.Subtitulo();
            lblVersion.ForeColor = Color.FromArgb(140, 150, 170);
            lblVersion.Name = "lblVersion";
            lblVersion.Padding = new Padding(16, 8, 8, 12);
            lblVersion.Size = new Size(220, 40);
            lblVersion.Text = "Sitio del Niño © 2026";
            lblVersion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panelContenedor
            // 
            panelContenedor.BackColor = EstiloUI.FondoClaro;
            panelContenedor.Dock = DockStyle.Fill;
            panelContenedor.Name = "panelContenedor";
            panelContenedor.Padding = new Padding(10, 8, 10, 10);
            panelContenedor.Size = new Size(1060, 720);
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1280, 720);
            Controls.Add(panelContenedor);
            Controls.Add(panelMenu);
            Font = new Font(EstiloUI.FuenteBase, 9F);
            MinimumSize = new Size(1150, 660);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Biblioteca CUBO — Sistema de Gestión";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            panelContenedor.BringToFront();
            panelMenu.ResumeLayout(false);
            panelLogo.ResumeLayout(false);
            ((ISupportInitialize)pictureBoxLogo).EndInit();
            panelNav.ResumeLayout(false);
            ResumeLayout(false);
        }
             // Apartado importante
        private static void ConfigurarBotonMenu(Button boton, string texto)
        {
            boton.Dock = DockStyle.Top;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = EstiloUI.HoverOscuro;
            boton.BackColor = EstiloUI.FondoOscuro;
            boton.ForeColor = Color.White;
            boton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            boton.Height = 50;
            boton.TextAlign = ContentAlignment.MiddleLeft;
            boton.Padding = new Padding(15, 0, 0, 0);
            boton.Cursor = Cursors.Hand;
            boton.Text = texto;
            boton.UseVisualStyleBackColor = false;
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
        private Label lblVersion;
        private Panel panelContenedor;
    }
}
