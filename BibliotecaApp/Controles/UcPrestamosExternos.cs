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
                    WHERE EstadoLibro IN ('Pendiente', 'Renovado')
                    ORDER BY FechaEntrega ASC;";

                var tabla = new System.Data.DataTable();
                using (var lector = comando.ExecuteReader())
                {
                    tabla.Load(lector);
                }

                dgvPrestamos.DataSource = tabla;

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
                // Construir URI mailto: individual
                string subject = Uri.EscapeDataString("Préstamo registrado - Biblioteca CUBO");
                string body = Uri.EscapeDataString(
                    $"Estimado/a {nombreUsuario},\n\n" +
                    $"Muchas gracias por hacer uso de la Biblioteca CUBO. Le confirmamos que el libro '{tituloLibro}' ha sido registrado bajo su nombre.\n\n" +
                    $"Le recordamos amablemente que la fecha de entrega esperada es el {fechaEntrega}.\n\n" +
                    $"¡Disfrute su lectura!");

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