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

        public UcPrestamosExternos()
        {
            InitializeComponent();
        }

        private void UcPrestamosExternos_Load(object sender, EventArgs e)
        {
            AplicarPlaceholders();
            dtpFechaPrestamo.Value = DateTime.Today;
            dtpFechaEntrega.Value = DateTime.Today.AddDays(15);
            CargarLibrosDisponibles();
            CargarPrestamosActivos();
        }

        /// <summary>Método público invocado por Form1 al navegar a este apartado.</summary>
        public void Actualizar()
        {
            CargarLibrosDisponibles();
            CargarPrestamosActivos();
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
        //  CARGA DEL COMBOBOX — solo títulos distintos, con autocompletado
        // ====================================================================

        /// <summary>
        /// Llena el ComboBox con los títulos únicos de libros disponibles.
        /// Se usa una lista simple de strings (no DataSource) para que el
        /// autocompletado SuggestAppend funcione correctamente.
        /// </summary>
        private void CargarLibrosDisponibles()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    SELECT DISTINCT Titulo
                    FROM Libros
                    WHERE Disponibilidad = 'Disponible'
                    ORDER BY Titulo;";

                var titulos = new List<string>();
                using (var lector = comando.ExecuteReader())
                {
                    while (lector.Read())
                        titulos.Add(lector.GetString(0));
                }

                cboLibro.Items.Clear();
                if (titulos.Count > 0)
                    cboLibro.Items.AddRange(titulos.ToArray());

                cboLibro.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar los libros disponibles: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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
                comando.CommandText = @"
                    SELECT ID,
                           NombreUsuario                        AS Usuario,
                           Telefono,
                           TituloLibro                          AS [Título del Libro],
                           strftime('%d/%m/%Y', FechaPrestamo)  AS [Fecha Préstamo],
                           CASE WHEN IFNULL(FechaRenovacion,'') = '' THEN '-'
                                ELSE strftime('%d/%m/%Y', FechaRenovacion) END AS [Fecha Renovación],
                           strftime('%d/%m/%Y', FechaEntrega)   AS [Entrega Esperada],
                           PersonalPresto                       AS [Personal que Prestó],
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

                if (dgvPrestamos.Columns["ID"] != null)
                    dgvPrestamos.Columns["ID"]!.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los préstamos: " + ex.Message,
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
        //  REGISTRAR PRÉSTAMO
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
        /// Inserta un nuevo préstamo y marca como "Prestado" el primer ejemplar
        /// disponible del título seleccionado. El Codigo se resuelve en tiempo real.
        /// </summary>
        private void RegistrarPrestamo()
        {
            string titulo = cboLibro.Text.Trim();

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                // Obtener el primer Codigo disponible de ese título.
                string? codigo = ObtenerCodigoDisponible(conexion, titulo);
                if (codigo == null)
                {
                    MessageBox.Show(
                        $"No hay ejemplares disponibles del título \"{titulo}\".",
                        "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                    // Marcar SOLO el ejemplar único como Prestado.
                    using (var marcar = conexion.CreateCommand())
                    {
                        marcar.Transaction = transaccion;
                        marcar.CommandText = "UPDATE Libros SET Disponibilidad = 'Prestado' WHERE Codigo = $codigo;";
                        marcar.Parameters.AddWithValue("$codigo", codigo);
                        marcar.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw;
                }

                MessageBox.Show("Préstamo registrado correctamente.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarParaNuevo();
                CargarLibrosDisponibles();
                CargarPrestamosActivos();
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
            string tituloNuevo = cboLibro.Text.Trim();

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

                // Si cambió el título, necesitamos EjemplarViejo (Prestado) y EjemplarNuevo (Disponible).
                if (string.Equals(tituloViejo, tituloNuevo, StringComparison.OrdinalIgnoreCase))
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
                CargarLibrosDisponibles();
                CargarPrestamosActivos();
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
            cmd.Parameters.AddWithValue("$titulo", cboLibro.Text.Trim());
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
        //  DEVOLUCIÓN
        // ====================================================================

        /// <summary>
        /// Marca el préstamo como "Entregado" y libera el ejemplar físico (por Codigo).
        /// </summary>
        private void btnDevolver_Click(object sender, EventArgs e)
        {
            if (dgvPrestamos.CurrentRow == null || dgvPrestamos.CurrentRow.Cells["ID"].Value == null)
            {
                MessageBox.Show("Seleccione un préstamo de la lista.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (txtPersonalRecibio.Text.Trim().Length == 0)
            {
                Notificar("Escriba el nombre del personal que recibió el libro.", txtPersonalRecibio);
                return;
            }

            int id = Convert.ToInt32(dgvPrestamos.CurrentRow.Cells["ID"].Value);
            string? titulo = dgvPrestamos.CurrentRow.Cells["Título del Libro"].Value?.ToString();

            if (MessageBox.Show("¿Confirmar la devolución del préstamo seleccionado?",
                    "Registrar Devolución", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                // Localizar el ejemplar físico por título + estado Prestado.
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

                MessageBox.Show("Devolución registrada. El libro vuelve a estar disponible.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarLibrosDisponibles();
                CargarPrestamosActivos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar la devolución: " + ex.Message,
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

                // Seleccionar el título en el ComboBox (por texto).
                cboLibro.Text = tituloLibro;

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
        //  ELIMINAR
        // ====================================================================

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvPrestamos.CurrentRow == null || dgvPrestamos.CurrentRow.Cells["ID"].Value == null)
            {
                MessageBox.Show("Seleccione un préstamo de la lista para eliminar.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id = Convert.ToInt32(dgvPrestamos.CurrentRow.Cells["ID"].Value);
            string? titulo = dgvPrestamos.CurrentRow.Cells["Título del Libro"].Value?.ToString();

            if (MessageBox.Show("¿Está seguro de eliminar este préstamo permanentemente?",
                    "Eliminar Préstamo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                string? codigoLibro = ObtenerCodigoPrestado(conexion, titulo ?? "");

                using var transaccion = conexion.BeginTransaction();
                try
                {
                    using (var eliminar = conexion.CreateCommand())
                    {
                        eliminar.Transaction = transaccion;
                        eliminar.CommandText = "DELETE FROM PrestamosExternos WHERE ID = $id;";
                        eliminar.Parameters.AddWithValue("$id", id);
                        eliminar.ExecuteNonQuery();
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

                if (_prstamoEditandoId == id)
                    LimpiarParaNuevo();

                MessageBox.Show("Préstamo eliminado correctamente.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarLibrosDisponibles();
                CargarPrestamosActivos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el préstamo: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ====================================================================
        //  VALIDACIÓN Y LIMPIEZA
        // ====================================================================

        private bool ValidarFormulario()
        {
            if (txtNombre.Text.Trim().Length == 0)
                return Notificar("Escriba el nombre del usuario.", txtNombre);

            if (cboLibro.Text.Trim().Length == 0)
                return Notificar("Seleccione o escriba el título del libro.", cboLibro);

            if (txtPersonalPresto.Text.Trim().Length == 0)
                return Notificar("Escriba el personal que realiza el préstamo.", txtPersonalPresto);

            return true;
        }

        private static bool Notificar(string mensaje, Control control)
        {
            MessageBox.Show(mensaje, "Datos incompletos",
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
            cboLibro.Text = string.Empty;
            cboEstado.SelectedIndex = 0;
            dtpFechaPrestamo.Value = DateTime.Today;
            dtpFechaEntrega.Value = DateTime.Today.AddDays(15);
            dtpFechaRenovacion.Value = DateTime.Today;
            dtpFechaRenovacion.Checked = false;
        }

        private void LimpiarParaNuevo()
        {
            LimpiarCampos();
            _prstamoEditandoId = null;
            btnRegistrar.Text = "Registrar Préstamo";
            EstiloUI.EstilizarBotonPrimario(btnRegistrar);
            txtNombre.Focus();
        }
    }
}
