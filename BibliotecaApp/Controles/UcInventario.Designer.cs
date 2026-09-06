using System.ComponentModel;

namespace BibliotecaApp
{
    partial class UcInventario
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

        #region Código generado por el Diseñador de componentes

        private void InitializeComponent()
        {
            panelEncabezado = new Panel();
            lblTitulo = new Label();
            lblSubtitulo = new Label();
            panelBusqueda = new Panel();
            lblBuscar = EstiloUI.CrearEtiqueta("Buscar por Código o Título:");
            txtBuscar = new TextBox();
            lblContador = new Label();
            dgvInventario = new DataGridView();
            panelEncabezado.SuspendLayout();
            panelBusqueda.SuspendLayout();
            ((ISupportInitialize)dgvInventario).BeginInit();
            SuspendLayout();
            // 
            // panelEncabezado
            // 
            panelEncabezado.BackColor = EstiloUI.Blanco;
            panelEncabezado.Controls.Add(lblTitulo);
            panelEncabezado.Controls.Add(lblSubtitulo);
            panelEncabezado.Dock = DockStyle.Top;
            panelEncabezado.Location = new Point(0, 0);
            panelEncabezado.Name = "panelEncabezado";
            panelEncabezado.Size = new Size(980, 62);
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = EstiloUI.TituloSeccion();
            lblTitulo.ForeColor = EstiloUI.TextoOscuro;
            lblTitulo.Location = new Point(16, 10);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Text = "Inventario General de Libros";
            // 
            // lblSubtitulo
            // 
            lblSubtitulo.AutoSize = true;
            lblSubtitulo.Font = EstiloUI.Subtitulo();
            lblSubtitulo.ForeColor = Color.Gray;
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Text = "Catálogo completo importado del archivo CSV — búsqueda instantánea";
            lblSubtitulo.Location = new Point(19, 36);
            // 
            // panelBusqueda
            // 
            panelBusqueda.Controls.Add(lblBuscar);
            panelBusqueda.Controls.Add(txtBuscar);
            panelBusqueda.Controls.Add(lblContador);
            panelBusqueda.Dock = DockStyle.Top;
            panelBusqueda.Location = new Point(0, 62);
            panelBusqueda.Name = "panelBusqueda";
            panelBusqueda.Padding = new Padding(14, 10, 14, 8);
            panelBusqueda.Size = new Size(980, 56);
            // 
            // lblBuscar
            // 
            lblBuscar.Location = new Point(14, 18);
            lblBuscar.Name = "lblBuscar";
            lblBuscar.AutoSize = false;
            lblBuscar.Size = new Size(190, 23);
            lblBuscar.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtBuscar
            // 
            txtBuscar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            EstiloUI.EstilizarEntrada(txtBuscar);
            txtBuscar.Location = new Point(210, 13);
            txtBuscar.Name = "txtBuscar";
            txtBuscar.PlaceholderText = "Escriba para filtrar el inventario en tiempo real...";
            txtBuscar.Size = new Size(590, 27);
            txtBuscar.TabIndex = 0;
            txtBuscar.TextChanged += txtBuscar_TextChanged;
            // 
            // lblContador
            // 
            lblContador.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblContador.AutoSize = true;
            lblContador.Font = EstiloUI.Etiqueta();
            lblContador.ForeColor = Color.Gray;
            lblContador.Location = new Point(820, 18);
            lblContador.Name = "lblContador";
            lblContador.Text = "0 libros";
            // 
            // dgvInventario
            // 
            dgvInventario.AllowUserToAddRows = false;
            dgvInventario.AllowUserToDeleteRows = false;
            dgvInventario.AllowUserToResizeRows = false;
            dgvInventario.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvInventario.BackgroundColor = EstiloUI.Blanco;
            dgvInventario.BorderStyle = BorderStyle.None;
            dgvInventario.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvInventario.Dock = DockStyle.Fill;
            dgvInventario.EnableHeadersVisualStyles = false;
            dgvInventario.MultiSelect = false;
            dgvInventario.ReadOnly = true;
            dgvInventario.RowHeadersVisible = false;
            dgvInventario.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInventario.CellDoubleClick += dgvInventario_CellDoubleClick;
            // 
            // UcInventario
            // 
            BackColor = EstiloUI.FondoClaro;
            Controls.Add(dgvInventario);
            Controls.Add(panelBusqueda);
            Controls.Add(panelEncabezado);
            Name = "UcInventario";
            Size = new Size(980, 600);
            Load += UcInventario_Load;
            panelEncabezado.ResumeLayout(false);
            panelEncabezado.PerformLayout();
            panelBusqueda.ResumeLayout(false);
            panelBusqueda.PerformLayout();
            ((ISupportInitialize)dgvInventario).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelEncabezado;
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Panel panelBusqueda;
        private Label lblBuscar;
        private TextBox txtBuscar;
        private Label lblContador;
        private DataGridView dgvInventario;
    }
}
