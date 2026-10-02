using System.Data;
using System.Drawing;
using System.Reflection;
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
        private DataTable _datosInventario = new();
        private DataView _vistaFiltrada;

        public UcInventario()
        {
            InitializeComponent();

            typeof(DataGridView)
                .GetProperty("DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(dgvInventario, true, null);

            // Suscribir evento de formato condicional para la columna Disponibilidad
            dgvInventario.CellFormatting += dgvInventario_CellFormatting;

            _vistaFiltrada = new DataView(_datosInventario);
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
                    ORDER BY Titulo;";

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

                _datosInventario = tabla;
                _vistaFiltrada = new DataView(_datosInventario);

                dgvInventario.DataSource = _vistaFiltrada;
                OcultarColumnaBusqueda();
                AjustarColumnas();
                lblContador.Text = $"{_vistaFiltrada.Count:N0} libro(s)";

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
            if (texto.Length == 0)
            {
                _vistaFiltrada.RowFilter = string.Empty;
            }
            else
            {
                string termino = EstiloUI.RemoverTildes(texto).ToLowerInvariant();

                // Escapar caracteres especiales de DataView RowFilter (LIKE).
                termino = termino
                    .Replace("[", "[[]")
                    .Replace("*", "[*]")
                    .Replace("%", "[%]")
                    .Replace("'", "''");

                _vistaFiltrada.RowFilter =
                    $"[{ColBusqueda}] LIKE '%{termino}%'";
            }

            lblContador.Text = $"{_vistaFiltrada.Count:N0} libro(s)";
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
    }
}
