using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    /// <summary>
    /// Módulo de solo lectura: muestra el historial completo de préstamos externos
    /// con coloreo dinámico por estado (Pendiente/Renovado/Entregado).
    /// </summary>
    public partial class UcHistorialPrestamos : UserControl
    {
        // Variables de paginación - Inicialización defensiva (valores > 0)
        private int paginaActual = 1;
        private int tamanoPagina = 25;
        private int totalPaginas = 1;

        public UcHistorialPrestamos()
        {
            InitializeComponent();
        }

        private void UcHistorialPrestamos_Load(object sender, EventArgs e)
        {
            this.Dock = DockStyle.Fill;
            CalcularTotalPaginas();
            CargarHistorial();
        }

        /// <summary>
        /// Calcula el total de páginas basándose en la cantidad de registros en la base de datos.
        /// </summary>
        private void CalcularTotalPaginas()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = "SELECT COUNT(*) FROM PrestamosExternos;";

                var resultado = comando.ExecuteScalar();
                int totalRegistros = (resultado != null && resultado != DBNull.Value) ? Convert.ToInt32(resultado) : 0;

                // Fallback de seguridad para tamanoPagina
                if (tamanoPagina <= 0) tamanoPagina = 25;

                totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanoPagina);
                if (totalPaginas <= 0) totalPaginas = 1; // Nunca puede haber 0 páginas
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al calcular total de páginas: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                totalPaginas = 1;
            }
        }

        /// <summary>
        /// Consulta el historial de préstamos externos paginado y lo vincula al DataGridView.
        /// </summary>
        public void CargarHistorial()
        {
            try
            {
                // 1. Desactivar Fill ANTES de tocar datos/columnas
                dgvHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

                // 2. Blindaje defensivo para paginaActual y offset
                if (paginaActual <= 0) paginaActual = 1;
                int offset = (paginaActual - 1) * tamanoPagina;
                if (offset < 0) offset = 0;

                using var conexion = ConexionDB.ObtenerConexion();
                using var comando = conexion.CreateCommand();
                comando.CommandText = @"
                    SELECT
                        ID,
                        NombreUsuario                         AS [Usuario],
                        DUI,
                        Correo,
                        Telefono,
                        TituloLibro                           AS [Título del Libro],
                        strftime('%d/%m/%Y', FechaPrestamo)   AS [Fecha Préstamo],
                        PersonalPresto                        AS [Personal que Prestó],
                        CASE WHEN IFNULL(FechaRenovacion, '') = '' THEN '-'
                             ELSE strftime('%d/%m/%Y', FechaRenovacion) END AS [Fecha Renovación],
                        PersonalRenovo                        AS [Personal que Renovó],
                        strftime('%d/%m/%Y', FechaEntrega)    AS [Entrega Esperada],
                        CASE WHEN IFNULL(FechaDevolucion, '') = '' THEN '-'
                             ELSE strftime('%d/%m/%Y', FechaDevolucion) END AS [Fecha Devolución],
                        PersonalRecibio                       AS [Personal que Recibió],
                        EstadoLibro                           AS [Estado]
                    FROM PrestamosExternos
                    ORDER BY FechaPrestamo DESC, ID DESC
                    LIMIT @tamanoPagina OFFSET @offset;";

                comando.Parameters.AddWithValue("@tamanoPagina", tamanoPagina);
                comando.Parameters.AddWithValue("@offset", offset);

                var tabla = new DataTable();
                using (var lector = comando.ExecuteReader())
                {
                    tabla.Load(lector);
                }

                dgvHistorial.DataSource = tabla;

                // 3. Configurar columnas DESPUÉS de DataSource, ANTES de Fill
                ConfigurarColumnasPostDataSource();

                // 4. ScrollBars y UI - Both permite scroll horizontal si las columnas no caben
                dgvHistorial.ScrollBars = ScrollBars.Both;

                lblPagina.Text = $"Página {paginaActual} de {totalPaginas}";
                btnAnterior.Enabled = (paginaActual > 1);
                btnSiguiente.Enabled = (paginaActual < totalPaginas);

                // Fuerza bruta: activar Fill cuando columnas existan y Width > 0
                this.BeginInvoke(new Action(() =>
                {
                    if (dgvHistorial.Width > 0 && dgvHistorial.Columns.Count > 0)
                    {
                        dgvHistorial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el historial: " + ex.Message,
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Configura columnas después de asignar DataSource. NO toca FillWeight de columnas ocultas.
        /// </summary>
        private void ConfigurarColumnasPostDataSource()
        {
            // Ocultar columna ID - Fill ignora automáticamente columnas invisibles
            if (dgvHistorial.Columns.Contains("ID"))
            {
                dgvHistorial.Columns["ID"].Visible = false;
                // NO tocar FillWeight - dejar en default (100). Fill excluye invisibles.
            }

            // Asegurar que columnas visibles tengan FillWeight > 0 (default es 100)
            foreach (DataGridViewColumn col in dgvHistorial.Columns)
            {
                if (col.Visible && col.FillWeight <= 0)
                    col.FillWeight = 100;

                // Permitir compresión agresiva en modo Fill si el espacio es insuficiente
                col.MinimumWidth = 40;
            }
        }

        private void btnAnterior_Click(object sender, EventArgs e)
        {
            if (paginaActual > 1)
            {
                paginaActual--;
                CargarHistorial();
            }
        }

        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            if (paginaActual < totalPaginas)
            {
                paginaActual++;
                CargarHistorial();
            }
        }

        /// <summary>
        /// Colorea únicamente la celda de la columna "Estado" según el estado del préstamo.
        /// </summary>
        private void dgvHistorial_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Solo aplicar color a la columna "Estado"
            var colEstado = dgvHistorial.Columns["Estado"];
            if (colEstado == null || e.ColumnIndex != colEstado.Index) return;

            var estadoObj = e.Value;
            if (estadoObj == null) return;

            string estado = estadoObj.ToString()?.Trim() ?? "";

            Color colorFondo;
            switch (estado)
            {
                case "Pendiente":
                    colorFondo = Color.FromArgb(255, 204, 204); // Rojo suave
                    break;
                case "Renovado":
                    colorFondo = Color.FromArgb(204, 235, 255); // Celeste suave
                    break;
                case "Entregado":
                    colorFondo = Color.FromArgb(204, 255, 204); // Verde suave
                    break;
                default:
                    return; // Sin color para estados no reconocidos
            }

            // Aplicar color solo a esta celda
            e.CellStyle.BackColor = colorFondo;
            e.CellStyle.SelectionBackColor = Color.FromArgb(
                Math.Max(0, colorFondo.R - 30),
                Math.Max(0, colorFondo.G - 30),
                Math.Max(0, colorFondo.B - 30));
        }
    }
}