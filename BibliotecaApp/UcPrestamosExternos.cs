using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    /// <summary>
    /// Apartado B: pr\u00e9stamos de libros para llevar a casa, renovaciones y devoluciones.
    /// El ComboBox muestra solo T\u00edtulos (con autocompletado). El Codigo \u00fanico del
    /// ejemplar f\u00edsico se resuelve en la BD al momento de cada operaci\u00f3n.
    /// </summary>
    public partial class UcPrestamosExternos : UserControl // HERNCIA
    {
        private int? _prstamoEditandoId = null;

        public UcPrestamosExternos()
        {
            InitializeComponent();

            splitPrestamos.Dock = DockStyle.Fill;
            splitPrestamos.Orientation = Orientation.Horizontal;
            splitPrestamos.SplitterDistance = 380;
            splitPrestamos.Panel2.AutoScroll = false;

            pnlDatos.Dock = DockStyle.Top;
            pnlBotonesAccion.Dock = DockStyle.Top;
            pnlContenedorGrid.Dock = DockStyle.Fill;
            dgvPrestamos.Dock = DockStyle.Fill;
            dgvPrestamos.ScrollBars = ScrollBars.Both;

            pnlDatos.SendToBack();
            pnlBotonesAccion.BringToFront();
        }

        private void UcPrestamosExternos_Load(object sender, EventArgs e)
        {
            AplicarPlaceholders();
            dtpFechaPrestamo.Value = DateTime.Today;
            dtpFechaEntrega.Value = DateTime.Today.AddDays(15);
            CargarPrestamosActivos();
        }

        /// <summary>M\u00e9todo p\u00fablico invocado por Form1 al navegar a este apartado.</summary>
        public void Actualizar()
        {
            CargarPrestamosActivos();
        }

        // ====================================================================
        //  FLUJO \u00c1GIL \u2014 carga directa desde Inventario
        // ====================================================================

        /// <summary>
        /// M\u00e9todo p\u00fablico invocado por Form1.CargarPrestamoDesdeInventario.
        /// Precarga el C\u00f3digo y el T\u00edtulo del ejemplar y bloquea ambos campos
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
        //  B\u00daSQUEDA R\u00c1PIDA POR C\u00d3DIGO DEL EJEMPLAR
        // ====================================================================

        private void txtCodigoLibro_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter) return;
            e.Handled = true;
            BuscarLibroPorCodigo();
        }

        /// <summary>
        /// Busca el ejemplar por su C\u00f3digo \u00fanico. Si existe y est\u00e1 'Disponible'
        /// autocompleta el T\u00edtulo y bloquea ambos campos; en caso contrario
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
                        MostrarAviso("El ejemplar ya est\u00e1 prestado.", EstiloUI.AlertaRojo);
                    }
                }
                else
                {
                    txtCodigoLibro.Clear();
                    txtTituloLibro.Text = string.Empty;
                    DesbloquearPorCodigo();
                    MostrarAviso("No existe un ejemplar con ese c\u00f3digo.", EstiloUI.AlertaRojo);
                }
            }
            catch (SqliteException ex)
            {
                LimpiarEstadoCodigo();
                MostrarAviso("No se pudo consultar la base de datos.", EstiloUI.AlertaRojo);
                MessageBox.Show("Error de base de datos al buscar el c\u00f3digo: " + ex.Message,
                    "Validaci\u00f3n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                LimpiarEstadoCodigo();
                MessageBox.Show("Error al buscar el c\u00f3digo: " + ex.Message,
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
        //  CONSULTA DE C\u00d3DIGOS \u2014 resolver ejemplar f\u00edsico desde el t\u00edtulo
        // ====================================================================

        /// <summary>
        /// Devuelve el Codigo del primer ejemplar disponible de un t\u00edtulo, o null si no hay stock.
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
        /// Devuelve el Codigo del primer ejemplar en estado Prestado de un t\u00edtulo, o null.
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
        //  CARGA DE PR\u00c9STAMOS ACTIVOS (DataGridView)
        // ====================================================================

        private void CargarPrestamosActivos()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                using var comando = conexion.CreateCommand();
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
                           EstadoLibro                          AS Estado
                    FROM PrestamosExternos
                    WHERE EstadoLibro = 'Pendiente'
                    ORDER BY FechaEntrega ASC;";

                var tabla = new System.Data.DataTable();
                using (var lector = comando.ExecuteReader())
                {
                    tabla.Load(lector);
                }

                dgvPrestamos.DataSource = tabla;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los pr\u00e9stamos: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvPrestamos_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var fila = dgvPrestamos.Rows[e.RowIndex];
            if (fila.Cells["Entrega Esperada"].Value is not string fechaTexto ||
                !DateTime.TryParseExact(fechaTexto, "dd/MM/yyyy",
                    null, System.Globalization.DateTimeStyles.None, out DateTime fechaEntrega))
                return;

            if (fechaEntrega.Date < DateTime.Today)
            {
                fila.DefaultCellStyle.BackColor = EstiloUI.AlertaRojo;
                fila.DefaultCellStyle.SelectionBackColor = EstiloUI.Acento;
            }
        }

        // ====================================================================
        //  REGISTRAR PR\u00c9STAMO
        // ====================================================================

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            if (_prstamoEditandoId.HasValue)
                ActualizarPrestamo(_prstamoEditandoId.Value);
            else
                RegistrarPrestamo();
        }

        /// <summary>
        /// Inserta un nuevo pr\u00e9stamo y marca como "Prestado" el primer ejemplar
        /// disponible del t\u00edtulo seleccionado. El Codigo se resuelve en tiempo real.
        /// Duplica la validaci\u00f3n de la UI por seguridad y nunca lanza una excepci\u00f3n
        /// sin controlar: captura SqliteException y cualquier otra Exception.
        /// </summary>
        private void RegistrarPrestamo()
        {
            string titulo = txtTituloLibro.Text.Trim();

            // Validaci\u00f3n preventiva extra (aunque ValidarFormulario ya corri\u00f3).
            if (string.IsNullOrWhiteSpace(titulo))
            {
                MessageBox.Show("Seleccione o escriba el t\u00edtulo del libro.",
                    "Validaci\u00f3n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                // Obtener el primer Codigo disponible de ese t\u00edtulo.
                string? codigo;
                try
                {
                    codigo = ObtenerCodigoDisponible(conexion, titulo);
                }
                catch (SqliteException ex)
                {
                    MessageBox.Show("Error de base de datos al consultar el ejemplar: " + ex.Message,
                        "Validaci\u00f3n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(codigo))
                {
                    MessageBox.Show(
                        $"No hay ejemplares disponibles del t\u00edtulo \"{titulo}\".",
                        "Validaci\u00f3n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                                 FechaPrestamo, PersonalPresto, FechaRenovacion, PersonalRenovo,
                                 FechaEntrega, PersonalRecibio, EstadoLibro)
                            VALUES
                                ($nombre, $correo, $dui, $telefono, $direccion, $titulo,
                                 $fechaPrestamo, $personalPresto, $fechaRenovacion, $personalRenovo,
                                 $fechaEntrega, $personalRecibio, $estado);";

                        insertar.Parameters.AddWithValue("$nombre", txtNombre.Text.Trim());
                        insertar.Parameters.AddWithValue("$correo", txtCorreo.Text.Trim());
                        insertar.Parameters.AddWithValue("$dui", txtDui.Text.Trim());
                        insertar.Parameters.AddWithValue("$telefono", txtTelefono.Text.Trim());
                        insertar.Parameters.AddWithValue("$direccion", txtDireccion.Text.Trim());
                        insertar.Parameters.AddWithValue("$titulo", titulo);
                        insertar.Parameters.AddWithValue("$fechaPrestamo", dtpFechaPrestamo.Value.ToString("yyyy-MM-dd"));
                        insertar.Parameters.AddWithValue("$personalPresto", txtPersonalPresto.Text.Trim());
                        insertar.Parameters.AddWithValue("$fechaRenovacion",
                            dtpFechaRenovacion.Checked ? dtpFechaRenovacion.Value.ToString("yyyy-MM-dd") : DBNull.Value);
                        insertar.Parameters.AddWithValue("$personalRenovo",
                            dtpFechaRenovacion.Checked ? txtPersonalRenovo.Text.Trim() : DBNull.Value);
                        insertar.Parameters.AddWithValue("$fechaEntrega", dtpFechaEntrega.Value.ToString("yyyy-MM-dd"));
                        insertar.Parameters.AddWithValue("$personalRecibio", DBNull.Value);
                        insertar.Parameters.AddWithValue("$estado", cboEstado.SelectedItem?.ToString() ?? "Pendiente");
                        insertar.ExecuteNonQuery();
                    }

                    // Marcar SOLO el ejemplar \u00fanico como Prestado.
                    using (var marcar = conexion.CreateCommand())
                    {
                        marcar.Transaction = transaccion;
                        marcar.CommandText = "UPDATE Libros SET Disponibilidad = 'Prestado' WHERE Codigo = $codigo;";
                        marcar.Parameters.AddWithValue("$codigo", codigo);
                        marcar.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                }
                catch (SqliteException ex)
                {
                    transaccion.Rollback();
                    MessageBox.Show("Error de base de datos al registrar el pr\u00e9stamo: " + ex.Message,
                        "Validaci\u00f3n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                catch (Exception ex)
                {
                    transaccion.Rollback();
                    MessageBox.Show("Error inesperado al registrar el pr\u00e9stamo: " + ex.Message,
                        "Validaci\u00f3n", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Pr\u00e9stamo registrado correctamente.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarParaNuevo();
                CargarPrestamosActivos();
            }
            catch (SqliteException ex)
            {
                MessageBox.Show("Error de conexi\u00f3n con la base de datos: " + ex.Message,
                    "Validaci\u00f3n", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar el pr\u00e9stamo: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ====================================================================
        //  ACTUALIZAR PR\u00c9STAMO (modo edici\u00f3n)
        // ====================================================================

        /// <summary>
        /// Actualiza un pr\u00e9stamo existente. Si cambi\u00f3 el t\u00edtulo, libera el ejemplar
        /// viejo y marca uno nuevo \u2014 ambos por Codigo \u00fanico.
        /// </summary>
        private void ActualizarPrestamo(int id)
        {
            string tituloNuevo = txtTituloLibro.Text.Trim();

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                // T\u00edtulo actual del pr\u00e9stamo (antes de editar).
                string tituloViejo = "";
                using (var cmd = conexion.CreateCommand())
                {
                    cmd.CommandText = "SELECT TituloLibro FROM PrestamosExternos WHERE ID = $id;";
                    cmd.Parameters.AddWithValue("$id", id);
                    object? r = cmd.ExecuteScalar();
                    if (r != null) tituloViejo = r.ToString() ?? "";
                }

                // Si cambi\u00f3 el t\u00edtulo, necesitamos EjemplarViejo (Prestado) y EjemplarNuevo (Disponible).
                if (string.Equals(tituloViejo, tituloNuevo, StringComparison.OrdinalIgnoreCase))
                {
                    // Mismo t\u00edtulo: solo actualizar datos del pr\u00e9stamo, sin tocar inventario.
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
                    // T\u00edtulo distinto: liberar ejemplar viejo + marcar ejemplar nuevo.
                    string? codigoViejo = ObtenerCodigoPrestado(conexion, tituloViejo);
                    string? codigoNuevo = ObtenerCodigoDisponible(conexion, tituloNuevo);

                    if (codigoNuevo == null)
                    {
                        MessageBox.Show(
                            $"No hay ejemplares disponibles del t\u00edtulo \"{tituloNuevo}\".",
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

                MessageBox.Show("Pr\u00e9stamo actualizado correctamente.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarParaNuevo();
                CargarPrestamosActivos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar el pr\u00e9stamo: " + ex.Message,
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
                    FechaRenovacion = $fechaRenovacion,
                    PersonalRenovo  = $personalRenovo,
                    FechaEntrega    = $fechaEntrega,
                    PersonalRecibio = $personalRecibio,
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
            cmd.Parameters.AddWithValue("$fechaRenovacion",
                dtpFechaRenovacion.Checked ? dtpFechaRenovacion.Value.ToString("yyyy-MM-dd") : DBNull.Value);
            cmd.Parameters.AddWithValue("$personalRenovo",
                dtpFechaRenovacion.Checked ? txtPersonalRenovo.Text.Trim() : DBNull.Value);
            cmd.Parameters.AddWithValue("$fechaEntrega", dtpFechaEntrega.Value.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$personalRecibio", txtPersonalRecibio.Text.Trim());
            cmd.Parameters.AddWithValue("$estado", cboEstado.SelectedItem?.ToString() ?? "Pendiente");
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        // ====================================================================
        //  DEVOLUCI\u00d3N
        // ====================================================================

        /// <summary>
        /// Marca el pr\u00e9stamo como "Entregado" y libera el ejemplar f\u00edsico (por Codigo).
        /// </summary>
        private void btnDevolver_Click(object sender, EventArgs e)
        {
            if (dgvPrestamos.CurrentRow == null || dgvPrestamos.CurrentRow.Cells["ID"].Value == null)
            {
                MessageBox.Show("Seleccione un pr\u00e9stamo de la lista.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (txtPersonalRecibio.Text.Trim().Length == 0)
            {
                Notificar("Escriba el nombre del personal que recibi\u00f3 el libro.", txtPersonalRecibio);
                return;
            }

            int id = Convert.ToInt32(dgvPrestamos.CurrentRow.Cells["ID"].Value);
            string? titulo = dgvPrestamos.CurrentRow.Cells["TituloLibro"].Value?.ToString();

            if (MessageBox.Show("\u00bfConfirmar la devoluci\u00f3n del pr\u00e9stamo seleccionado?",
                    "Registrar Devoluci\u00f3n", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                // Localizar el ejemplar f\u00edsico por t\u00edtulo + estado Prestado.
                string? codigoLibro = ObtenerCodigoPrestado(conexion, titulo ?? "");

                using var transaccion = conexion.BeginTransaction();
                try
                {
                    using (var actualizar = conexion.CreateCommand())
                    {
                        actualizar.Transaction = transaccion;
                        actualizar.CommandText = @"
                            UPDATE PrestamosExternos
                            SET EstadoLibro    = 'Entregado',
                                FechaEntrega   = $hoy,
                                PersonalRecibio = $personal
                            WHERE ID = $id;";
                        actualizar.Parameters.AddWithValue("$hoy", DateTime.Today.ToString("yyyy-MM-dd"));
                        actualizar.Parameters.AddWithValue("$personal", txtPersonalRecibio.Text.Trim());
                        actualizar.Parameters.AddWithValue("$id", id);
                        actualizar.ExecuteNonQuery();
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

                MessageBox.Show("Devoluci\u00f3n registrada. El libro vuelve a estar disponible.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarPrestamosActivos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar la devoluci\u00f3n: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ====================================================================
        //  MODIFICAR \u2014 carga el registro en el formulario
        // ====================================================================

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (dgvPrestamos.CurrentRow == null || dgvPrestamos.CurrentRow.Cells["ID"].Value == null)
            {
                MessageBox.Show("Seleccione un pr\u00e9stamo de la lista para modificar.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(dgvPrestamos.CurrentRow.Cells["ID"].Value);
            CargarPrestamoParaEditar(id);
        }

        /// <summary>
        /// Carga un pr\u00e9stamo existente en los controles del formulario.
        /// El ComboBox se selecciona por Text (t\u00edtulo) ya que es una lista simple.
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
                    MessageBox.Show("No se encontr\u00f3 el pr\u00e9stamo seleccionado.",
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

                if (DateTime.TryParse(fila["FechaPrestamo"]?.ToString(), out var fp))
                    dtpFechaPrestamo.Value = fp;

                txtPersonalPresto.Text = fila["PersonalPresto"]?.ToString() ?? "";

                string estado = fila["EstadoLibro"]?.ToString() ?? "Pendiente";
                if (cboEstado.Items.Contains(estado))
                    cboEstado.SelectedItem = estado;

                string fechaRenovacion = fila["FechaRenovacion"]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(fechaRenovacion) && DateTime.TryParse(fechaRenovacion, out var fr))
                {
                    dtpFechaRenovacion.Checked = true;
                    dtpFechaRenovacion.Value = fr;
                    txtPersonalRenovo.Text = fila["PersonalRenovo"]?.ToString() ?? "";
                }
                else
                {
                    dtpFechaRenovacion.Checked = false;
                    txtPersonalRenovo.Text = "";
                }

                if (DateTime.TryParse(fila["FechaEntrega"]?.ToString(), out var fe))
                    dtpFechaEntrega.Value = fe;

                txtPersonalRecibio.Text = fila["PersonalRecibio"]?.ToString() ?? "";

                // Seleccionar el t\u00edtulo en el ComboBox (por texto).
                txtTituloLibro.Text = tituloLibro;

                _prstamoEditandoId = id;
                btnRegistrar.Text = "Actualizar Pr\u00e9stamo";
                EstiloUI.EstilizarBotonPrimario(btnRegistrar);
                txtNombre.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el pr\u00e9stamo: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ====================================================================
        //  VALIDACI\u00d3N Y LIMPIEZA
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
                return Notificar("Seleccione o escriba el t\u00edtulo del libro.", txtTituloLibro);

            if (string.IsNullOrWhiteSpace(txtPersonalPresto.Text))
                return Notificar("Escriba el personal que realiza el pr\u00e9stamo.", txtPersonalPresto);

            // Fechas: los DateTimePicker siempre tienen una fecha v\u00e1lida, pero
            // reforzamos que el rango tenga coherencia.
            if (dtpFechaEntrega.Value.Date < dtpFechaPrestamo.Value.Date)
                return Notificar("La fecha de entrega no puede ser anterior a la de pr\u00e9stamo.", dtpFechaEntrega);

            return true;
        }

        private static bool Notificar(string mensaje, Control control)
        {
            MessageBox.Show(mensaje, "Validaci\u00f3n",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            control.Focus();
            return false;
        }

        private void LimpiarCampos()
        {
            foreach (var caja in new[] { txtNombre, txtCorreo, txtDui, txtTelefono,
                     txtDireccion, txtPersonalPresto, txtPersonalRecibio, txtPersonalRenovo })
            {
                caja.Clear();
            }
            txtTituloLibro.Text = string.Empty;
            cboEstado.SelectedIndex = 0;
            dtpFechaPrestamo.Value = DateTime.Today;
            dtpFechaEntrega.Value = DateTime.Today.AddDays(15);
            dtpFechaRenovacion.Value = DateTime.Today;
            dtpFechaRenovacion.Checked = false;
            LimpiarEstadoCodigo();
        }

        private void LimpiarParaNuevo()
        {
            LimpiarCampos();
            _prstamoEditandoId = null;
            btnRegistrar.Text = "Registrar Pr\u00e9stamo";
            EstiloUI.EstilizarBotonPrimario(btnRegistrar);
            txtNombre.Focus();
        }
    }
}
