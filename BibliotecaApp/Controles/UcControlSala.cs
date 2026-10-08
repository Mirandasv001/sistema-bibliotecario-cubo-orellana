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

        // Variables de paginación (estilo minimalista igual que UcPrestamosExternos)
        private int paginaActual = 1;
        private int tamanoPagina = 11; // Definitivo (producción): llena el DataGridView dejando una fila de margen
        private int totalPaginas = 1;

        // Botones de paginación (creados dinámicamente para no tocar Designer)
        private Button? btnAnterior;
        private Button? btnSiguiente;
        private Label? lblPagina;

        // Espaciador invisible que empuja la paginación al extremo derecho del FlowLayoutPanel
        private Control? _espaciadorPaginacion;

        public UcControlSala()
        {
            InitializeComponent();

            // El ComboBox de títulos debe reflejar siempre la BD real: otros
            // módulos (p. ej. "Vaciar Inventario" en UcInventario) pueden borrar
            // libros mientras esta pantalla está oculta. Al volver a mostrarla se
            // fuerza la recarga para no conservar títulos en memoria obsoletos.
            this.VisibleChanged += UcControlSala_VisibleChanged;

            // Ajustar panelBotones para que crezca y acomode tanto botones de acción (arriba) como paginación (abajo)
            panelBotones.AutoSize = true;
            panelBotones.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelBotones.Dock = DockStyle.Top; // Mantener dock superior para que el SplitContainer lo respete

            // Crear FlowLayoutPanel para paginación (estilo minimalista igual que UcPrestamosExternos)
            // Se agrega al final de panelBotones para que quede DEBAJO de los botones de acción
            var flpPaginacion = new FlowLayoutPanel
            {
                Name = "flpPaginacion",
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                Padding = new Padding(14, 6, 14, 6),
                BackColor = Color.Transparent
            };
            panelBotones.Controls.Add(flpPaginacion);
            // NO hacer BringToFront() - los botones de acción (Top) deben quedar arriba

            // Crear botones de paginación dinámicamente
            CrearBotonesPaginacion(flpPaginacion);

            // Configurar color de selección del DataGridView (azul oscuro institucional igual que UcPrestamosExternos)
            dgvRegistros.DefaultCellStyle.SelectionBackColor = EstiloUI.Acento;
            dgvRegistros.DefaultCellStyle.SelectionForeColor = Color.White;
        }

        /// <summary>
        /// Recarga el ComboBox de títulos desde la base de datos real.
        /// Método público: puede invocarse desde otros controles cuando el
        /// inventario cambia.
        /// </summary>
        public void RecargarLibros()
        {
            CargarTitulosDeLibros();
        }

        /// <summary>
        /// Configura eventos adicionales del DataGridView (CellFormatting para colores).
        /// </summary>
        private void ConfigurarEventosGrid()
        {
            dgvRegistros.CellFormatting += DgvRegistros_CellFormatting;
        }

        // ====================================================================
        //  PAGINACIÓN (botones creados dinámicamente en code-behind)
        //  Estilo minimalista idéntico a UcPrestamosExternos
        // ====================================================================

        /// <summary>
        /// Crea los botones de paginación (Anterior / Siguiente / lblPagina)
        /// sin tocar el archivo Designer.cs. Se colocan en el FlowLayoutPanel proporcionado.
        /// </summary>
        private void CrearBotonesPaginacion(FlowLayoutPanel flpPaginacion)
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
            flpPaginacion.Controls.Add(_espaciadorPaginacion);

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
            flpPaginacion.Controls.Add(btnAnterior);
            flpPaginacion.Controls.Add(lblPagina);
            flpPaginacion.Controls.Add(btnSiguiente);

            // Reajustar el espaciador cuando cambie el ancho (resize)
            flpPaginacion.Resize += (_, _) => ActualizarEspaciadorPaginacion(flpPaginacion);
            ActualizarEspaciadorPaginacion(flpPaginacion);
        }

        /// <summary>
        /// Calcula el ancho sobrante del FlowLayoutPanel y se lo asigna al espaciador,
        /// de modo que la paginación quede pegada al borde derecho.
        /// </summary>
        private void ActualizarEspaciadorPaginacion(FlowLayoutPanel flpPaginacion)
        {
            if (_espaciadorPaginacion == null || !flpPaginacion.IsHandleCreated) return;

            int ocupado = 0;
            foreach (Control c in flpPaginacion.Controls)
            {
                if (c == _espaciadorPaginacion || !c.Visible) continue;
                ocupado += c.Width + c.Margin.Horizontal;
            }

            // 35 px de margen derecho: evita que la paginación choque contra el borde
            const int margenDerecho = 35;

            int disponible = flpPaginacion.ClientSize.Width - ocupado
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
                CargarRegistros();
            }
        }

        private void btnSiguiente_Click(object? sender, EventArgs e)
        {
            if (paginaActual < totalPaginas)
            {
                paginaActual++;
                CargarRegistros();
            }
        }

        /// <summary>
        /// Calcula totalPaginas según los registros de la BD.
        /// Nunca devuelve 0 páginas (mínimo 1).
        /// </summary>
        private void CalcularTotalPaginas()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = "SELECT COUNT(*) FROM ControlUsuariosSala;";

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

        private void UcControlSala_VisibleChanged(object? sender, EventArgs e)
        {
            // Solo al hacerse visible (VisibleChanged también dispara al ocultarse)
            if (this.Visible)
                RecargarLibros();
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
        /// Muestra los registros históricos de la tabla ControlUsuariosSala
        /// con paginación (LIMIT/OFFSET), para permitir gestión completa (incl. borrado).
        /// </summary>
        private void CargarRegistros()
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
                    ORDER BY ID DESC
                    LIMIT @tamanoPagina OFFSET @offset;";
                comando.Parameters.AddWithValue("@tamanoPagina", tamanoPagina);
                comando.Parameters.AddWithValue("@offset", offset);

                var tabla = new DataTable();
                using (var lector = comando.ExecuteReader())
                {
                    tabla.Load(lector);
                }

                dgvRegistros.DataSource = tabla;

                // --- UI de paginación (botones dinámicos) ---
                if (lblPagina != null) lblPagina.Text = $"Página {paginaActual} de {totalPaginas}";
                if (btnAnterior != null) btnAnterior.Enabled = (paginaActual > 1);
                if (btnSiguiente != null) btnSiguiente.Enabled = (paginaActual < totalPaginas);

                // El texto de lblPagina es AutoSize y cambia de ancho: reajustar espaciador
                // Buscar el FlowLayoutPanel que contiene los botones de paginación
                var flpPaginacion = panelBotones.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
                if (flpPaginacion != null)
                    ActualizarEspaciadorPaginacion(flpPaginacion);

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
        /// Estilo idéntico a UcPrestamosExternos: selección con versión oscura del color de fondo y texto blanco.
        /// </summary>
        private void DgvRegistros_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            // Solo formatear la columna Estado
            if (dgvRegistros.Columns[e.ColumnIndex].Name != "Estado") return;

            string? estado = e.Value?.ToString();
            if (string.IsNullOrEmpty(estado)) return;

            Color colorFondo;
            Color colorTexto;
            if (estado.Equals(EstadoPendiente, StringComparison.OrdinalIgnoreCase))
            {
                colorFondo = Color.MistyRose;
                colorTexto = Color.DarkRed;
            }
            else if (estado.Equals(EstadoEntregado, StringComparison.OrdinalIgnoreCase))
            {
                colorFondo = Color.Honeydew;
                colorTexto = Color.DarkGreen;
            }
            else
            {
                return; // Sin color para estados no reconocidos
            }

            e.CellStyle.BackColor = colorFondo;
            e.CellStyle.ForeColor = colorTexto;

            // Selección: versión oscura del color de fondo (igual que UcPrestamosExternos) con texto blanco
            e.CellStyle.SelectionBackColor = Color.FromArgb(
                Math.Max(0, colorFondo.R - 30),
                Math.Max(0, colorFondo.G - 30),
                Math.Max(0, colorFondo.B - 30));
            e.CellStyle.SelectionForeColor = Color.White;
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
        //  Selección: solo guarda el ID de la fila seleccionada y sale de modo edición si cambia de fila
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
            // NO se cargan los datos en los controles aquí; eso se hace al pulsar "Modificar"
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
        // Carga los datos de la fila seleccionada en los controles
        // ------------------------------------------------------------------
        private void btnModificar_Click(object sender, EventArgs e)
        {
            // Validar que hay una fila seleccionada
            if (dgvRegistros.CurrentRow == null || dgvRegistros.CurrentRow.Cells["ID"].Value == null)
            {
                MessageBox.Show("Por favor, seleccione un registro de la tabla para modificar.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Obtener la fila seleccionada
            var fila = dgvRegistros.CurrentRow;

            // Cargar los datos en los controles
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

            // Usar FormAutenticacion unificado (AdminCubo / Admin123$)
            using (var frmAuth = new FormAutenticacion("AdminCubo", "Admin123$"))
            {
                if (frmAuth.ShowDialog(this) != DialogResult.OK) return;
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
        /// W-06: Implementación SIN reflection — usa VScrollBar público del control.
        /// </summary>
        private void ConfigurarDragScroll()
        {
            dgvRegistros.MouseDown += DgvRegistros_MouseDown;
            dgvRegistros.MouseMove += DgvRegistros_MouseMove;
            dgvRegistros.MouseUp += DgvRegistros_MouseUp;
            dgvRegistros.MouseLeave += DgvRegistros_MouseLeave;
        }

        private VScrollBar? _vScrollBar;

        private VScrollBar? GetVScrollBar()
        {
            if (_vScrollBar != null) return _vScrollBar;
            _vScrollBar = dgvRegistros.Controls.OfType<VScrollBar>().FirstOrDefault();
            return _vScrollBar;
        }

        private void DgvRegistros_MouseDown(object? sender, MouseEventArgs e)
        {
            // Solo botón izquierdo
            if (e.Button != MouseButtons.Left) return;
            if (dgvRegistros.Rows.Count == 0) return;

            var vScroll = GetVScrollBar();
            if (vScroll == null || !vScroll.Visible) return;

            // SOLO iniciar drag-scroll si se hace click en:
            // - Fondo vacío (HitTestType.None)
            // - Encabezado de fila (HitTestType.RowHeader)
            // NO en celdas (para no interferir con selección)
            var hit = dgvRegistros.HitTest(e.X, e.Y);
            if (hit.Type != DataGridViewHitTestType.None && hit.Type != DataGridViewHitTestType.RowHeader)
                return;

            _dragScrollActivo = true;
            _dragScrollInicio = e.Location;
            _scrollOffsetInicial = vScroll.Value;
            dgvRegistros.Cursor = Cursors.SizeNS;
            dgvRegistros.Capture = true;
        }

        private void DgvRegistros_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_dragScrollActivo) return;

            var vScroll = GetVScrollBar();
            if (vScroll == null) return;

            // Scroll suave por píxeles usando el Value del VScrollBar público
            int deltaY = _dragScrollInicio.Y - e.Location.Y;
            int nuevoValor = _scrollOffsetInicial + deltaY;

            // Clampear al rango válido del scrollbar
            nuevoValor = Math.Clamp(nuevoValor, vScroll.Minimum, vScroll.Maximum - vScroll.LargeChange + 1);

            vScroll.Value = nuevoValor;
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

        /// <summary>
        /// Limpia suscripciones a eventos para evitar fugas de memoria
        /// (C-04: VisibleChanged suscrito en constructor sin desuscribir).
        /// Usamos HandleDestroyed en lugar de Dispose para no colisionar con el Designer.
        /// </summary>
        protected override void OnHandleDestroyed(EventArgs e)
        {
            this.VisibleChanged -= UcControlSala_VisibleChanged;
            base.OnHandleDestroyed(e);
        }
    }
}
