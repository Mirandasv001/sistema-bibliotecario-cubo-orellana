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
        private const string EstadoPendiente = "Pendiente";
        private const string EstadoEntregado = "Entregado";

        /// <summary>Indica si estamos en modo edición (true) o visualización (false).</summary>
        private bool modoEdicion = false;

        public UcControlSala()
        {
            InitializeComponent();
        }

        private void UcControlSala_Load(object sender, EventArgs e)
        {
            dtpFecha.Value = DateTime.Today;
            CargarTitulosDeLibros();
            CargarRegistros();
            ConfigurarEventosGrid();
            ConfigurarDragScroll(); // ← Habilita arrastre vertical con cursor
            // Los campos están habilitados por defecto para nueva inserción.
            // El modo edición solo se activa al pulsar "Editar" con una fila seleccionada.
            LimpiarCampos();
        }

        /// <summary>
        /// Configura eventos adicionales del DataGridView (CellFormatting para colores).
        /// </summary>
        private void ConfigurarEventosGrid()
        {
            dgvRegistros.CellFormatting += DgvRegistros_CellFormatting;
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
        /// Muestra TODOS los registros históricos de la tabla ControlUsuariosSala
        /// sin filtro de fecha ni estado, para permitir gestión completa (incl. borrado).
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
                           PersonalTurno               AS PersonalTurno,
                           Estado                      AS Estado
                    FROM ControlUsuariosSala
                    ORDER BY ID DESC;";

                var tabla = new DataTable();
                using (var lector = comando.ExecuteReader())
                {
                    tabla.Load(lector);
                }

                dgvRegistros.DataSource = tabla;

                // Añadir columna Estado programáticamente si no existe
                if (!dgvRegistros.Columns.Contains("Estado"))
                {
                    var colEstado = new DataGridViewTextBoxColumn
                    {
                        Name = "Estado",
                        DataPropertyName = "Estado",
                        HeaderText = "Estado",
                        MinimumWidth = 100,
                        FillWeight = 9F,
                        ReadOnly = true,
                        SortMode = DataGridViewColumnSortMode.Automatic
                    };
                    dgvRegistros.Columns.Add(colEstado);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los registros: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Evento CellFormatting: aplica colores a la columna Estado según su valor.
        /// </summary>
        private void DgvRegistros_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            // Solo formatear la columna Estado
            if (dgvRegistros.Columns[e.ColumnIndex].Name != "Estado") return;

            string? estado = e.Value?.ToString();
            if (string.IsNullOrEmpty(estado)) return;

            if (estado.Equals(EstadoPendiente, StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.MistyRose;
                e.CellStyle.ForeColor = Color.DarkRed;
                e.CellStyle.SelectionBackColor = Color.LightCoral;
                e.CellStyle.SelectionForeColor = Color.DarkRed;
            }
            else if (estado.Equals(EstadoEntregado, StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.Honeydew;
                e.CellStyle.ForeColor = Color.DarkGreen;
                e.CellStyle.SelectionBackColor = Color.LightGreen;
                e.CellStyle.SelectionForeColor = Color.DarkGreen;
            }
        }

        /// <summary>
        /// Alterna entre modo edición y modo solo lectura.
        /// </summary>
        /// <param name="modoEdicionActivo">True = habilitar edición (Guardar Cambios), False = solo lectura (Editar)</param>
        private void EstablecerModoEdicion(bool modoEdicionActivo)
        {
            modoEdicion = modoEdicionActivo;

            // Controles del formulario
            txtNombre.ReadOnly = !modoEdicionActivo;
            cboGenero.Enabled = modoEdicionActivo;
            numEdad.Enabled = modoEdicionActivo;
            dtpFecha.Enabled = modoEdicionActivo;
            cboLibro.Enabled = modoEdicionActivo;
            txtPersonal.ReadOnly = !modoEdicionActivo;

            // Botón Modificar/Guardar
            if (modoEdicionActivo)
            {
                btnModificar.Text = "Guardar Cambios";
                EstiloUI.EstilizarBotonPrimario(btnModificar); // Color primario para guardar
                btnModificar.Click -= btnModificar_Click;
                btnModificar.Click += btnGuardarCambios_Click;
            }
            else
            {
                btnModificar.Text = "Editar";
                EstiloUI.EstilizarBotonSecundario(btnModificar); // Color secundario para editar
                btnModificar.Click -= btnGuardarCambios_Click;
                btnModificar.Click += btnModificar_Click;
            }

            // Cuando se entra en modo edición, el foco va al primer campo editable
            if (modoEdicionActivo)
                txtNombre.Focus();
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

            int nuevoId = Convert.ToInt32(fila.Cells["ID"].Value);

            // Si se selecciona otra fila, salir del modo edición
            if (modoEdicion && nuevoId != idSeleccionado)
            {
                EstablecerModoEdicion(false);
            }

            idSeleccionado = nuevoId;

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
                         HoraEntrega, HoraRecibido, PersonalTurno, Estado)
                    VALUES
                        ($fecha, $nombre, $genero, $edad, $libro,
                         $horaEntrega, $horaRecibido, $personal, $estado);";

                comando.Parameters.AddWithValue("$fecha", dtpFecha.Value.ToString("yyyy-MM-dd"));
                comando.Parameters.AddWithValue("$nombre", txtNombre.Text.Trim());
                comando.Parameters.AddWithValue("$genero", cboGenero.SelectedItem?.ToString() ?? (object)DBNull.Value);
                comando.Parameters.AddWithValue("$edad", (int)numEdad.Value);
                comando.Parameters.AddWithValue("$libro", cboLibro.Text.Trim());
                comando.Parameters.AddWithValue("$horaEntrega", DateTime.Now.ToString("HH:mm:ss"));
                comando.Parameters.AddWithValue("$horaRecibido", EstadoEnLectura);
                comando.Parameters.AddWithValue("$personal", txtPersonal.Text.Trim());
                comando.Parameters.AddWithValue("$estado", EstadoPendiente);

                comando.ExecuteNonQuery();

                MessageBox.Show("Lectura registrada. El estado quedó como '" + EstadoPendiente + "'.",
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
                    SET HoraRecibido = $hora,
                        Estado = $estado
                    WHERE ID = $id;";
                comando.Parameters.AddWithValue("$hora", DateTime.Now.ToString("HH:mm:ss"));
                comando.Parameters.AddWithValue("$estado", EstadoEntregado);
                comando.Parameters.AddWithValue("$id", idSeleccionado);

                int afectados = comando.ExecuteNonQuery();
                if (afectados > 0)
                {
                    MessageBox.Show("Devolución registrada correctamente. Estado: " + EstadoEntregado,
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
        //  Editar: habilita los campos para edición (botón "Editar")
        // ------------------------------------------------------------------
        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (!HayFilaSeleccionada()) return;

            // Entrar en modo edición
            EstablecerModoEdicion(true);
        }

        // ------------------------------------------------------------------
        //  Guardar Cambios: persiste la edición en BD (botón "Guardar Cambios")
        // ------------------------------------------------------------------
        private void btnGuardarCambios_Click(object sender, EventArgs e)
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

                // Salir del modo edición y refrescar
                EstablecerModoEdicion(false);
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
        //  Eliminar: borra el registro con autenticación de administrador
        // ------------------------------------------------------------------
        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (!HayFilaSeleccionada()) return;

            // Crear diálogo de credenciales dinámicamente
            using var dlg = new Form
            {
                Text = "Autenticación requerida",
                Size = new Size(340, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = EstiloUI.FondoClaro
            };

            var lblUsuario = EstiloUI.CrearEtiqueta("Usuario:");
            lblUsuario.Location = new Point(20, 20);
            lblUsuario.AutoSize = true;

            var txtUsuario = new TextBox
            {
                Location = new Point(20, 50),
                Width = 280,
                Font = new Font(EstiloUI.FuenteBase, 10F)
            };
            EstiloUI.EstilizarEntrada(txtUsuario);

            var lblClave = EstiloUI.CrearEtiqueta("Contraseña:");
            lblClave.Location = new Point(20, 95);
            lblClave.AutoSize = true;

            var txtClave = new TextBox
            {
                Location = new Point(20, 125),
                Width = 280,
                Font = new Font(EstiloUI.FuenteBase, 10F),
                PasswordChar = '•'
            };
            EstiloUI.EstilizarEntrada(txtClave);

            var btnAceptar = new Button
            {
                Text = "Aceptar",
                Location = new Point(110, 185),
                Size = new Size(100, 35),
                DialogResult = DialogResult.OK
            };
            EstiloUI.EstilizarBotonPrimario(btnAceptar);

            dlg.Controls.AddRange(new Control[] { lblUsuario, txtUsuario, lblClave, txtClave, btnAceptar });
            dlg.AcceptButton = btnAceptar;

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            const string usuarioValido = "UserCubo";
            const string claveValida = "1234$";

            if (txtUsuario.Text != usuarioValido || txtClave.Text != claveValida)
            {
                MessageBox.Show("Credenciales incorrectas. Acción cancelada.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Credenciales correctas: ejecutar DELETE
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = "DELETE FROM ControlUsuariosSala WHERE ID = $id;";
                comando.Parameters.AddWithValue("$id", idSeleccionado);

                int afectados = comando.ExecuteNonQuery();
                if (afectados > 0)
                {
                    MessageBox.Show("Registro eliminado correctamente.",
                        "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarRegistros();
                    LimpiarCampos();
                }
                else
                {
                    MessageBox.Show("No se encontró el registro para eliminar.",
                        "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el registro: " + ex.Message,
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
            // Salir del modo edición si estaba activo
            if (modoEdicion)
                EstablecerModoEdicion(false);

            // Asegurar que los campos estén HABILITADOS para nuevo ingreso
            txtNombre.ReadOnly = false;
            cboGenero.Enabled = true;
            numEdad.Enabled = true;
            dtpFecha.Enabled = true;
            cboLibro.Enabled = true;
            txtPersonal.ReadOnly = false;

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

        // ═══════════════════════════════════════════════════════════════════
        // DRAG SCROLL — Arrastre vertical del DataGridView (estilo táctil)
        // ═══════════════════════════════════════════════════════════════════

        private bool _dragScrollActivo;
        private Point _dragScrollInicio;
        private int _scrollOffsetInicial;

        /// <summary>
        /// Suscribe los eventos de ratón al DataGridView para habilitar
        /// el desplazamiento por arrastre (drag scroll) con cambio de cursor.
        /// </summary>
        private void ConfigurarDragScroll()
        {
            dgvRegistros.MouseDown += DgvRegistros_MouseDown;
            dgvRegistros.MouseMove += DgvRegistros_MouseMove;
            dgvRegistros.MouseUp += DgvRegistros_MouseUp;
            dgvRegistros.MouseLeave += DgvRegistros_MouseLeave;
        }

        private void DgvRegistros_MouseDown(object? sender, MouseEventArgs e)
        {
            // Solo botón izquierdo
            if (e.Button != MouseButtons.Left) return;
            if (dgvRegistros.Rows.Count == 0) return;

            // Verificar que hay scroll vertical disponible
            var vScrollProp = dgvRegistros.GetType().GetProperty("VerticalScrollBar",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var scrollBar = vScrollProp?.GetValue(dgvRegistros) as ScrollBar;
            if (scrollBar == null || !scrollBar.Visible) return;

            // SOLO iniciar drag-scroll si se hace click en:
            // - Fondo vacío (HitTestType.None)
            // - Encabezado de fila (HitTestType.RowHeader)
            // NO en celdas (para no interferir con selección)
            var hit = dgvRegistros.HitTest(e.X, e.Y);
            if (hit.Type != DataGridViewHitTestType.None && hit.Type != DataGridViewHitTestType.RowHeader)
                return;

            _dragScrollActivo = true;
            _dragScrollInicio = e.Location;
            _scrollOffsetInicial = GetVerticalScrollOffset();
            dgvRegistros.Cursor = Cursors.SizeNS;
            dgvRegistros.Capture = true; // Capturar ratón para recibir eventos aunque salga del control
        }

        private void DgvRegistros_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_dragScrollActivo) return;

            // Scroll suave por píxeles (no por filas)
            int deltaY = _dragScrollInicio.Y - e.Location.Y;
            int nuevoOffset = _scrollOffsetInicial + deltaY;

            // Clampear al rango válido del scroll interno
            int maxOffset = GetMaxVerticalScrollOffset();
            nuevoOffset = Math.Clamp(nuevoOffset, 0, maxOffset);

            SetVerticalScrollOffset(nuevoOffset);
        }

        private void DgvRegistros_MouseUp(object? sender, MouseEventArgs e)
        {
            if (!_dragScrollActivo) return;

            _dragScrollActivo = false;
            dgvRegistros.Cursor = Cursors.Default;
            dgvRegistros.Capture = false;
        }

        private void DgvRegistros_MouseLeave(object? sender, EventArgs e)
        {
            if (_dragScrollActivo && !dgvRegistros.Capture)
            {
                _dragScrollActivo = false;
                dgvRegistros.Cursor = Cursors.Default;
            }
        }

        /// <summary>Obtiene el offset vertical actual en píxeles (propiedad interna).</summary>
        private int GetVerticalScrollOffset()
        {
            var prop = dgvRegistros.GetType().GetProperty("VerticalOffset",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (int)(prop?.GetValue(dgvRegistros) ?? 0);
        }

        /// <summary>Obtiene el offset vertical máximo en píxeles.</summary>
        private int GetMaxVerticalScrollOffset()
        {
            var prop = dgvRegistros.GetType().GetProperty("VerticalScrollBar",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var scrollBar = prop?.GetValue(dgvRegistros) as ScrollBar;
            if (scrollBar == null) return 0;
            return Math.Max(0, scrollBar.Maximum - scrollBar.LargeChange + 1);
        }

        /// <summary>Establece el offset vertical en píxeles (propiedad interna).</summary>
        private void SetVerticalScrollOffset(int offset)
        {
            var prop = dgvRegistros.GetType().GetProperty("VerticalOffset",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prop?.SetValue(dgvRegistros, offset);
            dgvRegistros.Invalidate(); // Forzar repintado
        }
    }
}
