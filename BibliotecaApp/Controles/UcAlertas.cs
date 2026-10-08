using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

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
                Margin = new Padding(10, 0, 10, 10),
                Visible = false // Oculto: se usan botones individuales por fila en la grilla
            };
            btnNotificar.FlatAppearance.BorderSize = 0;
            btnNotificar.Click += btnNotificar_Click;
            Controls.Add(btnNotificar);
            btnNotificar.BringToFront();

            // Suscribir evento de clic en botones de la grilla
            dgvAlertas.CellContentClick += DgvAlertas_CellContentClick;
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
                                                                   AS [Días de Retraso],
                           Notificado
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

                // Mapear columna Notificado (oculta) para persistencia
                if (!dgvAlertas.Columns.Contains("Notificado"))
                {
                    var colNotificado = new DataGridViewTextBoxColumn
                    {
                        Name = "Notificado",
                        DataPropertyName = "Notificado",
                        HeaderText = "Notificado",
                        Visible = false
                    };
                    dgvAlertas.Columns.Add(colNotificado);
                }

                // Agregar columna de botón "Notificar" individual si no existe
                if (!dgvAlertas.Columns.Contains("btnNotificarIndividual"))
                {
                    var btnCol = new DataGridViewButtonColumn
                    {
                        Name = "btnNotificarIndividual",
                        HeaderText = "Notificar",
                        Text = "Enviar",
                        UseColumnTextForButtonValue = false, // Permite cambiar texto por celda
                        FlatStyle = FlatStyle.Flat, // Permite cambiar BackColor
                        FillWeight = 8F,
                        MinimumWidth = 80
                    };
                    dgvAlertas.Columns.Add(btnCol);
                }

                // Valor inicial para cada fila del botón y restaurar estado Notificado
                foreach (DataGridViewRow row in dgvAlertas.Rows)
                {
                    try
                    {
                        // Valor por defecto
                        row.Cells["btnNotificarIndividual"].Value = "Enviar";

                        // Si Notificado == 1, mostrar estado "Enviado" con color verde
                        var notificadoObj = row.Cells["Notificado"].Value;
                        if (notificadoObj != null && notificadoObj != DBNull.Value)
                        {
                            if (Convert.ToInt32(notificadoObj) == 1)
                            {
                                row.Cells["btnNotificarIndividual"].Value = "Enviado";
                                row.Cells["btnNotificarIndividual"].Style.BackColor = Color.LightGreen;
                                row.Cells["btnNotificarIndividual"].Style.ForeColor = Color.DarkGreen;
                                row.Cells["btnNotificarIndividual"].Style.SelectionBackColor = Color.LightGreen;
                                row.Cells["btnNotificarIndividual"].Style.SelectionForeColor = Color.DarkGreen;
                            }
                        }
                    }
                    catch
                    {
                        // Ignorar errores de casteo en fila individual
                    }
                }
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
        /// Notifica a los usuarios morosos por correo (mailto individual por usuario) y genera un CSV de respaldo en la carpeta temporal.
        /// Requiere que exista un botón llamado 'btnNotificar' suscrito a este evento.
        /// </summary>
        private async void btnNotificar_Click(object sender, EventArgs e)
        {
            // 1️⃣ VALIDAR QUE HAY FILAS CON CORREOS VÁLIDOS
            var filasValidas = dgvAlertas.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .Select(r => new
                {
                    Usuario = r.Cells["Usuario"]?.Value?.ToString()?.Trim() ?? "",
                    Correo = r.Cells["Correo"]?.Value?.ToString()?.Trim() ?? "",
                    TituloLibro = r.Cells["Título del Libro"]?.Value?.ToString()?.Trim() ?? ""
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Correo) && x.Correo.Contains("@"))
                .ToList();

            if (filasValidas.Count == 0)
            {
                MessageBox.Show("No hay correos válidos para notificar.", "Biblioteca CUBO",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2️⃣ GENERAR CSV DE RESPALDO EN LA CARPETA TEMPORAL DEL SISTEMA
            string csvPath = null;
            try
            {
                string tempPath = Path.GetTempPath();
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                csvPath = Path.Combine(tempPath, $"Morosos_Notificados_{timestamp}.csv");

                var sb = new StringBuilder();
                sb.AppendLine("Usuario,Titulo,Correo"); // Encabezados

                foreach (var fila in filasValidas)
                {
                    // Escapar comillas y envolver en comillas si contiene coma, salto de línea o comillas
                    string Escape(string s) => s.Contains(',') || s.Contains('"') || s.Contains('\n')
                        ? "\"" + s.Replace("\"", "\"\"") + "\""
                        : s;

                    sb.AppendLine($"{Escape(fila.Usuario)},{Escape(fila.TituloLibro)},{Escape(fila.Correo)}");
                }

                File.WriteAllText(csvPath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception exCsv)
            {
                MessageBox.Show($"No se pudo generar el CSV en la carpeta temporal:\n{exCsv.Message}",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Continuamos para intentar abrir los mailto
            }

            // 3️⃣ CREAR Y LANZAR MAILTO INDIVIDUAL POR CADA USUARIO MOROSO
            // W-05: Fire-and-forget para no bloquear el hilo de UI al abrir múltiples mailto:
            int emailsEnviados = 0;
            foreach (var fila in filasValidas)
            {
                // Capturar variables locales para el closure
                string usuario = fila.Usuario;
                string correo = fila.Correo;
                string tituloLibro = fila.TituloLibro;

                Task.Run(() =>
                {
                    try
                    {
                        string subject = "Aviso de préstamo vencido - Biblioteca CUBO";
                        string body = $"Estimado/a {usuario}, le informamos que el plazo para devolver el material bibliográfico '{tituloLibro}' ha expirado. Le solicitamos amablemente acercarse a las instalaciones del CUBO para devolver el libro a la brevedad. Gracias.";

                        string uri = $"mailto:{correo}" +
                                     $"?subject={Uri.EscapeDataString(subject)}" +
                                     $"&body={Uri.EscapeDataString(body)}";

                        var psi = new ProcessStartInfo(uri) { UseShellExecute = true };
                        Process.Start(psi);

                        // Incrementar contador de forma thread-safe
                        Interlocked.Increment(ref emailsEnviados);
                    }
                    catch (Exception exMail)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al abrir mailto para {correo}: {exMail.Message}");
                    }
                });
            }

            // Pequeña pausa para dar tiempo a que se lancen los procesos (fire-and-forget)
            await Task.Delay(100);

            // 4️⃣ CONFIRMACIÓN FINAL
            string msg = $"Se abrieron {emailsEnviados} ventana(s) de correo personalizadas (una por usuario moroso).";
            if (!string.IsNullOrEmpty(csvPath) && File.Exists(csvPath))
                msg += $"\n\nRespaldo CSV guardado en:\n{csvPath}";
            else
                msg += "\n\n⚠ No se pudo generar el archivo CSV de respaldo.";

            MessageBox.Show(msg, "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Maneja el clic en la columna de botones "Notificar" individual por cada moroso.
        /// Abre un mailto: personalizado y da feedback visual en la celda.
        /// </summary>
        private void DgvAlertas_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            // Validar que sea clic en la columna de botones y fila válida
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvAlertas.Columns[e.ColumnIndex].Name != "btnNotificarIndividual") return;

            var fila = dgvAlertas.Rows[e.RowIndex];

            // Extraer datos de la fila
            string nombreUsuario = fila.Cells["Usuario"].Value?.ToString()?.Trim() ?? "";
            string correo = fila.Cells["Correo"].Value?.ToString()?.Trim() ?? "";
            string tituloLibro = fila.Cells["Título del Libro"].Value?.ToString()?.Trim() ?? "";

            // Validar que hay correo
            if (string.IsNullOrWhiteSpace(correo) || !correo.Contains("@"))
            {
                MessageBox.Show("El usuario no tiene un correo electrónico válido registrado.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Construir URI mailto: individual
                string subject = Uri.EscapeDataString("Aviso de préstamo vencido - Biblioteca CUBO");
                string body = Uri.EscapeDataString(
                    $"Estimado/a {nombreUsuario}, le informamos que el plazo para devolver el material bibliográfico '{tituloLibro}' ha expirado. Le solicitamos amablemente acercarse a las instalaciones del CUBO para devolver el libro a la brevedad. Gracias.");

                string mailtoUri = $"mailto:{correo}?subject={subject}&body={body}";

                // Lanzar cliente de correo por defecto
                var psi = new ProcessStartInfo(mailtoUri) { UseShellExecute = true };
                Process.Start(psi);

                // Persistir en BD: Notificado = 1
                int idPrestamo = Convert.ToInt32(fila.Cells["ID"].Value);
                using (var conexion = ConexionDB.ObtenerConexion())
                using (var updateCmd = conexion.CreateCommand())
                {
                    updateCmd.CommandText = "UPDATE PrestamosExternos SET Notificado = 1 WHERE ID = @id;";
                    updateCmd.Parameters.AddWithValue("@id", idPrestamo);
                    updateCmd.ExecuteNonQuery();
                }

                // Feedback visual: cambiar botón a "Enviado" con color verde
                var celda = dgvAlertas.Rows[e.RowIndex].Cells[e.ColumnIndex];
                celda.Value = "Enviado";
                celda.Style.BackColor = Color.LightGreen;
                celda.Style.ForeColor = Color.DarkGreen;
                celda.Style.SelectionBackColor = Color.LightGreen;
                celda.Style.SelectionForeColor = Color.DarkGreen;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el cliente de correo:\n{ex.Message}",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
