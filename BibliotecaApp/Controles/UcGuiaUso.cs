using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BibliotecaApp
{
    /// <summary>
    /// Guía de Uso a pantalla completa para bibliotecarios.
    /// Manual operativo completo renderizado en un WebBrowser (HTML + imágenes
    /// copiadas a %TEMP% con nombres seguros → URIs file:/// estables).
    /// Todo por code-behind: el .Designer.cs no se modifica.
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

        /// <summary>
        /// Copia una imagen de Recursos a %TEMP% con un nombre seguro (solo
        /// letras/dígitos, sin espacios) y devuelve su URI file:/// absoluta,
        /// formato que el motor IE del WebBrowser sí carga dentro de
        /// DocumentText. Resuelve primero junto al .exe y luego hacia el
        /// código fuente. Devuelve cadena vacía si la imagen no existe.
        /// </summary>
        private static string PrepararUriImagen(string nombreArchivo)
        {
            try
            {
                // 1) Resolver el origen: junto al .exe o fallback a ..\..\..\Recursos
                string rutaOrigen = Path.Combine(AppContext.BaseDirectory, "Recursos", nombreArchivo);
                if (!File.Exists(rutaOrigen))
                {
                    string rutaFuente = Path.GetFullPath(
                        Path.Combine(AppContext.BaseDirectory, @"..\..\..\Recursos", nombreArchivo));
                    if (File.Exists(rutaFuente))
                    {
                        rutaOrigen = rutaFuente;
                    }
                }

                // 2) Nombre seguro en %TEMP% (sin espacios ni acentos)
                string nombreSeguro = "cubo_guia_";
                foreach (char c in Path.GetFileName(nombreArchivo))
                {
                    bool valido = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')
                                  || (c >= '0' && c <= '9') || c == '.' || c == '_' || c == '-';
                    nombreSeguro += valido ? c : '_';
                }
                string rutaDestino = Path.Combine(Path.GetTempPath(), nombreSeguro);

                // 3) Copiar (si IE retiene una sesión anterior, se reutiliza el ya existente)
                if (File.Exists(rutaOrigen))
                {
                    try
                    {
                        File.Copy(rutaOrigen, rutaDestino, true);
                    }
                    catch
                    {
                        // Copia bloqueada: si el archivo de Temp ya existe, es válido
                    }
                }

                // 4) Devolver la URI estandarizada (file:/// con %20 donde haga falta)
                if (File.Exists(rutaDestino))
                {
                    string uri = new Uri(rutaDestino).AbsoluteUri;
                    System.Diagnostics.Debug.WriteLine($"[GuiaUso] URI de '{nombreArchivo}': {uri}");
                    return uri;
                }

                System.Diagnostics.Debug.WriteLine($"[GuiaUso] Imagen NO encontrada: {nombreArchivo}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GuiaUso] Error preparando '{nombreArchivo}': {ex.Message}");
                return string.Empty;
            }
        }

        private void CargarManual()
        {
            try
            {
                if (webBrowserManual == null) return;

                // --- Imágenes servidas desde %TEMP% con nombres seguros (sin espacios):
                //     generan URIs file:/// absolutas que el motor IE sí renderiza.
                //     (Nota: en disco "Imagen 4" es .jpg, no .png.)
                string uriImagen = PrepararUriImagen("MODULO CUBO.jpg");
                string uriImgExcel1 = PrepararUriImagen("formato_excel 1.png");
                string uriImgExcel2 = PrepararUriImagen("formato sin encabezados 2.png");
                string uriImgExcel3 = PrepararUriImagen("guardar como csv 3.png");
                string uriImgExcel4 = PrepararUriImagen("Imagen 4.jpg");
                string uriImgExcel5 = PrepararUriImagen("Inventario cargado 5.png");
                string uriImgAlertas6 = PrepararUriImagen("Alerta 6.jpg"); // en disco: Alerta 6.jpg (versión nítida)
                string uriImgEstadistica7 = PrepararUriImagen("Estadistica 7.png");

                string html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; padding: 30px 40px; color: #333; line-height: 1.6; }}
        h1 {{ color: #1a365d; font-size: 27px; margin-bottom: 5px; text-transform: uppercase; }}
        .subtitle {{ color: #666; font-size: 17px; font-style: italic; margin-bottom: 35px; border-bottom: 2px solid #e2e8f0; padding-bottom: 10px; }}
        h2 {{ color: #2b6cb0; font-size: 21px; margin-top: 40px; border-bottom: 1px solid #cbd5e1; padding-bottom: 8px; }}
        h3 {{ color: #2d3748; font-size: 19px; margin-top: 25px; margin-bottom: 10px; }}
        p, li {{ font-size: 17px; margin-bottom: 10px; color: #4a5568; }}
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
        <img src='{uriImagen}' alt='Guía de Estantería M1A4' />
    </div>

    <h2>3. IMPORTACIÓN MASIVA DE INVENTARIO (CSV)</h2>
    <p>Para cargar el catálogo completo de libros de forma automática, el sistema utiliza archivos CSV. Es <b>estrictamente obligatorio</b> respetar el orden de las columnas para que la base de datos funcione correctamente.</p>
    <p><i>Nota de Seguridad:</i> Esta función requiere credenciales de <b>Administrador</b> para evitar la duplicación o corrupción accidental del catálogo general.</p>

    <ol>
        <li><b>Preparar el documento:</b> Abre tu inventario en Excel o Google Sheets. Asegúrate de que las columnas estén exactamente en este orden: <i>Código, Título, Autor, Editorial, Estado, Ubicación</i>.</li>
        <div class='img-container'>
            <img src='{uriImgExcel1}' alt='Formato Excel con encabezados' style='max-width: 80%;' />
        </div>

        <li><b>Eliminar los encabezados:</b> Antes de guardar, debes <b>eliminar la Fila 1</b> (donde dice ""CÓDIGO"", ""TÍTULO"", etc.). El archivo final solo debe contener los datos de los libros, comenzando directamente desde la primera fila.</li>
        <div class='img-container'>
            <img src='{uriImgExcel2}' alt='Formato Excel sin encabezados' style='max-width: 80%;' />
        </div>

        <li><b>Guardar como CSV:</b> Ve a Archivo > Guardar como... y en el tipo de archivo selecciona obligatoriamente <b>CSV (delimitado por comas)</b>.</li>
        <div class='img-container'>
            <img src='{uriImgExcel3}' alt='Guardar como CSV' style='max-width: 60%;' />
        </div>

        <li><b>Importar al sistema:</b> Ingresa al sistema con tu perfil de Administrador, ve al módulo <b>Inventario</b>, haz clic en el botón superior <b>Importar CSV</b> y selecciona el archivo que acabas de guardar.</li>
        <div class='img-container'>
            <img src='{uriImgExcel4}' alt='Botón Importar CSV' style='max-width: 80%;' />
        </div>

        <li><b>¡Importación Exitosa!:</b> Si los pasos se siguieron correctamente, el sistema mostrará un mensaje de éxito y el catálogo completo aparecerá inmediatamente en la tabla, listo para ser utilizado en préstamos y consultas.</li>
        <div class='img-container'>
            <img src='{uriImgExcel5}' alt='Inventario cargado con éxito' style='max-width: 90%; border: 2px solid #4CAF50;' />
        </div>
    </ol>

    <h2>4. GUÍA PASO A PASO POR MÓDULOS</h2>

    <h3>4.1. Control de Sala (Lectura Interna)</h3>
    <p>Este módulo es para los usuarios que toman un libro y lo leen dentro de las instalaciones del CUBO.</p>
    <ol>
        <li><b>Registrar:</b> Escribe el Nombre del usuario, su Edad, selecciona el Género y anota el Título del libro. Haz clic en el botón oscuro <b>Registrar Lectura</b>. <i>(Nota: Al hacer esto, el sistema cambiará automáticamente la disponibilidad de ese libro a <span class='highlight'>'En Sala'</span> en el Inventario).</i></li>
        <li><b>Devolución:</b> Cuando el usuario termine de leer, búscalo en la tabla inferior. Haz un solo clic sobre su nombre para seleccionar la fila y presiona el botón <b>Marcar Devolución</b>. El estado cambiará a 'Entregado', se registrará la hora de recepción, y el libro volverá a estar <span class='highlight'>'Disponible'</span>.</li>
    </ol>

    <h3>4.2. Inventario General de Libros</h3>
    <p>El catálogo digital de toda la biblioteca.</p>
    <ol>
        <li><b>Búsqueda Rápida:</b> Usa la barra superior para buscar por título, autor o código del libro. La tabla se filtrará automáticamente.</li>
        <li><b>Proceso de Préstamo (Doble Clic):</b> Para prestar un libro, es <b>obligatorio</b> buscarlo en esta tabla y hacer <b>Doble Clic</b> sobre su fila. Así es como el sistema vincula el libro exacto y te envía directamente al módulo de Préstamos Externos con los datos ya cargados.</li>
        <li><b>Control de Disponibilidad:</b> El sistema gestiona dónde está cada ejemplar de forma automática. En la última columna verás si un libro está <b>'Disponible'</b> (en el estante), <b>'En Sala'</b> (leyéndose dentro del CUBO) o <b>'Pendiente'</b> (prestado a casa).</li>
    </ol>

    <h3>4.3. Préstamos Externos (Para llevar a casa)</h3>
    <p>Gestión estricta de los libros que salen de las instalaciones.</p>
    <ol>
        <li><b>Registro de Usuario:</b> Tras hacer Doble Clic en el inventario, llegarás aquí. Llena el nombre, DUI, teléfono, correo y dirección de la persona. <i>(Al guardar el préstamo, la disponibilidad del libro cambiará a <span class='highlight'>'Pendiente'</span> de forma automática).</i></li>
        <li><b>Fechas y Flexibilidad:</b> Al seleccionar la fecha actual, el sistema calculará automáticamente <b>8 días de vigencia</b> como límite sugerido. Sin embargo, el calendario es flexible y te permite ajustar manualmente la fecha de entrega si se requiere otorgar más o menos tiempo.</li>
        <li><b>Notificación por Correo:</b> Tras guardar, busca el registro en la tabla y presiona el <b>botón Enviar</b>. Esto le mandará al usuario un correo con el resumen y la fecha programada de devolución.</li>
        <li><b>Renovación:</b> Si el usuario solicita más tiempo para seguir leyendo, selecciona su registro y usa el botón <b>Renovar (fila)</b>. El sistema te pedirá ingresar el nombre del personal que autoriza la extensión y recalculará la nueva fecha.</li>
        <li><b>Devolución:</b> Si el usuario trae su libro para entregarlo, selecciona el registro y haz clic en <b>Registrar Devolución (fila)</b>. El sistema te pedirá el nombre del personal que está recibiendo el ejemplar en ese momento. El estado cambiará a 'Entregado' <i>(y el libro volverá a estar 'Disponible' en el inventario)</i>.</li>
        <li><b>Modificar Datos:</b> Únicamente si te equivocaste al ingresar los datos del usuario o del libro, selecciona el registro, corrige la información en las casillas superiores y presiona el botón <b>Modificar</b>.</li>
    </ol>

    <h3>4.4. Alertas de Vencidos</h3>
    <p>Este módulo trabaja en automático. Cada vez que entres, el sistema revisará todos los préstamos externos. Si un libro superó la fecha límite de entrega y sigue marcado como 'Pendiente', aparecerá en esta lista roja.</p>
    <ul>
        <li><b>Notificación de Retraso:</b> En esta pantalla también encontrarás un <b>botón Enviar</b>. Al presionarlo, el sistema mandará un correo con un mensaje especial ajustado para solicitar al usuario moroso la devolución inmediata del ejemplar.</li>
    </ul>
    <div class='img-container'>
        <img src='{uriImgAlertas6}' alt='Pantalla Alertas de Vencidos' style='max-width: 90%; margin-top: 10px;' />
    </div>

    <h3>4.5. Estadísticas</h3>
    <p>Panel de control general. Usa los calendarios para seleccionar una fecha de ""Desde"" y ""Hasta"", presiona ""Filtrar"" y el sistema generará gráficas exactas mostrando cuántos hombres y mujeres visitaron la sala o se llevaron libros en ese periodo de tiempo. Ideal para los reportes mensuales de gestión.</p>
    <div class='img-container'>
        <img src='{uriImgEstadistica7}' alt='Pantalla de Estadísticas' style='max-width: 90%; margin-top: 10px;' />
    </div>

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
