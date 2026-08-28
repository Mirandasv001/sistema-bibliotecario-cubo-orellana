using System.ComponentModel;

namespace BibliotecaApp
{
    partial class UcAlertas
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
            dgvAlertas = new DataGridView();
            colId = new DataGridViewTextBoxColumn();
            colUsuario = new DataGridViewTextBoxColumn();
            colTelefono = new DataGridViewTextBoxColumn();
            colCorreo = new DataGridViewTextBoxColumn();
            colTitulo = new DataGridViewTextBoxColumn();
            colEntrega = new DataGridViewTextBoxColumn();
            colDiasRetraso = new DataGridViewTextBoxColumn();
            panelEncabezado.SuspendLayout();
            ((ISupportInitialize)dgvAlertas).BeginInit();
            SuspendLayout();
            // 
            // panelEncabezado
            // 
            panelEncabezado.BackColor = EstiloUI.Blanco;
            panelEncabezado.Controls.Add(lblTitulo);
            panelEncabezado.Controls.Add(lblSubtitulo);
            panelEncabezado.Dock = DockStyle.Top;
            panelEncabezado.Location = new Point(0, 0);
            panelEncabezado.Size = new Size(980, 62);
            // 
            // lblTitulo
            // 
            lblTitulo.AutoSize = true;
            lblTitulo.Font = EstiloUI.TituloSeccion();
            lblTitulo.ForeColor = EstiloUI.TextoOscuro;
            lblTitulo.Location = new Point(16, 10);
            lblTitulo.Name = "lblTitulo";
            lblTitulo.Text = "Alertas de Préstamos Vencidos";
            // 
            // lblSubtitulo
            // 
            lblSubtitulo.AutoSize = true;
            lblSubtitulo.Font = EstiloUI.Subtitulo();
            lblSubtitulo.ForeColor = Color.Gray;
            lblSubtitulo.Location = new Point(19, 36);
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Text = "Usuarios morosos con préstamos activos pasados de su fecha esperada";
            // 
            // dgvAlertas
            // 
            dgvAlertas.AllowUserToAddRows = false;
            dgvAlertas.AllowUserToDeleteRows = false;
            dgvAlertas.AllowUserToResizeRows = false;
            dgvAlertas.AutoGenerateColumns = false;
            dgvAlertas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvAlertas.BackgroundColor = EstiloUI.Blanco;
            dgvAlertas.BorderStyle = BorderStyle.None;
            dgvAlertas.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAlertas.Dock = DockStyle.Fill;
            dgvAlertas.EnableHeadersVisualStyles = false;
            dgvAlertas.MultiSelect = false;
            dgvAlertas.ReadOnly = true;
            dgvAlertas.RowHeadersVisible = false;
            dgvAlertas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAlertas.Columns.AddRange(new DataGridViewColumn[]
            {
                colId, colUsuario, colTelefono, colCorreo,
                colTitulo, colEntrega, colDiasRetraso
            });
            dgvAlertas.DataError += dgvAlertas_DataError;
            // 
            // Columnas estrictamente mapeadas al SELECT
            // 
            ConfigurarColumna(colId, "ID", "ID", 40, 5F, visible: false);
            ConfigurarColumna(colUsuario, "Usuario", "Usuario", 180, 18F);
            ConfigurarColumna(colTelefono, "Teléfono", "Teléfono", 110, 10F);
            ConfigurarColumna(colCorreo, "Correo", "Correo", 180, 16F);
            ConfigurarColumna(colTitulo, "Título del Libro", "Título del Libro", 260, 26F);
            ConfigurarColumna(colEntrega, "Entrega Esperada", "Entrega Esperada", 110, 10F);
            ConfigurarColumna(colDiasRetraso, "Días de Retraso", "Días de Retraso", 100, 8F);
            // 
            // UcAlertas
            // 
            BackColor = EstiloUI.FondoClaro;
            Controls.Add(dgvAlertas);
            Controls.Add(panelEncabezado);
            Name = "UcAlertas";
            Size = new Size(980, 600);
            Load += UcAlertas_Load;
            panelEncabezado.ResumeLayout(false);
            panelEncabezado.PerformLayout();
            ((ISupportInitialize)dgvAlertas).EndInit();
            ResumeLayout(false);
        }

        private static void ConfigurarColumna(DataGridViewTextBoxColumn columna,
            string dataPropertyName, string encabezado, int anchoMinimo, float fillWeight, bool visible = true)
        {
            columna.DataPropertyName = dataPropertyName;
            columna.HeaderText = encabezado;
            columna.Name = dataPropertyName;
            columna.MinimumWidth = anchoMinimo;
            columna.FillWeight = fillWeight;
            columna.Visible = visible;
            columna.ReadOnly = true;
            columna.SortMode = DataGridViewColumnSortMode.Automatic;
        }

        #endregion

        private Panel panelEncabezado;
        private Label lblTitulo;
        private Label lblSubtitulo;
        private DataGridView dgvAlertas;
        private DataGridViewTextBoxColumn colId;
        private DataGridViewTextBoxColumn colUsuario;
        private DataGridViewTextBoxColumn colTelefono;
        private DataGridViewTextBoxColumn colCorreo;
        private DataGridViewTextBoxColumn colTitulo;
        private DataGridViewTextBoxColumn colEntrega;
        private DataGridViewTextBoxColumn colDiasRetraso;
    }
}
