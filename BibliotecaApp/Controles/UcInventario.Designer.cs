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
            panelBusqueda = new Panel();
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
            panelEncabezado.BackColor = Color.White;
            panelEncabezado.Controls.Add(lblTitulo);
            panelEncabezado.Dock = DockStyle.Top;
            panelEncabezado.Location = new Point(0, 0);
            panelEncabezado.Name = "panelEncabezado";
            panelEncabezado.Size = new Size(980, 62);
            panelEncabezado.TabIndex = 2;
            panelEncabezado.Paint += panelEncabezado_Paint;
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(43, 45, 66);
            lblTitulo.Location = new Point(16, 10);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Size = new Size(339, 32);
            lblTitulo.TabIndex = 0;
            lblTitulo.Text = "Inventario General de Libros";
            // 
            // panelBusqueda
            // 
            panelBusqueda.Controls.Add(txtBuscar);
            panelBusqueda.Controls.Add(lblContador);
            panelBusqueda.Dock = DockStyle.Top;
            panelBusqueda.Location = new Point(0, 62);
            panelBusqueda.Name = "panelBusqueda";
            panelBusqueda.Padding = new Padding(14, 10, 14, 8);
            panelBusqueda.Size = new Size(980, 56);
            panelBusqueda.TabIndex = 1;
            // 
            // txtBuscar
            // 
            txtBuscar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
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
            lblContador.Font = new Font("Segoe UI", 9.5F);
            lblContador.ForeColor = Color.Gray;
            lblContador.Location = new Point(820, 18);
            lblContador.Name = "lblContador";
            lblContador.Size = new Size(62, 21);
            lblContador.TabIndex = 1;
            lblContador.Text = "0 libros";
            // 
            // dgvInventario
            // 
            dgvInventario.AllowUserToAddRows = false;
            dgvInventario.AllowUserToDeleteRows = false;
            dgvInventario.AllowUserToResizeRows = false;
            dgvInventario.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvInventario.BackgroundColor = Color.White;
            dgvInventario.BorderStyle = BorderStyle.None;
            dgvInventario.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvInventario.Dock = DockStyle.Fill;
            dgvInventario.EnableHeadersVisualStyles = false;
            dgvInventario.Location = new Point(0, 118);
            dgvInventario.MultiSelect = false;
            dgvInventario.Name = "dgvInventario";
            dgvInventario.ReadOnly = true;
            dgvInventario.RowHeadersVisible = false;
            dgvInventario.RowHeadersWidth = 51;
            dgvInventario.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInventario.Size = new Size(980, 482);
            dgvInventario.TabIndex = 0;
            dgvInventario.CellDoubleClick += dgvInventario_CellDoubleClick;
            // 
            // UcInventario
            // 
            BackColor = Color.AliceBlue;
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
        private Panel panelBusqueda;
        private Label lblBuscar;
        private TextBox txtBuscar;
        private Label lblContador;
        private DataGridView dgvInventario;
    }
}
