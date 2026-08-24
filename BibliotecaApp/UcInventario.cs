using System.Data;
using System.Reflection;
using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    /// <summary>
    /// Apartado C: inventario general de libros con búsqueda en tiempo real.
    /// </summary>
    public partial class UcInventario : UserControl
    {
        public UcInventario()
        {
            InitializeComponent();

            // Doble búfer para un desplazamiento suave con ~4,000 filas.
            typeof(DataGridView)
                .GetProperty("DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(dgvInventario, true, null);
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
            CargarInventario(txtBuscar.Text.Trim());
        }

        private void CargarInventario(string filtro = "")
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();

                if (filtro.Length == 0)
                {
                    comando.CommandText = @"
                        SELECT Codigo   AS Código,
                               Titulo   AS Título,
                               Autor,
                               Editorial,
                               Estado,
                               Ubicacion AS Ubicación,
                               Disponibilidad
                        FROM Libros ORDER BY Titulo;";
                }
                else
                {
                    // Se escapan los comodines LIKE para que la búsqueda sea literal.
                    string patron = "%" + filtro
                        .Replace("\\", "\\\\")
                        .Replace("%", "\\%")
                        .Replace("_", "\\_") + "%";

                    comando.CommandText = @"
                        SELECT Codigo   AS Código,
                               Titulo   AS Título,
                               Autor,
                               Editorial,
                               Estado,
                               Ubicacion AS Ubicación,
                               Disponibilidad
                        FROM Libros
                        WHERE Codigo LIKE $patron ESCAPE '\'
                           OR Titulo LIKE $patron ESCAPE '\'
                        ORDER BY Titulo;";
                    comando.Parameters.AddWithValue("$patron", patron);
                }

                var tabla = new DataTable();
                using (var lector = comando.ExecuteReader())
                {
                    tabla.Load(lector);
                }

                dgvInventario.DataSource = tabla;
                lblContador.Text = $"{tabla.Rows.Count:N0} libro(s)";

                if (dgvInventario.Columns["Título"] != null)
                    dgvInventario.Columns["Título"]!.FillWeight = 55;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el inventario: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
