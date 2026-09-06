using System.Data;

namespace BibliotecaApp
{
    /// <summary>
    /// Apartado: alertas de préstamos vencidos. Consulta los préstamos
    /// activos (EstadoLibro = 'Pendiente') cuya fecha esperada ya pasó y
    /// muestra a los usuarios morosos con sus datos de contacto y días de retraso.
    /// </summary>
    public partial class UcAlertas : UserControl
    {
        public UcAlertas()
        {
            InitializeComponent();
        }

        private void UcAlertas_Load(object sender, EventArgs e)
        {
            CargarMorosos();
        }

        /// <summary>Método público invocado por Form1 al navegar a este apartado.</summary>
        public void Actualizar()
        {
            CargarMorosos();
        }

        /// <summary>
        /// Trae los préstamos activos y vencidos, con el cálculo dinámico de los
        /// días de retraso.
        ///
        /// CORRECCIÓN (producto cartesiano):
        /// Antes cruzábamos 'PrestamosExternos' con 'Libros' mediante
        ///   LEFT JOIN Libros l ON l.Titulo = p.TituloLibro
        /// Pero un mismo título tiene MUCHAS copias físicas en 'Libros' (cada una
        /// con su Codigo único). Ese enlace por Título multiplicaba cada préstamo
        /// por el número de copias → filas duplicadas visualmente.
        ///
        /// La tabla 'PrestamosExternos' YA guarda internamente el NombreUsuario,
        /// Telefono, Correo y TituloLibro del ejemplar prestado. Por eso NO
        /// necesitamos cruzar con 'Libros': un SELECT directo a 'PrestamosExternos'
        /// devuelve estrictamente UNA fila por cada préstamo activo vencido.
        ///
        /// Regla de oro de las BD: si ya tienes los datos que necesitas en una
        /// tabla, NO la cruces con otra solo por comodidad — un JOIN innecesario
        /// por una columna no única (como el título) es una fuente clásica de
        /// duplicación. El JOIN solo tiene sentido si relacionas por clave única
        /// (ej. un futuro campo 'Codigo_Ejemplar' en el préstamo), no por título.
        /// </summary>
        private void CargarMorosos()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    SELECT ID,
                           NombreUsuario                          AS Usuario,
                           Telefono                               AS Teléfono,
                           Correo                                 AS Correo,
                           TituloLibro                            AS [Título del Libro],
                           strftime('%d/%m/%Y', FechaEntrega)     AS [Entrega Esperada],
                           CAST(julianday('now') - julianday(FechaEntrega) AS INTEGER)
                                                                   AS [Días de Retraso]
                    FROM PrestamosExternos
                    WHERE EstadoLibro = 'Pendiente'
                      AND julianday(FechaEntrega) < julianday('now')
                    ORDER BY julianday(FechaEntrega) ASC;";

                var tabla = new DataTable();
                using (var lector = comando.ExecuteReader())
                {
                    tabla.Load(lector);
                }

                dgvAlertas.DataSource = tabla;

                if (dgvAlertas.Columns["ID"] != null)
                    dgvAlertas.Columns["ID"]!.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar las alertas: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Red de seguridad: ante cualquier fallo de formato/mapeo de una celda,
        /// cancela la excepción para que la aplicación no se congele ni muestre
        /// el cuadro de diálogo predeterminado de Windows.
        /// </summary>
        private void dgvAlertas_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                $"DataError en UcAlertas: {e.Exception?.GetType().Name}: {e.Exception?.Message} (col {e.ColumnIndex}, fila {e.RowIndex})");
            e.ThrowException = false;
        }
    }
}
