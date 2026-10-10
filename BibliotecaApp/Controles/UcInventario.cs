using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    /// <summary>
    /// Apartado C: inventario general de libros con búsqueda en tiempo real
    /// sensible a tildes y mayúsculas (ignora ambas).
    /// </summary>
    public partial class UcInventario : UserControl
    {
        private const string ColBusqueda = "_BusquedaNormalizada";

        /// <summary>
        /// Orden físico de la biblioteca: PRIMERO por ubicación (agrupa M1, M2, M3...)
        /// y SEGUNDO por código del ejemplar. Así los códigos gubernamentales
        /// (que empiezan por números) no saltan al principio de la lista.
        /// Se aplica tanto en la carga (SQL) como en el filtrado en tiempo real (DataView).
        /// </summary>
        private const string OrdenPorUbicacion = "[Ubicación] ASC, [Código] ASC";
        private DataTable _datosInventario = new();
        private DataView _vistaFiltrada;

        // Botones de administración (nivel de clase para control de visibilidad por rol)
        private Button? _btnImportarCSV;
        private Button? _btnVaciarInventario;

        // Filtro desplegable de Ubicación (estilo Excel, generado en code-behind)
        private ComboBox? _cmbFiltroUbicacion;
        private Label? _lblFiltroUbicacion;
        private bool _cargandoFiltroUbicacion; // evita reentrancia al repoblar el combo

        /// <summary>Primer ítem del ComboBox: desactiva el filtro de ubicación.</summary>
        private const string FiltroTodasUbicaciones = "Todas las ubicaciones";

        public UcInventario()
        {
            InitializeComponent();

            typeof(DataGridView)
                .GetProperty("DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(dgvInventario, true, null);

            // Color de selección unificado (azul oscuro institucional igual que UcPrestamosExternos)
            dgvInventario.DefaultCellStyle.SelectionBackColor = EstiloUI.Acento;
            dgvInventario.DefaultCellStyle.SelectionForeColor = Color.White;

            // Suscribir evento de formato condicional para la columna Disponibilidad
            dgvInventario.CellFormatting += dgvInventario_CellFormatting;

            _vistaFiltrada = new DataView(_datosInventario);

            // Filtro desplegable de Ubicación (estilo Excel) en panelBusqueda
            CrearFiltroUbicacion();

            // Agregar botones de administración en panelEncabezado, alineados a la par del título
            int btnWidth = 130;
            int btnHeight = 35;

            _btnImportarCSV = new Button
            {
                Name = "btnImportarCSV",
                Text = "Importar CSV",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(btnWidth, btnHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Cursor = Cursors.Hand,
                Visible = false // Oculto por defecto, solo visible para Admins
            };
            _btnImportarCSV.FlatAppearance.BorderSize = 1;
            _btnImportarCSV.FlatAppearance.BorderColor = EstiloUI.Acento;
            _btnImportarCSV.ForeColor = EstiloUI.Acento;
            _btnImportarCSV.Click += BtnImportarCSV_Click;
            panelEncabezado.Controls.Add(_btnImportarCSV);

            _btnVaciarInventario = new Button
            {
                Name = "btnVaciarInventario",
                Text = "Vaciar Inventario",
                FlatStyle = FlatStyle.Flat,
                Size = new Size(btnWidth, btnHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Cursor = Cursors.Hand,
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                Visible = false // Oculto por defecto, solo visible para Admins
            };
            _btnVaciarInventario.FlatAppearance.BorderSize = 0;
            _btnVaciarInventario.Click += BtnVaciarInventario_Click;
            panelEncabezado.Controls.Add(_btnVaciarInventario);

            // Posicionar botones y aplicar visibilidad por rol después de que el layout esté listo
            this.Load += (s, e) =>
            {
                int margenIzquierdo = 25; // Separación respecto al título
                int espaciadoEntreBotones = 12;

                // Centrado vertical respecto al título
                int posY = lblTitulo.Location.Y + ((lblTitulo.Height - _btnImportarCSV!.Height) / 2);
                if (posY < 10) posY = lblTitulo.Location.Y;

                // Posición del primer botón (Importar CSV)
                int posXImportar = lblTitulo.Right + margenIzquierdo;
                _btnImportarCSV.Location = new Point(posXImportar, posY);

                // Posición del segundo botón (Vaciar Inventario)
                int posXVaciar = _btnImportarCSV.Right + espaciadoEntreBotones;
                _btnVaciarInventario!.Location = new Point(posXVaciar, posY);

                _btnImportarCSV.BringToFront();
                _btnVaciarInventario.BringToFront();

                // Visibilidad por rol: solo Administradores
                bool esAdmin = SesionGlobal.EsAdmin;
                _btnImportarCSV.Visible = esAdmin;
                _btnVaciarInventario.Visible = esAdmin;
            };
        }

        private void UcInventario_Load(object sender, EventArgs e)
        {
            CargarInventario();
        }

        /// <summary>Método público invocado por Form1 al navegar a este apartado.</summary>
        public void Actualizar()
        {
            CargarInventario();
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltro(txtBuscar.Text.Trim());
        }

        /// <summary>
        /// Carga todos los libros desde la BD una sola vez y agrega una columna
        /// oculta con el texto normalizado (sin tildes, minúsculas) para búsquedas.
        /// </summary>
        private void CargarInventario()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    SELECT Codigo   AS Código,
                           Titulo   AS Título,
                           Autor,
                           Editorial,
                           Estado,
                           Ubicacion AS Ubicación,
                           Disponibilidad
                    FROM Libros
                    ORDER BY Ubicacion ASC, Codigo ASC;";

                var tabla = new DataTable();
                using (var lector = comando.ExecuteReader())
                {
                    tabla.Load(lector);
                }

                // Columna oculta para búsqueda normalizada (sin acentos, minúsculas).
                tabla.Columns.Add(ColBusqueda, typeof(string));
                foreach (DataRow fila in tabla.Rows)
                {
                    string codigo = fila["Código"]?.ToString() ?? "";
                    string titulo = fila["Título"]?.ToString() ?? "";
                    fila[ColBusqueda] = EstiloUI.RemoverTildes(
                        codigo + " " + titulo).ToLowerInvariant();
                }

                // W-18: Dispose DataTable/DataView anterior para liberar memoria antes de reasignar
                _vistaFiltrada?.Dispose();
                _datosInventario?.Dispose();

                _datosInventario = tabla;
                _vistaFiltrada = new DataView(_datosInventario);
                // Orden físico garantizado también a nivel de vista
                // (misma cláusula que el ORDER BY de la consulta SQL).
                _vistaFiltrada.Sort = OrdenPorUbicacion;

                dgvInventario.DataSource = _vistaFiltrada;
                OcultarColumnaBusqueda();
                AjustarColumnas();
                lblContador.Text = $"{_vistaFiltrada.Count:N0} libro(s)";

                // Repoblar el ComboBox con las ubicaciones únicas del DataTable
                // recién cargado (conserva la selección si el valor sigue existiendo).
                CargarUbicacionesEnFiltro();

                // Reaplicar filtro activo si se recarga la BD.
                if (txtBuscar.Text.Length > 0)
                    AplicarFiltro(txtBuscar.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el inventario: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Filtra el DataView usando la columna normalizada.
        /// La búsqueda es insensible a tildes y mayúsculas/minúsculas.
        /// </summary>
        private void AplicarFiltro(string texto)
        {
            // RowFilter es únicamente la parte WHERE de la consulta: no admite
            // ORDER BY. El orden se fija en el DataView con la misma cláusula
            // que la carga principal: Ubicacion ASC, Codigo ASC.
            _vistaFiltrada.Sort = OrdenPorUbicacion;

            var condiciones = new List<string>();

            // 1) Búsqueda por texto: columna oculta normalizada (Código + Título)
            //    O Ubicación física en crudo ("M1", "M2"...). El LIKE de DataView
            //    es insensible a mayúsculas ("m1" = "M1").
            if (texto.Length > 0)
            {
                string termino = EstiloUI.RemoverTildes(texto).ToLowerInvariant();

                // Escapar caracteres especiales de DataView RowFilter (LIKE).
                termino = termino
                    .Replace("[", "[[]")
                    .Replace("*", "[*]")
                    .Replace("%", "[%]")
                    .Replace("'", "''");

                condiciones.Add($"([{ColBusqueda}] LIKE '%{termino}%' OR [Ubicación] LIKE '%{termino}%')");
            }

            // 2) Filtro de MÓDULO del ComboBox (si no es "Todas las ubicaciones"):
            //    "M1" debe enganchar M1, M1A1, M1B2... → LIKE con comodín final.
            string moduloSeleccionado = _cmbFiltroUbicacion?.SelectedItem?.ToString() ?? string.Empty;
            if (moduloSeleccionado.Length > 0 && moduloSeleccionado != FiltroTodasUbicaciones)
            {
                // Escapar caracteres especiales del LIKE: el prefijo es literal
                string prefijo = moduloSeleccionado
                    .Replace("[", "[[]")
                    .Replace("*", "[*]")
                    .Replace("%", "[%]")
                    .Replace("'", "''");
                condiciones.Add($"[Ubicación] LIKE '{prefijo}%'");
            }

            // Ambas condiciones se combinan con AND; sin ninguna, vista completa.
            _vistaFiltrada.RowFilter = condiciones.Count > 0
                ? string.Join(" AND ", condiciones)
                : string.Empty;

            lblContador.Text = $"{_vistaFiltrada.Count:N0} libro(s)";
        }

        /// <summary>
        /// Crea dinámicamente el ComboBox de filtro por Ubicación (estilo Excel)
        /// y su etiqueta, en una segunda fila de panelBusqueda alineada con
        /// txtBuscar (columna de control en x=210). DropDownList para impedir
        /// texto libre. No se toca UcInventario.Designer.cs.
        /// </summary>
        private void CrearFiltroUbicacion()
        {
            _lblFiltroUbicacion = new Label
            {
                Name = "lblFiltroUbicacion",
                Text = "Ubicación:",
                AutoSize = true,
                Font = EstiloUI.Etiqueta(),
                ForeColor = Color.FromArgb(43, 45, 66), // #2B2D42, mismo tono del título
                Location = new Point(16, 56)            // segunda fila, a la izquierda
            };

            _cmbFiltroUbicacion = new ComboBox
            {
                Name = "cmbFiltroUbicacion",
                Location = new Point(210, 53),          // misma columna que txtBuscar
                Size = new Size(240, 27),
                DropDownWidth = 260,                    // popup algo más ancho: se lee completo
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right // escala con la ventana como txtBuscar
            };
            EstiloUI.EstilizarEntrada(_cmbFiltroUbicacion); // DropDownList + estilo uniforme de la app
            _cmbFiltroUbicacion.SelectedIndexChanged += CmbFiltroUbicacion_SelectedIndexChanged;

            // Abrir una segunda fila en el panel para el filtro (fila 1 queda intacta)
            panelBusqueda.Height = 92;

            panelBusqueda.Controls.Add(_lblFiltroUbicacion);
            panelBusqueda.Controls.Add(_cmbFiltroUbicacion);
        }

        /// <summary>
        /// Llena el ComboBox con los MÓDULOS PRINCIPALES únicos (los 2 primeros
        /// caracteres de cada Ubicación: "M1A1" → "M1"), en el orden físico del
        /// DataTable (Ubicación ASC), más "Todas las ubicaciones" en el índice 0.
        /// Conserva la selección previa si el módulo sigue existiendo; si no,
        /// vuelve a "Todas". Idempotente y seguro de llamar en cada recarga.
        /// </summary>
        private void CargarUbicacionesEnFiltro()
        {
            if (_cmbFiltroUbicacion == null) return;

            string seleccionAnterior = _cmbFiltroUbicacion.SelectedItem?.ToString()
                ?? FiltroTodasUbicaciones;

            _cargandoFiltroUbicacion = true; // SelectedIndexChanged no refiltra mientras se repuebla
            try
            {
                _cmbFiltroUbicacion.Items.Clear();
                _cmbFiltroUbicacion.Items.Add(FiltroTodasUbicaciones);

                // Solo MÓDULOS: 2 primeros caracteres de cada ubicación.
                // Null-seguro: ubicaciones vacías o de 1 carácter se ignoran.
                var modulosVistos = new HashSet<string>(StringComparer.Ordinal);
                foreach (DataRow fila in _datosInventario.Rows)
                {
                    string ubicacion = fila["Ubicación"]?.ToString() ?? "";
                    if (ubicacion.Length < 2) continue;

                    string modulo = ubicacion.Substring(0, 2);
                    if (modulosVistos.Add(modulo)) // valores únicos, orden del DataTable
                        _cmbFiltroUbicacion.Items.Add(modulo);
                }

                int indice = _cmbFiltroUbicacion.Items.IndexOf(seleccionAnterior);
                _cmbFiltroUbicacion.SelectedIndex = indice >= 0 ? indice : 0;
            }
            finally
            {
                _cargandoFiltroUbicacion = false;
            }

            // Refiltrar con la selección resultante (p. ej. si la ubicación
            // seleccionada ya no existe y se volvió a "Todas las ubicaciones").
            AplicarFiltro(txtBuscar.Text.Trim());
        }

        /// <summary>
        /// Cambió la ubicación seleccionada en el ComboBox: combinar con el
        /// texto de txtBuscar vía AND dentro de AplicarFiltro.
        /// </summary>
        private void CmbFiltroUbicacion_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_cargandoFiltroUbicacion) return; // evita ciclos al repoblar el combo
            AplicarFiltro(txtBuscar.Text.Trim());
        }

        private void OcultarColumnaBusqueda()
        {
            if (dgvInventario.Columns.Contains(ColBusqueda))
                dgvInventario.Columns[ColBusqueda]!.Visible = false;
        }

        private void AjustarColumnas()
        {
            if (dgvInventario.Columns["Título"] != null)
                dgvInventario.Columns["Título"]!.FillWeight = 55;
        }

        // ====================================================================
        //  DOBLE CLIC → iniciar préstamo externo
        // ====================================================================

        private void dgvInventario_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var fila = dgvInventario.Rows[e.RowIndex];

            string disponibilidad = fila.Cells["Disponibilidad"].Value?.ToString() ?? "";
            if (!string.Equals(disponibilidad, "Disponible", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Este ejemplar ya está prestado y no puede iniciarse un préstamo.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string codigo = fila.Cells["Código"].Value?.ToString() ?? "";
            string titulo = fila.Cells["Título"].Value?.ToString() ?? "";

            if (codigo.Length == 0) return;

            if (FindForm() is Form1 principal)
                principal.CargarPrestamoDesdeInventario(codigo, titulo);
        }

        /// <summary>
        /// Formato condicional para la columna Disponibilidad:
        /// - "Prestado" → fondo rosa suave (LightPink) y texto rojo oscuro (DarkRed)
        /// - "Disponible" u otros → estilo por defecto (sin cambios)
        /// </summary>
        private void dgvInventario_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            // Solo actuar sobre la columna "Disponibilidad"
            if (e.ColumnIndex < 0 || e.ColumnIndex >= dgvInventario.Columns.Count) return;
            if (dgvInventario.Columns[e.ColumnIndex].Name != "Disponibilidad") return;
            if (e.Value == null) return;

            string valor = e.Value.ToString();
            if (string.Equals(valor, "Prestado", StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.BackColor = Color.LightPink;
                e.CellStyle.ForeColor = Color.DarkRed;
            }
            // Para "Disponible" o cualquier otro valor, no modificamos el estilo
            // y el DataGridView usa su estilo por defecto
        }

        // ====================================================================
        //  ADMIN: VACIAR INVENTARIO
        // ====================================================================
        private void BtnVaciarInventario_Click(object? sender, EventArgs e)
        {
            // --- Autenticación de seguridad (mismo modal que Control de Sala: AdminCubo / Admin123$) ---
            using (var frmAuth = new FormAutenticacion("AdminCubo", "Admin123$"))
            {
                if (frmAuth.ShowDialog(this) != DialogResult.OK) return; // Si cancela o falla auth, salir
            }

            // --- Confirmación final ---
            var resultado = MessageBox.Show(
                "¿Está seguro de que desea ELIMINAR TODOS los libros del inventario?\n\nEsta acción no se puede deshacer.",
                "Confirmar vaciado de inventario",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (resultado != DialogResult.Yes)
                return;

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                // Eliminar todos los libros
                using (var cmd = conexion.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Libros;";
                    cmd.ExecuteNonQuery();
                }

                // W-08: ELIMINADO — 'Libros.Codigo' es TEXT PRIMARY KEY (no AUTOINCREMENT),
                // por tanto sqlite_sequence no aplica. La línea siguiente era código muerto.

                // PERSISTENCIA: marcar el vaciado. Sin esta bandera, el arranque
                // volvería a importar el catálogo CSV y el inventario reaparecería.
                ConexionDB.MarcarInventarioVaciado(true);

                // CRÍTICO: limpiar de inmediato el DataSource del DataGridView para
                // que la tabla se vea vacía al instante, sin esperar a la recarga.
                dgvInventario.DataSource = null;

                // Recargar desde la BD real: reconstruye la tabla vacía, el
                // contador de libros y reaplica el filtro de búsqueda activo.
                CargarInventario();

                MessageBox.Show("Inventario vaciado correctamente.", "Biblioteca CUBO",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al vaciar el inventario:\n{ex.Message}",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ====================================================================
        //  ADMIN: IMPORTAR CSV MASIVO
        // ====================================================================
        private void BtnImportarCSV_Click(object? sender, EventArgs e)
        {
            // Validar si hay filas (descontando la fila de nuevo registro si AllowUserToAddRows es true)
            int filasReales = dgvInventario.AllowUserToAddRows ? dgvInventario.Rows.Count - 1 : dgvInventario.Rows.Count;
            if (filasReales > 0)
            {
                MessageBox.Show("El inventario actual contiene registros. Para importar un nuevo catálogo, primero debes vaciar el inventario existente.", "Acción Denegada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var ofd = new OpenFileDialog
            {
                Title = "Seleccionar archivo CSV de inventario",
                Filter = "Archivos CSV (*.csv)|*.csv|Todos los archivos (*.*)|*.*",
                Multiselect = false
            };

            if (ofd.ShowDialog(this) != DialogResult.OK)
                return;

            string rutaArchivo = ofd.FileName;

            try
            {
                int filasInsertadas = 0;
                int lineasProcesadas = 0;

                using var conexion = ConexionDB.ObtenerConexion();
                using var transaction = conexion.BeginTransaction();

                // Preparar comando para verificar disponibilidad actual antes de insertar/reemplazar
                using var checkCmd = conexion.CreateCommand();
                checkCmd.Transaction = transaction;
                checkCmd.CommandText = "SELECT Disponibilidad FROM Libros WHERE Codigo = @codigo;";
                var pCheckCodigo = checkCmd.Parameters.Add("@codigo", SqliteType.Text);

                // Preparar comando INSERT OR REPLACE parametrizado (tolera duplicados en Codigo UNIQUE)
                using var cmd = conexion.CreateCommand();
                cmd.Transaction = transaction;
                cmd.CommandText = @"
                    INSERT OR REPLACE INTO Libros (Codigo, Titulo, Autor, Editorial, Estado, Ubicacion, Disponibilidad)
                    VALUES (@codigo, @titulo, @autor, @editorial, @estado, @ubicacion, @disponibilidad);";

                // Parámetros reutilizables
                var pCodigo = cmd.Parameters.Add("@codigo", SqliteType.Text);
                var pTitulo = cmd.Parameters.Add("@titulo", SqliteType.Text);
                var pAutor = cmd.Parameters.Add("@autor", SqliteType.Text);
                var pEditorial = cmd.Parameters.Add("@editorial", SqliteType.Text);
                var pEstado = cmd.Parameters.Add("@estado", SqliteType.Text);
                var pUbicacion = cmd.Parameters.Add("@ubicacion", SqliteType.Text);
                var pDisponibilidad = cmd.Parameters.Add("@disponibilidad", SqliteType.Text);

                // Regex para split CSV ignorando comas dentro de comillas
                // Patrón: coma seguida de número par de comillas hasta el final
                var regexSplit = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                using var reader = new StreamReader(rutaArchivo, System.Text.Encoding.UTF8);
                bool primeraLinea = true;

                while (!reader.EndOfStream)
                {
                    string? linea = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(linea)) continue;

                    if (primeraLinea)
                    {
                        primeraLinea = false;
                        continue; // Saltar headers
                    }

                    string[] campos = regexSplit.Split(linea);

                    // Limpiar comillas envolventes de cada campo
                    for (int i = 0; i < campos.Length; i++)
                    {
                        string c = campos[i].Trim();
                        if (c.StartsWith("\"") && c.EndsWith("\"") && c.Length >= 2)
                            c = c.Substring(1, c.Length - 2);
                        c = c.Replace("\"\"", "\""); // Comillas escapadas
                        campos[i] = c;
                    }

                    // Esperamos al menos: Código, Título, Autor, Editorial, Estado, Ubicación
                    // Disponibilidad es opcional (default 'Disponible')
                    if (campos.Length < 6)
                    {
                        lineasProcesadas++;
                        continue; // Saltar líneas inválidas
                    }

                    // Sanitizar: Trim() a todos los campos, crítico para Codigo (clave única)
                    string codigo = campos.Length > 0 ? campos[0].Trim() : "";
                    string titulo = campos.Length > 1 ? campos[1].Trim() : "";
                    string autor = campos.Length > 2 ? campos[2].Trim() : "";
                    string editorial = campos.Length > 3 ? campos[3].Trim() : "";
                    string estado = campos.Length > 4 ? campos[4].Trim() : "";
                    string ubicacion = campos.Length > 5 ? campos[5].Trim() : "";
                    string disponibilidadCsv = (campos.Length > 6 && !string.IsNullOrWhiteSpace(campos[6]))
                        ? campos[6].Trim()
                        : "Disponible";

                    // C-05: No sobrescribir 'Prestado' a 'Disponible' si el ejemplar ya está prestado.
                    // Verificamos la disponibilidad actual en BD antes de insertar/reemplazar.
                    string disponibilidadFinal = disponibilidadCsv;
                    pCheckCodigo.Value = codigo;
                    object? result = checkCmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        string disponibilidadActual = result.ToString() ?? "";
                        if (string.Equals(disponibilidadActual, "Prestado", StringComparison.OrdinalIgnoreCase))
                        {
                            // El ejemplar está prestado: preservar estado 'Prestado' para no desincronizar préstamos activos.
                            disponibilidadFinal = "Prestado";
                        }
                    }

                    pCodigo.Value = codigo;
                    pTitulo.Value = titulo;
                    pAutor.Value = autor;
                    pEditorial.Value = editorial;
                    pEstado.Value = estado;
                    pUbicacion.Value = ubicacion;
                    pDisponibilidad.Value = disponibilidadFinal;

                    cmd.ExecuteNonQuery();
                    filasInsertadas++;
                    lineasProcesadas++;
                }

                transaction.Commit();

                // El catálogo vuelve a estar poblado: retirar la bandera de
                // "vaciado" para que el arranque retome la importación normal.
                ConexionDB.MarcarInventarioVaciado(false);

                MessageBox.Show(
                    $"Importación completada.\n\nArchivo: {Path.GetFileName(rutaArchivo)}\nLíneas procesadas: {lineasProcesadas}\nLibros insertados: {filasInsertadas}",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);

                CargarInventario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al importar el CSV:\n{ex.Message}",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void panelEncabezado_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}