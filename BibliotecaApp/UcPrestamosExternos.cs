using System.Data;
using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    /// <summary>
    /// Apartado B: préstamos de libros para llevar a casa, renovaciones y devoluciones.
    /// </summary>
    public partial class UcPrestamosExternos : UserControl
    {
        public UcPrestamosExternos()
        {
            InitializeComponent();
        }

        private void UcPrestamosExternos_Load(object sender, EventArgs e)
        {
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
        //  Carga de datos
        // ------------------------------------------------------------------
        private void CargarLibrosDisponibles()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    SELECT Titulo FROM Libros
                    WHERE Disponibilidad = 'Disponible'
                    ORDER BY Titulo;";

                object? guardado = cboLibro.SelectedItem;
                cboLibro.Items.Clear();

                using var lector = comando.ExecuteReader();
                while (lector.Read())
                {
                    string titulo = lector.GetString(0);
                    if (titulo.Length > 0) cboLibro.Items.Add(titulo);
                }

                if (guardado != null && cboLibro.Items.Contains(guardado))
                    cboLibro.SelectedItem = guardado;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar los libros disponibles: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

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

                var tabla = new DataTable();
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

        /// <summary>Resalta en rojo los préstamos cuya fecha de entrega ya venció.</summary>
        private void dgvPrestamos_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var fila = dgvPrestamos.Rows[e.RowIndex];
            if (fila.Cells["Entrega Esperada"].Value is not string fechaTexto ||
                !DateTime.TryParseExact(fechaTexto, "dd/MM/yyyy",
                    null, System.Globalization.DateTimeStyles.None, out DateTime fechaEntrega))
            {
                return;
            }

            if (fechaEntrega.Date < DateTime.Today)
            {
                fila.DefaultCellStyle.BackColor = EstiloUI.AlertaRojo;
                fila.DefaultCellStyle.SelectionBackColor = EstiloUI.Acento;
            }
        }

        // ------------------------------------------------------------------
        //  Registro de préstamo
        // ------------------------------------------------------------------
        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            string titulo = cboLibro.Text.Trim();

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                // Verificar que el libro exista y esté disponible.
                using (var verificar = conexion.CreateCommand())
                {
                    verificar.CommandText =
                        "SELECT Disponibilidad FROM Libros WHERE Titulo = $titulo LIMIT 1;";
                    verificar.Parameters.AddWithValue("$titulo", titulo);

                    object? resultado = verificar.ExecuteScalar();
                    if (resultado == null)
                    {
                        MessageBox.Show("El libro indicado no existe en el inventario.",
                            "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (!string.Equals(resultado.ToString(), "Disponible", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("El libro seleccionado no está disponible actualmente.",
                            "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Insertar préstamo y marcar libro como prestado en una sola transacción.
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
                                ($nombre, $correo, $dui, $telefono, $direccion, $libro,
                                 $fechaPrestamo, $personalPresto, $fechaRenovacion, $personalRenovo,
                                 $fechaEntrega, $personalRecibio, $estado);";

                        insertar.Parameters.AddWithValue("$nombre", txtNombre.Text.Trim());
                        insertar.Parameters.AddWithValue("$correo", txtCorreo.Text.Trim());
                        insertar.Parameters.AddWithValue("$dui", txtDui.Text.Trim());
                        insertar.Parameters.AddWithValue("$telefono", txtTelefono.Text.Trim());
                        insertar.Parameters.AddWithValue("$direccion", txtDireccion.Text.Trim());
                        insertar.Parameters.AddWithValue("$libro", titulo);
                        insertar.Parameters.AddWithValue("$fechaPrestamo", dtpFechaPrestamo.Value.ToString("yyyy-MM-dd"));
                        insertar.Parameters.AddWithValue("$personalPresto", txtPersonalPresto.Text.Trim());
                        insertar.Parameters.AddWithValue("$fechaRenovacion",
                            dtpFechaRenovacion.Checked ? dtpFechaRenovacion.Value.ToString("yyyy-MM-dd") : DBNull.Value);
                        insertar.Parameters.AddWithValue("$personalRenovo",
                            dtpFechaRenovacion.Checked ? txtPersonalRenovo.Text.Trim() : DBNull.Value);
                        insertar.Parameters.AddWithValue("$fechaEntrega", dtpFechaEntrega.Value.ToString("yyyy-MM-dd"));
                        insertar.Parameters.AddWithValue("$personalRecibio", txtPersonalRecibio.Text.Trim());
                        insertar.Parameters.AddWithValue("$estado", cboEstado.SelectedItem?.ToString() ?? "Pendiente");
                        insertar.ExecuteNonQuery();
                    }

                    using (var marcar = conexion.CreateCommand())
                    {
                        marcar.Transaction = transaccion;
                        marcar.CommandText = @"
                            UPDATE Libros SET Disponibilidad = 'Prestado'
                            WHERE Titulo = $titulo AND Disponibilidad <> 'Prestado';";
                        marcar.Parameters.AddWithValue("$titulo", titulo);
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
                LimpiarCampos();
                CargarLibrosDisponibles();
                CargarPrestamosActivos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar el préstamo: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------------
        //  Devolución
        // ------------------------------------------------------------------
        private void btnDevolver_Click(object sender, EventArgs e)
        {
            if (dgvPrestamos.CurrentRow == null || dgvPrestamos.CurrentRow.Cells["ID"].Value == null)
            {
                MessageBox.Show("Seleccione un préstamo de la lista.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                using var transaccion = conexion.BeginTransaction();
                try
                {
                    using (var actualizar = conexion.CreateCommand())
                    {
                        actualizar.Transaction = transaccion;
                        actualizar.CommandText = @"
                            UPDATE PrestamosExternos
                            SET EstadoLibro   = 'Entregado',
                                FechaEntrega  = $hoy,
                                PersonalRecibio = CASE WHEN $personal = '' THEN PersonalRecibio ELSE $personal END
                            WHERE ID = $id;";
                        actualizar.Parameters.AddWithValue("$hoy", DateTime.Today.ToString("yyyy-MM-dd"));
                        actualizar.Parameters.AddWithValue("$personal", txtPersonalRecibio.Text.Trim());
                        actualizar.Parameters.AddWithValue("$id", id);
                        actualizar.ExecuteNonQuery();
                    }

                    using (var liberar = conexion.CreateCommand())
                    {
                        liberar.Transaction = transaccion;
                        liberar.CommandText = @"
                            UPDATE Libros SET Disponibilidad = 'Disponible'
                            WHERE Titulo = $titulo;";
                        liberar.Parameters.AddWithValue("$titulo", titulo ?? string.Empty);
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

        // ------------------------------------------------------------------
        //  Validación y limpieza
        // ------------------------------------------------------------------
        private bool ValidarFormulario()
        {
            if (txtNombre.Text.Trim().Length == 0)
                return Notificar("Escriba el nombre del usuario.", txtNombre);

            if (cboLibro.Text.Trim().Length == 0)
                return Notificar("Seleccione el título del libro a prestar.", cboLibro);

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

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            foreach (var caja in new[] { txtNombre, txtCorreo, txtDui, txtTelefono,
                     txtDireccion, txtPersonalPresto, txtPersonalRecibio, txtPersonalRenovo })
            {
                caja.Clear();
            }
            cboLibro.SelectedIndex = -1;
            cboLibro.Text = string.Empty;
            cboEstado.SelectedIndex = 0;
            dtpFechaPrestamo.Value = DateTime.Today;
            dtpFechaEntrega.Value = DateTime.Today.AddDays(15);
            dtpFechaRenovacion.Value = DateTime.Today;
            dtpFechaRenovacion.Checked = false;
            txtNombre.Focus();
        }
    }
}
