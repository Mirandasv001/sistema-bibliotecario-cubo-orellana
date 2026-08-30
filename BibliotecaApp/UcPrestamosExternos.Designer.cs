using System.ComponentModel;


 //  Apartado importante 
namespace BibliotecaApp
{
    partial class UcPrestamosExternos
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
            lblSeccionUsuario = CrearSeccion("DATOS DEL USUARIO");
            lblNombre = EstiloUI.CrearEtiqueta("Nombre del Usuario:");
            txtNombre = new TextBox();
            lblCorreo = EstiloUI.CrearEtiqueta("Correo:");
            txtCorreo = new TextBox();
            lblDui = EstiloUI.CrearEtiqueta("DUI:");
            txtDui = new TextBox();
            lblTelefono = EstiloUI.CrearEtiqueta("Teléfono:");
            txtTelefono = new TextBox();
            lblDireccion = EstiloUI.CrearEtiqueta("Dirección:");
            txtDireccion = new TextBox();
            lblSeccionPrestamo = CrearSeccion("DATOS DEL PRÉSTAMO");
            lblCodigoLibro = EstiloUI.CrearEtiqueta("Código del Ejemplar:");
            txtCodigoLibro = new TextBox();
            lblAvisoCodigo = new Label();
            lblLibro = EstiloUI.CrearEtiqueta("Título del Libro:");
            txtTituloLibro = new TextBox();
            lblFechaPrestamo = EstiloUI.CrearEtiqueta("Fecha de Préstamo:");
            dtpFechaPrestamo = new DateTimePicker();
            lblPersonalPresto = EstiloUI.CrearEtiqueta("Personal que Prestó:");
            txtPersonalPresto = new TextBox();
            lblEstado = EstiloUI.CrearEtiqueta("Estado:");
            txtEstado = new TextBox();
            lblFechaEntrega = EstiloUI.CrearEtiqueta("Fecha de Entrega Esperada:");
            dtpFechaEntrega = new DateTimePicker();
            lblPersonalRecibio = EstiloUI.CrearEtiqueta("Personal que Recibió:");
            txtPersonalRecibio = new TextBox();
            lblSeccionRenovacion = CrearSeccion("DEVOLUCIÓN / RENOVACIÓN (OPCIONAL)");
            lblFechaRenovacion = EstiloUI.CrearEtiqueta("Fecha de Renovación:");
            dtpFechaRenovacion = new DateTimePicker();
            lblPersonalRenovo = EstiloUI.CrearEtiqueta("Personal que Renovó:");
            txtPersonalRenovo = new TextBox();
            pnlDatos = new Panel();
            grpDatos = new GroupBox();
            tlpCampos = new TableLayoutPanel();
            pnlBotonesAccion = new Panel();
            flpBotones = new FlowLayoutPanel();
            btnRegistrar = new Button();
            btnDevolver = new Button();
            btnModificar = new Button();
            pnlContenedorGrid = new Panel();
            dgvPrestamos = new DataGridView();
            splitPrestamos = new SplitContainer();

            SuspendLayout();
            pnlDatos.SuspendLayout();
            grpDatos.SuspendLayout();
            tlpCampos.SuspendLayout();
            panelEncabezado.SuspendLayout();
            pnlBotonesAccion.SuspendLayout();
            flpBotones.SuspendLayout();
            pnlContenedorGrid.SuspendLayout();
            ((ISupportInitialize)dgvPrestamos).BeginInit();
            ((ISupportInitialize)splitPrestamos).BeginInit();
            splitPrestamos.Panel1.SuspendLayout();
            splitPrestamos.Panel2.SuspendLayout();
            //
            // panelEncabezado
            //
            panelEncabezado.BackColor = EstiloUI.FondoPergamino;
            panelEncabezado.Controls.Add(lblTitulo);
            panelEncabezado.Controls.Add(lblSubtitulo);
            panelEncabezado.Dock = DockStyle.Top;
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
            lblTitulo.Text = "Préstamos de Libros para Llevar a Casa";
            // 
            // lblSubtitulo
            // 
            lblSubtitulo.AutoSize = true;
            lblSubtitulo.Font = EstiloUI.Subtitulo();
            lblSubtitulo.ForeColor = Color.Gray;
            lblSubtitulo.Location = new Point(19, 36);
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Text = "Gestión de préstamos externos, renovaciones y devoluciones";
            //
            // pnlDatos
            //
            pnlDatos.AutoSize = true;
            pnlDatos.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlDatos.BackColor = EstiloUI.FondoPergamino;
            pnlDatos.Controls.Add(grpDatos);
            pnlDatos.Dock = DockStyle.Top;
            pnlDatos.Name = "pnlDatos";
            pnlDatos.Padding = new Padding(12, 8, 12, 8);
            // 
            // grpDatos
            // 
            grpDatos.AutoSize = true;
            grpDatos.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            grpDatos.Controls.Add(tlpCampos);
            grpDatos.Dock = DockStyle.Top;
            grpDatos.Font = EstiloUI.Etiqueta();
            grpDatos.Name = "grpDatos";
            grpDatos.Padding = new Padding(12, 4, 12, 8);
            grpDatos.TabStop = false;
            grpDatos.Text = "Registro de Préstamo Externo";
            // 
            // tlpCampos
            // 
            tlpCampos.AutoSize = true;
            tlpCampos.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tlpCampos.ColumnCount = 6;
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            tlpCampos.Dock = DockStyle.Fill;
            tlpCampos.Name = "tlpCampos";
            tlpCampos.RowCount = 11;
            for (int i = 0; i < 11; i++)
                tlpCampos.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            int f = 0;
            tlpCampos.Controls.Add(lblSeccionUsuario, 0, f); tlpCampos.SetColumnSpan(lblSeccionUsuario, 6); f++;
            tlpCampos.Controls.Add(lblNombre, 0, f); tlpCampos.Controls.Add(txtNombre, 1, f);
            tlpCampos.Controls.Add(lblCorreo, 2, f); tlpCampos.Controls.Add(txtCorreo, 3, f);
            tlpCampos.Controls.Add(lblDui, 4, f); tlpCampos.Controls.Add(txtDui, 5, f); f++;
            tlpCampos.Controls.Add(lblTelefono, 0, f); tlpCampos.Controls.Add(txtTelefono, 1, f);
            tlpCampos.Controls.Add(lblDireccion, 2, f); tlpCampos.Controls.Add(txtDireccion, 3, f);
            tlpCampos.SetColumnSpan(txtDireccion, 3); f++;
            tlpCampos.Controls.Add(lblSeccionPrestamo, 0, f); tlpCampos.SetColumnSpan(lblSeccionPrestamo, 6); f++;
            tlpCampos.Controls.Add(lblCodigoLibro, 0, f); tlpCampos.Controls.Add(txtCodigoLibro, 1, f);
            tlpCampos.Controls.Add(lblAvisoCodigo, 2, f); tlpCampos.SetColumnSpan(lblAvisoCodigo, 4); f++;
            tlpCampos.Controls.Add(lblLibro, 0, f); tlpCampos.Controls.Add(txtTituloLibro, 1, f);
            tlpCampos.SetColumnSpan(txtTituloLibro, 5); f++;
            tlpCampos.Controls.Add(lblFechaPrestamo, 0, f); tlpCampos.Controls.Add(dtpFechaPrestamo, 1, f);
            tlpCampos.Controls.Add(lblPersonalPresto, 2, f); tlpCampos.Controls.Add(txtPersonalPresto, 3, f);
            tlpCampos.Controls.Add(lblEstado, 4, f); tlpCampos.Controls.Add(txtEstado, 5, f); f++;
            tlpCampos.Controls.Add(lblFechaEntrega, 0, f); tlpCampos.Controls.Add(dtpFechaEntrega, 1, f); f++;
            tlpCampos.Controls.Add(lblSeccionRenovacion, 0, f); tlpCampos.SetColumnSpan(lblSeccionRenovacion, 6); f++;
            tlpCampos.Controls.Add(lblFechaRenovacion, 0, f); tlpCampos.Controls.Add(dtpFechaRenovacion, 1, f);
            tlpCampos.Controls.Add(lblPersonalRenovo, 2, f); tlpCampos.Controls.Add(txtPersonalRenovo, 3, f); f++;
            tlpCampos.Controls.Add(lblPersonalRecibio, 0, f); tlpCampos.Controls.Add(txtPersonalRecibio, 1, f);

            foreach (Control c in new Control[] { txtNombre, txtCorreo, txtDui, txtTelefono,
                     txtDireccion, txtTituloLibro, dtpFechaPrestamo, txtPersonalPresto,
                     txtEstado, dtpFechaEntrega, txtPersonalRecibio,
                     dtpFechaRenovacion, txtPersonalRenovo, txtCodigoLibro })
            {
                EstiloUI.EstilizarEntrada(c);
                c.Dock = DockStyle.Fill;
                c.Margin = new Padding(3, 0, 15, 6);
            }

            txtCodigoLibro.PlaceholderText = "Código del ejemplar — presione Enter";
            txtCodigoLibro.KeyPress += txtCodigoLibro_KeyPress;
            txtCodigoLibro.ReadOnly = true;
            txtTituloLibro.ReadOnly = true;
            lblAvisoCodigo.AutoSize = true;
            lblAvisoCodigo.Font = EstiloUI.Etiqueta();
            lblAvisoCodigo.Margin = new Padding(3, 8, 3, 2);
            lblAvisoCodigo.Dock = DockStyle.Fill;
            lblAvisoCodigo.TextAlign = ContentAlignment.MiddleLeft;
            lblAvisoCodigo.Text = "";

            txtEstado.ReadOnly = true;
            txtEstado.BackColor = SystemColors.Control;
            txtEstado.TabStop = false;
            txtEstado.Text = "Pendiente";
            dtpFechaRenovacion.ShowCheckBox = true;
            dtpFechaRenovacion.Checked = false;
            dtpFechaRenovacion.ValueChanged += dtpFechaRenovacion_ValueChanged;
            dtpFechaPrestamo.ValueChanged += dtpFechaPrestamo_ValueChanged;
            //
            // pnlBotonesAccion
            //
            pnlBotonesAccion.BackColor = EstiloUI.FondoPergamino;
            pnlBotonesAccion.Controls.Add(flpBotones);
            pnlBotonesAccion.Dock = DockStyle.Top;
            pnlBotonesAccion.Name = "pnlBotonesAccion";
            pnlBotonesAccion.Padding = new Padding(14, 6, 14, 6);
            pnlBotonesAccion.Size = new Size(980, 56);
            // 
            // flpBotones
            // 
            flpBotones.AutoSize = true;
            flpBotones.Dock = DockStyle.Fill;
            flpBotones.FlowDirection = FlowDirection.LeftToRight;
            flpBotones.Name = "flpBotones";
            flpBotones.WrapContents = false;
            flpBotones.Controls.Add(btnRegistrar);
            flpBotones.Controls.Add(btnDevolver);
            flpBotones.Controls.Add(btnModificar);
            // 
            // btnRegistrar
            // 
            btnRegistrar.AutoSize = true;
            btnRegistrar.Margin = new Padding(0, 0, 8, 0);
            btnRegistrar.Name = "btnRegistrar";
            btnRegistrar.Size = new Size(190, 38);
            btnRegistrar.Text = "Registrar Préstamo";
            btnRegistrar.UseVisualStyleBackColor = false;
            EstiloUI.EstilizarBotonPrimario(btnRegistrar);
            btnRegistrar.Click += btnRegistrar_Click;
            // 
            // btnDevolver
            // 
            btnDevolver.AutoSize = true;
            btnDevolver.Margin = new Padding(0, 0, 8, 0);
            btnDevolver.Name = "btnDevolver";
            btnDevolver.Size = new Size(230, 38);
            btnDevolver.Text = "Registrar Devolución (fila)";
            EstiloUI.EstilizarBotonSecundario(btnDevolver);
            btnDevolver.Click += btnDevolver_Click;
            // 
            // btnModificar
            // 
            btnModificar.AutoSize = true;
            btnModificar.Margin = new Padding(0, 0, 0, 0);
            btnModificar.Name = "btnModificar";
            btnModificar.Size = new Size(120, 38);
            btnModificar.Text = "Modificar";
            EstiloUI.EstilizarBotonSecundario(btnModificar);
            btnModificar.Click += btnModificar_Click;
            //
            // pnlContenedorGrid
            //
            pnlContenedorGrid.BackColor = EstiloUI.FondoPergamino;
            pnlContenedorGrid.Controls.Add(dgvPrestamos);
            pnlContenedorGrid.Dock = DockStyle.Fill;
            pnlContenedorGrid.Name = "pnlContenedorGrid";
            // 
            // splitPrestamos
            // 
            splitPrestamos.Dock = DockStyle.Fill;
            splitPrestamos.FixedPanel = FixedPanel.Panel1;
            splitPrestamos.Location = new Point(0, 0);
            splitPrestamos.Name = "splitPrestamos";
            splitPrestamos.Orientation = Orientation.Horizontal;
            splitPrestamos.Size = new Size(980, 650);
            splitPrestamos.SplitterDistance = 380;
            splitPrestamos.SplitterWidth = 6;
            // 
            // splitPrestamos.Panel1
            // 
            splitPrestamos.Panel1.BackColor = EstiloUI.FondoPergamino;
            splitPrestamos.Panel1.AutoScroll = true;
            splitPrestamos.Panel1.Controls.Add(pnlBotonesAccion);
            splitPrestamos.Panel1.Controls.Add(pnlDatos);
            splitPrestamos.Panel1.Controls.Add(panelEncabezado);
            // 
            // splitPrestamos.Panel2
            // 
            splitPrestamos.Panel2.AutoScroll = false;
            splitPrestamos.Panel2.BackColor = EstiloUI.FondoPergamino;
            splitPrestamos.Panel2.Controls.Add(pnlContenedorGrid);
            // 
            // dgvPrestamos
            // 
            dgvPrestamos.AllowUserToAddRows = false;
            dgvPrestamos.AllowUserToDeleteRows = false;
            dgvPrestamos.AllowUserToResizeRows = false;
            dgvPrestamos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPrestamos.BackgroundColor = EstiloUI.FondoPergamino;
            dgvPrestamos.BorderStyle = BorderStyle.None;
            dgvPrestamos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPrestamos.Dock = DockStyle.Fill;
            dgvPrestamos.EnableHeadersVisualStyles = false;
            dgvPrestamos.MultiSelect = false;
            dgvPrestamos.ReadOnly = true;
            dgvPrestamos.RowHeadersVisible = false;
            dgvPrestamos.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPrestamos.ScrollBars = ScrollBars.Both;
            dgvPrestamos.AutoGenerateColumns = false;
            dgvPrestamos.CellFormatting += dgvPrestamos_CellFormatting;
            //
            // Columnas del DataGridView (todas de tipo Texto)
            //
            var colId = new DataGridViewTextBoxColumn { Name = "ID", HeaderText = "ID", DataPropertyName = "ID", Visible = false };
            var colUsuario = new DataGridViewTextBoxColumn { Name = "Usuario", HeaderText = "Usuario", DataPropertyName = "Usuario" };
            var colDui = new DataGridViewTextBoxColumn { Name = "DUI", HeaderText = "DUI", DataPropertyName = "DUI" };
            var colCorreo = new DataGridViewTextBoxColumn { Name = "Correo", HeaderText = "Correo", DataPropertyName = "Correo" };
            var colTelefono = new DataGridViewTextBoxColumn { Name = "Telefono", HeaderText = "Teléfono", DataPropertyName = "Telefono" };
            var colTitulo = new DataGridViewTextBoxColumn { Name = "TituloLibro", HeaderText = "Título del Libro", DataPropertyName = "TituloLibro" };
            var colFechaPrestamo = new DataGridViewTextBoxColumn { Name = "FechaPrestamo", HeaderText = "Fecha Préstamo", DataPropertyName = "FechaPrestamo" };
            var colFechaRenovacion = new DataGridViewTextBoxColumn { Name = "FechaRenovacion", HeaderText = "Fecha Renovación", DataPropertyName = "FechaRenovacion" };
            var colEntregaEsperada = new DataGridViewTextBoxColumn { Name = "Entrega Esperada", HeaderText = "Entrega Esperada", DataPropertyName = "Entrega Esperada" };
            var colPersonalPresto = new DataGridViewTextBoxColumn { Name = "PersonalPresto", HeaderText = "Personal que Prestó", DataPropertyName = "PersonalPresto" };
            var colEstado = new DataGridViewTextBoxColumn { Name = "Estado", HeaderText = "Estado", DataPropertyName = "Estado" };

            dgvPrestamos.Columns.AddRange(new DataGridViewColumn[] {
                colId, colUsuario, colDui, colCorreo, colTelefono, colTitulo,
                colFechaPrestamo, colFechaRenovacion, colEntregaEsperada,
                colPersonalPresto, colEstado
            });
            //
            // UcPrestamosExternos
            // 
            BackColor = EstiloUI.FondoClaro;
            Controls.Add(splitPrestamos);
            Name = "UcPrestamosExternos";
            Size = new Size(980, 650);
            Load += UcPrestamosExternos_Load;

            ((ISupportInitialize)dgvPrestamos).EndInit();
            splitPrestamos.Panel2.ResumeLayout(false);
            splitPrestamos.Panel1.ResumeLayout(false);
            splitPrestamos.Panel1.PerformLayout();
            ((ISupportInitialize)splitPrestamos).EndInit();
            pnlContenedorGrid.ResumeLayout(false);
            flpBotones.ResumeLayout(false);
            flpBotones.PerformLayout();
            pnlBotonesAccion.ResumeLayout(false);
            pnlBotonesAccion.PerformLayout();
            panelEncabezado.ResumeLayout(false);
            panelEncabezado.PerformLayout();
            tlpCampos.ResumeLayout(false);
            tlpCampos.PerformLayout();
            grpDatos.ResumeLayout(false);
            grpDatos.PerformLayout();
            pnlDatos.ResumeLayout(false);
            pnlDatos.PerformLayout();
            ResumeLayout(false);
        }

        private static Label CrearSeccion(string texto)
        {
            return new Label
            {
                Text = texto,
                AutoSize = true,
                Font = new Font(EstiloUI.FuenteBase, 9F, FontStyle.Bold | FontStyle.Underline),
                ForeColor = EstiloUI.Acento,
                Margin = new Padding(3, 10, 3, 4)
            };
        }

        #endregion

        private Panel panelEncabezado;
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Panel pnlDatos;
        private GroupBox grpDatos;
        private TableLayoutPanel tlpCampos;
        private Label lblSeccionUsuario;
        private Label lblNombre;
        private TextBox txtNombre;
        private Label lblCorreo;
        private TextBox txtCorreo;
        private Label lblDui;
        private TextBox txtDui;
        private Label lblTelefono;
        private TextBox txtTelefono;
        private Label lblDireccion;
        private TextBox txtDireccion;
        private Label lblSeccionPrestamo;
        private Label lblCodigoLibro;
        private TextBox txtCodigoLibro;
        private Label lblAvisoCodigo;
        private Label lblLibro;
        private TextBox txtTituloLibro;
        private Label lblFechaPrestamo;
        private DateTimePicker dtpFechaPrestamo;
        private Label lblPersonalPresto;
        private TextBox txtPersonalPresto;
        private Label lblEstado;
        private TextBox txtEstado;
        private Label lblFechaEntrega;
        private DateTimePicker dtpFechaEntrega;
        private Label lblPersonalRecibio;
        private TextBox txtPersonalRecibio;
        private Label lblSeccionRenovacion;
        private Label lblFechaRenovacion;
        private DateTimePicker dtpFechaRenovacion;
        private Label lblPersonalRenovo;
        private TextBox txtPersonalRenovo;
        private Panel pnlBotonesAccion;
        private FlowLayoutPanel flpBotones;
        private Button btnRegistrar;
        private Button btnDevolver;
        private Button btnModificar;
        private Panel pnlContenedorGrid;
        private DataGridView dgvPrestamos;
        private SplitContainer splitPrestamos;
    }
}
