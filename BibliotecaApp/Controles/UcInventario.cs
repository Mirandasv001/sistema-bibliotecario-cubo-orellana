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

        // Botón "Agregar Libro" (code-behind) — abre el modal de alta de ejemplares
        // (vive en panelEncabezado, a la derecha de "Vaciar Inventario")
        private Button? btnAgregarLibro;

        // Botón "Eliminar Libro (fila)" (code-behind) — borra el registro seleccionado con auth
        // (misma fila, tamaño y estilo que "Vaciar Inventario")
        private Button? btnEliminarLibro;
        private Button? btnEditarLibro;

        /// <summary>Primer ítem del ComboBox: desactiva el filtro de ubicación.</summary>
        private const string FiltroTodasUbicaciones = "Todas las ubicaciones";

        // Opciones especiales al final del combo de Ubicación (catálogo configurable)
        private const string ItemAgregarNueva = "+ Agregar nueva";
        private const string ItemEliminarUbicacion = "- Eliminar ubicación";

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

            // Etiqueta "Buscar:" de la fila 1: el Designer la declara pero nunca
            // la instancia, así que se crea aquí en code-behind con el mismo
            // estilo que "Ubicación:" y alineada en X con ella (columna 16).
            lblBuscar = new Label
            {
                Name = "lblBuscar",
                Text = "Buscar:",
                AutoSize = true,
                Font = EstiloUI.Etiqueta(),               // misma fuente que "Ubicación:"
                ForeColor = Color.FromArgb(43, 45, 66),   // #2B2D42, idéntico a su par
                Location = new Point(16, txtBuscar.Location.Y + 3) // misma X; vertical con la fila 1 (13 + 3 = 16)
            };
            panelBusqueda.Controls.Add(lblBuscar);

            // Filtro desplegable de Ubicación (estilo Excel) en panelBusqueda.
            // Se crea ANTES de posicionar la barra: devuelve la columna X
            // compartida que usan la barra y el combo (misma línea vertical).
            int colControles = CrearFiltroUbicacion();

            // ══ ALINEACIÓN Y GROSOR: barra y combo en la misma columna X ══

            // 1) Placeholder nuevo solicitado (sobrescribe el del Designer)
            txtBuscar.PlaceholderText = "Búsqueda de libros...";

            // 2) Barra de búsqueda: 5 px más alta que el original (27 → 32)
            txtBuscar.AutoSize = false;
            txtBuscar.Height += 5;
            txtBuscar.Width += 20; // +20 px de aire: el texto de búsqueda se lee completo

            // 3) Misma X exacta que el combo, conservando el borde derecho
            //    de la barra (junto al contador de libros)
            int bordeDerechoBarra = txtBuscar.Location.X + txtBuscar.Width;
            txtBuscar.Location = new Point(colControles, txtBuscar.Location.Y);
            txtBuscar.Width = bordeDerechoBarra - txtBuscar.Location.X;

            // 4) Centro visual de la fila 1: barra, "Buscar:" y "N libro(s)"
            //    comparten eje Y (contador: Y=18, alto preferido → centro 28)
            Size tamLblBuscar = lblBuscar.GetPreferredSize(Size.Empty);
            int centroFila1 = lblContador.Location.Y
                             + lblContador.GetPreferredSize(Size.Empty).Height / 2;
            txtBuscar.Location = new Point(txtBuscar.Location.X,
                                            centroFila1 - txtBuscar.Height / 2);
            lblBuscar.Location = new Point(lblBuscar.Left,
                                            centroFila1 - tamLblBuscar.Height / 2);

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

            // --- Botones de acciones de libros: mismo contenedor, tamaño y estilo que Vaciar ---
            btnAgregarLibro = new Button
            {
                Name = "btnAgregarLibro",
                Text = "Agregar Libro",
                FlatStyle = FlatStyle.Flat,
                Size = _btnVaciarInventario.Size,          // copia el tamaño del botón de referencia
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Cursor = Cursors.Hand
            };
            btnAgregarLibro.FlatAppearance.BorderSize = 0;
            btnAgregarLibro.BackColor = EstiloUI.FondoOscuro; // azul oscuro institucional (#1B2B42), paleta del proyecto
            btnAgregarLibro.ForeColor = Color.White;
            btnAgregarLibro.Click += btnAgregarLibro_Click;
            panelEncabezado.Controls.Add(btnAgregarLibro);

            btnEliminarLibro = new Button
            {
                Name = "btnEliminarLibro",
                Text = "Eliminar Libro (fila)",
                FlatStyle = FlatStyle.Flat,
                Size = _btnVaciarInventario.Size,          // copia el tamaño del botón de referencia
                Width = _btnVaciarInventario.Width + 20,   // +20 px para que el nuevo texto encaje con holgura
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Cursor = Cursors.Hand
            };
            btnEliminarLibro.FlatAppearance.BorderSize = 0;
            btnEliminarLibro.BackColor = _btnVaciarInventario.BackColor; // IndianRed (idéntico)
            btnEliminarLibro.ForeColor = _btnVaciarInventario.ForeColor; // White (idéntico)
            btnEliminarLibro.Click += btnEliminarLibro_Click;
            panelEncabezado.Controls.Add(btnEliminarLibro);

            // --- Botón Editar: a la derecha de Eliminar, estilo acento (no destructivo) ---
            btnEditarLibro = new Button
            {
                Name = "btnEditarLibro",
                Text = "Editar Libro (fila)",
                FlatStyle = FlatStyle.Flat,
                Size = _btnVaciarInventario.Size,          // copia el tamaño del botón de referencia
                Width = _btnVaciarInventario.Width + 20,   // holgura para el texto
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Cursor = Cursors.Hand
            };
            btnEditarLibro.FlatAppearance.BorderSize = 0;
            btnEditarLibro.BackColor = EstiloUI.Acento;   // azul acento (#3A506B), paleta del proyecto
            btnEditarLibro.ForeColor = Color.White;
            btnEditarLibro.Click += btnEditarLibro_Click;
            panelEncabezado.Controls.Add(btnEditarLibro);

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

                // Botones de libros: misma fila que Vaciar, a su derecha (+15 entre cada uno)
                btnAgregarLibro!.Top = _btnVaciarInventario.Top;
                btnAgregarLibro.Left = _btnVaciarInventario.Right + 15;
                btnEliminarLibro!.Top = _btnVaciarInventario.Top;
                btnEliminarLibro.Left = btnAgregarLibro.Right + 15;
                btnEditarLibro!.Top = _btnVaciarInventario.Top;
                btnEditarLibro.Left = btnEliminarLibro.Right + 15;

                _btnImportarCSV.BringToFront();
                _btnVaciarInventario.BringToFront();
                btnAgregarLibro.BringToFront();
                btnEliminarLibro.BringToFront();
                btnEditarLibro.BringToFront();

                // Visibilidad por rol: solo Administradores
                bool esAdmin = SesionGlobal.EsAdmin;
                _btnImportarCSV.Visible = esAdmin;
                _btnVaciarInventario.Visible = esAdmin;
                btnAgregarLibro.Visible = esAdmin;
                btnEliminarLibro.Visible = esAdmin;
                btnEditarLibro.Visible = esAdmin;
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
        /// y su etiqueta, en una segunda fila de panelBusqueda. Devuelve la
        /// columna X compartida (10 px después de la etiqueta más ancha) que
        /// usan el combo y la barra de búsqueda, para que ambos empiecen en la
        /// misma línea vertical. DropDownList para impedir texto libre.
        /// No se toca UcInventario.Designer.cs.
        /// </summary>
        private int CrearFiltroUbicacion()
        {
            // Columna de etiquetas de panelBusqueda (fila 1 y fila 2 comparten X)
            const int colEtiquetas = 16;   // "Buscar:" y "Ubicación:" (mismo eje X)
            const int margenEtiqueta = 10; // gap estético etiqueta → control

            _lblFiltroUbicacion = new Label
            {
                Name = "lblFiltroUbicacion",
                Text = "Ubicación:",
                AutoSize = true,
                Font = EstiloUI.Etiqueta(),
                ForeColor = Color.FromArgb(43, 45, 66), // #2B2D42, mismo tono del título
                Location = new Point(colEtiquetas, 56)  // misma columna de etiquetas que "Buscar:"
            };

            _cmbFiltroUbicacion = new ComboBox
            {
                Name = "cmbFiltroUbicacion",
                Location = new Point(colEtiquetas, 53), // X definitivo: columna compartida, más abajo
                Width = 195,                             // 180 + 15px extra: el texto "Todas las ubicaciones" encaja completo
                DropDownWidth = 260                      // popup algo más ancho: se lee completo
            };
            EstiloUI.EstilizarEntrada(_cmbFiltroUbicacion); // DropDownList + estilo uniforme de la app
            _cmbFiltroUbicacion.SelectedIndexChanged += CmbFiltroUbicacion_SelectedIndexChanged;

            // Abrir una segunda fila en el panel para el filtro (fila 1 queda intacta)
            // 107 = 92 + 15px: aire extra para que dgvInventario (Dock=Fill) respire
            // respecto a los filtros — el dock recalcula solo su Top y Height.
            panelBusqueda.Height = 107;

            panelBusqueda.Controls.Add(_lblFiltroUbicacion);
            panelBusqueda.Controls.Add(_cmbFiltroUbicacion);

            // Columna X compartida: barra y combo empiezan en la misma línea
            // vertical, 10 px después de la etiqueta MÁS ancha de la cabecera
            int colControles = Math.Max(
                lblBuscar.Left + lblBuscar.GetPreferredSize(Size.Empty).Width,
                _lblFiltroUbicacion.Left + _lblFiltroUbicacion.GetPreferredSize(Size.Empty).Width)
                + margenEtiqueta;

            _cmbFiltroUbicacion.Location = new Point(colControles, _cmbFiltroUbicacion.Location.Y);
            return colControles;
        }

        /// <summary>
        /// Abre el modal dinámico de alta de ejemplar y, si se guardó (OK),
        /// recarga el inventario para mostrar el libro nuevo.
        /// </summary>
        private void btnAgregarLibro_Click(object? sender, EventArgs e)
        {
            using var modal = CrearFormularioAgregarLibro();
            if (modal.ShowDialog(this) == DialogResult.OK)
                CargarInventario();
        }

        /// <summary>
        /// Abre el modal en modo EDICIÓN precargado con la fila seleccionada
        /// en el grid. Valida que exista selección y que la fila tenga código
        /// (Primary Key de Libros).
        /// </summary>
        private void btnEditarLibro_Click(object? sender, EventArgs e)
        {
            // Validar que haya una fila seleccionada en el grid
            if (dgvInventario.CurrentRow == null || dgvInventario.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Seleccione el libro a editar en la tabla.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var fila = dgvInventario.CurrentRow;
            string[] datosFila =
            {
                fila.Cells["Código"]?.Value?.ToString() ?? "",      // [0] Código (PK, bloqueada en el modal)
                fila.Cells["Título"]?.Value?.ToString() ?? "",      // [1]
                fila.Cells["Autor"]?.Value?.ToString() ?? "",       // [2]
                fila.Cells["Editorial"]?.Value?.ToString() ?? "",   // [3]
                fila.Cells["Estado"]?.Value?.ToString() ?? "",      // [4]
                fila.Cells["Ubicación"]?.Value?.ToString() ?? ""    // [5]
            };

            if (datosFila[0].Length == 0)
            {
                MessageBox.Show("La fila seleccionada no tiene código de libro.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var modal = CrearFormularioAgregarLibro(datosFila);
            if (modal.ShowDialog(this) == DialogResult.OK)
                CargarInventario();
        }

        /// <summary>
        /// Crea en code-behind el formulario modal "Agregar Nuevo Libro"
        /// (Código, Título, Autor, Editorial, Estado, Ubicación) con su INSERT
        /// parametrizado en la tabla Libros. Disponibilidad se inserta fija
        /// como 'Disponible' (regla de negocio: no se pregunta al usuario).
        /// Modo edición: al pasar los datos de la fila (datosFilaEditar) el
        /// modal se transforma en "Editar Libro" — campos precargados, Código
        /// bloqueado (Primary Key), botón "Guardar cambios" + "Cancelar" y
        /// UPDATE parametrizado por Codigo. No se toca UcInventario.Designer.cs.
        /// </summary>
        private Form CrearFormularioAgregarLibro(string[]? datosFilaEditar = null)
        {
            bool enModoEdicion = datosFilaEditar != null;

            var modal = new Form
            {
                Text = enModoEdicion ? "Editar Libro" : "Agregar Nuevo Libro",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false
            };

            // Una fila por campo: etiqueta (columna 20) + TextBox (columna 120)
            string[] etiquetas = { "Código:", "Título:", "Autor:", "Editorial:", "Estado:", "Ubicación:" };
            var cajas = new TextBox[etiquetas.Length];

            for (int i = 0; i < etiquetas.Length; i++)
            {
                var lbl = EstiloUI.CrearEtiqueta(etiquetas[i]); // mismo estilo que las etiquetas de la app
                lbl.Location = new Point(20, 20 + i * 34);

                cajas[i] = new TextBox
                {
                    Location = new Point(120, 17 + i * 34),
                    Width = 270
                };
                EstiloUI.EstilizarEntrada(cajas[i]);

                // Modo edición: precarga el dato de la fila seleccionada
                if (enModoEdicion)
                    cajas[i].Text = datosFilaEditar![i];

                modal.Controls.AddRange(new Control[] { lbl, cajas[i] });
            }

            var txtCodigo = cajas[0];
            var txtTitulo = cajas[1];
            var txtAutor = cajas[2];
            var txtEditorial = cajas[3];
            var txtEstado = cajas[4];
            var txtUbicacion = cajas[5];

            // Modo edición: el Código es la Primary Key, se protege contra cambios
            if (enModoEdicion)
            {
                txtCodigo.ReadOnly = true;
                txtCodigo.BackColor = Color.FromArgb(238, 238, 238); // gris suave: solo lectura
            }

            int yBotones = 17 + etiquetas.Length * 34 + 8;

            var btnGuardar = new Button
            {
                Name = "btnGuardar",
                Text = enModoEdicion ? "Guardar cambios" : "Guardar",
                Size = new Size(enModoEdicion ? 150 : 120, 38),
                Location = new Point(enModoEdicion ? 245 : 270, yBotones),
                Cursor = Cursors.Hand
            };
            EstiloUI.EstilizarBotonPrimario(btnGuardar);

            btnGuardar.Click += (_, _) =>
            {
                // Validación: todos los campos son obligatorios
                if (cajas.Any(c => string.IsNullOrWhiteSpace(c.Text)))
                {
                    MessageBox.Show("Complete todos los campos antes de guardar.",
                        "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    using var conexion = ConexionDB.ObtenerConexion();
                    using var cmd = conexion.CreateCommand();

                    if (enModoEdicion)
                    {
                        // UPDATE parametrizado: el filtro es la Primary Key (Codigo).
                        // Disponibilidad NO se toca: el estado del ejemplar
                        // (Disponible/Prestado/En Sala) lo gestionan los préstamos.
                        cmd.CommandText = @"
                            UPDATE Libros SET Titulo   = @titulo,
                                              Autor    = @autor,
                                              Editorial = @editorial,
                                              Estado   = @estado,
                                              Ubicacion = @ubicacion
                            WHERE Codigo = @codigo;";
                    }
                    else
                    {
                        cmd.CommandText = @"
                            INSERT INTO Libros (Codigo, Titulo, Autor, Editorial, Estado, Ubicacion, Disponibilidad)
                            VALUES (@codigo, @titulo, @autor, @editorial, @estado, @ubicacion, 'Disponible');";
                    }

                    cmd.Parameters.AddWithValue("@codigo", txtCodigo.Text.Trim());
                    cmd.Parameters.AddWithValue("@titulo", txtTitulo.Text.Trim());
                    cmd.Parameters.AddWithValue("@autor", txtAutor.Text.Trim());
                    cmd.Parameters.AddWithValue("@editorial", txtEditorial.Text.Trim());
                    cmd.Parameters.AddWithValue("@estado", txtEstado.Text.Trim());
                    cmd.Parameters.AddWithValue("@ubicacion", txtUbicacion.Text.Trim());
                    cmd.ExecuteNonQuery();

                    MessageBox.Show(enModoEdicion ? "Libro actualizado correctamente."
                                                  : "Libro registrado correctamente.",
                        "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    modal.DialogResult = DialogResult.OK; // el llamador recarga el inventario
                    modal.Close();
                }
                catch (Exception ex)
                {
                    // Causa habitual del INSERT: Código duplicado (PRIMARY KEY de Libros)
                    MessageBox.Show("No se pudo guardar el libro: " + ex.Message,
                        "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            modal.Controls.Add(btnGuardar);
            modal.AcceptButton = btnGuardar; // Enter también guarda

            if (enModoEdicion)
            {
                var btnCancelar = new Button
                {
                    Name = "btnCancelar",
                    Text = "Cancelar",
                    Size = new Size(120, 38),
                    Location = new Point(105, yBotones),
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.Cancel
                };
                EstiloUI.EstilizarBotonSecundario(btnCancelar);
                modal.Controls.Add(btnCancelar);
                modal.CancelButton = btnCancelar; // Esc cierra sin guardar
            }

            modal.ClientSize = new Size(420, btnGuardar.Bottom + 20);

            return modal;
        }

        /// <summary>
        /// Elimina físicamente el libro seleccionado en el grid.
        /// Seguridad: pide autenticación con FormAutenticacion (AdminCubo / Admin123$)
        /// y solo si el modal devuelve OK ejecuta el DELETE, dentro de una
        /// transacción (Commit/Rollback) y con parámetro SQLite.
        /// </summary>
        private void btnEliminarLibro_Click(object? sender, EventArgs e)
        {
            // Codigo del libro seleccionado en el grid (columna autogenerada "Código")
            object? valorCodigo = dgvInventario.CurrentRow?.Cells["Código"]?.Value;
            string codigo = valorCodigo?.ToString() ?? string.Empty;
            if (codigo.Length == 0)
            {
                MessageBox.Show("Seleccione un libro de la lista para eliminar.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // --- Autenticación de seguridad (mismo modal que Préstamos: AdminCubo / Admin123$) ---
            using (var frmAuth = new FormAutenticacion("AdminCubo", "Admin123$"))
            {
                if (frmAuth.ShowDialog(this) != DialogResult.OK) return;
            }

            var resultado = MessageBox.Show(
                $"¿Está seguro de que desea ELIMINAR el libro \"{codigo}\" definitivamente?\n\nEsta acción no se puede deshacer.",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (resultado != DialogResult.Yes) return;

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();

                // DELETE como unidad transaccional: o se borra completo o no se borra nada
                using var transaccion = conexion.BeginTransaction();
                try
                {
                    using var cmd = conexion.CreateCommand();
                    cmd.Transaction = transaccion;
                    cmd.CommandText = "DELETE FROM Libros WHERE Codigo = @codigo;";
                    cmd.Parameters.AddWithValue("@codigo", codigo);
                    cmd.ExecuteNonQuery();
                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw;
                }

                MessageBox.Show("Libro eliminado correctamente.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarInventario(); // refresca el DataGridView
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar el libro: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            // Las opciones especiales jamás valen como "selección previa"
            if (seleccionAnterior == ItemAgregarNueva || seleccionAnterior == ItemEliminarUbicacion)
                seleccionAnterior = FiltroTodasUbicaciones;

            // Módulos configurables: se leen de UbicacionesConfig (no de Libros),
            // así el admin controla qué módulos existen en este centro.
            var modulos = new List<string>();
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText =
                    "SELECT NombreUbicacion FROM UbicacionesConfig ORDER BY NombreUbicacion ASC;";
                using var lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    string modulo = lector.IsDBNull(0) ? "" : lector.GetString(0).Trim();
                    if (modulo.Length > 0) modulos.Add(modulo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al leer las ubicaciones: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            _cargandoFiltroUbicacion = true; // SelectedIndexChanged no refiltra mientras se repuebla
            try
            {
                _cmbFiltroUbicacion.Items.Clear();
                _cmbFiltroUbicacion.Items.Add(FiltroTodasUbicaciones);

                foreach (string modulo in modulos)
                    _cmbFiltroUbicacion.Items.Add(modulo);

                // Opciones especiales SIEMPRE al final (sin importar el contenido de la BD)
                _cmbFiltroUbicacion.Items.Add(ItemEliminarUbicacion);
                _cmbFiltroUbicacion.Items.Add(ItemAgregarNueva);

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

            string? seleccionado = _cmbFiltroUbicacion?.SelectedItem?.ToString();

            if (seleccionado == ItemAgregarNueva) // registrar ubicación nueva
            {
                RegistrarNuevaUbicacion();
                return;
            }

            if (seleccionado == ItemEliminarUbicacion) // quitar módulo del catálogo
            {
                EliminarUbicacionExistente();
                return;
            }

            AplicarFiltro(txtBuscar.Text.Trim());
        }

        /// <summary>
        /// Mini modal dinámico (TextBox + Aceptar/Cancelar) para pedir una ubicación.
        /// Devuelve el texto recortado o null si se canceló / quedó vacío.
        /// </summary>
        private string? PedirUbicacion(string mensaje)
        {
            using var modal = new Form
            {
                Text = "Ubicación",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(340, 130)
            };

            var lblTexto = new Label
            {
                Text = mensaje,
                AutoSize = true,
                Location = new Point(15, 15)
            };

            var txtValor = new TextBox { Location = new Point(15, 45), Width = 310 };
            EstiloUI.EstilizarEntrada(txtValor); // mismo estilo de entradas de la app

            var btnAceptar = new Button
            {
                Text = "Aceptar",
                DialogResult = DialogResult.OK,
                Location = new Point(160, 85),
                Size = new Size(80, 28)
            };
            var btnCancelar = new Button
            {
                Text = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Location = new Point(245, 85),
                Size = new Size(80, 28)
            };

            modal.Controls.AddRange(new Control[] { lblTexto, txtValor, btnAceptar, btnCancelar });
            modal.AcceptButton = btnAceptar;   // Enter acepta
            modal.CancelButton = btnCancelar;  // Esc cancela

            return modal.ShowDialog(this) == DialogResult.OK
                ? txtValor.Text.Trim()
                : null;
        }

        /// <summary>
        /// Persiste la ubicación nueva en UbicacionesConfig (INSERT parametrizado),
        /// recarga el combo y la deja seleccionada. Si se cancela o se deja vacío,
        /// vuelve a "Todas las ubicaciones".
        /// </summary>
        private void RegistrarNuevaUbicacion()
        {
            string? nueva = PedirUbicacion("Ingrese la nueva ubicación (Ej: M6):");

            if (string.IsNullOrWhiteSpace(nueva))
            {
                VolverATodasLasUbicaciones();
                return;
            }

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText = "INSERT INTO UbicacionesConfig (NombreUbicacion) VALUES (@nombre);";
                cmd.Parameters.AddWithValue("@nombre", nueva);
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException)
            {
                // Ya existía (PRIMARY KEY): no es error, solo se recarga y selecciona
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la ubicación: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                VolverATodasLasUbicaciones();
                return;
            }

            CargarUbicacionesEnFiltro(); // recarga desde la BD (incluye el nuevo módulo)
            SeleccionarUbicacion(nueva); // la deja seleccionada y refiltra
        }

        /// <summary>
        /// Elimina una ubicación del catálogo (DELETE parametrizado en UbicacionesConfig)
        /// y recarga el combo. Valida que no sea vacía ni "Todas las ubicaciones".
        /// </summary>
        private void EliminarUbicacionExistente()
        {
            string? nombre = PedirUbicacion("Ingrese la ubicación a eliminar (Ej: M5):");

            if (string.IsNullOrWhiteSpace(nombre) ||
                nombre == FiltroTodasUbicaciones ||
                nombre == ItemAgregarNueva ||
                nombre == ItemEliminarUbicacion)
            {
                VolverATodasLasUbicaciones(); // cancelado, vacío o valor inválido
                return;
            }

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText = "DELETE FROM UbicacionesConfig WHERE NombreUbicacion = @nombre;";
                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar la ubicación: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            CargarUbicacionesEnFiltro(); // sin la ubicación; vuelve a "Todas" si ya no existía
        }

        /// <summary>Fija el índice 0 ("Todas las ubicaciones") y refiltra.</summary>
        private void VolverATodasLasUbicaciones()
        {
            _cargandoFiltroUbicacion = true;
            try { _cmbFiltroUbicacion!.SelectedIndex = 0; }
            finally { _cargandoFiltroUbicacion = false; }
            AplicarFiltro(txtBuscar.Text.Trim());
        }

        /// <summary>Selecciona una ubicación concreta del combo y refiltra (o "Todas").</summary>
        private void SeleccionarUbicacion(string ubicacion)
        {
            _cargandoFiltroUbicacion = true;
            try
            {
                int indice = _cmbFiltroUbicacion!.Items.IndexOf(ubicacion);
                _cmbFiltroUbicacion.SelectedIndex = indice >= 0 ? indice : 0;
            }
            finally { _cargandoFiltroUbicacion = false; }
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
        /// - "En Sala" → fondo amarillo suave (LemonChiffon) y texto gris oscuro (legible)
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
            else if (string.Equals(valor, "En Sala", StringComparison.OrdinalIgnoreCase))
            {
                // Amarillo suave LemonChiffon con texto casi negro: contraste sin agredir la vista
                e.CellStyle.BackColor = Color.FromArgb(255, 250, 205);
                e.CellStyle.ForeColor = Color.FromArgb(45, 45, 45);
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