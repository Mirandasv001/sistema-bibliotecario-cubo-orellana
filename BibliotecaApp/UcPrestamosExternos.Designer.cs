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
            splitContainer = new SplitContainer();
            panelEncabezado = new Panel();
            lblTitulo = new Label();
            lblSubtitulo = new Label();
            grpDatos = new GroupBox();
            tlpCampos = new TableLayoutPanel();
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
            cboLibro = new ComboBox();
            lblFechaPrestamo = EstiloUI.CrearEtiqueta("Fecha de Préstamo:");
            dtpFechaPrestamo = new DateTimePicker();
            lblPersonalPresto = EstiloUI.CrearEtiqueta("Personal que Prestó:");
            txtPersonalPresto = new TextBox();
            lblEstado = EstiloUI.CrearEtiqueta("Estado:");
            cboEstado = new ComboBox();
            lblFechaEntrega = EstiloUI.CrearEtiqueta("Fecha de Entrega Esperada:");
            dtpFechaEntrega = new DateTimePicker();
            lblPersonalRecibio = EstiloUI.CrearEtiqueta("Personal que Recibió:");
            txtPersonalRecibio = new TextBox();
            lblSeccionRenovacion = CrearSeccion("DEVOLUCIÓN / RENOVACIÓN (OPCIONAL)");
            lblFechaRenovacion = EstiloUI.CrearEtiqueta("Fecha de Renovación:");
            dtpFechaRenovacion = new DateTimePicker();
            lblPersonalRenovo = EstiloUI.CrearEtiqueta("Personal que Renovó:");
            txtPersonalRenovo = new TextBox();
            panelBotones = new Panel();
            btnRegistrar = new Button();
            btnDevolver = new Button();
            btnModificar = new Button();
            dgvPrestamos = new DataGridView();

            ((ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            SuspendLayout();
            panelEncabezado.SuspendLayout();
            grpDatos.SuspendLayout();
            tlpCampos.SuspendLayout();
            panelBotones.SuspendLayout();
            ((ISupportInitialize)dgvPrestamos).BeginInit();
            // 
            // splitContainer
            // 
            splitContainer.BackColor = EstiloUI.GrisBorde;
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.FixedPanel = FixedPanel.None;
            splitContainer.Location = new Point(0, 0);
            splitContainer.Name = "splitContainer";
            splitContainer.Orientation = Orientation.Horizontal;
            splitContainer.Panel1MinSize = 250;
            splitContainer.SplitterDistance = 420;
            splitContainer.SplitterWidth = 6;
            splitContainer.Size = new Size(980, 650);
            splitContainer.SplitterIncrement = 1;
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.AutoScroll = true;
            splitContainer.Panel1.BackColor = EstiloUI.FondoPergamino;
            splitContainer.Panel1.Controls.Add(panelBotones);
            splitContainer.Panel1.Controls.Add(grpDatos);
            splitContainer.Panel1.Controls.Add(panelEncabezado);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.BackColor = EstiloUI.FondoPergamino;
            splitContainer.Panel2.Controls.Add(dgvPrestamos);
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
            tlpCampos.Controls.Add(lblLibro, 0, f); tlpCampos.Controls.Add(cboLibro, 1, f);
            tlpCampos.SetColumnSpan(cboLibro, 5); f++;
            tlpCampos.Controls.Add(lblFechaPrestamo, 0, f); tlpCampos.Controls.Add(dtpFechaPrestamo, 1, f);
            tlpCampos.Controls.Add(lblPersonalPresto, 2, f); tlpCampos.Controls.Add(txtPersonalPresto, 3, f);
            tlpCampos.Controls.Add(lblEstado, 4, f); tlpCampos.Controls.Add(cboEstado, 5, f); f++;
            tlpCampos.Controls.Add(lblFechaEntrega, 0, f); tlpCampos.Controls.Add(dtpFechaEntrega, 1, f); f++;
            tlpCampos.Controls.Add(lblSeccionRenovacion, 0, f); tlpCampos.SetColumnSpan(lblSeccionRenovacion, 6); f++;
            tlpCampos.Controls.Add(lblFechaRenovacion, 0, f); tlpCampos.Controls.Add(dtpFechaRenovacion, 1, f);
            tlpCampos.Controls.Add(lblPersonalRenovo, 2, f); tlpCampos.Controls.Add(txtPersonalRenovo, 3, f); f++;
            tlpCampos.Controls.Add(lblPersonalRecibio, 0, f); tlpCampos.Controls.Add(txtPersonalRecibio, 1, f);

            foreach (Control c in new Control[] { txtNombre, txtCorreo, txtDui, txtTelefono,
                     txtDireccion, cboLibro, dtpFechaPrestamo, txtPersonalPresto,
                     cboEstado, dtpFechaEntrega, txtPersonalRecibio,
                     dtpFechaRenovacion, txtPersonalRenovo, txtCodigoLibro })
            {
                EstiloUI.EstilizarEntrada(c);
                c.Dock = DockStyle.Fill;
                c.Margin = new Padding(3, 0, 15, 6);
            }

            txtCodigoLibro.PlaceholderText = "Código del ejemplar — presione Enter";
            txtCodigoLibro.KeyPress += txtCodigoLibro_KeyPress;
            lblAvisoCodigo.AutoSize = true;
            lblAvisoCodigo.Font = EstiloUI.Etiqueta();
            lblAvisoCodigo.Margin = new Padding(3, 8, 3, 2);
            lblAvisoCodigo.Dock = DockStyle.Fill;
            lblAvisoCodigo.TextAlign = ContentAlignment.MiddleLeft;
            lblAvisoCodigo.Text = "";

            cboLibro.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cboLibro.AutoCompleteSource = AutoCompleteSource.ListItems;
            cboEstado.DropDownStyle = ComboBoxStyle.DropDownList;
            cboEstado.Items.AddRange(new object[] { "Pendiente", "Entregado", "Renovado" });
            cboEstado.SelectedIndex = 0;
            dtpFechaRenovacion.ShowCheckBox = true;
            dtpFechaRenovacion.Checked = false;
            // 
            // panelBotones
            // 
            panelBotones.Controls.Add(btnRegistrar);
            panelBotones.Controls.Add(btnDevolver);
            panelBotones.Controls.Add(btnModificar);
            panelBotones.Dock = DockStyle.Top;
            panelBotones.Name = "panelBotones";
            panelBotones.Padding = new Padding(14, 6, 14, 6);
            panelBotones.Size = new Size(980, 56);
            // 
            // btnRegistrar
            // 
            btnRegistrar.Location = new Point(196, 8);
            btnRegistrar.Name = "btnRegistrar";
            btnRegistrar.Size = new Size(190, 38);
            btnRegistrar.Text = "Registrar Préstamo";
            btnRegistrar.UseVisualStyleBackColor = false;
            EstiloUI.EstilizarBotonPrimario(btnRegistrar);
            btnRegistrar.Click += btnRegistrar_Click;
            // 
            // btnDevolver
            // 
            btnDevolver.Location = new Point(410, 8);
            btnDevolver.Name = "btnDevolver";
            btnDevolver.Size = new Size(230, 38);
            btnDevolver.Text = "Registrar Devolución (fila)";
            EstiloUI.EstilizarBotonSecundario(btnDevolver);
            btnDevolver.Click += btnDevolver_Click;
            // 
            // btnModificar
            // 
            btnModificar.Location = new Point(664, 8);
            btnModificar.Name = "btnModificar";
            btnModificar.Size = new Size(120, 38);
            btnModificar.Text = "Modificar";
            EstiloUI.EstilizarBotonSecundario(btnModificar);
            btnModificar.Click += btnModificar_Click;
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
            dgvPrestamos.CellFormatting += dgvPrestamos_CellFormatting;
            // 
            // UcPrestamosExternos
            // 
            BackColor = EstiloUI.FondoClaro;
            Controls.Add(splitContainer);
            Name = "UcPrestamosExternos";
            Size = new Size(980, 650);
            Load += UcPrestamosExternos_Load;

            ((ISupportInitialize)dgvPrestamos).EndInit();
            panelBotones.ResumeLayout(false);
            tlpCampos.ResumeLayout(false);
            tlpCampos.PerformLayout();
            grpDatos.ResumeLayout(false);
            grpDatos.PerformLayout();
            panelEncabezado.ResumeLayout(false);
            panelEncabezado.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel1.PerformLayout();
            splitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)splitContainer).EndInit();
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

        private SplitContainer splitContainer;
        private Panel panelEncabezado;
        private Label lblTitulo;
        private Label lblSubtitulo;
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
        private ComboBox cboLibro;
        private Label lblFechaPrestamo;
        private DateTimePicker dtpFechaPrestamo;
        private Label lblPersonalPresto;
        private TextBox txtPersonalPresto;
        private Label lblEstado;
        private ComboBox cboEstado;
        private Label lblFechaEntrega;
        private DateTimePicker dtpFechaEntrega;
        private Label lblPersonalRecibio;
        private TextBox txtPersonalRecibio;
        private Label lblSeccionRenovacion;
        private Label lblFechaRenovacion;
        private DateTimePicker dtpFechaRenovacion;
        private Label lblPersonalRenovo;
        private TextBox txtPersonalRenovo;
        private Panel panelBotones;
        private Button btnRegistrar;
        private Button btnDevolver;
        private Button btnModificar;
        private DataGridView dgvPrestamos;
    }
}
