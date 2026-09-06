using System.Data;
using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    /// <summary>
    /// Apartado A: control de lectura en sala con flujo Check-in / Check-out y
    /// CRUD completo sobre la tabla ControlUsuariosSala.
    /// </summary>
    public partial class UcControlSala : UserControl
    {
        /// <summary>ID de la fila seleccionada en el grid (0 = ninguna).</summary>
        private int idSeleccionado;

        private const string EstadoEnLectura = "En lectura";

        public UcControlSala()
        {
            InitializeComponent();
        }

        private void UcControlSala_Load(object sender, EventArgs e)
        {
            dtpFecha.Value = DateTime.Today;
            CargarTitulosDeLibros();
            CargarRegistros();
        }

        // ------------------------------------------------------------------
        //  Carga de datos
        // ------------------------------------------------------------------
        private void CargarTitulosDeLibros()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = "SELECT Titulo FROM Libros ORDER BY Titulo;";

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
                MessageBox.Show("No se pudieron cargar los títulos: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Muestra los registros del día actual más los que sigan 'En lectura'
        /// de días anteriores (para poder marcar su devolución).
        /// </summary>
        private void CargarRegistros()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    SELECT ID,
                           strftime('%d/%m/%Y', Fecha) AS Fecha,
                           NombreUsuario               AS Usuario,
                           Genero                      AS Género,
                           Edad                        AS Edad,
                           TituloLibro                 AS TituloLibro,
                           HoraEntrega                 AS HoraEntrega,
                           HoraRecibido                AS HoraRecibido,
                           PersonalTurno               AS PersonalTurno
                    FROM ControlUsuariosSala
                    WHERE Fecha = $hoy OR HoraRecibido = $enLectura
                    ORDER BY ID DESC;";
                comando.Parameters.AddWithValue("$hoy", DateTime.Today.ToString("yyyy-MM-dd"));
                comando.Parameters.AddWithValue("$enLectura", EstadoEnLectura);

                var tabla = new DataTable();
                using (var lector = comando.ExecuteReader())
                {
                    tabla.Load(lector);
                }

                dgvRegistros.DataSource = tabla;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los registros: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------------
        //  Selección: puebla los controles con la fila tocada
        // ------------------------------------------------------------------
        private void dgvRegistros_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvRegistros.Rows.Count)
                return;

            var fila = dgvRegistros.Rows[e.RowIndex];
            if (fila.Cells["ID"].Value == null)
                return;

            idSeleccionado = Convert.ToInt32(fila.Cells["ID"].Value);

            txtNombre.Text = fila.Cells["Usuario"].Value?.ToString() ?? string.Empty;

            string genero = fila.Cells["Género"].Value?.ToString() ?? string.Empty;
            cboGenero.SelectedItem = cboGenero.Items.Contains(genero) ? genero : null;

            if (int.TryParse(fila.Cells["Edad"].Value?.ToString(), out int edad)
                && edad >= numEdad.Minimum && edad <= numEdad.Maximum)
            {
                numEdad.Value = edad;
            }

            if (DateTime.TryParseExact(fila.Cells["Fecha"].Value?.ToString(),
                    "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None,
                    out DateTime fecha))
            {
                dtpFecha.Value = fecha;
            }

            cboLibro.Text = fila.Cells["TituloLibro"].Value?.ToString() ?? string.Empty;
            txtPersonal.Text = fila.Cells["PersonalTurno"].Value?.ToString() ?? string.Empty;
        }

        // ------------------------------------------------------------------
        //  Check-in: registrar entrada a leer
        // ------------------------------------------------------------------
        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    INSERT INTO ControlUsuariosSala
                        (Fecha, NombreUsuario, Genero, Edad, TituloLibro,
                         HoraEntrega, HoraRecibido, PersonalTurno)
                    VALUES
                        ($fecha, $nombre, $genero, $edad, $libro,
                         $horaEntrega, $horaRecibido, $personal);";

                comando.Parameters.AddWithValue("$fecha", dtpFecha.Value.ToString("yyyy-MM-dd"));
                comando.Parameters.AddWithValue("$nombre", txtNombre.Text.Trim());
                comando.Parameters.AddWithValue("$genero", cboGenero.SelectedItem?.ToString() ?? (object)DBNull.Value);
                comando.Parameters.AddWithValue("$edad", (int)numEdad.Value);
                comando.Parameters.AddWithValue("$libro", cboLibro.Text.Trim());
                comando.Parameters.AddWithValue("$horaEntrega", DateTime.Now.ToString("HH:mm:ss"));
                comando.Parameters.AddWithValue("$horaRecibido", EstadoEnLectura);
                comando.Parameters.AddWithValue("$personal", txtPersonal.Text.Trim());

                comando.ExecuteNonQuery();

                MessageBox.Show("Lectura registrada. El libro quedó como '" + EstadoEnLectura + "'.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarCampos();
                CargarRegistros();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el registro: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------------
        //  Check-out: marcar devolución del libro
        // ------------------------------------------------------------------
        private void btnMarcarDevolucion_Click(object sender, EventArgs e)
        {
            if (!HayFilaSeleccionada()) return;

            string estadoActual = dgvRegistros.CurrentRow!
                .Cells["HoraRecibido"].Value?.ToString() ?? string.Empty;

            if (!estadoActual.Equals(EstadoEnLectura, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("El registro seleccionado ya tiene su devolución registrada.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    UPDATE ControlUsuariosSala
                    SET HoraRecibido = $hora
                    WHERE ID = $id;";
                comando.Parameters.AddWithValue("$hora", DateTime.Now.ToString("HH:mm:ss"));
                comando.Parameters.AddWithValue("$id", idSeleccionado);

                int afectados = comando.ExecuteNonQuery();
                if (afectados > 0)
                {
                    MessageBox.Show("Devolución registrada correctamente.",
                        "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarRegistros();
                    LimpiarCampos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al marcar la devolución: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------------
        //  Modificar (UPDATE)
        // ------------------------------------------------------------------
        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (!HayFilaSeleccionada()) return;
            if (!ValidarFormulario()) return;

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    UPDATE ControlUsuariosSala
                    SET Fecha         = $fecha,
                        NombreUsuario = $nombre,
                        Genero        = $genero,
                        Edad          = $edad,
                        TituloLibro   = $libro,
                        PersonalTurno = $personal
                    WHERE ID = $id;";

                comando.Parameters.AddWithValue("$fecha", dtpFecha.Value.ToString("yyyy-MM-dd"));
                comando.Parameters.AddWithValue("$nombre", txtNombre.Text.Trim());
                comando.Parameters.AddWithValue("$genero", cboGenero.SelectedItem?.ToString() ?? (object)DBNull.Value);
                comando.Parameters.AddWithValue("$edad", (int)numEdad.Value);
                comando.Parameters.AddWithValue("$libro", cboLibro.Text.Trim());
                comando.Parameters.AddWithValue("$personal", txtPersonal.Text.Trim());
                comando.Parameters.AddWithValue("$id", idSeleccionado);

                comando.ExecuteNonQuery();

                MessageBox.Show("Registro modificado correctamente.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimpiarCampos();
                CargarRegistros();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al modificar el registro: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------------
        //  Validación y limpieza
        // ------------------------------------------------------------------
        private bool HayFilaSeleccionada()
        {
            if (idSeleccionado > 0 && dgvRegistros.CurrentRow != null)
                return true;

            MessageBox.Show("Seleccione un registro de la tabla.",
                "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        private bool ValidarFormulario()
        {
            if (txtNombre.Text.Trim().Length == 0)
                return Notificar("Escriba el nombre del usuario.", txtNombre);

            if (cboGenero.SelectedIndex < 0)
                return Notificar("Seleccione el género del usuario.", cboGenero);

            if (cboLibro.Text.Trim().Length == 0)
                return Notificar("Seleccione o escriba el título del libro consultado.", cboLibro);

            if (txtPersonal.Text.Trim().Length == 0)
                return Notificar("Escriba el personal en turno.", txtPersonal);

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
            idSeleccionado = 0;
            dgvRegistros.ClearSelection();
            txtNombre.Clear();
            cboGenero.SelectedIndex = -1;
            numEdad.Value = 12;
            cboLibro.SelectedIndex = -1;
            cboLibro.Text = string.Empty;
            txtPersonal.Clear();
            dtpFecha.Value = DateTime.Today;
            txtNombre.Focus();
        }
    }
}
