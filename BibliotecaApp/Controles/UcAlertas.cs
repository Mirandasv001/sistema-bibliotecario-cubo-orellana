using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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

            // Botón "Notificar morosos" creado por código (evita tocar el diseñador)
            var btnNotificar = new Button
            {
                Name = "btnNotificar",
                Text = "Notificar morosos",
                Dock = DockStyle.Bottom,
                Height = 44,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 102, 153),   // Azul oscuro tipo CUBO
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 10, 10)
            };
            btnNotificar.FlatAppearance.BorderSize = 0;
            btnNotificar.Click += btnNotificar_Click;
            Controls.Add(btnNotificar);
            btnNotificar.BringToFront();
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
                    WHERE EstadoLibro IN ('Pendiente', 'Renovado')
                      AND date(FechaEntrega) < date('now', 'localtime')
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

        /// <summary>
        /// Notifica a los usuarios morosos por correo (mailto) y genera un CSV de respaldo en el Escritorio.
        /// Requiere que exista un botón llamado 'btnNotificar' suscrito a este evento.
        /// </summary>
        private void btnNotificar_Click(object sender, EventArgs e)
        {
            // 1️⃣ EXTRAER Y FILTRAR CORREOS VÁLIDOS (distinct, no nulos, con @)
            var correos = dgvAlertas.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .Select(r => r.Cells["Correo"]?.Value?.ToString()?.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c) && c.Contains("@"))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (correos.Count == 0)
            {
                MessageBox.Show("No hay correos válidos para notificar.", "Biblioteca CUBO",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2️⃣ GENERAR CSV DE RESPALDO EN EL ESCRITORIO
            string csvPath = null;
            try
            {
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                csvPath = Path.Combine(desktop, $"Morosos_Notificados_{timestamp}.csv");

                var sb = new StringBuilder();
                sb.AppendLine("Usuario,Titulo,Correo"); // Encabezados

                foreach (DataGridViewRow row in dgvAlertas.Rows)
                {
                    if (row.IsNewRow) continue;

                    string usuario = row.Cells["Usuario"]?.Value?.ToString()?.Trim() ?? "";
                    string titulo = row.Cells["Título del Libro"]?.Value?.ToString()?.Trim() ?? "";
                    string correo = row.Cells["Correo"]?.Value?.ToString()?.Trim() ?? "";

                    // Escapar comillas y envolver en comillas si contiene coma, salto de línea o comillas
                    string Escape(string s) => s.Contains(',') || s.Contains('"') || s.Contains('\n')
                        ? "\"" + s.Replace("\"", "\"\"") + "\""
                        : s;

                    sb.AppendLine($"{Escape(usuario)},{Escape(titulo)},{Escape(correo)}");
                }

                File.WriteAllText(csvPath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception exCsv)
            {
                MessageBox.Show($"No se pudo generar el CSV en el Escritorio:\n{exCsv.Message}",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Continuamos para intentar abrir el mailto
            }

            // 3️⃣ ARMAR URI MAILTO (BCC para privacidad)
            string emailsBcc = string.Join(",", correos);
            string subject = "Aviso: Préstamo de libro vencido - Biblioteca CUBO";
            string body = "Estimado usuario, le informamos que el plazo para devolver el material bibliográfico ha expirado. Le solicitamos amablemente acercarse a las instalaciones del CUBO para devolver el libro a la brevedad. Gracias.";

            string uri = $"mailto:?bcc={Uri.EscapeDataString(emailsBcc)}" +
                         $"&subject={Uri.EscapeDataString(subject)}" +
                         $"&body={Uri.EscapeDataString(body)}";

            // 4️⃣ LANZAR CLIENTE DE CORREO POR DEFECTO
            try
            {
                var psi = new ProcessStartInfo(uri) { UseShellExecute = true };
                Process.Start(psi);
            }
            catch (Exception exMail)
            {
                MessageBox.Show($"No se pudo abrir el cliente de correo:\n{exMail.Message}",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 5️⃣ CONFIRMACIÓN FINAL
            string msg = "Se abrió la ventana de correo con los destinatarios en copia oculta (BCC).";
            if (!string.IsNullOrEmpty(csvPath) && File.Exists(csvPath))
                msg += $"\n\nRespaldo CSV guardado en:\n{csvPath}";
            else
                msg += "\n\n⚠ No se pudo generar el archivo CSV de respaldo.";

            MessageBox.Show(msg, "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
