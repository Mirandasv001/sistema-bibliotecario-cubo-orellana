using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    public partial class UcInventario : UserControl
    {
        private const string ColBusqueda = "_BusquedaNormalizada";

        private const string OrdenPorUbicacion = "[Ubicación] ASC, [Código] ASC";
        private DataTable _datosInventario = new();
        private DataView _vistaFiltrada;

        private Button? _btnImportarCSV;
        private Button? _btnVaciarInventario;

        private ComboBox? _cmbFiltroUbicacion;
        private Label? _lblFiltroUbicacion;
        private bool _cargandoFiltroUbicacion; 
        private Button? btnAgregarLibro;

        private Button? btnEliminarLibro;
        private Button? btnEditarLibro;

        private const string FiltroTodasUbicaciones = "Todas las ubicaciones";

        private const string ItemAgregarNueva = "+ Agregar nueva";
        private const string ItemEliminarUbicacion = "- Eliminar ubicación";

        public UcInventario()
        {
            InitializeComponent();

            typeof(DataGridView)
                .GetProperty("DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(dgvInventario, true, null);

            dgvInventario.DefaultCellStyle.SelectionBackColor = EstiloUI.Acento;
            dgvInventario.DefaultCellStyle.SelectionForeColor = Color.White;

            // Suscribir evento de formato condicional para la columna Disponibilidad
            dgvInventario.CellFormatting += dgvInventario_CellFormatting;

            _vistaFiltrada = new DataView(_datosInventario);

            lblBuscar = new Label
            {
                Name = "lblBuscar",
                Text = "Buscar:",
                AutoSize = true,
                Font = EstiloUI.Etiqueta(),               
                ForeColor = Color.FromArgb(43, 45, 66),   // #2B2D42, idéntico a su par
                Location = new Point(16, txtBuscar.Location.Y + 3) 
            };
            panelBusqueda.Controls.Add(lblBuscar);

            int colControles = CrearFiltroUbicacion();

            txtBuscar.PlaceholderText = "Búsqueda de libros...";

            txtBuscar.AutoSize = false;
            txtBuscar.Height += 5;
            txtBuscar.Width += 20; 
            int bordeDerechoBarra = txtBuscar.Location.X + txtBuscar.Width;
            txtBuscar.Location = new Point(colControles, txtBuscar.Location.Y);
            txtBuscar.Width = bordeDerechoBarra - txtBuscar.Location.X;

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
            btnAgregarLibro.BackColor = EstiloUI.FondoOscuro; 
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
            btnEliminarLibro.BackColor = _btnVaciarInventario.BackColor; 
            btnEliminarLibro.ForeColor = _btnVaciarInventario.ForeColor; 
            btnEliminarLibro.Click += btnEliminarLibro_Click;
            panelEncabezado.Controls.Add(btnEliminarLibro);

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
            btnEditarLibro.BackColor = EstiloUI.Acento;   
            btnEditarLibro.ForeColor = Color.White;
            btnEditarLibro.Click += btnEditarLibro_Click;
            panelEncabezado.Controls.Add(btnEditarLibro);

            this.Load += (s, e) =>
            {
                int margenIzquierdo = 25; // Separación respecto al título
                int espaciadoEntreBotones = 12;

                // Centrado vertical respecto al título
                int posY = lblTitulo.Location.Y + ((lblTitulo.Height - _btnImportarCSV!.Height) / 2);
                if (posY < 10) posY = lblTitulo.Location.Y;

                int posXImportar = lblTitulo.Right + margenIzquierdo;
                _btnImportarCSV.Location = new Point(posXImportar, posY);

                int posXVaciar = _btnImportarCSV.Right + espaciadoEntreBotones;
                _btnVaciarInventario!.Location = new Point(posXVaciar, posY);

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

        public void Actualizar()
        {
            CargarInventario();
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            AplicarFiltro(txtBuscar.Text.Trim());
        }

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

                tabla.Columns.Add(ColBusqueda, typeof(string));
                foreach (DataRow fila in tabla.Rows)
                {
                    string codigo = fila["Código"]?.ToString() ?? "";
                    string titulo = fila["Título"]?.ToString() ?? "";
                    fila[ColBusqueda] = EstiloUI.RemoverTildes(
                        codigo + " " + titulo).ToLowerInvariant();
                }

                _vistaFiltrada?.Dispose();
                _datosInventario?.Dispose();

                _datosInventario = tabla;
                _vistaFiltrada = new DataView(_datosInventario);
                _vistaFiltrada.Sort = OrdenPorUbicacion;

                dgvInventario.DataSource = _vistaFiltrada;
                OcultarColumnaBusqueda();
                AjustarColumnas();
                lblContador.Text = $"{_vistaFiltrada.Count:N0} libro(s)";

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

        private void AplicarFiltro(string texto)
        {
            _vistaFiltrada.Sort = OrdenPorUbicacion;

            var condiciones = new List<string>();

            if (texto.Length > 0)
            {
                string termino = EstiloUI.RemoverTildes(texto).ToLowerInvariant();

                termino = termino
                    .Replace("[", "[[]")
                    .Replace("*", "[*]")
                    .Replace("%", "[%]")
                    .Replace("'", "''");

                condiciones.Add($"([{ColBusqueda}] LIKE '%{termino}%' OR [Ubicación] LIKE '%{termino}%')");
            }

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

            _vistaFiltrada.RowFilter = condiciones.Count > 0
                ? string.Join(" AND ", condiciones)
                : string.Empty;

            lblContador.Text = $"{_vistaFiltrada.Count:N0} libro(s)";
        }

        private int CrearFiltroUbicacion()
        {
            const int colEtiquetas = 16;   
            const int margenEtiqueta = 10; 

            _lblFiltroUbicacion = new Label
            {
                Name = "lblFiltroUbicacion",
                Text = "Ubicación:",
                AutoSize = true,
                Font = EstiloUI.Etiqueta(),
                ForeColor = Color.FromArgb(43, 45, 66), // #2B2D42, mismo tono del título
                Location = new Point(colEtiquetas, 56)  
            };

            _cmbFiltroUbicacion = new ComboBox
            {
                Name = "cmbFiltroUbicacion",
                Location = new Point(colEtiquetas, 53), // X definitivo: columna compartida, más abajo
                Width = 195,                             
                DropDownWidth = 260                      // popup algo más ancho: se lee completo
            };
            EstiloUI.EstilizarEntrada(_cmbFiltroUbicacion); // DropDownList + estilo uniforme de la app
            _cmbFiltroUbicacion.SelectedIndexChanged += CmbFiltroUbicacion_SelectedIndexChanged;

            panelBusqueda.Height = 107;

            panelBusqueda.Controls.Add(_lblFiltroUbicacion);
            panelBusqueda.Controls.Add(_cmbFiltroUbicacion);

            int colControles = Math.Max(
                lblBuscar.Left + lblBuscar.GetPreferredSize(Size.Empty).Width,
                _lblFiltroUbicacion.Left + _lblFiltroUbicacion.GetPreferredSize(Size.Empty).Width)
                + margenEtiqueta;

            _cmbFiltroUbicacion.Location = new Point(colControles, _cmbFiltroUbicacion.Location.Y);
            return colControles;
        }

        private void btnAgregarLibro_Click(object? sender, EventArgs e)
        {
            using var modal = CrearFormularioAgregarLibro();
            if (modal.ShowDialog(this) == DialogResult.OK)
                CargarInventario();
        }

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
                fila.Cells["Código"]?.Value?.ToString() ?? "",      
                fila.Cells["Título"]?.Value?.ToString() ?? "",      
                fila.Cells["Autor"]?.Value?.ToString() ?? "",       
                fila.Cells["Editorial"]?.Value?.ToString() ?? "",   
                fila.Cells["Estado"]?.Value?.ToString() ?? "",      
                fila.Cells["Ubicación"]?.Value?.ToString() ?? ""    
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

        private void btnEliminarLibro_Click(object? sender, EventArgs e)
        {
            object? valorCodigo = dgvInventario.CurrentRow?.Cells["Código"]?.Value;
            string codigo = valorCodigo?.ToString() ?? string.Empty;
            if (codigo.Length == 0)
            {
                MessageBox.Show("Seleccione un libro de la lista para eliminar.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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

        private void CargarUbicacionesEnFiltro()
        {
            if (_cmbFiltroUbicacion == null) return;

            string seleccionAnterior = _cmbFiltroUbicacion.SelectedItem?.ToString()
                ?? FiltroTodasUbicaciones;

            if (seleccionAnterior == ItemAgregarNueva || seleccionAnterior == ItemEliminarUbicacion)
                seleccionAnterior = FiltroTodasUbicaciones;

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

                _cmbFiltroUbicacion.Items.Add(ItemEliminarUbicacion);
                _cmbFiltroUbicacion.Items.Add(ItemAgregarNueva);

                int indice = _cmbFiltroUbicacion.Items.IndexOf(seleccionAnterior);
                _cmbFiltroUbicacion.SelectedIndex = indice >= 0 ? indice : 0;
            }
            finally
            {
                _cargandoFiltroUbicacion = false;
            }

            AplicarFiltro(txtBuscar.Text.Trim());
        }

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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la ubicación: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                VolverATodasLasUbicaciones();
                return;
            }

            CargarUbicacionesEnFiltro(); 
            SeleccionarUbicacion(nueva); // la deja seleccionada y refiltra
        }

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

            CargarUbicacionesEnFiltro(); 
        }

        private void VolverATodasLasUbicaciones()
        {
            _cargandoFiltroUbicacion = true;
            try { _cmbFiltroUbicacion!.SelectedIndex = 0; }
            finally { _cargandoFiltroUbicacion = false; }
            AplicarFiltro(txtBuscar.Text.Trim());
        }

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

        private void dgvInventario_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
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
        }

        private void BtnVaciarInventario_Click(object? sender, EventArgs e)
        {
            using (var frmAuth = new FormAutenticacion("AdminCubo", "Admin123$"))
            {
                if (frmAuth.ShowDialog(this) != DialogResult.OK) return; // Si cancela o falla auth, salir
            }

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

                ConexionDB.MarcarInventarioVaciado(true);

                dgvInventario.DataSource = null;

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

        private void BtnImportarCSV_Click(object? sender, EventArgs e)
        {
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

                    if (campos.Length < 6)
                    {
                        lineasProcesadas++;
                        continue; // Saltar líneas inválidas
                    }

                    string codigo = campos.Length > 0 ? campos[0].Trim() : "";
                    string titulo = campos.Length > 1 ? campos[1].Trim() : "";
                    string autor = campos.Length > 2 ? campos[2].Trim() : "";
                    string editorial = campos.Length > 3 ? campos[3].Trim() : "";
                    string estado = campos.Length > 4 ? campos[4].Trim() : "";
                    string ubicacion = campos.Length > 5 ? campos[5].Trim() : "";
                    string disponibilidadCsv = (campos.Length > 6 && !string.IsNullOrWhiteSpace(campos[6]))
                        ? campos[6].Trim()
                        : "Disponible";

                    string disponibilidadFinal = disponibilidadCsv;
                    pCheckCodigo.Value = codigo;
                    object? result = checkCmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        string disponibilidadActual = result.ToString() ?? "";
                        if (string.Equals(disponibilidadActual, "Prestado", StringComparison.OrdinalIgnoreCase))
                        {
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