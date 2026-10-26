using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    public partial class UcControlSala : UserControl
    {
        private int idSeleccionado;

        private const string EstadoEnLectura = "En lectura";
        private const string EstadoPendiente = "Pendiente";
        private const string EstadoEntregado = "Entregado";

        private bool modoEdicion = false;

        private int paginaActual = 1;
        private int tamanoPagina = 11; 
        private int totalPaginas = 1;

        private Button? btnAnterior;
        private Button? btnSiguiente;
        private Label? lblPagina;

        private Button? btnEliminarPagina;

        // Espaciador invisible que empuja la paginación al extremo derecho del FlowLayoutPanel
        private Control? _espaciadorPaginacion;

        public UcControlSala()
        {
            InitializeComponent();

            cboGenero.Items.Clear();
            cboGenero.Items.Add("Masculino");
            cboGenero.Items.Add("Femenino");
            cboGenero.SelectedIndex = -1;

            this.VisibleChanged += UcControlSala_VisibleChanged;

            panelBotones.AutoSize = true;
            panelBotones.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelBotones.Dock = DockStyle.Top; 
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
            btnEliminarPagina = new Button
            {
                Name = "btnEliminarPagina",
                Text = "Eliminar página",
                AutoSize = false,
                Location = new Point(btnEliminar.Right + 6, btnEliminar.Top),
                Size = new Size(155, btnEliminar.Height),  
                Margin = new Padding(3, 3, 3, 3)  
            };
            EstiloUI.EstilizarBotonSecundario(btnEliminarPagina);
            btnEliminarPagina.Click += BtnEliminarPagina_Click;

            // Visibilidad por rol: ambos borrados solo para Administradores
            btnEliminar.Visible = SesionGlobal.EsAdmin;
            btnEliminarPagina.Visible = SesionGlobal.EsAdmin;

            // Mismo contenedor padre que los botones principales.
            btnEliminar.Parent.Controls.Add(btnEliminarPagina);
            btnEliminar.Parent.Controls.SetChildIndex(btnEliminarPagina, btnEliminar.Parent.Controls.GetChildIndex(btnEliminar) + 1);

            // Crear botones de paginación dinámicamente
            CrearBotonesPaginacion(flpPaginacion);

            dgvRegistros.DefaultCellStyle.SelectionBackColor = EstiloUI.Acento;
            dgvRegistros.DefaultCellStyle.SelectionForeColor = Color.White;

            dgvRegistros.RowHeadersVisible = false;

            dgvRegistros.SizeChanged += DgvRegistros_SizeChanged;
        }

        public void RecargarLibros()
        {
            CargarTitulosDeLibros();
        }

        private void ConfigurarEventosGrid()
        {
            dgvRegistros.CellFormatting += DgvRegistros_CellFormatting;
        }

        private void CrearBotonesPaginacion(FlowLayoutPanel flpPaginacion)
        {
            _espaciadorPaginacion = new Label
            {
                Name = "espaciadorPaginacion",
                Text = string.Empty,
                AutoSize = false,
                Size = new Size(0, 1),
                Margin = new Padding(0)
            };
            flpPaginacion.Controls.Add(_espaciadorPaginacion);

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

            flpPaginacion.Controls.Add(btnAnterior);
            flpPaginacion.Controls.Add(lblPagina);
            flpPaginacion.Controls.Add(btnSiguiente);

            flpPaginacion.Resize += (_, _) => ActualizarEspaciadorPaginacion(flpPaginacion);
            ActualizarEspaciadorPaginacion(flpPaginacion);
        }

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

        private void CalcularTotalPaginas()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = "SELECT COUNT(*) FROM ControlUsuariosSala;";

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
            int altoFila = dgvRegistros.RowTemplate.Height > 0
                ? dgvRegistros.RowTemplate.Height
                : 20; 

            int disponibles = dgvRegistros.ClientSize.Height - dgvRegistros.ColumnHeadersHeight;
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

        private void DgvRegistros_SizeChanged(object? sender, EventArgs e)
        {
            if (IsDisposed || Disposing) return;

            int anterior = tamanoPagina;
            RecalcularTamanoPagina();
            if (tamanoPagina == anterior) return; // sin cambio de capacidad: nada que recargar

            CargarRegistros(); // recalcula totalPaginas y acomoda paginaActual
        }

        private void UcControlSala_VisibleChanged(object? sender, EventArgs e)
        {
            if (this.Visible)
                RecargarLibros();
        }

        private void UcControlSala_Load(object sender, EventArgs e)
        {
            dtpFecha.Value = DateTime.Today;
            CargarTitulosDeLibros();
            RecalcularTamanoPagina();
            CargarRegistros();
            ConfigurarEventosGrid();
            ConfigurarDragScroll(); 
            LimpiarCampos();

            // Se reevalúa al cargar: el rol puede haber cambiado entre sesiones
            bool esAdmin = SesionGlobal.EsAdmin;
            btnEliminar.Visible = esAdmin;
            if (btnEliminarPagina != null) btnEliminarPagina.Visible = esAdmin;
        }

        private void CargarTitulosDeLibros()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = "SELECT Titulo FROM Libros WHERE Disponibilidad = 'Disponible' ORDER BY Titulo;";

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

        private void CargarRegistros()
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

                if (lblPagina != null) lblPagina.Text = $"Página {paginaActual} de {totalPaginas}";
                if (btnAnterior != null) btnAnterior.Enabled = (paginaActual > 1);
                if (btnSiguiente != null) btnSiguiente.Enabled = (paginaActual < totalPaginas);

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

        private void BtnEliminarPagina_Click(object? sender, EventArgs e)
        {
            if (!SesionGlobal.EsAdmin)
            {
                MessageBox.Show("Solo un Administrador puede eliminar registros.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Borrado masivo: exige contraseña de Administrador
            if (!FormAutenticacion.SolicitarAdmin(this)) return;

            var idsVisibles = new List<int>();
            foreach (DataGridViewRow fila in dgvRegistros.Rows)
            {
                if (fila.IsNewRow) continue;
                object? valor = fila.Cells["ID"].Value;
                if (valor != null && int.TryParse(valor.ToString(), out int id))
                    idsVisibles.Add(id);
            }

            if (idsVisibles.Count == 0)
            {
                MessageBox.Show("No hay registros visibles para eliminar.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirmar = MessageBox.Show(
                $"¿Eliminar los {idsVisibles.Count} registro(s) de la página actual?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirmar != DialogResult.Yes) return;

            try
            {
                int eliminados = 0;

                using (var conexion = ConexionDB.ObtenerConexion())
                using (var transaccion = conexion.BeginTransaction())
                {
                    string parametros = string.Join(",", idsVisibles.Select((_, i) => $"@id{i}"));

                    // Regla de borrado: nada se borra si algún ejemplar sigue en lectura
                    using (var validar = conexion.CreateCommand())
                    {
                        validar.Transaction = transaccion;
                        validar.CommandText = $@"
                            SELECT COUNT(*) FROM ControlUsuariosSala
                            WHERE ID IN ({parametros})
                              AND HoraRecibido = 'En lectura';";

                        for (int i = 0; i < idsVisibles.Count; i++)
                            validar.Parameters.AddWithValue($"@id{i}", idsVisibles[i]);

                        long enLectura = Convert.ToInt64(validar.ExecuteScalar());
                        if (enLectura > 0)
                        {
                            transaccion.Rollback();
                            MessageBox.Show(
                                $"No se puede eliminar la página: {enLectura} registro(s) tienen su ejemplar en lectura.\n" +
                                "Registre primero las devoluciones.",
                                "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Libera por código los ejemplares de los registros que se van a borrar
                    using (var codigos = conexion.CreateCommand())
                    {
                        codigos.Transaction = transaccion;
                        codigos.CommandText =
                            $"SELECT ID, IFNULL(CodigoLibro, ''), IFNULL(TituloLibro, '') " +
                            $"FROM ControlUsuariosSala WHERE ID IN ({parametros});";

                        for (int i = 0; i < idsVisibles.Count; i++)
                            codigos.Parameters.AddWithValue($"@id{i}", idsVisibles[i]);

                        var pendientes = new List<(int Id, string Titulo)>();
                        using (var lector = codigos.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                string codigo = lector.GetString(1).Trim();
                                if (codigo.Length > 0)
                                    LiberarPorCodigo(conexion, transaccion, codigo);
                                else
                                    pendientes.Add((lector.GetInt32(0), lector.GetString(2)));
                            }
                        }

                        // Registros antiguos sin código: solo se libera si el título es único
                        foreach (var (id, titulo) in pendientes)
                        {
                            string codigo = ResolverCodigoUnicoPorTitulo(conexion, transaccion, titulo);
                            if (codigo.Length > 0)
                                LiberarPorCodigo(conexion, transaccion, codigo);
                        }
                    }

                    using var comando = conexion.CreateCommand();
                    comando.Transaction = transaccion;
                    comando.CommandText =
                        $"DELETE FROM ControlUsuariosSala WHERE ID IN ({parametros}) AND HoraRecibido <> 'En lectura';";

                    for (int i = 0; i < idsVisibles.Count; i++)
                        comando.Parameters.AddWithValue($"@id{i}", idsVisibles[i]);

                    eliminados = comando.ExecuteNonQuery();
                    transaccion.Commit();
                }

                CargarRegistros();

                MessageBox.Show($"Se eliminaron {eliminados} registro(s).",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar la página: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvRegistros_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            e.CellStyle.SelectionBackColor = dgvRegistros.DefaultCellStyle.SelectionBackColor;
            e.CellStyle.SelectionForeColor = Color.White;

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
        }

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
        }

        private static void LiberarEjemplarEnSala(SqliteConnection conexion, SqliteTransaction transaccion, string codigoLibro)
        {
            if (string.IsNullOrWhiteSpace(codigoLibro)) return;

            using var cmd = conexion.CreateCommand();
            cmd.Transaction = transaccion;
            cmd.CommandText = @"
                UPDATE Libros SET Disponibilidad = 'Disponible'
                WHERE Codigo = @codigo
                  AND Disponibilidad <> 'Disponible'
                  AND NOT EXISTS (
                      SELECT 1 FROM ControlUsuariosSala
                      WHERE CodigoLibro = @codigo AND HoraRecibido = 'En lectura')
                  AND NOT EXISTS (
                      SELECT 1 FROM PrestamosExternos
                      WHERE CodigoLibro = @codigo AND EstadoLibro IN ('Pendiente', 'Renovado'));";
            cmd.Parameters.AddWithValue("@codigo", codigoLibro);
            cmd.ExecuteNonQuery();
        }

        private static void LiberarPorCodigo(SqliteConnection conexion, SqliteTransaction transaccion, string codigoLibro)
        {
            using var cmd = conexion.CreateCommand();
            cmd.Transaction = transaccion;
            cmd.CommandText = @"
                UPDATE Libros SET Disponibilidad = 'Disponible'
                WHERE Codigo = @codigo
                  AND Disponibilidad <> 'Disponible'
                  AND NOT EXISTS (
                      SELECT 1 FROM ControlUsuariosSala
                      WHERE CodigoLibro = @codigo AND HoraRecibido = 'En lectura')
                  AND NOT EXISTS (
                      SELECT 1 FROM PrestamosExternos
                      WHERE CodigoLibro = @codigo AND EstadoLibro IN ('Pendiente', 'Renovado'));";
            cmd.Parameters.AddWithValue("@codigo", codigoLibro);
            cmd.ExecuteNonQuery();
        }

        private static string ResolverCodigoUnicoPorTitulo(SqliteConnection conexion, SqliteTransaction transaccion, string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo)) return string.Empty;

            using var cmd = conexion.CreateCommand();
            cmd.Transaction = transaccion;
            cmd.CommandText = "SELECT Codigo FROM Libros WHERE Titulo = $titulo;";
            cmd.Parameters.AddWithValue("$titulo", titulo);

            string codigo = string.Empty;
            using var lector = cmd.ExecuteReader();
            while (lector.Read())
            {
                string candidato = lector.GetString(0);
                if (codigo.Length > 0 && !string.Equals(codigo, candidato, StringComparison.OrdinalIgnoreCase))
                    return string.Empty; // título ambiguo: no se identifica al azar
                codigo = candidato;
            }

            return codigo;
        }

        // Devuelve el código del ejemplar; si el registro antiguo no lo tiene,
        // usa el título solo cuando identifica UN ejemplar (sin LIMIT 1).
        private static string ResolverCodigoDelRegistro(SqliteConnection conexion, SqliteTransaction transaccion, int id)
        {
            using var cmd = conexion.CreateCommand();
            cmd.Transaction = transaccion;
            cmd.CommandText =
                "SELECT IFNULL(CodigoLibro, ''), IFNULL(TituloLibro, '') FROM ControlUsuariosSala WHERE ID = $id;";
            cmd.Parameters.AddWithValue("$id", id);

            string codigo = string.Empty;
            string titulo = string.Empty;
            using (var lector = cmd.ExecuteReader())
            {
                if (lector.Read())
                {
                    codigo = lector.GetString(0).Trim();
                    titulo = lector.GetString(1);
                }
            }

            if (codigo.Length > 0 || titulo.Length == 0) return codigo;

            using var cmdTitulo = conexion.CreateCommand();
            cmdTitulo.Transaction = transaccion;
            cmdTitulo.CommandText = "SELECT Codigo FROM Libros WHERE Titulo = $titulo;";
            cmdTitulo.Parameters.AddWithValue("$titulo", titulo);

            using var lectorTitulos = cmdTitulo.ExecuteReader();
            while (lectorTitulos.Read())
            {
                string candidato = lectorTitulos.GetString(0);
                if (codigo.Length > 0 && !string.Equals(codigo, candidato, StringComparison.OrdinalIgnoreCase))
                    return string.Empty; // título ambiguo: no se identifica al azar
                codigo = candidato;
            }

            return codigo;
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            string titulo = cboLibro.Text.Trim();

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var transaccion = conexion.BeginTransaction();
                try
                {
                    string? codigo;
                    using (var cmdCodigo = conexion.CreateCommand())
                    {
                        cmdCodigo.Transaction = transaccion;
                        cmdCodigo.CommandText = @"
                            SELECT Codigo FROM Libros
                            WHERE Titulo = @titulo AND Disponibilidad = 'Disponible'
                            LIMIT 1;";
                        cmdCodigo.Parameters.AddWithValue("@titulo", titulo);
                        codigo = cmdCodigo.ExecuteScalar()?.ToString();
                    }

                    if (string.IsNullOrEmpty(codigo))
                    {
                        transaccion.Rollback();
                        MessageBox.Show(
                            $"No hay ejemplares disponibles de \"{titulo}\" en el inventario.",
                            "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    using (var cmdTomar = conexion.CreateCommand())
                    {
                        cmdTomar.Transaction = transaccion;
                        cmdTomar.CommandText = @"
                            UPDATE Libros SET Disponibilidad = 'En Sala'
                            WHERE Codigo = @codigo AND Disponibilidad = 'Disponible';";
                        cmdTomar.Parameters.AddWithValue("@codigo", codigo);

                        if (cmdTomar.ExecuteNonQuery() == 0)
                        {
                            transaccion.Rollback();
                            MessageBox.Show(
                                "El ejemplar dejó de estar disponible en este instante. Intente de nuevo.",
                                "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    using (var insertar = conexion.CreateCommand())
                    {
                        insertar.Transaction = transaccion;
                        insertar.CommandText = @"
                            INSERT INTO ControlUsuariosSala
                                (Fecha, NombreUsuario, Genero, Edad, TituloLibro,
                                 HoraEntrega, HoraRecibido, PersonalTurno, Estado, CodigoLibro)
                            VALUES
                                ($fecha, $nombre, $genero, $edad, $libro,
                                 $horaEntrega, $horaRecibido, $personal, $estado, $codigo);";

                        insertar.Parameters.AddWithValue("$fecha", dtpFecha.Value.ToString("yyyy-MM-dd"));
                        insertar.Parameters.AddWithValue("$nombre", txtNombre.Text.Trim());
                        insertar.Parameters.AddWithValue("$genero", cboGenero.SelectedItem?.ToString() ?? (object)DBNull.Value);
                        insertar.Parameters.AddWithValue("$edad", (int)numEdad.Value);
                        insertar.Parameters.AddWithValue("$libro", titulo);
                        insertar.Parameters.AddWithValue("$horaEntrega", DateTime.Now.ToString("HH:mm:ss"));
                        insertar.Parameters.AddWithValue("$horaRecibido", EstadoEnLectura);
                        insertar.Parameters.AddWithValue("$personal", txtPersonal.Text.Trim());
                        insertar.Parameters.AddWithValue("$estado", EstadoPendiente);
                        insertar.Parameters.AddWithValue("$codigo", codigo ?? string.Empty);

                        insertar.ExecuteNonQuery();
                    }

                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw; // el catch exterior muestra el mensaje unificado
                }

                // Refrescar combo: el título ocupado ya no debe ofrecerse
                CargarTitulosDeLibros();

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

        private void btnMarcarDevolucion_Click(object sender, EventArgs e)
        {
            if (!HayFilaSeleccionada()) return;

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var transaccion = conexion.BeginTransaction();
                int afectados;
                string codigoLiberado = string.Empty;
                try
                {
                    // Estado y código autoritativos desde la propia BD (no desde la fila pintada)
                    bool enLectura;
                    using (var cmdLeer = conexion.CreateCommand())
                    {
                        cmdLeer.Transaction = transaccion;
                        cmdLeer.CommandText =
                            "SELECT IFNULL(HoraRecibido, '') FROM ControlUsuariosSala WHERE ID = $id;";
                        cmdLeer.Parameters.AddWithValue("$id", idSeleccionado);

                        object? hora = cmdLeer.ExecuteScalar();
                        if (hora == null)
                        {
                            transaccion.Rollback();
                            MessageBox.Show("El registro ya no existe en la base de datos.",
                                "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            CargarRegistros();
                            return;
                        }

                        enLectura = string.Equals(hora.ToString(), EstadoEnLectura,
                            StringComparison.OrdinalIgnoreCase);
                    }

                    if (!enLectura)
                    {
                        transaccion.Rollback();
                        MessageBox.Show("El registro seleccionado ya tiene su devolución registrada.",
                            "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    using (var comando = conexion.CreateCommand())
                    {
                        comando.Transaction = transaccion;
                        comando.CommandText = @"
                            UPDATE ControlUsuariosSala
                            SET HoraRecibido = $hora,
                                Estado = $estado
                            WHERE ID = $id
                              AND HoraRecibido = $enLectura;";
                        comando.Parameters.AddWithValue("$hora", DateTime.Now.ToString("HH:mm:ss"));
                        comando.Parameters.AddWithValue("$estado", EstadoEntregado);
                        comando.Parameters.AddWithValue("$id", idSeleccionado);
                        comando.Parameters.AddWithValue("$enLectura", EstadoEnLectura);

                        afectados = comando.ExecuteNonQuery();
                    }

                    if (afectados > 0)
                    {
                        // La liberación se hace por código del ejemplar, nunca por título
                        codigoLiberado = ResolverCodigoDelRegistro(conexion, transaccion, idSeleccionado);
                        LiberarEjemplarEnSala(conexion, transaccion, codigoLiberado);
                    }

                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw; // el catch exterior muestra el mensaje unificado
                }

                if (afectados > 0)
                {
                    CargarTitulosDeLibros(); // el título liberado vuelve al combo
                    if (codigoLiberado.Length == 0)
                    {
                        MessageBox.Show(
                            "Devolución registrada, pero el ejemplar no se pudo identificar por código.\n" +
                            "Revise el inventario para corregir su disponibilidad.",
                            "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Devolución registrada correctamente. Estado: " + EstadoEntregado,
                            "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
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

        private void btnGuardarCambios_Click(object sender, EventArgs e)
        {
            if (!HayFilaSeleccionada()) return;
            if (!ValidarFormulario()) return;

            string estadoRegistro = dgvRegistros.CurrentRow?
                .Cells["HoraRecibido"].Value?.ToString() ?? string.Empty;
            string tituloEnGrilla = dgvRegistros.CurrentRow?
                .Cells["TituloLibro"].Value?.ToString() ?? string.Empty;

            if (estadoRegistro.Equals(EstadoEnLectura, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(tituloEnGrilla, cboLibro.Text.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "No se puede cambiar el libro de una lectura en curso. Registre primero la devolución.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (!HayFilaSeleccionada()) return;

            if (!FormAutenticacion.SolicitarAdmin(this)) return;

            // Credenciales correctas: ejecutar DELETE sincronizado con el inventario
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var transaccion = conexion.BeginTransaction();
                int afectados;
                try
                {
                    string codigoLibro = string.Empty;
                    bool estabaEnLectura = false;

                    // Estado y código autoritativos desde la propia BD
                    using (var cmdLeer = conexion.CreateCommand())
                    {
                        cmdLeer.Transaction = transaccion;
                        cmdLeer.CommandText =
                            "SELECT IFNULL(HoraRecibido, '') FROM ControlUsuariosSala WHERE ID = $id;";
                        cmdLeer.Parameters.AddWithValue("$id", idSeleccionado);

                        object? hora = cmdLeer.ExecuteScalar();
                        if (hora == null)
                        {
                            transaccion.Rollback();
                            MessageBox.Show("No se encontró el registro para eliminar.",
                                "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            CargarRegistros();
                            return;
                        }

                        estabaEnLectura = string.Equals(hora.ToString(), EstadoEnLectura,
                            StringComparison.OrdinalIgnoreCase);
                    }

                    // Regla de borrado: no se borra un registro cuyo ejemplar sigue en lectura
                    if (estabaEnLectura)
                    {
                        transaccion.Rollback();
                        MessageBox.Show(
                            "No se puede eliminar el registro: el ejemplar sigue en lectura.\n" +
                            "Registre primero la devolución.",
                            "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    using (var comando = conexion.CreateCommand())
                    {
                        comando.Transaction = transaccion;
                        comando.CommandText =
                            "DELETE FROM ControlUsuariosSala WHERE ID = $id AND HoraRecibido <> $enLectura;";
                        comando.Parameters.AddWithValue("$id", idSeleccionado);
                        comando.Parameters.AddWithValue("$enLectura", EstadoEnLectura);

                        afectados = comando.ExecuteNonQuery();
                    }

                    // Ejemplar ya devuelto: se libera explícitamente por código
                    if (afectados > 0)
                    {
                        codigoLibro = ResolverCodigoDelRegistro(conexion, transaccion, idSeleccionado);
                        LiberarEjemplarEnSala(conexion, transaccion, codigoLibro);
                    }

                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw; // el catch exterior muestra el mensaje unificado
                }

                if (afectados > 0)
                {
                    CargarTitulosDeLibros(); // por si se liberó un ejemplar
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

        private bool _dragScrollActivo;
        private Point _dragScrollInicio;
        private int _scrollOffsetInicial;

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

        protected override void OnHandleDestroyed(EventArgs e)
        {
            this.VisibleChanged -= UcControlSala_VisibleChanged;
            base.OnHandleDestroyed(e);
        }
    }
}
