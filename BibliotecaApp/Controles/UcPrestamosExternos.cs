using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    /// <summary>
    /// Apartado B: préstamos de libros para llevar a casa, renovaciones y devoluciones.
    /// El ComboBox muestra solo Títulos (con autocompletado). El Codigo único del
    /// ejemplar físico se resuelve en la BD al momento de cada operación.
    /// </summary>
    public partial class UcPrestamosExternos : UserControl // HERNCIA
    {
        private int? _prstamoEditandoId = null;
        
        // Variables de paginación
        private int paginaActual = 1;
        private int tamanoPagina = 11; // Definitivo (producción): llena el DataGridView dejando una fila de margen
        private int totalPaginas = 1;
        
        // Botones de paginación (creados dinámicamente para no tocar Designer)
        private Button? btnAnterior;
        private Button? btnSiguiente;
        private Label? lblPagina;
        
        // Botones de administración (creados dinámicamente para no tocar Designer)
        private Button? btnEliminar;
        private Button? btnLimpiar;

        // Espaciador invisible que empuja la paginación al extremo derecho de flpBotones
        private Control? _espaciadorPaginacion;

        public UcPrestamosExternos()
        {
            InitializeComponent();

            // Configurar filas dinámicas (rescatado del diseñador para evitar errores)
            for (int i = 0; i < 8; i++)
            {
                tlpCampos.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            splitPrestamos.Dock = DockStyle.Fill;
            splitPrestamos.Orientation = Orientation.Horizontal;

            // Ajuste a contenido real (eliminada sección Renovación/Devolución) + 30px margen botones
            splitPrestamos.SplitterDistance = 410;

            splitPrestamos.Panel2.AutoScroll = false;

            pnlDatos.Dock = DockStyle.Top;
            pnlBotonesAccion.Dock = DockStyle.Top;
            pnlContenedorGrid.Dock = DockStyle.Fill;
            dgvPrestamos.Dock = DockStyle.Fill;
            dgvPrestamos.ScrollBars = ScrollBars.Both;

            // Suscribir evento de clic en botones de la grilla
            dgvPrestamos.CellContentClick += DgvPrestamos_CellContentClick;

            // Suscribir evento de selección para habilitar/deshabilitar notificaciones según estado
            dgvPrestamos.SelectionChanged += DgvPrestamos_SelectionChanged;

            // Suscribir evento CellFormatting para coloreo dinámico de la columna Estado
            dgvPrestamos.CellFormatting += DgvPrestamos_CellFormatting_Estado;

            // Crear botones de administración (Eliminar y Limpiar) dinámicamente.
            // Se agregan ANTES que la paginación para que queden junto a "Modificar".
            CrearBotonesAdministracion();

            // Crear botones de paginación dinámicamente (sin tocar Designer).
            // Van al FINAL de la fila y un espaciador los empuja al extremo derecho.
            CrearBotonesPaginacion();

            // Asegurar que la columna Notificado exista en la BD
            AsegurarColumnaNotificado();

            pnlDatos.SendToBack();
            pnlBotonesAccion.BringToFront();
        }

        /// <summary>
        /// Agrega la columna Notificado a la tabla PrestamosExternos si no existe.
        /// SQLite lanza error si la columna ya existe; el catch lo ignora silenciosamente.
        /// </summary>
        private void AsegurarColumnaNotificado()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN Notificado INTEGER DEFAULT 0;";
                comando.ExecuteNonQuery();
            }
            catch
            {
                // Si la columna ya existe, SQLite lanza excepción; ignoramos para continuar flujo normal.
            }
        }

        private void UcPrestamosExternos_Load(object sender, EventArgs e)
        {
            AplicarPlaceholders();

            // Fecha de Préstamo: sin restricción de MinDate (puede ser cualquier fecha)
            // Fecha de Entrega Esperada: nunca menor a la Fecha de Préstamo
            dtpFechaEntrega.MinDate = dtpFechaPrestamo.Value;

            dtpFechaPrestamo.Value = DateTime.Today;
            dtpFechaEntrega.Value = DateTime.Today.AddDays(8);

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

            CargarPrestamosActivos();
        }

        /// <summary>Método público invocado por Form1 al navegar a este apartado.</summary>
        public void Actualizar()
        {
            CargarPrestamosActivos();
        }

        // ====================================================================
        //  FLUJO ÁGIL — carga directa desde Inventario
        // ====================================================================

        /// <summary>
        /// Método público invocado por Form1.CargarPrestamoDesdeInventario.
        /// Precarga el Código y el Título del ejemplar y bloquea ambos campos
        /// para que el operador solo complete los datos del usuario.
        /// </summary>
        public void CargarDesdeInventario(string codigo, string titulo)
        {
            txtCodigoLibro.Text = codigo;
            txtTituloLibro.Text = titulo;
            BloquearPorCodigo();
            MostrarAviso($"Ejemplar \"{codigo}\" disponible. Campos de libro bloqueados.", EstiloUI.Acento);
            txtNombre.Focus();
        }

        // ====================================================================
        //  BÚSQUEDA RÁPIDA POR CÓDIGO DEL EJEMPLAR
        // ====================================================================

        private void txtCodigoLibro_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter) return;
            e.Handled = true;
            BuscarLibroPorCodigo();
        }

        /// <summary>
        /// Busca el ejemplar por su Código único. Si existe y está 'Disponible'
        /// autocompleta el Título y bloquea ambos campos; en caso contrario
        /// limpia el campo y muestra una alerta visual en lblAvisoCodigo.
        /// </summary>
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

        // ------------------------------------------------------------------
        //  Placeholders nativos (Win32 EM_SETCUEBANNER)
        // ------------------------------------------------------------------
        private void AplicarPlaceholders()
        {
            EstiloUI.EstablecerPlaceholder(txtDui, "Ej: 00000000-0");
            EstiloUI.EstablecerPlaceholder(txtCorreo, "ejemplo@correo.com");
            EstiloUI.EstablecerPlaceholder(txtTelefono, "Ej: 2222-2222");
        }

        // ====================================================================
        //  CONSULTA DE CÓDIGOS — resolver ejemplar físico desde el título
        // ====================================================================

        /// <summary>
        /// Devuelve el Codigo del primer ejemplar disponible de un título, o null si no hay stock.
        /// </summary>
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

        /// <summary>
        /// Devuelve el Codigo del primer ejemplar en estado Prestado de un título, o null.
        /// </summary>
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

        // ====================================================================
        //  CARGA DE PRÉSTAMOS ACTIVOS (DataGridView)
        // ====================================================================

        private void CargarPrestamosActivos()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                using var comando = conexion.CreateCommand();

                // --- Paginación: calcula total de páginas y offset defensivo ---
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
                           TituloLibro,
                           strftime('%d/%m/%Y', FechaPrestamo)  AS FechaPrestamo,
                           CASE WHEN IFNULL(FechaRenovacion,'') = '' THEN '-'
                                ELSE strftime('%d/%m/%Y', FechaRenovacion) END AS FechaRenovacion,
                           strftime('%d/%m/%Y', FechaEntrega)   AS [Entrega Esperada],
                           PersonalPresto,
                           EstadoLibro                          AS Estado,
                           Notificado
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

                // --- UI de paginación (botones dinámicos) ---
                if (lblPagina != null) lblPagina.Text = $"Página {paginaActual} de {totalPaginas}";
                if (btnAnterior != null) btnAnterior.Enabled = (paginaActual > 1);
                if (btnSiguiente != null) btnSiguiente.Enabled = (paginaActual < totalPaginas);

                // El texto de lblPagina es AutoSize y cambia de ancho: reajustar espaciador
                ActualizarEspaciadorPaginacion();

                // Asegurar que la columna Notificado exista en el grid (AutoGenerateColumns = false)
                if (!dgvPrestamos.Columns.Contains("Notificado"))
                {
                    var colNotificado = new DataGridViewTextBoxColumn
                    {
                        Name = "Notificado",
                        DataPropertyName = "Notificado",
                        HeaderText = "Notificado",
                        Visible = false // Oculta para el usuario
                    };
                    dgvPrestamos.Columns.Add(colNotificado);
                }

                // Agregar columna de botón "Notificar" si no existe
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

                // Valor inicial para cada fila del botón y restaurar estado Notificado
                foreach (DataGridViewRow row in dgvPrestamos.Rows)
                {
                    try
                    {
                        // Valor por defecto
                        row.Cells["btnMensaje"].Value = "Enviar";

                        // Si Notificado == 1, mostrar estado "Enviado" con color verde
                        var notificadoObj = row.Cells["Notificado"].Value;
                        if (notificadoObj != null && notificadoObj != DBNull.Value)
                        {
                            if (Convert.ToInt32(notificadoObj) == 1)
                            {
                                row.Cells["btnMensaje"].Value = "Enviado";
                                row.Cells["btnMensaje"].Style.BackColor = Color.LightGreen;
                                row.Cells["btnMensaje"].Style.ForeColor = Color.DarkGreen;
                                row.Cells["btnMensaje"].Style.SelectionBackColor = Color.LightGreen;
                                row.Cells["btnMensaje"].Style.SelectionForeColor = Color.DarkGreen;
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

        // ====================================================================
        //  PAGINACIÓN (botones creados dinámicamente en code-behind)
        // ====================================================================

        /// <summary>
        /// Crea los botones de paginación (Anterior / Siguiente / lblPagina)
        /// sin tocar el archivo Designer.cs. Se colocan al FINAL de flpBotones,
        /// precedidos por un espaciador que los empuja al extremo derecho.
        /// </summary>
        private void CrearBotonesPaginacion()
        {
            // --- Espaciador invisible: ocupa el hueco sobrante de la fila ---
            _espaciadorPaginacion = new Label
            {
                Name = "espaciadorPaginacion",
                Text = string.Empty,
                AutoSize = false,
                Size = new Size(0, 1),
                Margin = new Padding(0)
            };
            flpBotones.Controls.Add(_espaciadorPaginacion);

            // --- Botón "Anterior": estilo minimalista, icono cuadrado 35px ---
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

            // --- Etiqueta de página ---
            lblPagina = new Label
            {
                Name = "lblPagina",
                Text = "Página 1 de 1",
                AutoSize = true,
                Margin = new Padding(10, 10, 10, 5),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // --- Botón "Siguiente": estilo minimalista, icono cuadrado 35px ---
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

            // Orden de izquierda a derecha en la esquina: btnAnterior -> lblPagina -> btnSiguiente
            flpBotones.Controls.Add(btnAnterior);
            flpBotones.Controls.Add(lblPagina);
            flpBotones.Controls.Add(btnSiguiente);

            // Reajustar el espaciador cuando cambie el ancho de la fila (resize del formulario)
            flpBotones.Resize += (_, _) => ActualizarEspaciadorPaginacion();
            ActualizarEspaciadorPaginacion();
        }

        /// <summary>
        /// Calcula el ancho sobrante de flpBotones y se lo asigna al espaciador,
        /// de modo que la paginación quede pegada al borde derecho de la barra.
        /// </summary>
        private void ActualizarEspaciadorPaginacion()
        {
            if (_espaciadorPaginacion == null || !flpBotones.IsHandleCreated) return;

            int ocupado = 0;
            foreach (Control c in flpBotones.Controls)
            {
                if (c == _espaciadorPaginacion || !c.Visible) continue;
                ocupado += c.Width + c.Margin.Horizontal;
            }

            // 35 px de margen derecho: evita que la paginación choque contra el
            // borde visible del FlowLayoutPanel y se desborde de la barra.
            const int margenDerecho = 35;

            int disponible = flpBotones.ClientSize.Width - ocupado
                             - _espaciadorPaginacion.Margin.Horizontal - margenDerecho;

            // Nunca se aplica un ancho negativo (ventana demasiado angosta)
            int nuevoAncho = disponible < 0 ? 0 : disponible;

            // Evita ciclos de layout: solo escribe si el valor realmente cambió
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

        /// <summary>
        /// Calcula totalPaginas según los registros activos de la BD.
        /// Nunca devuelve 0 páginas (mínimo 1).
        /// </summary>
        private void CalcularTotalPaginas()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = "SELECT COUNT(*) FROM PrestamosExternos WHERE EstadoLibro IN ('Pendiente', 'Renovado', 'Entregado');";

                var resultado = comando.ExecuteScalar();
                int totalRegistros = (resultado != null && resultado != DBNull.Value) ? Convert.ToInt32(resultado) : 0;

                if (tamanoPagina <= 0) tamanoPagina = 25; // Fallback de seguridad

                totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina);
                if (totalPaginas <= 0) totalPaginas = 1;
            }
            catch
            {
                totalPaginas = 1;
            }
        }

        // ====================================================================
        //  BOTONES DE ADMINISTRACIÓN (Eliminar / Limpiar) — solo administrador
        // ====================================================================

        /// <summary>
        /// Crea btnEliminar y btnLimpiar dinámicamente (sin tocar Designer)
        /// y los agrega a flpBotones. La visibilidad por rol se aplica en el Load.
        /// </summary>
        private void CrearBotonesAdministracion()
        {
            btnEliminar = new Button
            {
                Name = "btnEliminar",
                Text = "Eliminar préstamo",
                AutoSize = true,
                Margin = new Padding(5, 0, 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Visible = false // Solo administrador (se muestra en Load)
            };
            EstiloUI.EstilizarBotonSecundario(btnEliminar);
            btnEliminar.Click += btnEliminar_Click;

            btnLimpiar = new Button
            {
                Name = "btnLimpiar",
                Text = "Limpiar campos",
                AutoSize = true,
                Margin = new Padding(5, 0, 5, 5),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Visible = false // Solo administrador (se muestra en Load)
            };
            EstiloUI.EstilizarBotonSecundario(btnLimpiar);
            btnLimpiar.Click += btnLimpiar_Click;

            flpBotones.Controls.Add(btnEliminar);
            flpBotones.Controls.Add(btnLimpiar);
        }

        /// <summary>
        /// Elimina (DELETE) el préstamo seleccionado. Acción exclusiva de administrador:
        /// pide autenticación con FormAutenticacion (AdminCubo / Admin123$) y solo
        /// permite eliminar registros en estado 'Entregado'. Los usuarios normales
        /// jamás usan DELETE (solo UPDATE).
        /// </summary>
        private void btnEliminar_Click(object? sender, EventArgs e)
        {
            if (dgvPrestamos.CurrentRow == null || dgvPrestamos.CurrentRow.Cells["ID"].Value == null)
            {
                MessageBox.Show("Seleccione un préstamo de la lista para eliminar.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // --- Autenticación de seguridad (mismo modal que Inventario: AdminCubo / Admin123$) ---
            using (var frmAuth = new FormAutenticacion("AdminCubo", "Admin123$"))
            {
                if (frmAuth.ShowDialog(this) != DialogResult.OK) return;
            }

            int id = Convert.ToInt32(dgvPrestamos.CurrentRow.Cells["ID"].Value);
            string estado = dgvPrestamos.CurrentRow.Cells["Estado"].Value?.ToString()?.Trim() ?? "";

            // Solo se pueden eliminar préstamos ya entregados (cerrados).
            if (!string.Equals(estado, "Entregado", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Solo se pueden eliminar préstamos con estado 'Entregado'.\n" +
                    "Registre primero la devolución del libro.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                "¿Está seguro de que desea ELIMINAR este préstamo definitivamente?\n\nEsta acción no se puede deshacer.",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (resultado != DialogResult.Yes) return;

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText = "DELETE FROM PrestamosExternos WHERE ID = @id AND EstadoLibro = 'Entregado';";
                cmd.Parameters.AddWithValue("@id", id);

                if (cmd.ExecuteNonQuery() == 0)
                {
                    MessageBox.Show("No se pudo eliminar el préstamo (ya no existe o no está en estado 'Entregado').",
                        "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
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

        /// <summary>
        /// Limpia los campos del formulario dejándolo listo para un nuevo préstamo.
        /// </summary>
        private void btnLimpiar_Click(object? sender, EventArgs e)
        {
            LimpiarParaNuevo();
        }

        // ====================================================================
        //  EVENTOS DEL DataGridView (nombres exactos que exige el Designer)
        // ====================================================================

        /// <summary>
        /// Bloquea/habilita las acciones de la fila seleccionada según el estado:
        /// un préstamo 'Entregado' no puede renovarse ni notificarse.
        /// </summary>
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
                    celdaBtn.ReadOnly = filaEntregada; // Bloquea el clic de "Enviar"

                    if (filaEntregada)
                    {
                        celdaBtn.Value = "Entregado";
                        celdaBtn.Style.BackColor = Color.FromArgb(204, 255, 204);
                        celdaBtn.Style.ForeColor = Color.DarkGreen;
                        celdaBtn.Style.SelectionBackColor = Color.FromArgb(204, 255, 204);
                        celdaBtn.Style.SelectionForeColor = Color.DarkGreen;
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

            var col = dgvPrestamos.Columns[e.ColumnIndex];

            // La columna Estado la pinta DgvPrestamos_CellFormatting_Estado
            if (col.Name == "Estado") return;

            // El botón "Notificar" conserva sus colores propios (Enviado / Entregado)
            if (col.Name == "btnMensaje") return;

            // Resto de la fila: siempre fondo blanco
            e.CellStyle.BackColor = Color.White;
            e.CellStyle.SelectionBackColor = EstiloUI.Acento;
        }

        /// <summary>
        /// Colorea únicamente la celda de la columna "Estado":
        /// Pendiente = rojo (255,204,204) · Renovado = celeste (204,235,255) ·
        /// Entregado = verde (204,255,204).
        /// </summary>
        private void DgvPrestamos_CellFormatting_Estado(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

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
            e.CellStyle.SelectionBackColor = Color.FromArgb(
                Math.Max(0, colorFondo.R - 30),
                Math.Max(0, colorFondo.G - 30),
                Math.Max(0, colorFondo.B - 30));
        }

        /// <summary>
        /// Maneja el clic en la columna de botones "Notificar" para abrir un mailto: individual
        /// con los datos del préstamo (nombre, correo, título, fecha de entrega).
        /// </summary>
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
                // Construir URI mailto: con subject y body según requerimientos exactos
                // Subject: "Confirmación de préstamo - Biblioteca CUBO"
                // Body: "Hola [usuario], muchas gracias por elegir la Biblioteca CUBO.\n\nTe confirmamos el préstamo del libro '[libro]'. Te recordamos amablemente que la fecha esperada de devolución o renovación es el [fecha].\n\n¡Esperamos que disfrutes tu lectura!"
                string subject = Uri.EscapeDataString("Confirmación de préstamo - Biblioteca CUBO");
                string body = Uri.EscapeDataString(
                    $"Hola {nombreUsuario}, muchas gracias por elegir la Biblioteca CUBO.\n\n" +
                    $"Te confirmamos el préstamo del libro '{tituloLibro}'. Te recordamos amablemente que la fecha esperada de devolución o renovación es el {fechaEntrega}.\n\n" +
                    $"¡Esperamos que disfrutes tu lectura!");

                string mailtoUri = $"mailto:{correo}?subject={subject}&body={body}";

                // Lanzar cliente de correo por defecto
                var psi = new ProcessStartInfo(mailtoUri) { UseShellExecute = true };
                Process.Start(psi);

                // Feedback visual: cambiar botón a "Enviado" con color verde
                var celda = dgvPrestamos.Rows[e.RowIndex].Cells[e.ColumnIndex];
                celda.Value = "Enviado";
                celda.Style.BackColor = Color.LightGreen;
                celda.Style.ForeColor = Color.DarkGreen;
                celda.Style.SelectionBackColor = Color.LightGreen;
                celda.Style.SelectionForeColor = Color.DarkGreen;

                // Persistir en BD: Notificado = 1
                int idPrestamo = Convert.ToInt32(fila.Cells["ID"].Value);
                using (var conexion = ConexionDB.ObtenerConexion())
                using (var updateCmd = conexion.CreateCommand())
                {
                    updateCmd.CommandText = "UPDATE PrestamosExternos SET Notificado = 1 WHERE ID = @id;";
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

        // ====================================================================
        //  REGISTRAR PRÉSTAMO
        // ====================================================================

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

        /// <summary>
        /// Inserta un nuevo préstamo y marca como "Prestado" el primer ejemplar
        /// disponible del título seleccionado. El Codigo se resuelve en tiempo real.
        /// Duplica la validación de la UI por seguridad y nunca lanza una excepción
        /// sin controlar: captura SqliteException y cualquier otra Exception.
        /// </summary>
        private void RegistrarPrestamo()
        {
            string titulo = txtTituloLibro.Text.Trim();

            // Validación preventiva extra (aunque ValidarFormulario ya corrió).
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
                                (NombreUsuario, Correo, DUI, Telefono, Direccion, TituloLibro,
                                 FechaPrestamo, PersonalPresto, FechaEntrega, EstadoLibro, CodigoLibro)
                            VALUES
                                ($nombre, $correo, $dui, $telefono, $direccion, $titulo,
                                 $fechaPrestamo, $personalPresto, $fechaEntrega, $estado, $codigo);";

                        insertar.Parameters.AddWithValue("$nombre", txtNombre.Text.Trim());
                        insertar.Parameters.AddWithValue("$correo", txtCorreo.Text.Trim());
                        insertar.Parameters.AddWithValue("$dui", txtDui.Text.Trim());
                        insertar.Parameters.AddWithValue("$telefono", txtTelefono.Text.Trim());
                        insertar.Parameters.AddWithValue("$direccion", txtDireccion.Text.Trim());
                        insertar.Parameters.AddWithValue("$titulo", titulo);
                        insertar.Parameters.AddWithValue("$fechaPrestamo", dtpFechaPrestamo.Value.ToString("yyyy-MM-dd"));
                        insertar.Parameters.AddWithValue("$personalPresto", txtPersonalPresto.Text.Trim());
                        insertar.Parameters.AddWithValue("$fechaEntrega", dtpFechaEntrega.Value.ToString("yyyy-MM-dd"));
                        insertar.Parameters.AddWithValue("$estado",
                            string.IsNullOrWhiteSpace(txtEstado.Text) ? "Pendiente" : txtEstado.Text.Trim());
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

        // ====================================================================
        //  ACTUALIZAR PRÉSTAMO (modo edición)
        // ====================================================================

        /// <summary>
        /// Actualiza un préstamo existente. Si cambió el título, libera el ejemplar
        /// viejo y marca uno nuevo — ambos por Codigo único.
        /// </summary>
        private void ActualizarPrestamo(int id)
        {
            string tituloNuevo = txtTituloLibro.Text.Trim();

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                // Título actual del préstamo (antes de editar).
                string tituloViejo = "";
                using (var cmd = conexion.CreateCommand())
                {
                    cmd.CommandText = "SELECT TituloLibro FROM PrestamosExternos WHERE ID = $id;";
                    cmd.Parameters.AddWithValue("$id", id);
                    object? r = cmd.ExecuteScalar();
                    if (r != null) tituloViejo = r.ToString() ?? "";
                }

                // Renovación: SOLO se actualizan fecha/personal de renovación y el
                // estado a 'Renovado'. No se ejecuta devolución ni se borra el registro.
                if (string.Equals(txtEstado.Text, "Renovado", StringComparison.OrdinalIgnoreCase))
                {
                    using var transaccion = conexion.BeginTransaction();
                    try
                    {
                        RenovarPrestamo(conexion, transaccion, id, DateTime.Now, "");
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
                    // Título distinto: liberar ejemplar viejo + marcar ejemplar nuevo.
                    string? codigoViejo = ObtenerCodigoPrestado(conexion, tituloViejo);
                    string? codigoNuevo = ObtenerCodigoDisponible(conexion, tituloNuevo);

                    if (codigoNuevo == null)
                    {
                        MessageBox.Show(
                            $"No hay ejemplares disponibles del título \"{tituloNuevo}\".",
                            "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    using var transaccion = conexion.BeginTransaction();
                    try
                    {
                        ActualizarDatosPrestamo(conexion, transaccion, id);

                        if (!string.IsNullOrEmpty(codigoViejo))
                        {
                            using var liberar = conexion.CreateCommand();
                            liberar.Transaction = transaccion;
                            liberar.CommandText = "UPDATE Libros SET Disponibilidad = 'Disponible' WHERE Codigo = $codigo;";
                            liberar.Parameters.AddWithValue("$codigo", codigoViejo);
                            liberar.ExecuteNonQuery();
                        }

                        using var marcar = conexion.CreateCommand();
                        marcar.Transaction = transaccion;
                        marcar.CommandText = "UPDATE Libros SET Disponibilidad = 'Prestado' WHERE Codigo = $codigo;";
                        marcar.Parameters.AddWithValue("$codigo", codigoNuevo);
                        marcar.ExecuteNonQuery();

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

        /// <summary>
        /// Ejecuta el UPDATE de los campos del formulario en PrestamosExternos.
        /// Se separa para reutilizar dentro de transacciones con manejo de inventario.
        /// </summary>
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

        /// <summary>
        /// Renovación exclusiva: actualiza FechaRenovacion, PersonalRenovo,
        /// la nueva Fecha de Entrega Esperada y EstadoLibro = 'Renovado'.
        /// No toca inventario ni ejecuta lógica de devolución.
        /// </summary>
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

        // ====================================================================
        //  DEVOLUCIÓN
        // ====================================================================

        /// <summary>
        /// Marca el préstamo como "Entregado" y libera el ejemplar físico (por Codigo).
        /// Usa fecha del sistema y popup para el nombre del personal.
        /// </summary>
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
                                PersonalRecibio = $personal
                            WHERE ID = $id
                              AND EstadoLibro IN ('Pendiente', 'Renovado');";
                        actualizar.Parameters.AddWithValue("$hoy", fechaDevolucion.ToString("yyyy-MM-dd"));
                        actualizar.Parameters.AddWithValue("$personal", personal);
                        actualizar.Parameters.AddWithValue("$id", id);
                        if (actualizar.ExecuteNonQuery() != 1)
                            throw new InvalidOperationException("El préstamo ya fue devuelto o ya no existe.");
                    }

                    if (!string.IsNullOrEmpty(codigoLibro))
                    {
                        using var liberar = conexion.CreateCommand();
                        liberar.Transaction = transaccion;
                        liberar.CommandText = "UPDATE Libros SET Disponibilidad = 'Disponible' WHERE Codigo = $codigo;";
                        liberar.Parameters.AddWithValue("$codigo", codigoLibro);
                        liberar.ExecuteNonQuery();
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

        /// <summary>
        /// Muestra un modal estilo InputBox para solicitar el nombre del personal.
        /// </summary>
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

        /// <summary>
        /// Renueva el préstamo seleccionado: fecha actual, personal por popup, estado = Renovado.
        /// </summary>
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

        // ====================================================================
        //  MODIFICAR — carga el registro en el formulario
        // ====================================================================

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

        /// <summary>
        /// Carga un préstamo existente en los controles del formulario.
        /// El ComboBox se selecciona por Text (título) ya que es una lista simple.
        /// </summary>
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

                string tituloLibro = fila["TituloLibro"]?.ToString() ?? "";
                string codigoLibro = fila["CodigoLibro"]?.ToString() ?? "";

                if (DateTime.TryParse(fila["FechaPrestamo"]?.ToString(), out var fp))
                    dtpFechaPrestamo.Value = fp;

                txtPersonalPresto.Text = fila["PersonalPresto"]?.ToString() ?? "";

                txtEstado.Text = fila["EstadoLibro"]?.ToString() ?? "Pendiente";

                if (DateTime.TryParse(fila["FechaEntrega"]?.ToString(), out var fe))
                    dtpFechaEntrega.Value = fe;

                // Seleccionar el título en el ComboBox (por texto).
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

        // ====================================================================
        //  VALIDACIÓN Y LIMPIEZA
        // ====================================================================

        /// <summary>
        /// Valida los campos obligatorios del formulario usando
        /// string.IsNullOrWhiteSpace. Si algo falta, muestra una advertencia
        /// amigable y detiene el flujo devolviendo false (nunca rompe el programa).
        /// </summary>
        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
                return Notificar("Escriba el nombre del usuario.", txtNombre);

            if (string.IsNullOrWhiteSpace(txtTituloLibro.Text))
                return Notificar("Seleccione o escriba el título del libro.", txtTituloLibro);

            if (string.IsNullOrWhiteSpace(txtPersonalPresto.Text))
                return Notificar("Escriba el personal que realiza el préstamo.", txtPersonalPresto);

            // Fechas: los DateTimePicker siempre tienen una fecha válida, pero
            // reforzamos que el rango tenga coherencia.
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

        /// <summary>
        /// Autocompletado inteligente: al elegir la fecha de préstamo, sugiere
        /// la entrega esperada sumando exactamente 8 días. El control
        /// dtpFechaEntrega permanece habilitado para ajuste manual.
        /// Además, actualiza el MinDate de la fecha de entrega para que nunca
        /// sea anterior a la fecha de préstamo.
        /// </summary>
        private void dtpFechaPrestamo_ValueChanged(object? sender, EventArgs e)
        {
            dtpFechaEntrega.MinDate = dtpFechaPrestamo.Value;
            dtpFechaEntrega.Value = dtpFechaPrestamo.Value.AddDays(8);
        }

        private void LimpiarCampos()
        {
            foreach (var caja in new[] { txtNombre, txtCorreo, txtDui, txtTelefono,
                     txtDireccion, txtPersonalPresto })
            {
                caja.Clear();
            }
            txtTituloLibro.Text = string.Empty;

            dtpFechaPrestamo.Value = DateTime.Today;
            dtpFechaEntrega.Value = DateTime.Today.AddDays(8);
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