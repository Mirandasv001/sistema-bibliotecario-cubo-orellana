using System.ComponentModel;

namespace BibliotecaApp
{
    partial class UcControlSala
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
            grpDatos = new GroupBox();
            tlpCampos = new TableLayoutPanel();
            lblFecha = EstiloUI.CrearEtiqueta("Fecha:");
            dtpFecha = new DateTimePicker();
            lblGenero = EstiloUI.CrearEtiqueta("Género:");
            cboGenero = new ComboBox();
            lblNombre = EstiloUI.CrearEtiqueta("Nombre del Usuario:");
            txtNombre = new TextBox();
            lblEdad = EstiloUI.CrearEtiqueta("Edad:");
            numEdad = new NumericUpDown();
            lblPersonal = EstiloUI.CrearEtiqueta("Personal en Turno:");
            txtPersonal = new TextBox();
            lblLibro = EstiloUI.CrearEtiqueta("Título del Libro:");
            cboLibro = new ComboBox();
            panelBotones = new Panel();
            btnRegistrar = new Button();
            btnMarcarDevolucion = new Button();
            btnModificar = new Button();
            btnEliminar = new Button();
            btnLimpiar = new Button();
            dgvRegistros = new DataGridView();
            colId = new DataGridViewTextBoxColumn();
            colFecha = new DataGridViewTextBoxColumn();
            colUsuario = new DataGridViewTextBoxColumn();
            colGenero = new DataGridViewTextBoxColumn();
            colEdad = new DataGridViewTextBoxColumn();
            colTituloLibro = new DataGridViewTextBoxColumn();
            colHoraEntrega = new DataGridViewTextBoxColumn();
            colHoraRecibido = new DataGridViewTextBoxColumn();
            colPersonalTurno = new DataGridViewTextBoxColumn();
            panelEncabezado.SuspendLayout();
            grpDatos.SuspendLayout();
            tlpCampos.SuspendLayout();
            ((ISupportInitialize)numEdad).BeginInit();
            panelBotones.SuspendLayout();
            ((ISupportInitialize)dgvRegistros).BeginInit();
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
            lblTitulo.Text = "Control de Usuarios en Sala (Lectura)";
            // 
            // lblSubtitulo
            // 
            lblSubtitulo.AutoSize = true;
            lblSubtitulo.Font = EstiloUI.Subtitulo();
            lblSubtitulo.ForeColor = Color.Gray;
            lblSubtitulo.Location = new Point(19, 36);
            lblSubtitulo.Name = "lblSubtitulo";
            lblSubtitulo.Text = "Check-in al entrar a leer y check-out al devolver el libro";
            // 
            // grpDatos
            // 
            grpDatos.Controls.Add(tlpCampos);
            grpDatos.Dock = DockStyle.Top;
            grpDatos.Font = EstiloUI.Etiqueta();
            grpDatos.Location = new Point(0, 62);
            grpDatos.Name = "grpDatos";
            grpDatos.Padding = new Padding(12, 4, 12, 8);
            grpDatos.Size = new Size(980, 168);
            grpDatos.TabStop = false;
            grpDatos.Text = "Datos del Usuario";
            // 
            // tlpCampos
            // 
            tlpCampos.AutoSize = true;
            tlpCampos.ColumnCount = 4;
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tlpCampos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpCampos.Controls.Add(lblFecha, 0, 0);
            tlpCampos.Controls.Add(dtpFecha, 1, 0);
            tlpCampos.Controls.Add(lblGenero, 2, 0);
            tlpCampos.Controls.Add(cboGenero, 3, 0);
            tlpCampos.Controls.Add(lblNombre, 0, 1);
            tlpCampos.Controls.Add(txtNombre, 1, 1);
            tlpCampos.Controls.Add(lblEdad, 2, 1);
            tlpCampos.Controls.Add(numEdad, 3, 1);
            tlpCampos.Controls.Add(lblPersonal, 0, 2);
            tlpCampos.Controls.Add(txtPersonal, 1, 2);
            tlpCampos.Controls.Add(lblLibro, 2, 2);
            tlpCampos.Controls.Add(cboLibro, 3, 2);
            tlpCampos.Dock = DockStyle.Fill;
            tlpCampos.Location = new Point(15, 24);
            tlpCampos.Name = "tlpCampos";
            tlpCampos.RowCount = 3;
            for (int i = 0; i < 3; i++)
                tlpCampos.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tlpCampos.Size = new Size(950, 110);
            // 
            // Entradas con estilo uniforme
            // 
            foreach (Control c in new Control[] { dtpFecha, cboGenero, txtNombre, numEdad,
                     txtPersonal, cboLibro })
            {
                EstiloUI.EstilizarEntrada(c);
                c.Dock = DockStyle.Fill;
                c.Margin = new Padding(3, 4, 15, 6);
            }
            // 
            // dtpFecha / cboGenero / numEdad / cboLibro
            // 
            dtpFecha.Format = DateTimePickerFormat.Short;
            cboGenero.DropDownStyle = ComboBoxStyle.DropDownList;
            cboGenero.Items.AddRange(new object[] { "Masculino", "Femenino", "Otro" });
            numEdad.Maximum = 120;
            numEdad.Minimum = 1;
            numEdad.Value = 12;
            cboLibro.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cboLibro.AutoCompleteSource = AutoCompleteSource.ListItems;
            // 
            // panelBotones
            // 
            panelBotones.Controls.Add(btnRegistrar);
            panelBotones.Controls.Add(btnMarcarDevolucion);
            panelBotones.Controls.Add(btnModificar);
            panelBotones.Controls.Add(btnEliminar);
            panelBotones.Controls.Add(btnLimpiar);
            panelBotones.Dock = DockStyle.Top;
            panelBotones.Location = new Point(0, 230);
            panelBotones.Name = "panelBotones";
            panelBotones.Padding = new Padding(14, 6, 14, 6);
            panelBotones.Size = new Size(980, 56);
            // 
            // btnRegistrar (Check-in)
            // 
            btnRegistrar.Location = new Point(14, 8);
            btnRegistrar.Name = "btnRegistrar";
            btnRegistrar.Size = new Size(170, 38);
            btnRegistrar.Text = "Registrar Lectura";
            btnRegistrar.UseVisualStyleBackColor = false;
            EstiloUI.EstilizarBotonPrimario(btnRegistrar);
            btnRegistrar.Click += btnRegistrar_Click;
            // 
            // btnMarcarDevolucion (Check-out)
            // 
            btnMarcarDevolucion.Location = new Point(196, 8);
            btnMarcarDevolucion.Name = "btnMarcarDevolucion";
            btnMarcarDevolucion.Size = new Size(175, 38);
            btnMarcarDevolucion.Text = "Marcar Devolución";
            btnMarcarDevolucion.UseVisualStyleBackColor = false;
            EstiloUI.EstilizarBotonSecundario(btnMarcarDevolucion);
            btnMarcarDevolucion.Click += btnMarcarDevolucion_Click;
            // 
            // btnModificar
            // 
            btnModificar.Location = new Point(383, 8);
            btnModificar.Name = "btnModificar";
            btnModificar.Size = new Size(120, 38);
            btnModificar.Text = "Modificar";
            btnModificar.UseVisualStyleBackColor = false;
            EstiloUI.EstilizarBotonSecundario(btnModificar);
            btnModificar.Click += btnModificar_Click;
            // 
            // btnEliminar
            // 
            btnEliminar.Location = new Point(515, 8);
            btnEliminar.Name = "btnEliminar";
            btnEliminar.Size = new Size(115, 38);
            btnEliminar.Text = "Eliminar";
            btnEliminar.UseVisualStyleBackColor = false;
            btnEliminar.FlatAppearance.BorderColor = Color.FromArgb(200, 90, 90);
            btnEliminar.ForeColor = Color.FromArgb(170, 60, 60);
            EstiloUI.EstilizarBotonSecundario(btnEliminar);
            btnEliminar.Click += btnEliminar_Click;
            // 
            // btnLimpiar
            // 
            btnLimpiar.Location = new Point(642, 8);
            btnLimpiar.Name = "btnLimpiar";
            btnLimpiar.Size = new Size(110, 38);
            btnLimpiar.Text = "Limpiar";
            EstiloUI.EstilizarBotonSecundario(btnLimpiar);
            btnLimpiar.Click += btnLimpiar_Click;
            // 
            // dgvRegistros
            // 
            dgvRegistros.AllowUserToAddRows = false;
            dgvRegistros.AllowUserToDeleteRows = false;
            dgvRegistros.AllowUserToResizeRows = false;
            dgvRegistros.AutoGenerateColumns = false;
            dgvRegistros.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRegistros.BackgroundColor = EstiloUI.Blanco;
            dgvRegistros.BorderStyle = BorderStyle.None;
            dgvRegistros.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvRegistros.Columns.AddRange(new DataGridViewColumn[]
            {
                colId, colFecha, colUsuario, colGenero, colEdad,
                colTituloLibro, colHoraEntrega, colHoraRecibido, colPersonalTurno
            });
            dgvRegistros.Dock = DockStyle.Fill;
            dgvRegistros.EnableHeadersVisualStyles = false;
            dgvRegistros.MultiSelect = false;
            dgvRegistros.ReadOnly = true;
            dgvRegistros.RowHeadersVisible = false;
            dgvRegistros.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRegistros.CellClick += dgvRegistros_CellClick;
            // 
            // Columnas estrictamente de texto mapeadas al SELECT
            // 
            ConfigurarColumna(colId, "ID", "ID", 40, 6F, visible: false);
            ConfigurarColumna(colFecha, "Fecha", "Fecha", 95, 9F);
            ConfigurarColumna(colUsuario, "Usuario", "Usuario", 180, 18F);
            ConfigurarColumna(colGenero, "Género", "Género", 90, 8F);
            ConfigurarColumna(colEdad, "Edad", "Edad", 55, 6F);
            ConfigurarColumna(colTituloLibro, "TituloLibro", "Título del Libro", 260, 27F);
            ConfigurarColumna(colHoraEntrega, "HoraEntrega", "Hora Entrega", 90, 8F);
            ConfigurarColumna(colHoraRecibido, "HoraRecibido", "Hora Recibido", 105, 10F);
            ConfigurarColumna(colPersonalTurno, "PersonalTurno", "Personal", 130, 8F);
            // 
            // UcControlSala
            // 
            BackColor = EstiloUI.FondoClaro;
            Controls.Add(dgvRegistros);
            Controls.Add(panelBotones);
            Controls.Add(grpDatos);
            Controls.Add(panelEncabezado);
            Name = "UcControlSala";
            Size = new Size(980, 600);
            Load += UcControlSala_Load;
            panelEncabezado.ResumeLayout(false);
            panelEncabezado.PerformLayout();
            grpDatos.ResumeLayout(false);
            grpDatos.PerformLayout();
            tlpCampos.ResumeLayout(false);
            tlpCampos.PerformLayout();
            ((ISupportInitialize)numEdad).EndInit();
            panelBotones.ResumeLayout(false);
            ((ISupportInitialize)dgvRegistros).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
        private GroupBox grpDatos;
        private TableLayoutPanel tlpCampos;
        private Label lblFecha;
        private DateTimePicker dtpFecha;
        private Label lblGenero;
        private ComboBox cboGenero;
        private Label lblNombre;
        private TextBox txtNombre;
        private Label lblEdad;
        private NumericUpDown numEdad;
        private Label lblPersonal;
        private TextBox txtPersonal;
        private Label lblLibro;
        private ComboBox cboLibro;
        private Panel panelBotones;
        private Button btnRegistrar;
        private Button btnMarcarDevolucion;
        private Button btnModificar;
        private Button btnEliminar;
        private Button btnLimpiar;
        private DataGridView dgvRegistros;
        private DataGridViewTextBoxColumn colId;
        private DataGridViewTextBoxColumn colFecha;
        private DataGridViewTextBoxColumn colUsuario;
        private DataGridViewTextBoxColumn colGenero;
        private DataGridViewTextBoxColumn colEdad;
        private DataGridViewTextBoxColumn colTituloLibro;
        private DataGridViewTextBoxColumn colHoraEntrega;
        private DataGridViewTextBoxColumn colHoraRecibido;
        private DataGridViewTextBoxColumn colPersonalTurno;
    }
}
