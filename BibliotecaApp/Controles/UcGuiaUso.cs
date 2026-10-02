using System.Drawing;
using System.Windows.Forms;

namespace BibliotecaApp
{
    /// <summary>
    /// Guía de Uso a pantalla completa para bibliotecarios.
    /// Muestra el manual operativo completo en un RichTextBox con formato enriquecido.
    /// </summary>
    public partial class UcGuiaUso : UserControl
    {
        public UcGuiaUso()
        {
            InitializeComponent();
        }

        private void UcGuiaUso_Load(object sender, EventArgs e)
        {
            CargarManual();
        }

        /// <summary>
        /// Método público invocado por Form1 al navegar a este apartado.
        /// </summary>
        public void Actualizar()
        {
            CargarManual();
        }

        private void CargarManual()
        {
            try
            {
                rtbManual.Clear();
                rtbManual.SelectionFont = new Font("Segoe UI", 11F, FontStyle.Regular);
                rtbManual.SelectionColor = Color.FromArgb(0x2B, 0x2D, 0x42); // TextoOscuro

                // Texto completo del manual
                string manual = @"SISTEMA DE GESTIÓN BIBLIOTECARIA - CUBO ING. RIGOBERTO ORELLANA
Manual Operativo para Bibliotecarios

CONTROL DE SALA
Este módulo registra las visitas diarias a las instalaciones del CUBO.

Registro: Llene los datos del visitante (Nombre, Género, Edad) y el libro que leerá dentro de las instalaciones. Haga clic en ""Registrar Lectura"".

Devolución Interna: Seleccione al usuario en la tabla y haga clic en ""Marcar Devolución"" cuando termine su lectura.

Seguridad: La eliminación de registros está protegida. Requerirá ingresar nuevamente sus credenciales de administrador para evitar borrados accidentales.

INVENTARIO DE LIBROS
Catálogo digital completo de la biblioteca.

Búsqueda: Puede filtrar el catálogo escribiendo el código del ejemplar o el título del libro en la barra superior.

Préstamo Rápido: Si hace ""Doble Clic"" sobre cualquier libro de la lista, el sistema lo llevará automáticamente al módulo de Préstamos Externos con los datos del libro ya cargados.

PRÉSTAMOS EXTERNOS
Gestión de libros que los usuarios llevan a casa.

Nuevo Préstamo: Ingrese los datos de contacto del usuario. El sistema calculará automáticamente 8 días como fecha límite de entrega.

Devoluciones y Renovaciones: Busque el registro, cambie el Estado en la lista desplegable y asigne el nombre del personal responsable. El sistema validará automáticamente que los campos requeridos estén llenos según la acción.

ALERTAS DE VENCIDOS
Control de morosidad y recuperación de libros.

Notificaciones: La tabla muestra los usuarios que han excedido su fecha límite. Al hacer clic en ""Notificar Morosos"", el sistema abrirá automáticamente el gestor de correos de la computadora (Outlook/Gmail) con un mensaje redactado y los correos en Copia Oculta (CCO) para proteger la privacidad de los usuarios.

ESTADÍSTICAS
Panel de análisis para reportes de alcance.

Uso: Seleccione un rango de fechas (""Desde"" y ""Hasta"") y presione ""Filtrar"". El sistema generará gráficos exactos del total de visitas divididas por género para facilitar los informes administrativos.";

                rtbManual.Text = manual;

                // Aplicar formato enriquecido (negritas a títulos principales)
                AplicarFormato();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando manual: {ex.Message}");
                rtbManual.Text = "Error al cargar el manual de usuario.";
            }
        }

        private void AplicarFormato()
        {
            try
            {
                // Configuración de fuentes
                Font tituloPrincipal = new Font("Segoe UI", 14F, FontStyle.Bold);
                Font tituloSeccion = new Font("Segoe UI", 12F, FontStyle.Bold);
                Font textoNormal = new Font("Segoe UI", 11F, FontStyle.Regular);
                Font textoBold = new Font("Segoe UI", 11F, FontStyle.Bold);

                Color colorTitulo = Color.FromArgb(0x1B, 0x2B, 0x42); // FondoOscuro
                Color colorSeccion = Color.FromArgb(0x3A, 0x50, 0x6B); // Acento
                Color colorTexto = Color.FromArgb(0x2B, 0x2D, 0x42);  // TextoOscuro

                // Título principal
                AplicarFormatoTexto("SISTEMA DE GESTIÓN BIBLIOTECARIA - CUBO ING. RIGOBERTO ORELLANA", tituloPrincipal, colorTitulo);
                AplicarFormatoTexto("Manual Operativo para Bibliotecarios", new Font("Segoe UI", 12F, FontStyle.Italic), colorSeccion);

                // Secciones principales
                AplicarFormatoTexto("CONTROL DE SALA", tituloSeccion, colorSeccion);
                AplicarFormatoTexto("INVENTARIO DE LIBROS", tituloSeccion, colorSeccion);
                AplicarFormatoTexto("PRÉSTAMOS EXTERNOS", tituloSeccion, colorSeccion);
                AplicarFormatoTexto("ALERTAS DE VENCIDOS", tituloSeccion, colorSeccion);
                AplicarFormatoTexto("ESTADÍSTICAS", tituloSeccion, colorSeccion);

                // Subtítulos en negrita
                AplicarFormatoTexto("Registro:", textoBold, colorTexto);
                AplicarFormatoTexto("Devolución Interna:", textoBold, colorTexto);
                AplicarFormatoTexto("Seguridad:", textoBold, colorTexto);
                AplicarFormatoTexto("Búsqueda:", textoBold, colorTexto);
                AplicarFormatoTexto("Préstamo Rápido:", textoBold, colorTexto);
                AplicarFormatoTexto("Nuevo Préstamo:", textoBold, colorTexto);
                AplicarFormatoTexto("Devoluciones y Renovaciones:", textoBold, colorTexto);
                AplicarFormatoTexto("Notificaciones:", textoBold, colorTexto);
                AplicarFormatoTexto("Uso:", textoBold, colorTexto);

                // Restaurar cursor al inicio
                rtbManual.SelectionStart = 0;
                rtbManual.SelectionLength = 0;
                rtbManual.ScrollToCaret();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error aplicando formato: {ex.Message}");
            }
        }

        private void AplicarFormatoTexto(string textoBuscar, Font fuente, Color color)
        {
            int inicio = 0;
            while (true)
            {
                int indice = rtbManual.Text.IndexOf(textoBuscar, inicio, StringComparison.Ordinal);
                if (indice == -1) break;

                rtbManual.Select(indice, textoBuscar.Length);
                rtbManual.SelectionFont = fuente;
                rtbManual.SelectionColor = color;

                inicio = indice + textoBuscar.Length;
            }
        }
    }
}