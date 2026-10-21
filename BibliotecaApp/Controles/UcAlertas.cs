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
    public partial class UcAlertas : UserControl
    {
        public UcAlertas()
        {
            InitializeComponent();

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

            dgvAlertas.Paint += DgvAlertas_Paint;
        }

        private void UcAlertas_Load(object sender, EventArgs e)
        {
            CargarMorosos();
        }

        private void DgvAlertas_Paint(object? sender, PaintEventArgs e)
        {
            if (dgvAlertas.Rows.Count > 0) return; // hay datos: nada que pintar

            using var formato = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var fuente = EstiloUI.FuenteVacia; 
            var area = e.ClipRectangle;

            e.Graphics.DrawString(
                "No hay usuarios morosos",
                fuente,
                Brushes.Gray,
                new RectangleF(area.X, area.Y, area.Width, area.Height),
                formato);
        }

        public void Actualizar()
        {
            CargarMorosos();
        }

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
                           CorreoAlertaEnviado
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

                if (!dgvAlertas.Columns.Contains("CorreoAlertaEnviado"))
                {
                    var colNotificado = new DataGridViewTextBoxColumn
                    {
                        Name = "CorreoAlertaEnviado",
                        DataPropertyName = "CorreoAlertaEnviado",
                        HeaderText = "CorreoAlertaEnviado",
                        Visible = false
                    };
                    dgvAlertas.Columns.Add(colNotificado);
                }

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

                        var notificadoObj = row.Cells["CorreoAlertaEnviado"].Value;
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

        private void dgvAlertas_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                $"DataError en UcAlertas: {e.Exception?.GetType().Name}: {e.Exception?.Message} (col {e.ColumnIndex}, fila {e.RowIndex})");
            e.ThrowException = false;
        }

        private async void btnNotificar_Click(object sender, EventArgs e)
        {
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

            string csvPath = null;
            try
            {
                string tempPath = Path.GetTempPath();
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
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
                        string titulo = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                            .ToTitleCase(tituloLibro.ToLower());

                        string subject = "Aviso de prestamo vencido - Biblioteca CUBO";
                        string body = $"Estimado/a {usuario},\r\n\r\n" +
                                      $"El plazo para devolver \"{titulo}\" ha vencido.\r\n" +
                                      $"Quedamos atentos para recibirlo en el CUBO.\r\n\r\n" +
                                      $"Gracias.";

                        string uri = $"mailto:{correo}" +
                                     $"?subject={Uri.EscapeDataString(subject)}" +
                                     $"&body={Uri.EscapeDataString(body)}";

                        var psi = new ProcessStartInfo(uri) { UseShellExecute = true };
                        Process.Start(psi);

                        Interlocked.Increment(ref emailsEnviados);
                    }
                    catch (Exception exMail)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al abrir mailto para {correo}: {exMail.Message}");
                    }
                });
            }

            await Task.Delay(100);

            string msg = $"Se abrieron {emailsEnviados} ventana(s) de correo personalizadas (una por usuario moroso).";
            if (!string.IsNullOrEmpty(csvPath) && File.Exists(csvPath))
                msg += $"\n\nRespaldo CSV guardado en:\n{csvPath}";
            else
                msg += "\n\n⚠ No se pudo generar el archivo CSV de respaldo.";

            MessageBox.Show(msg, "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

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
                string titulo = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                    .ToTitleCase(tituloLibro.ToLower());

                string subject = "Aviso de prestamo vencido - Biblioteca CUBO";
                string body = $"Estimado/a {nombreUsuario},\r\n\r\n" +
                              $"El plazo para devolver \"{titulo}\" ha vencido.\r\n" +
                              $"Quedamos atentos para recibirlo en el CUBO.\r\n\r\n" +
                              $"Gracias.";

                string mailtoUri = $"mailto:{correo}" +
                                   $"?subject={Uri.EscapeDataString(subject)}" +
                                   $"&body={Uri.EscapeDataString(body)}";

                // Lanzar cliente de correo por defecto
                var psi = new ProcessStartInfo(mailtoUri) { UseShellExecute = true };
                Process.Start(psi);

                int idPrestamo = Convert.ToInt32(fila.Cells["ID"].Value);
                using (var conexion = ConexionDB.ObtenerConexion())
                using (var updateCmd = conexion.CreateCommand())
                {
                    updateCmd.CommandText = "UPDATE PrestamosExternos SET CorreoAlertaEnviado = 1 WHERE ID = @id;";
                    updateCmd.Parameters.AddWithValue("@id", idPrestamo);
                    updateCmd.ExecuteNonQuery();
                }

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
