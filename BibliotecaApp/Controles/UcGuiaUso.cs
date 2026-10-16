using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BibliotecaApp
{
    /// <summary>
    /// Guía de Uso a pantalla completa para bibliotecarios.
    /// Manual operativo completo renderizado en un WebBrowser (HTML + imagen
    /// local vía file:///). Todo por code-behind: el .Designer.cs no se modifica.
    /// </summary>
    public partial class UcGuiaUso : UserControl
    {
        // WebBrowser que renderiza el manual completo (texto + imagen + scroll nativo)
        private WebBrowser? webBrowserManual;

        public UcGuiaUso()
        {
            InitializeComponent();
            ConstruirLayoutManual();
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

        /// <summary>
        /// Limpia los controles manuales del Designer (rtbManual) e instala un
        /// WebBrowser con Dock=Fill que renderiza el manual con scroll nativo.
        /// Idempotente: solo construye el layout la primera vez.
        /// </summary>
        private void ConstruirLayoutManual()
        {
            if (webBrowserManual != null) return;

            // Quitar el RichTextBox del Designer (provoca el corte de texto)
            Controls.Remove(rtbManual);
            rtbManual.Dispose();

            webBrowserManual = new WebBrowser
            {
                Name = "webBrowserManual",
                Dock = DockStyle.Fill,
                IsWebBrowserContextMenuEnabled = false,
                AllowNavigation = false,
                ScrollBarsEnabled = true
            };

            Controls.Add(webBrowserManual);
            webBrowserManual.BringToFront();
        }

        private void CargarManual()
        {
            try
            {
                if (webBrowserManual == null) return;

                // --- Diagnóstico de rutas: buscar "MODULO CUBO.jpg" en varias ubicaciones ---
                string nombreArchivo = "MODULO CUBO.jpg";
                string rutaBase = System.IO.Path.Combine(AppContext.BaseDirectory, "Recursos", nombreArchivo);
                System.Diagnostics.Debug.WriteLine($"[GuiaUso] Evaluando: {rutaBase}");

                string? rutaImagen = null;
                if (System.IO.File.Exists(rutaBase))
                {
                    rutaImagen = rutaBase;
                }
                else
                {
                    // Fallback subiendo niveles hacia el código fuente (falla de copiado en el .csproj)
                    string rutaFuente = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, @"..\..\..\Recursos", nombreArchivo));
                    System.Diagnostics.Debug.WriteLine($"[GuiaUso] Evaluando: {rutaFuente}");
                    if (System.IO.File.Exists(rutaFuente))
                    {
                        rutaImagen = rutaFuente;
                    }
                }

                // --- Inyección infalible: bytes -> Base64 (evita restricciones de rutas en IE) ---
                string base64Image = "";
                if (rutaImagen != null)
                {
                    try
                    {
                        byte[] imageArray = System.IO.File.ReadAllBytes(rutaImagen);
                        base64Image = Convert.ToBase64String(imageArray);
                        System.Diagnostics.Debug.WriteLine($"[GuiaUso] Imagen encontrada: {rutaImagen} ({imageArray.Length} bytes)");
                    }
                    catch (Exception exImg)
                    {
                        System.Diagnostics.Debug.WriteLine($"[GuiaUso] Error leyendo imagen: {exImg.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[GuiaUso] Imagen NO encontrada en ninguna ubicación.");
                }

                string html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; padding: 30px 40px; color: #333; line-height: 1.6; }}
        h1 {{ color: #1a365d; font-size: 24px; margin-bottom: 5px; text-transform: uppercase; }}
        .subtitle {{ color: #666; font-size: 15px; font-style: italic; margin-bottom: 35px; border-bottom: 2px solid #e2e8f0; padding-bottom: 10px; }}
        h2 {{ color: #2b6cb0; font-size: 18px; margin-top: 40px; border-bottom: 1px solid #cbd5e1; padding-bottom: 8px; }}
        h3 {{ color: #2d3748; font-size: 16px; margin-top: 25px; margin-bottom: 10px; }}
        p, li {{ font-size: 14px; margin-bottom: 10px; color: #4a5568; }}
        ul, ol {{ margin-top: 5px; margin-bottom: 20px; padding-left: 25px; }}
        li {{ margin-bottom: 8px; }}
        .highlight {{ background-color: #ebf8ff; padding: 2px 6px; border-radius: 4px; color: #2b6cb0; font-weight: bold; }}
        /* Ajuste de imagen más pequeña y centrada */
        .img-container {{ text-align: center; margin: 30px 0; }}
        img {{ max-width: 55%; height: auto; border: 1px solid #e2e8f0; box-shadow: 0 4px 12px rgba(0,0,0,0.08); border-radius: 6px; }}
    </style>
</head>
<body>
    <h1>SISTEMA DE GESTIÓN BIBLIOTECARIA</h1>
    <div class='subtitle'>Manual Operativo y Guía Paso a Paso para Bibliotecarios</div>

    <h2>1. ROLES Y NIVELES DE ACCESO</h2>
    <p>El sistema está blindado con niveles de seguridad para evitar la pérdida accidental de datos del inventario y del historial de visitantes. Los perfiles son:</p>
    <ul>
        <li><b>Administrador:</b> Tiene control y acceso total al sistema. Es el único perfil autorizado para visualizar y utilizar botones de acciones críticas como <span class='highlight'>Vaciar Inventario</span>, <span class='highlight'>Eliminar (fila)</span>, <span class='highlight'>Eliminar página</span>, <span class='highlight'>Editar Libro</span> y realizar la <span class='highlight'>Importación CSV</span>.</li>
        <li><b>Operador:</b> Diseñado para el personal de apoyo y trabajo diario. Puede registrar lecturas, realizar préstamos externos, marcar devoluciones y consultar el inventario. Por seguridad, todas las opciones de borrado y modificación estructural están ocultas y bloqueadas.</li>
    </ul>

    <h2>2. SISTEMA DE UBICACIÓN FÍSICA (NOMENCLATURA)</h2>
    <p>Para localizar un libro rápidamente en la sala o saber exactamente dónde guardarlo al ser devuelto, el sistema utiliza coordenadas alfanuméricas exactas.</p>
    <p>Como ejemplo, <b>El CUBO Ing. Rigoberto Orellana cuenta con 5 módulos</b> físicos de estanterías. El código se divide en tres partes:</p>
    <ul>
        <li><b>Módulo (Ej. M1 al M5):</b> Representa el librero de madera completo.</li>
        <li><b>Columna (A, B, C, D, E...):</b> Son las divisiones verticales dentro de ese módulo, contando siempre de izquierda a derecha.</li>
        <li><b>Cuadro / Nivel (1, 2, 3, 4...):</b> Son los espacios o tramos horizontales dentro de una columna, contados desde arriba hacia abajo.</li>
    </ul>
    <p><i>Ejemplo Práctico:</i> Si la ficha del libro dice ubicación <span class='highlight'>M1A4</span>, significa que debes caminar al <b>Módulo 1</b>, ubicar la primera <b>Columna (A)</b> y buscar en el <b>Cuarto cuadro (hacia abajo)</b>.</p>

    <div class='img-container'>
        <img src='data:image/jpeg;base64,{base64Image}' alt='Guía de Estantería M1A4' />
    </div>

    <h2>3. GUÍA PASO A PASO POR MÓDULOS</h2>

    <h3>3.1. Control de Sala (Lectura Interna)</h3>
    <p>Este módulo es para los usuarios que toman un libro y lo leen dentro de las instalaciones del CUBO.</p>
    <ol>
        <li><b>Registrar:</b> Escribe el Nombre del usuario, su Edad, selecciona el Género y anota el Título del libro. Haz clic en el botón oscuro <b>Registrar Lectura</b>.</li>
        <li><b>Devolución:</b> Cuando el usuario termine de leer, búscalo en la tabla inferior. Haz un solo clic sobre su nombre para seleccionar la fila (se pondrá azul) y presiona el botón <b>Marcar Devolución</b>. El estado cambiará a 'Entregado' y se registrará la hora exacta de recepción.</li>
    </ol>

    <h3>3.2. Inventario General de Libros</h3>
    <p>El catálogo digital de toda la biblioteca.</p>
    <ol>
        <li><b>Búsqueda Rápida:</b> Usa la barra superior para escribir una palabra clave del título, el autor o el código del libro. La tabla se filtrará automáticamente en tiempo real.</li>
        <li><b>Atajo de Préstamo:</b> Si encuentras el libro que un usuario quiere llevarse a casa, haz <b>Doble Clic</b> sobre esa fila. El sistema te llevará automáticamente a la pantalla de Préstamos Externos con todos los datos del libro ya llenos, ahorrándote tiempo.</li>
        <li><b>Importación masiva (Solo Admin):</b> Permite cargar miles de libros de un solo golpe usando un archivo Excel guardado como CSV.</li>
    </ol>

    <h3>3.3. Préstamos Externos (Para llevar a casa)</h3>
    <p>Gestión estricta de los libros que salen de las instalaciones.</p>
    <ol>
        <li><b>Registro de Usuario:</b> Llena el nombre, DUI, teléfono, correo y dirección de la persona que se lleva el libro.</li>
        <li><b>Fechas:</b> Al seleccionar la fecha actual, el sistema calculará automáticamente <b>8 días de vigencia</b> como fecha límite de entrega.</li>
        <li><b>Notificación por Correo:</b> Una vez guardado el préstamo, búscalo en la tabla y presiona el botón verde <b>Enviar</b> ubicado en la última columna. Esto le enviará al usuario un correo electrónico profesional con el resumen de su préstamo y la fecha en que debe devolverlo.</li>
        <li><b>Devolver o Renovar:</b> Selecciona el registro en la tabla. Si el usuario trae el libro, cambia el estado a 'Entregado' y presiona <b>Modificar</b>. Si pide más tiempo, usa el botón <b>Renovar (fila)</b> para recalcular la fecha.</li>
    </ol>

    <h3>3.4. Alertas de Vencidos</h3>
    <p>Este módulo trabaja en automático. Cada vez que entres, el sistema revisará todos los préstamos externos. Si un libro superó la fecha de entrega y sigue marcado como 'Pendiente', aparecerá en esta lista roja para que puedas contactar al usuario moroso y solicitar el ejemplar.</p>

    <h3>3.5. Estadísticas</h3>
    <p>Panel de control gerencial. Usa los calendarios para seleccionar una fecha de ""Inicio"" y ""Fin"", presiona ""Filtrar"" y el sistema generará gráficas exactas mostrando cuántos hombres y mujeres visitaron la sala o se llevaron libros en ese periodo de tiempo. Ideal para los reportes mensuales del CUBO.</p>

</body>
</html>";

                // Asumiendo que instanciaste un WebBrowser llamado webBrowserManual
                webBrowserManual.DocumentText = html;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando manual: {ex.Message}");
                if (webBrowserManual != null)
                    webBrowserManual.DocumentText = "<body style='font-family:Segoe UI;padding:20px;'>Error al cargar el manual de usuario.</body>";
            }
        }
    }
}
