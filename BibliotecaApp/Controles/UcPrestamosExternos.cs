using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    public partial class UcPrestamosExternos : UserControl // HERNCIA
    {
        private int? _prstamoEditandoId = null;
        
        // Variables de paginación
        private int paginaActual = 1;
        private int tamanoPagina = 11; 
        private int totalPaginas = 1;
        
        private Button? btnAnterior;
        private Button? btnSiguiente;
        private Label? lblPagina;
        
        private Button? btnEliminar;
        private Button? btnLimpiar;

        // Espaciador invisible que empuja la paginación al extremo derecho de flpBotones
        private Control? _espaciadorPaginacion;

        private ComboBox cmbGeneroPrestamo = null!;

        public UcPrestamosExternos()
        {
            InitializeComponent();

            for (int i = 0; i < 8; i++)
            {
                tlpCampos.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            lblSubtitulo.Text = "Control de Préstamos y Devoluciones";
            lblSubtitulo.Font = EstiloUI.FuenteSubtitulo;
            Size tamSubtitulo = lblSubtitulo.GetPreferredSize(Size.Empty);
            lblSubtitulo.Location = new Point(16,
                (panelEncabezado.Height - tamSubtitulo.Height) / 2);

            splitPrestamos.Dock = DockStyle.Fill;
            splitPrestamos.Orientation = Orientation.Horizontal;

            splitPrestamos.SplitterDistance = 410;

            splitPrestamos.Panel2.AutoScroll = false;

            pnlDatos.Dock = DockStyle.Top;
            pnlBotonesAccion.Dock = DockStyle.Top;
            pnlContenedorGrid.Dock = DockStyle.Fill;

            dgvPrestamos.Dock = DockStyle.None;
            dgvPrestamos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvPrestamos.Location = new Point(0, 0);
            dgvPrestamos.Size = new Size(
                pnlContenedorGrid.ClientSize.Width,
                pnlContenedorGrid.ClientSize.Height);
            dgvPrestamos.ScrollBars = ScrollBars.Both;

            // Suscribir evento de clic en botones de la grilla
            dgvPrestamos.CellContentClick += DgvPrestamos_CellContentClick;

            // Suscribir evento de selección para habilitar/deshabilitar notificaciones según estado
            dgvPrestamos.SelectionChanged += DgvPrestamos_SelectionChanged;

            // Suscribir evento CellFormatting para coloreo dinámico de la columna Estado
            dgvPrestamos.CellFormatting += DgvPrestamos_CellFormatting_Estado;

            dgvPrestamos.SizeChanged += DgvPrestamos_SizeChanged;

            dgvPrestamos.DefaultCellStyle.SelectionBackColor = EstiloUI.Acento;
            dgvPrestamos.DefaultCellStyle.SelectionForeColor = Color.White;

            CrearBotonesAdministracion();

            CrearBotonesPaginacion();

            CrearCampoGenero();

            // Asegurar que la columna Notificado exista en la BD
            AsegurarColumnaNotificado();

            pnlDatos.SendToBack();
            pnlBotonesAccion.BringToFront();
        }

        private void AsegurarColumnaNotificado()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN Notificado INTEGER DEFAULT 0;";
                comando.ExecuteNonQuery();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 1 || ex.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase))
            {
            }
            catch (Exception ex)
            {
                // Cualquier otra excepción: log y relanzar para no fallar silenciosamente.
                System.Diagnostics.Debug.WriteLine($"AsegurarColumnaNotificado: {ex.Message}");
                throw;
            }
        }

        private void UcPrestamosExternos_Load(object sender, EventArgs e)
        {
            AplicarPlaceholders();

            dtpFechaEntrega.MinDate = dtpFechaPrestamo.Value;

            dtpFechaPrestamo.Value = FechaSegura(dtpFechaPrestamo, DateTime.Today);
            dtpFechaEntrega.Value = FechaSegura(dtpFechaEntrega, DateTime.Today.AddDays(8));

            // Control de visibilidad por rol: ocultar botones destructivos para no administradores
            if (SesionGlobal.HaySesionActiva)
            {
                bool esAdmin = SesionGlobal.EsAdmin;
                if (btnEliminar != null) btnEliminar.Visible = esAdmin;
                if (btnLimpiar != null) btnLimpiar.Visible = esAdmin;
            }
            else
            {
                // Sin sesión activa: ocultar botones de administración
                if (btnEliminar != null) btnEliminar.Visible = false;
                if (btnLimpiar != null) btnLimpiar.Visible = false;
            }

            // La visibilidad de los botones admin cambia el ancho ocupado: reajustar espaciador
            ActualizarEspaciadorPaginacion();

            // Panel de campos sin scrollbar gris: Panel1 mide lo que realmente necesita
            AjustarSplitterAContenido();

            RecalcularTamanoPagina();
            CargarPrestamosActivos();

            if (tlpCampos.Controls["lblGenero"] is Label lblGenero)
            {
                tlpCampos.SetColumnSpan(txtDireccion, 1);

                int filaUsuario = tlpCampos.GetRow(txtDireccion);

                tlpCampos.SetColumn(lblGenero, 4);
                tlpCampos.SetRow(lblGenero, filaUsuario);

                tlpCampos.SetColumn(cmbGeneroPrestamo, 5);
                tlpCampos.SetRow(cmbGeneroPrestamo, filaUsuario);

                cmbGeneroPrestamo.Dock = DockStyle.Fill;
                cmbGeneroPrestamo.Margin = new Padding(3, 0, 15, 4);

                tlpCampos.PerformLayout();
            }
        }

        public void Actualizar()
        {
            if (CalcularTamanoPagina()) paginaActual = 1;
            CargarPrestamosActivos();
        }

        private void AjustarSplitterAContenido()
        {
            splitPrestamos.Panel1.PerformLayout();

            int altoRequerido = panelEncabezado.Height
                              + pnlDatos.Height
                              + pnlBotonesAccion.Height
                              + 8; // holgura anti-redondeo de píxeles

            int minimo = splitPrestamos.Panel1MinSize; // 25 por defecto
            int maximo = splitPrestamos.Height - splitPrestamos.SplitterWidth - splitPrestamos.Panel2MinSize;
            if (maximo < minimo) maximo = minimo; // ventana extremadamente pequeña

            int deseado = Math.Clamp(altoRequerido, minimo, maximo);
            if (splitPrestamos.SplitterDistance != deseado)
                splitPrestamos.SplitterDistance = deseado;
        }

        public void CargarDesdeInventario(string codigo, string titulo)
        {
            txtCodigoLibro.Text = codigo;
            txtTituloLibro.Text = titulo;
            BloquearPorCodigo();
            MostrarAviso($"Ejemplar \"{codigo}\" disponible. Campos de libro bloqueados.", EstiloUI.Acento);
            txtNombre.Focus();
        }

        private void txtCodigoLibro_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter) return;
            e.Handled = true;
            BuscarLibroPorCodigo();
        }

        private void BuscarLibroPorCodigo()
        {
            string codigo = txtCodigoLibro.Text.Trim();
            if (codigo.Length == 0)
            {
                LimpiarEstadoCodigo();
                return;
            }

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    SELECT Titulo, Disponibilidad
                    FROM Libros
                    WHERE Codigo = $codigo
                    LIMIT 1;";
                comando.Parameters.AddWithValue("$codigo", codigo);

                using var lector = comando.ExecuteReader();
                if (lector.Read())
                {
                    string titulo = lector.GetString(0);
                    string disponibilidad = lector.GetString(1);

                    if (string.Equals(disponibilidad, "Disponible", StringComparison.OrdinalIgnoreCase))
                    {
                        txtTituloLibro.Text = titulo;
                        BloquearPorCodigo();
                        MostrarAviso($"Ejemplar \"{codigo}\" disponible. Campos de libro bloqueados.",
                            EstiloUI.Acento);
                    }
                    else
                    {
                        txtCodigoLibro.Clear();
                        txtTituloLibro.Text = string.Empty;
                        DesbloquearPorCodigo();
                        MostrarAviso("El ejemplar ya está prestado.", EstiloUI.AlertaRojo);
                    }
                }
                else
                {
                    txtCodigoLibro.Clear();
                    txtTituloLibro.Text = string.Empty;
                    DesbloquearPorCodigo();
                    MostrarAviso("No existe un ejemplar con ese código.", EstiloUI.AlertaRojo);
                }
            }
            catch (SqliteException ex)
            {
                LimpiarEstadoCodigo();
                MostrarAviso("No se pudo consultar la base de datos.", EstiloUI.AlertaRojo);
                MessageBox.Show("Error de base de datos al buscar el código: " + ex.Message,
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                LimpiarEstadoCodigo();
                MessageBox.Show("Error al buscar el código: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BloquearPorCodigo()
        {
            txtCodigoLibro.ReadOnly = true;
            txtTituloLibro.ReadOnly = true;
        }

        private void DesbloquearPorCodigo()
        {
            txtCodigoLibro.ReadOnly = false;
            txtTituloLibro.ReadOnly = false;
        }

        private void MostrarAviso(string mensaje, Color color)
        {
            lblAvisoCodigo.Text = mensaje;
            lblAvisoCodigo.ForeColor = color;
        }

        private void LimpiarEstadoCodigo()
        {
            DesbloquearPorCodigo();
            txtCodigoLibro.Clear();
            lblAvisoCodigo.Text = "";
        }

        private void AplicarPlaceholders()
        {
            EstiloUI.EstablecerPlaceholder(txtDui, "Ej: 00000000-0");
            EstiloUI.EstablecerPlaceholder(txtCorreo, "ejemplo@correo.com");
            EstiloUI.EstablecerPlaceholder(txtTelefono, "Ej: 2222-2222");
        }

        private void CrearCampoGenero()
        {
            var lblGenero = EstiloUI.CrearEtiqueta("Género:");
            lblGenero.Name = "lblGenero";

            cmbGeneroPrestamo = new ComboBox
            {
                Name = "cmbGeneroPrestamo",
                Dock = DockStyle.Fill
            };
            cmbGeneroPrestamo.Items.AddRange(new object[] { "Masculino", "Femenino" });
            EstiloUI.EstilizarEntrada(cmbGeneroPrestamo); // DropDownList + Flat + márgenes uniformes
            cmbGeneroPrestamo.Margin = new Padding(3, 0, 15, 4); 
            int filaEntrega = tlpCampos.GetRow(dtpFechaEntrega);
            tlpCampos.Controls.Add(lblGenero, 2, filaEntrega);
            tlpCampos.Controls.Add(cmbGeneroPrestamo, 3, filaEntrega);
        }

        private static string? ObtenerCodigoDisponible(SqliteConnection conexion, string titulo)
        {
            using var cmd = conexion.CreateCommand();
            cmd.CommandText = @"
                SELECT Codigo FROM Libros
                WHERE Titulo = $titulo AND Disponibilidad = 'Disponible'
                LIMIT 1;";
            cmd.Parameters.AddWithValue("$titulo", titulo);
            return cmd.ExecuteScalar()?.ToString();
        }

        private static string? ObtenerCodigoPrestado(SqliteConnection conexion, string titulo)
        {
            using var cmd = conexion.CreateCommand();
            cmd.CommandText = @"
                SELECT Codigo FROM Libros
                WHERE Titulo = $titulo AND Disponibilidad = 'Prestado'
                LIMIT 1;";
            cmd.Parameters.AddWithValue("$titulo", titulo);
            return cmd.ExecuteScalar()?.ToString();
        }

        private void CargarPrestamosActivos()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                using var comando = conexion.CreateCommand();

                CalcularTotalPaginas();
                if (paginaActual > totalPaginas) paginaActual = totalPaginas;
                if (paginaActual <= 0) paginaActual = 1;
                int offset = (paginaActual - 1) * tamanoPagina;
                if (offset < 0) offset = 0;

                comando.CommandText = @"
                    SELECT ID,
                           NombreUsuario                        AS Usuario,
                           DUI,
                           Correo,
                           Telefono,
                           CASE WHEN IFNULL(Genero,'') = '' THEN '-'
                                ELSE Genero END                 AS Genero,
                           TituloLibro,
                           strftime('%d/%m/%Y', FechaPrestamo)  AS FechaPrestamo,
                           CASE WHEN IFNULL(FechaRenovacion,'') = '' THEN '-'
                                ELSE strftime('%d/%m/%Y', FechaRenovacion) END AS FechaRenovacion,
                           strftime('%d/%m/%Y', FechaEntrega)   AS [Entrega Esperada],
                           PersonalPresto,
                           CASE WHEN IFNULL(PersonalRenovo,'') = '' THEN '-'
                                ELSE PersonalRenovo END             AS PersonalRenovo,
                           CASE WHEN IFNULL(PersonalDevolvio,'') = '' THEN '-'
                                ELSE PersonalDevolvio END           AS PersonalDevolvio,
                           EstadoLibro                          AS Estado,
                           CorreoBienvenidaEnviado
                    FROM PrestamosExternos
                    WHERE EstadoLibro IN ('Pendiente', 'Renovado', 'Entregado')
                    ORDER BY ID DESC
                    LIMIT @tamanoPagina OFFSET @offset;";
                comando.Parameters.AddWithValue("@tamanoPagina", tamanoPagina);
                comando.Parameters.AddWithValue("@offset", offset);

                var tabla = new System.Data.DataTable();
                using (var lector = comando.ExecuteReader())
                {
                    tabla.Load(lector);
                }

                dgvPrestamos.DataSource = tabla;

                if (lblPagina != null) lblPagina.Text = $"Página {paginaActual} de {totalPaginas}";
                if (btnAnterior != null) btnAnterior.Enabled = (paginaActual > 1);
                if (btnSiguiente != null) btnSiguiente.Enabled = (paginaActual < totalPaginas);

                // El texto de lblPagina es AutoSize y cambia de ancho: reajustar espaciador
                ActualizarEspaciadorPaginacion();

                if (!dgvPrestamos.Columns.Contains("CorreoBienvenidaEnviado"))
                {
                    var colNotificado = new DataGridViewTextBoxColumn
                    {
                        Name = "CorreoBienvenidaEnviado",
                        DataPropertyName = "CorreoBienvenidaEnviado",
                        HeaderText = "CorreoBienvenidaEnviado",
                        Visible = false // Oculta para el usuario
                    };
                    dgvPrestamos.Columns.Add(colNotificado);
                }

                if (!dgvPrestamos.Columns.Contains("PersonalRenovo"))
                {
                    var colPersonalRenovo = new DataGridViewTextBoxColumn
                    {
                        Name = "PersonalRenovo",
                        DataPropertyName = "PersonalRenovo",
                        HeaderText = "Personal Renovó",
                        FillWeight = 80F,
                        MinimumWidth = 95,
                        SortMode = DataGridViewColumnSortMode.Automatic
                    };
                    dgvPrestamos.Columns.Add(colPersonalRenovo);
                }

                if (!dgvPrestamos.Columns.Contains("PersonalDevolvio"))
                {
                    var colPersonalDevolvio = new DataGridViewTextBoxColumn
                    {
                        Name = "PersonalDevolvio",
                        DataPropertyName = "PersonalDevolvio",
                        HeaderText = "Personal Devolvió",
                        FillWeight = 80F,
                        MinimumWidth = 100,
                        SortMode = DataGridViewColumnSortMode.Automatic
                    };
                    dgvPrestamos.Columns.Add(colPersonalDevolvio);
                }

                if (!dgvPrestamos.Columns.Contains("Genero"))
                {
                    var colGenero = new DataGridViewTextBoxColumn
                    {
                        Name = "Genero",
                        DataPropertyName = "Genero",
                        HeaderText = "Género",
                        FillWeight = 70F,
                        MinimumWidth = 85,
                        SortMode = DataGridViewColumnSortMode.Automatic
                    };
                    int index = dgvPrestamos.Columns["Telefono"]?.Index ?? dgvPrestamos.Columns.Count - 1;
                    dgvPrestamos.Columns.Insert(index + 1, colGenero);
                }

                if (!dgvPrestamos.Columns.Contains("btnMensaje"))
                {
                    var btnCol = new DataGridViewButtonColumn
                    {
                        Name = "btnMensaje",
                        HeaderText = "Notificar",
                        Text = "Enviar",
                        UseColumnTextForButtonValue = false, // Permite cambiar texto por celda
                        FlatStyle = FlatStyle.Flat, // Permite cambiar BackColor
                        FillWeight = 8F,
                        MinimumWidth = 80
                    };
                    dgvPrestamos.Columns.Add(btnCol);
                }

                ReordenarColumnasGrid();

                foreach (var nombreColFecha in new[]
                         {
                             "FechaPrestamo", "FechaRenovacion", "Entrega Esperada"
                         })
                {
                    var colFecha = dgvPrestamos.Columns[nombreColFecha];
                    if (colFecha != null)
                        colFecha.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }

                // Valor inicial para cada fila del botón y restaurar estado Notificado
                foreach (DataGridViewRow row in dgvPrestamos.Rows)
                {
                    try
                    {
                        // Valor por defecto
                        row.Cells["btnMensaje"].Value = "Enviar";

                        var notificadoObj = row.Cells["CorreoBienvenidaEnviado"].Value;
                        if (notificadoObj != null && notificadoObj != DBNull.Value)
                        {
                            if (Convert.ToInt32(notificadoObj) == 1)
                            {
                                row.Cells["btnMensaje"].Value = "Enviado";
                                row.Cells["btnMensaje"].Style.BackColor = Color.LightGreen;
                                row.Cells["btnMensaje"].Style.ForeColor = Color.DarkGreen;
                            }
                        }
                    }
                    catch
                    {
                        // Ignorar errores de casteo en fila individual para no romper la carga completa
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los préstamos: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CrearBotonesPaginacion()
        {
            _espaciadorPaginacion = new Label
            {
                Name = "espaciadorPaginacion",
                Text = string.Empty,
                AutoSize = false,
                Size = new Size(0, 1),
                Margin = new Padding(0)
            };
            flpBotones.Controls.Add(_espaciadorPaginacion);

            btnAnterior = new Button
            {
                Name = "btnAnterior",
                Text = "<",
                AutoSize = false,
                Size = new Size(35, 40),
                Margin = new Padding(5, 0, 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Enabled = false
            };
            EstiloUI.EstilizarBotonSecundario(btnAnterior);
            btnAnterior.AutoSize = false;
            btnAnterior.Size = new Size(35, 40);
            btnAnterior.Click += btnAnterior_Click;

            lblPagina = new Label
            {
                Name = "lblPagina",
                Text = "Página 1 de 1",
                AutoSize = true,
                Margin = new Padding(10, 10, 10, 5),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            btnSiguiente = new Button
            {
                Name = "btnSiguiente",
                Text = ">",
                AutoSize = false,
                Size = new Size(35, 40),
                Margin = new Padding(5, 0, 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            EstiloUI.EstilizarBotonSecundario(btnSiguiente);
            btnSiguiente.AutoSize = false;
            btnSiguiente.Size = new Size(35, 40);
            btnSiguiente.Click += btnSiguiente_Click;

            flpBotones.Controls.Add(btnAnterior);
            flpBotones.Controls.Add(lblPagina);
            flpBotones.Controls.Add(btnSiguiente);

            var weakThis = new WeakReference<UcPrestamosExternos>(this);
            EventHandler resizeHandler = null;
            resizeHandler = (_, _) =>
            {
                if (weakThis.TryGetTarget(out var target))
                    target.ActualizarEspaciadorPaginacion();
                else
                    flpBotones.Resize -= resizeHandler; 
            };
            flpBotones.Resize += resizeHandler;
            ActualizarEspaciadorPaginacion();
        }

        private void ActualizarEspaciadorPaginacion()
        {
            if (_espaciadorPaginacion == null || !flpBotones.IsHandleCreated) return;

            int ocupado = 0;
            foreach (Control c in flpBotones.Controls)
            {
                if (c == _espaciadorPaginacion || !c.Visible) continue;
                ocupado += c.Width + c.Margin.Horizontal;
            }

            const int margenDerecho = 35;

            int disponible = flpBotones.ClientSize.Width - ocupado
                             - _espaciadorPaginacion.Margin.Horizontal - margenDerecho;

            int nuevoAncho = disponible < 0 ? 0 : disponible;

            if (_espaciadorPaginacion.Width != nuevoAncho)
                _espaciadorPaginacion.Width = nuevoAncho;
        }

        private void btnAnterior_Click(object? sender, EventArgs e)
        {
            if (paginaActual > 1)
            {
                paginaActual--;
                CargarPrestamosActivos();
            }
        }

        private void btnSiguiente_Click(object? sender, EventArgs e)
        {
            if (paginaActual < totalPaginas)
            {
                paginaActual++;
                CargarPrestamosActivos();
            }
        }

        private void CalcularTotalPaginas()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = "SELECT COUNT(*) FROM PrestamosExternos WHERE EstadoLibro IN ('Pendiente', 'Renovado', 'Entregado');";

                var resultado = comando.ExecuteScalar();
                int totalRegistros = (resultado != null && resultado != DBNull.Value) ? Convert.ToInt32(resultado) : 0;

                if (tamanoPagina <= 0) tamanoPagina = 25; 

                totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina);
                if (totalPaginas <= 0) totalPaginas = 1;
            }
            catch
            {
                totalPaginas = 1;
            }
        }

        private void RecalcularTamanoPagina()
        {
            int altoFila = dgvPrestamos.RowTemplate.Height > 0
                ? dgvPrestamos.RowTemplate.Height
                : 20; 

            int disponibles = dgvPrestamos.ClientSize.Height - dgvPrestamos.ColumnHeadersHeight;
            if (disponibles <= 0) return; 

            int filasPosibles = disponibles / altoFila;
            tamanoPagina = Math.Max(1, filasPosibles); // nunca menos de 1 fila por página
        }

        private bool CalcularTamanoPagina()
        {
            int anterior = tamanoPagina;
            RecalcularTamanoPagina();
            return tamanoPagina != anterior;
        }

        private void DgvPrestamos_SizeChanged(object? sender, EventArgs e)
        {
            if (IsDisposed || Disposing) return;

            int anterior = tamanoPagina;
            RecalcularTamanoPagina();
            if (tamanoPagina == anterior) return; // sin cambio de capacidad: nada que recargar

            CargarPrestamosActivos(); // recalcula totalPaginas y acomoda paginaActual
        }

        private void ReordenarColumnasGrid()
        {
            string[] ordenDeseado =
            {
                "ID", "Usuario", "DUI", "Correo", "Telefono", "Genero", "TituloLibro",
                "FechaPrestamo", "PersonalPresto", "FechaRenovacion", "PersonalRenovo",
                "Entrega Esperada", "PersonalDevolvio", "Estado", "CorreoBienvenidaEnviado", "btnMensaje"
            };

            for (int i = 0; i < ordenDeseado.Length; i++)
            {
                var columna = dgvPrestamos.Columns[ordenDeseado[i]];
                if (columna != null)
                    columna.DisplayIndex = i;
            }
        }

        private void CrearBotonesAdministracion()
        {
            btnEliminar = new Button
            {
                Name = "btnEliminar",
                Text = "Eliminar préstamo",
                AutoSize = true,
                Margin = new Padding(5, 0, 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Visible = false 
            };
            EstiloUI.EstilizarBotonSecundario(btnEliminar);
            btnEliminar.Click += btnEliminar_Click;

            btnLimpiar = new Button
            {
                Name = "btnLimpiar",
                Text = "Eliminar página",
                AutoSize = true,
                Margin = new Padding(5, 0, 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Visible = false 
            };
            EstiloUI.EstilizarBotonSecundario(btnLimpiar);
            btnLimpiar.Click += btnLimpiar_Click;

            flpBotones.Controls.Add(btnEliminar);
            flpBotones.Controls.Add(btnLimpiar);
        }

        private void btnEliminar_Click(object? sender, EventArgs e)
        {
            if (dgvPrestamos.CurrentRow == null || dgvPrestamos.CurrentRow.Cells["ID"].Value == null)
            {
                MessageBox.Show("Seleccione un préstamo de la lista para eliminar.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(dgvPrestamos.CurrentRow.Cells["ID"].Value);
            string estado = dgvPrestamos.CurrentRow.Cells["Estado"].Value?.ToString()?.Trim() ?? "";
            string tituloFila = dgvPrestamos.CurrentRow.Cells["TituloLibro"].Value?.ToString() ?? "";

            if (!string.Equals(estado, "Entregado", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Solo se pueden eliminar préstamos con estado 'Entregado'.\n" +
                    "Registre primero la devolución del libro.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var frmAuth = new FormAutenticacion("AdminCubo", "Admin123$"))
            {
                if (frmAuth.ShowDialog(this) != DialogResult.OK) return;
            }

            var resultado = MessageBox.Show(
                "¿Está seguro de que desea ELIMINAR este préstamo definitivamente?\n\nEsta acción no se puede deshacer.",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (resultado != DialogResult.Yes) return;

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                using var transaccion = conexion.BeginTransaction();
                try
                {
                    string? codigoLibro;
                    using (var obtenerCodigo = conexion.CreateCommand())
                    {
                        obtenerCodigo.Transaction = transaccion;
                        obtenerCodigo.CommandText =
                            "SELECT CodigoLibro FROM PrestamosExternos WHERE ID = $id;";
                        obtenerCodigo.Parameters.AddWithValue("$id", id);
                        codigoLibro = obtenerCodigo.ExecuteScalar()?.ToString();
                    }

                    if (string.IsNullOrEmpty(codigoLibro))
                    {
                        using var obtenerPorTitulo = conexion.CreateCommand();
                        obtenerPorTitulo.Transaction = transaccion;
                        obtenerPorTitulo.CommandText = @"
                            SELECT Codigo FROM Libros
                            WHERE Titulo = $titulo AND Disponibilidad = 'Prestado'
                            LIMIT 1;";
                        obtenerPorTitulo.Parameters.AddWithValue("$titulo", tituloFila);
                        codigoLibro = obtenerPorTitulo.ExecuteScalar()?.ToString();
                    }

                    using (var cmd = conexion.CreateCommand())
                    {
                        cmd.Transaction = transaccion;
                        cmd.CommandText = "DELETE FROM PrestamosExternos WHERE ID = @id AND EstadoLibro = 'Entregado';";
                        cmd.Parameters.AddWithValue("@id", id);

                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            transaccion.Rollback();
                            MessageBox.Show("No se pudo eliminar el préstamo (ya no existe o no está en estado 'Entregado').",
                                "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    if (!string.IsNullOrEmpty(codigoLibro))
                    {
                        using var liberar = conexion.CreateCommand();
                        liberar.Transaction = transaccion;
                        liberar.CommandText = @"
                            UPDATE Libros SET Disponibilidad = 'Disponible'
                            WHERE Codigo = $codigo
                              AND NOT EXISTS (
                                  SELECT 1 FROM PrestamosExternos
                                  WHERE CodigoLibro = $codigo
                                    AND ID <> $id
                                    AND EstadoLibro IN ('Pendiente', 'Renovado'));";
                        liberar.Parameters.AddWithValue("$codigo", codigoLibro);
                        liberar.Parameters.AddWithValue("$id", id);
                        liberar.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw;
                }

                MessageBox.Show("Préstamo eliminado correctamente.", "Biblioteca CUBO",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                CargarPrestamosActivos();
                NotificarCambioPrestamos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el préstamo: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLimpiar_Click(object? sender, EventArgs e)
        {
            using (var frmAuth = new FormAutenticacion("AdminCubo", "Admin123$"))
            {
                if (frmAuth.ShowDialog(this) != DialogResult.OK) return;
            }

            EliminarPaginaActual();
        }

        private void EliminarPaginaActual()
        {
            var idsPagina = new List<long>();
            foreach (DataGridViewRow fila in dgvPrestamos.Rows)
            {
                if (fila.IsNewRow) continue;

                object? valorId = fila.Cells["ID"].Value;
                if (valorId != null && valorId != DBNull.Value
                    && long.TryParse(valorId.ToString(), out long id))
                {
                    idsPagina.Add(id);
                }
            }

            if (idsPagina.Count == 0)
            {
                MessageBox.Show("La página actual no tiene registros para eliminar.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirmar = MessageBox.Show(
                $"Se eliminarán {idsPagina.Count} registro(s) de la página actual.\n" +
                "Esta acción no se puede deshacer. ¿Continuar?",
                "Biblioteca CUBO", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var transaccion = conexion.BeginTransaction();
                int eliminados;
                try
                {
                    // DELETE parametrizado reutilizando un único parámetro
                    using var comando = conexion.CreateCommand();
                    comando.Transaction = transaccion;
                    comando.CommandText = "DELETE FROM PrestamosExternos WHERE ID = $id;";

                    var parametro = comando.CreateParameter();
                    parametro.ParameterName = "$id";
                    comando.Parameters.Add(parametro);

                    eliminados = 0;
                    foreach (long id in idsPagina)
                    {
                        parametro.Value = id;
                        eliminados += comando.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw; 
                }

                MessageBox.Show($"{eliminados} registro(s) eliminado(s) de la base de datos.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar la página: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            CargarPrestamosActivos();
        }

        private void DgvPrestamos_SelectionChanged(object? sender, EventArgs e)
        {
            try
            {
                bool entregado = false;

                if (dgvPrestamos.CurrentRow != null &&
                    dgvPrestamos.CurrentRow.Cells["ID"].Value != null)
                {
                    string estado = dgvPrestamos.CurrentRow.Cells["Estado"].Value?.ToString()?.Trim() ?? "";
                    entregado = string.Equals(estado, "Entregado", StringComparison.OrdinalIgnoreCase);
                }

                // Candados de UI: devolución / renovación / notificación inactivos si ya fue entregado
                btnDevolver.Enabled = !entregado;
                btnRenovarFila.Enabled = !entregado;

                foreach (DataGridViewRow fila in dgvPrestamos.Rows)
                {
                    if (fila.Cells.Count == 0) continue;

                    string est = fila.Cells["Estado"].Value?.ToString()?.Trim() ?? "";
                    bool filaEntregada = string.Equals(est, "Entregado", StringComparison.OrdinalIgnoreCase);

                    var celdaBtn = fila.Cells["btnMensaje"];
                    celdaBtn.ReadOnly = filaEntregada; 

                    if (filaEntregada)
                    {
                        celdaBtn.Value = "Entregado";
                        celdaBtn.Style.BackColor = Color.FromArgb(204, 255, 204);
                        celdaBtn.Style.ForeColor = Color.DarkGreen;
                    }
                    else if (celdaBtn.Value?.ToString() == "Entregado")
                    {
                        celdaBtn.Value = "Enviar";
                        celdaBtn.Style.BackColor = Color.Empty;
                        celdaBtn.Style.ForeColor = Color.Empty;
                        celdaBtn.Style.SelectionBackColor = Color.Empty;
                        celdaBtn.Style.SelectionForeColor = Color.Empty;
                    }
                }
            }
            catch
            {
                // Evita excepciones durante la reordenación interna del grid
            }
        }

        private void dgvPrestamos_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            e.CellStyle.SelectionBackColor = dgvPrestamos.DefaultCellStyle.SelectionBackColor;
            e.CellStyle.SelectionForeColor = Color.White;

            var col = dgvPrestamos.Columns[e.ColumnIndex];

            // La columna Estado la pinta DgvPrestamos_CellFormatting_Estado
            if (col.Name == "Estado") return;

            if (col.Name == "btnMensaje") return;

            e.CellStyle.BackColor = Color.White;
        }

        private void DgvPrestamos_CellFormatting_Estado(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            e.CellStyle.SelectionBackColor = dgvPrestamos.DefaultCellStyle.SelectionBackColor;
            e.CellStyle.SelectionForeColor = Color.White;

            var colEstado = dgvPrestamos.Columns["Estado"];
            if (colEstado == null || e.ColumnIndex != colEstado.Index) return;

            string estado = e.Value?.ToString()?.Trim() ?? "";

            Color colorFondo;
            switch (estado)
            {
                case "Pendiente":
                    colorFondo = Color.FromArgb(255, 204, 204); // Rojo suave
                    break;
                case "Renovado":
                    colorFondo = Color.FromArgb(204, 235, 255); // Celeste suave
                    break;
                case "Entregado":
                    colorFondo = Color.FromArgb(204, 255, 204); // Verde suave
                    break;
                default:
                    return; // Sin color para estados no reconocidos
            }

            e.CellStyle.BackColor = colorFondo;
        }

        private void DgvPrestamos_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            // Validar que sea clic en la columna de botones y fila válida
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvPrestamos.Columns[e.ColumnIndex].Name != "btnMensaje") return;

            var fila = dgvPrestamos.Rows[e.RowIndex];

            // Extraer datos de la fila
            string nombreUsuario = fila.Cells["Usuario"].Value?.ToString()?.Trim() ?? "";
            string correo = fila.Cells["Correo"].Value?.ToString()?.Trim() ?? "";
            string tituloLibro = fila.Cells["TituloLibro"].Value?.ToString()?.Trim() ?? "";
            string fechaEntrega = fila.Cells["Entrega Esperada"].Value?.ToString()?.Trim() ?? "";

            // Validar que hay correo
            if (string.IsNullOrWhiteSpace(correo) || !correo.Contains("@"))
            {
                MessageBox.Show("El usuario no tiene un correo electrónico válido registrado.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string titulo = CultureInfo.CurrentCulture.TextInfo
                    .ToTitleCase(tituloLibro.ToLower());

                string subject = "Confirmacion de prestamo - Biblioteca CUBO";
                string body = $"Hola {nombreUsuario}, muchas gracias por elegir la Biblioteca CUBO.\r\n\r\n" +
                              $"Te confirmamos el prestamo del libro \"{titulo}\".\r\n" +
                              $"La fecha esperada de devolucion o renovacion es el {fechaEntrega}.\r\n\r\n" +
                              "Esperamos que disfrutes tu lectura.";

                string mailtoUri = $"mailto:{correo}" +
                                   $"?subject={Uri.EscapeDataString(subject)}" +
                                   $"&body={Uri.EscapeDataString(body)}";

                var psi = new ProcessStartInfo(mailtoUri) { UseShellExecute = true };
                Process.Start(psi);

                var celda = dgvPrestamos.Rows[e.RowIndex].Cells[e.ColumnIndex];
                celda.Value = "Enviado";
                celda.Style.BackColor = Color.LightGreen;
                celda.Style.ForeColor = Color.DarkGreen;
                int idPrestamo = Convert.ToInt32(fila.Cells["ID"].Value);
                using (var conexion = ConexionDB.ObtenerConexion())
                using (var updateCmd = conexion.CreateCommand())
                {
                    updateCmd.CommandText = "UPDATE PrestamosExternos SET CorreoBienvenidaEnviado = 1 WHERE ID = @id;";
                    updateCmd.Parameters.AddWithValue("@id", idPrestamo);
                    updateCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el cliente de correo:\n{ex.Message}",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            // 1. VALIDACIÓN ESTRICTA SOLO PARA ALTA DE PRÉSTAMO
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtDui.Text) ||
                string.IsNullOrWhiteSpace(txtCodigoLibro.Text) ||
                string.IsNullOrWhiteSpace(txtPersonalPresto.Text))
            {
                MessageBox.Show("Por favor, complete todos los datos requeridos para registrar el préstamo (Nombre, DUI, Código del Libro y Personal que Prestó).", "Validación de Préstamo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_prstamoEditandoId.HasValue)
                ActualizarPrestamo(_prstamoEditandoId.Value);
            else
                RegistrarPrestamo();
        }

        private void RegistrarPrestamo()
        {
            string titulo = txtTituloLibro.Text.Trim();

            if (string.IsNullOrWhiteSpace(titulo))
            {
                MessageBox.Show("Seleccione o escriba el título del libro.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                // Obtener el primer Codigo disponible de ese título.
                string? codigo = string.IsNullOrWhiteSpace(txtCodigoLibro.Text)
                    ? null
                    : txtCodigoLibro.Text.Trim();
                try
                {
                    codigo ??= ObtenerCodigoDisponible(conexion, titulo);
                }
                catch (SqliteException ex)
                {
                    MessageBox.Show("Error de base de datos al consultar el ejemplar: " + ex.Message,
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(codigo))
                {
                    MessageBox.Show(
                        $"No hay ejemplares disponibles del título \"{titulo}\".",
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using var transaccion = conexion.BeginTransaction();
                try
                {
                    using (var insertar = conexion.CreateCommand())
                    {
                        insertar.Transaction = transaccion;
                        insertar.CommandText = @"
                            INSERT INTO PrestamosExternos
                                (NombreUsuario, Correo, DUI, Telefono, Genero, Direccion, TituloLibro,
                                 FechaPrestamo, PersonalPresto, FechaEntrega, EstadoLibro, CodigoLibro)
                            VALUES
                                ($nombre, $correo, $dui, $telefono, $genero, $direccion, $titulo,
                                 $fechaPrestamo, $personalPresto, $fechaEntrega, $estado, $codigo);";

                        insertar.Parameters.AddWithValue("$nombre", txtNombre.Text.Trim());
                        insertar.Parameters.AddWithValue("$correo", txtCorreo.Text.Trim());
                        insertar.Parameters.AddWithValue("$dui", txtDui.Text.Trim());
                        insertar.Parameters.AddWithValue("$telefono", txtTelefono.Text.Trim());
                        insertar.Parameters.AddWithValue("$genero", cmbGeneroPrestamo.Text.Trim());
                        insertar.Parameters.AddWithValue("$direccion", txtDireccion.Text.Trim());
                        insertar.Parameters.AddWithValue("$titulo", titulo);
                        insertar.Parameters.AddWithValue("$fechaPrestamo", dtpFechaPrestamo.Value.ToString("yyyy-MM-dd"));
                        insertar.Parameters.AddWithValue("$personalPresto", txtPersonalPresto.Text.Trim());
                        insertar.Parameters.AddWithValue("$fechaEntrega", dtpFechaEntrega.Value.ToString("yyyy-MM-dd"));
                        insertar.Parameters.AddWithValue("$estado", "Pendiente");
                        insertar.Parameters.AddWithValue("$codigo", codigo);
                        insertar.ExecuteNonQuery();
                    }

                    // Marcar SOLO el ejemplar único como Prestado.
                    using (var marcar = conexion.CreateCommand())
                    {
                        marcar.Transaction = transaccion;
                        marcar.CommandText = @"
                            UPDATE Libros SET Disponibilidad = 'Prestado'
                            WHERE Codigo = $codigo
                              AND Titulo = $titulo
                              AND Disponibilidad = 'Disponible';";
                        marcar.Parameters.AddWithValue("$codigo", codigo);
                        marcar.Parameters.AddWithValue("$titulo", titulo);
                        if (marcar.ExecuteNonQuery() != 1)
                            throw new InvalidOperationException("El ejemplar ya no está disponible o no coincide con el título seleccionado.");
                    }

                    transaccion.Commit();
                }
                catch (SqliteException ex)
                {
                    transaccion.Rollback();
                    MessageBox.Show("Error de base de datos al registrar el préstamo: " + ex.Message,
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    MessageBox.Show("Error inesperado al registrar el préstamo: " + ex.Message,
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Préstamo registrado correctamente.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarParaNuevo();
                CargarPrestamosActivos();
                NotificarCambioPrestamos();
            }
            catch (SqliteException ex)
            {
                MessageBox.Show("Error de conexión con la base de datos: " + ex.Message,
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar el préstamo: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarPrestamo(int id)
        {
            string tituloNuevo = txtTituloLibro.Text.Trim();

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                string tituloViejo = "";
                using (var cmd = conexion.CreateCommand())
                {
                    cmd.CommandText = "SELECT TituloLibro FROM PrestamosExternos WHERE ID = $id;";
                    cmd.Parameters.AddWithValue("$id", id);
                    object? r = cmd.ExecuteScalar();
                    if (r != null) tituloViejo = r.ToString() ?? "";
                }

                if (string.Equals(txtEstado.Text, "Renovado", StringComparison.OrdinalIgnoreCase))
                {
                    using var transaccion = conexion.BeginTransaction();
                    try
                    {
                        RenovarPrestamo(conexion, transaccion, id, DateTime.Now,
                            string.IsNullOrWhiteSpace(SesionGlobal.NombreUsuario)
                                ? "Sistema"
                                : SesionGlobal.NombreUsuario);
                        transaccion.Commit();
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
                else if (string.Equals(tituloViejo, tituloNuevo, StringComparison.OrdinalIgnoreCase))
                {
                    // Mismo título: solo actualizar datos del préstamo, sin tocar inventario.
                    using var transaccion = conexion.BeginTransaction();
                    try
                    {
                        ActualizarDatosPrestamo(conexion, transaccion, id);
                        transaccion.Commit();
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
                else
                {
                    using var transaccion = conexion.BeginTransaction();
                    try
                    {
                        string? codigoViejo;
                        using (var cmdViejo = conexion.CreateCommand())
                        {
                            cmdViejo.Transaction = transaccion;
                            cmdViejo.CommandText = "SELECT CodigoLibro FROM PrestamosExternos WHERE ID = $id;";
                            cmdViejo.Parameters.AddWithValue("$id", id);
                            codigoViejo = cmdViejo.ExecuteScalar()?.ToString();
                        }

                        if (string.IsNullOrEmpty(codigoViejo))
                            codigoViejo = ObtenerCodigoPrestado(conexion, tituloViejo);

                        string? codigoNuevo = ObtenerCodigoDisponible(conexion, tituloNuevo);

                        if (codigoNuevo == null)
                        {
                            transaccion.Rollback();
                            MessageBox.Show(
                                $"No hay ejemplares disponibles del título \"{tituloNuevo}\".",
                                "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        // Actualizar datos del préstamo
                        ActualizarDatosPrestamo(conexion, transaccion, id);

                        // Liberar ejemplar viejo SOLO si ningún otro préstamo activo lo retiene.
                        if (!string.IsNullOrEmpty(codigoViejo))
                        {
                            using var liberar = conexion.CreateCommand();
                            liberar.Transaction = transaccion;
                            liberar.CommandText = @"
                                UPDATE Libros SET Disponibilidad = 'Disponible'
                                WHERE Codigo = $codigo
                                  AND NOT EXISTS (
                                      SELECT 1 FROM PrestamosExternos
                                      WHERE CodigoLibro = $codigo
                                        AND ID <> $id
                                        AND EstadoLibro IN ('Pendiente', 'Renovado'));";
                            liberar.Parameters.AddWithValue("$codigo", codigoViejo);
                            liberar.Parameters.AddWithValue("$id", id);
                            liberar.ExecuteNonQuery();
                        }

                        // Marcar ejemplar nuevo como Prestado, verificando que siga disponible.
                        using var marcar = conexion.CreateCommand();
                        marcar.Transaction = transaccion;
                        marcar.CommandText = @"
                            UPDATE Libros SET Disponibilidad = 'Prestado'
                            WHERE Codigo = $codigo
                              AND Titulo = $titulo
                              AND Disponibilidad = 'Disponible';";
                        marcar.Parameters.AddWithValue("$codigo", codigoNuevo);
                        marcar.Parameters.AddWithValue("$titulo", tituloNuevo);
                        if (marcar.ExecuteNonQuery() != 1)
                        {
                            transaccion.Rollback();
                            throw new InvalidOperationException(
                                "El ejemplar nuevo ya no está disponible (posible carrera).");
                        }

                        transaccion.Commit();
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }

                MessageBox.Show("Préstamo actualizado correctamente.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarParaNuevo();
                CargarPrestamosActivos();
                NotificarCambioPrestamos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar el préstamo: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ActualizarDatosPrestamo(SqliteConnection conexion, SqliteTransaction transaccion, int id)
        {
            using var cmd = conexion.CreateCommand();
            cmd.Transaction = transaccion;
            cmd.CommandText = @"
                UPDATE PrestamosExternos SET
                    NombreUsuario   = $nombre,
                    Correo          = $correo,
                    DUI             = $dui,
                    Telefono        = $telefono,
                    Genero          = $genero,
                    Direccion       = $direccion,
                    TituloLibro     = $titulo,
                    FechaPrestamo   = $fechaPrestamo,
                    PersonalPresto  = $personalPresto,
                    FechaEntrega    = $fechaEntrega,
                    EstadoLibro     = $estado
                WHERE ID = $id;";

            cmd.Parameters.AddWithValue("$nombre", txtNombre.Text.Trim());
            cmd.Parameters.AddWithValue("$correo", txtCorreo.Text.Trim());
            cmd.Parameters.AddWithValue("$dui", txtDui.Text.Trim());
            cmd.Parameters.AddWithValue("$telefono", txtTelefono.Text.Trim());
            cmd.Parameters.AddWithValue("$genero", cmbGeneroPrestamo.Text.Trim());
            cmd.Parameters.AddWithValue("$direccion", txtDireccion.Text.Trim());
            cmd.Parameters.AddWithValue("$titulo", txtTituloLibro.Text.Trim());
            cmd.Parameters.AddWithValue("$fechaPrestamo", dtpFechaPrestamo.Value.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$personalPresto", txtPersonalPresto.Text.Trim());
            cmd.Parameters.AddWithValue("$fechaEntrega", dtpFechaEntrega.Value.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$estado",
                string.IsNullOrWhiteSpace(txtEstado.Text) ? "Pendiente" : txtEstado.Text.Trim());
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        private void RenovarPrestamo(SqliteConnection conexion, SqliteTransaction transaccion, int id, DateTime fechaRenovacion, string personalRenovo)
        {
            using var cmd = conexion.CreateCommand();
            cmd.Transaction = transaccion;
            cmd.CommandText = @"
                UPDATE PrestamosExternos SET
                    FechaRenovacion   = $fechaRenovacion,
                    PersonalRenovo    = $personalRenovo,
                    EstadoLibro       = 'Renovado',
                    FechaEntrega      = $nuevaFechaEntrega
                WHERE ID = $id;";
            cmd.Parameters.AddWithValue("$fechaRenovacion", fechaRenovacion.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$personalRenovo", personalRenovo);
            cmd.Parameters.AddWithValue("$nuevaFechaEntrega", dtpFechaEntrega.Value.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        private void btnDevolver_Click(object sender, EventArgs e)
        {
            if (dgvPrestamos.CurrentRow == null || dgvPrestamos.CurrentRow.Cells["ID"].Value == null)
            {
                MessageBox.Show("Seleccione un préstamo de la lista.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Popup para obtener nombre del personal
            string personal = ObtenerNombrePersonal("recibió");
            if (string.IsNullOrWhiteSpace(personal))
                return;

            int id = Convert.ToInt32(dgvPrestamos.CurrentRow.Cells["ID"].Value);
            string? titulo = dgvPrestamos.CurrentRow.Cells["TituloLibro"].Value?.ToString();
            DateTime fechaDevolucion = DateTime.Now;

            if (MessageBox.Show("¿Confirmar la devolución del préstamo seleccionado?",
                    "Registrar Devolución", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                // Localizar el ejemplar físico por título + estado Prestado.
                string? codigoLibro;
                using (var obtenerCodigo = conexion.CreateCommand())
                {
                    obtenerCodigo.CommandText = "SELECT CodigoLibro FROM PrestamosExternos WHERE ID = $id;";
                    obtenerCodigo.Parameters.AddWithValue("$id", id);
                    codigoLibro = obtenerCodigo.ExecuteScalar()?.ToString();
                }

                // Compatibilidad con préstamos registrados antes de CodigoLibro.
                codigoLibro ??= ObtenerCodigoPrestado(conexion, titulo ?? "");

                using var transaccion = conexion.BeginTransaction();
                try
                {
                    using (var actualizar = conexion.CreateCommand())
                    {
                        actualizar.Transaction = transaccion;
                        actualizar.CommandText = @"
                            UPDATE PrestamosExternos
                            SET EstadoLibro    = 'Entregado',
                                FechaDevolucion = $hoy,
                                PersonalRecibio = $personal,
                                PersonalDevolvio = $personal
                            WHERE ID = $id
                              AND EstadoLibro IN ('Pendiente', 'Renovado');";
                        actualizar.Parameters.AddWithValue("$hoy", fechaDevolucion.ToString("yyyy-MM-dd"));
                        actualizar.Parameters.AddWithValue("$personal", personal);
                        actualizar.Parameters.AddWithValue("$id", id);
                        if (actualizar.ExecuteNonQuery() != 1)
                            throw new InvalidOperationException("El préstamo ya fue devuelto o ya no existe.");
                    }

                    if (string.IsNullOrEmpty(codigoLibro))
                    {
                        string tituloFila = dgvPrestamos.CurrentRow?.Cells["TituloLibro"]?.Value?.ToString() ?? "";
                        using var obtenerPorTitulo = conexion.CreateCommand();
                        obtenerPorTitulo.Transaction = transaccion;
                        obtenerPorTitulo.CommandText = @"
                            SELECT Codigo FROM Libros
                            WHERE Titulo = $titulo AND Disponibilidad = 'Prestado'
                            LIMIT 1;";
                        obtenerPorTitulo.Parameters.AddWithValue("$titulo", tituloFila);
                        codigoLibro = obtenerPorTitulo.ExecuteScalar()?.ToString();
                    }

                    if (!string.IsNullOrEmpty(codigoLibro))
                    {
                        using var liberar = conexion.CreateCommand();
                        liberar.Transaction = transaccion;
                        liberar.CommandText = @"
                            UPDATE Libros SET Disponibilidad = 'Disponible'
                            WHERE Codigo = $codigo
                              AND NOT EXISTS (
                                  SELECT 1 FROM PrestamosExternos
                                  WHERE CodigoLibro = $codigo
                                    AND ID <> $id
                                    AND EstadoLibro IN ('Pendiente', 'Renovado'));";
                        liberar.Parameters.AddWithValue("$codigo", codigoLibro);
                        liberar.Parameters.AddWithValue("$id", id);
                        int filas = liberar.ExecuteNonQuery();
                        if (filas == 0)
                        {
                            transaccion.Rollback();
                            throw new InvalidOperationException(
                                "No se pudo liberar el ejemplar (código no encontrado o " +
                                "otro préstamo activo lo retiene). La devolución se canceló.");
                        }
                    }

                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw;
                }

                MessageBox.Show("Devolución registrada. El libro vuelve a estar disponible.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarPrestamosActivos();
                NotificarCambioPrestamos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar la devolución: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ObtenerNombrePersonal(string accion)
        {
            using var form = new Form
            {
                Text = "Personal - " + accion,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(380, 150),
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false
            };

            var lbl = new Label
            {
                Text = $"Ingrese el nombre del personal que {accion}:",
                AutoSize = true,
                Location = new Point(20, 20),
                Font = new Font("Segoe UI", 10F)
            };

            var txt = new TextBox
            {
                Location = new Point(20, 50),
                Width = 340,
                Font = new Font("Segoe UI", 10F)
            };

            var btnOk = new Button
            {
                Text = "Aceptar",
                DialogResult = DialogResult.OK,
                Location = new Point(170, 95),
                Size = new Size(90, 35)
            };
            EstiloUI.EstilizarBotonPrimario(btnOk);

            var btnCancel = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Location = new Point(270, 95),
                Size = new Size(90, 35)
            };
            EstiloUI.EstilizarBotonSecundario(btnCancel);

            form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
            form.AcceptButton = btnOk;
            form.CancelButton = btnCancel;

            return form.ShowDialog(this) == DialogResult.OK ? txt.Text.Trim() : string.Empty;
        }

        private void btnRenovarFila_Click(object sender, EventArgs e)
        {
            if (dgvPrestamos.CurrentRow == null || dgvPrestamos.CurrentRow.Cells["ID"].Value == null)
            {
                MessageBox.Show("Seleccione un préstamo de la lista.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string personal = ObtenerNombrePersonal("renovó");
            if (string.IsNullOrWhiteSpace(personal))
                return;

            int id = Convert.ToInt32(dgvPrestamos.CurrentRow.Cells["ID"].Value);
            DateTime fechaRenovacion = DateTime.Now;

            if (MessageBox.Show("¿Confirmar la renovación del préstamo seleccionado?",
                    "Renovar Préstamo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var transaccion = conexion.BeginTransaction();
                try
                {
                    RenovarPrestamo(conexion, transaccion, id, fechaRenovacion, personal);
                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw;
                }

                MessageBox.Show("Renovación registrada correctamente.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarPrestamosActivos();
                NotificarCambioPrestamos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar la renovación: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (dgvPrestamos.CurrentRow == null || dgvPrestamos.CurrentRow.Cells["ID"].Value == null)
            {
                MessageBox.Show("Seleccione un préstamo de la lista para modificar.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(dgvPrestamos.CurrentRow.Cells["ID"].Value);
            CargarPrestamoParaEditar(id);
        }

        private void CargarPrestamoParaEditar(int id)
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                var tabla = new System.Data.DataTable();
                using (var cmd = conexion.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM PrestamosExternos WHERE ID = $id;";
                    cmd.Parameters.AddWithValue("$id", id);
                    using var lector = cmd.ExecuteReader();
                    tabla.Load(lector);
                }

                if (tabla.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontró el préstamo seleccionado.",
                        "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var fila = tabla.Rows[0];

                txtNombre.Text = fila["NombreUsuario"]?.ToString() ?? "";
                txtCorreo.Text = fila["Correo"]?.ToString() ?? "";
                txtDui.Text = fila["DUI"]?.ToString() ?? "";
                txtTelefono.Text = fila["Telefono"]?.ToString() ?? "";
                txtDireccion.Text = fila["Direccion"]?.ToString() ?? "";

                string generoFila = fila["Genero"]?.ToString() ?? "";
                cmbGeneroPrestamo.SelectedIndex = -1;
                int indiceGenero = cmbGeneroPrestamo.FindStringExact(generoFila);
                if (indiceGenero >= 0)
                    cmbGeneroPrestamo.SelectedIndex = indiceGenero;

                string tituloLibro = fila["TituloLibro"]?.ToString() ?? "";
                string codigoLibro = fila["CodigoLibro"]?.ToString() ?? "";

                if (DateTime.TryParse(fila["FechaPrestamo"]?.ToString(), out var fp))
                    dtpFechaPrestamo.Value = FechaSegura(dtpFechaPrestamo, fp);

                txtPersonalPresto.Text = fila["PersonalPresto"]?.ToString() ?? "";

                txtEstado.Text = fila["EstadoLibro"]?.ToString() ?? "Pendiente";

                if (DateTime.TryParse(fila["FechaEntrega"]?.ToString(), out var fe))
                    dtpFechaEntrega.Value = FechaSegura(dtpFechaEntrega, fe);

                txtTituloLibro.Text = tituloLibro;
                txtCodigoLibro.Text = codigoLibro;
                if (!string.IsNullOrWhiteSpace(codigoLibro))
                    BloquearPorCodigo();

                _prstamoEditandoId = id;
                btnRegistrar.Text = "Actualizar Préstamo";
                EstiloUI.EstilizarBotonPrimario(btnRegistrar);
                txtNombre.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el préstamo: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
                return Notificar("Escriba el nombre del usuario.", txtNombre);

            if (string.IsNullOrWhiteSpace(txtTituloLibro.Text))
                return Notificar("Seleccione o escriba el título del libro.", txtTituloLibro);

            if (string.IsNullOrWhiteSpace(txtPersonalPresto.Text))
                return Notificar("Escriba el personal que realiza el préstamo.", txtPersonalPresto);

            if (dtpFechaEntrega.Value.Date < dtpFechaPrestamo.Value.Date)
                return Notificar("La fecha de entrega no puede ser anterior a la de préstamo.", dtpFechaEntrega);

            return true;
        }

        private static bool Notificar(string mensaje, Control control)
        {
            MessageBox.Show(mensaje, "Validación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            control.Focus();
            return false;
        }

        private static readonly DateTime MinFechaSoportada = new DateTime(1753, 1, 1);

        private static DateTime FechaSegura(DateTimePicker control, DateTime valor)
        {
            DateTime piso = control.MinDate > MinFechaSoportada ? control.MinDate : MinFechaSoportada;

            if (valor >= piso && valor <= control.MaxDate)
                return valor;

            // Valor de rescate seguro
            DateTime rescate = DateTime.Now;
            if (rescate < piso) rescate = piso;
            if (rescate > control.MaxDate) rescate = control.MaxDate;
            return rescate;
        }

        private void dtpFechaPrestamo_ValueChanged(object? sender, EventArgs e)
        {
            dtpFechaEntrega.MinDate = dtpFechaPrestamo.Value;
            dtpFechaEntrega.Value = FechaSegura(dtpFechaEntrega, dtpFechaPrestamo.Value.AddDays(8));
        }

        private void LimpiarCampos()
        {
            foreach (var caja in new[] { txtNombre, txtCorreo, txtDui, txtTelefono,
                     txtDireccion, txtPersonalPresto })
            {
                caja.Clear();
            }
            txtTituloLibro.Text = string.Empty;

            txtEstado.Text = "Pendiente";

            cmbGeneroPrestamo.SelectedIndex = -1;

            dtpFechaEntrega.MinDate = MinFechaSoportada;
            dtpFechaPrestamo.Value = FechaSegura(dtpFechaPrestamo, DateTime.Today);   
            dtpFechaEntrega.Value = FechaSegura(dtpFechaEntrega, DateTime.Today.AddDays(8));
            dtpFechaEntrega.MinDate = dtpFechaPrestamo.Value;

            LimpiarEstadoCodigo();
        }

        private void LimpiarParaNuevo()
        {
            LimpiarCampos();

            _prstamoEditandoId = null;

            btnRegistrar.Text = "Registrar Préstamo";
            EstiloUI.EstilizarBotonPrimario(btnRegistrar);
            txtNombre.Focus();
        }

        private void NotificarCambioPrestamos()
        {
            if (FindForm() is Form1 principal)
                principal.NotificarCambioPrestamos();
        }
    }
}