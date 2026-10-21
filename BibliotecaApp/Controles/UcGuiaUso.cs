using System;
using System.Text;
using System.Windows.Forms;

namespace BibliotecaApp
{
    /// <summary>
    /// Guia de Uso a pantalla completa para bibliotecarios.
    /// Genera el manual en HTML e incrusta las imagenes como data URIs
    /// (data:image/...;base64) tomadas de <see cref="RecursosBase64"/>, de modo
    /// que viajan dentro del ensamblado y no se pierden al generar el instalador.
    /// El contenido se muestra en un WebBrowser agregado en code-behind, sin tocar el Designer.
    /// </summary>
    public partial class UcGuiaUso : UserControl
    {
        // Navegador agregado dinamicamente (no se modifica el .Designer.cs)
        private readonly WebBrowser _navegador;

        // HTML ya construido, pendiente de renderizar en el navegador
        private string _htmlManual = string.Empty;

        // Evita doble render cuando Load y Actualizar se disparan en el mismo ciclo
        private bool _renderPendiente;

        public UcGuiaUso()
        {
            InitializeComponent();

            _navegador = new WebBrowser
            {
                Name = "navManual",
                Dock = DockStyle.Fill,
                IsWebBrowserContextMenuEnabled = false,
                ScriptErrorsSuppressed = true,
                AllowWebBrowserDrop = false
            };
            this.Controls.Add(_navegador);
            _navegador.BringToFront();

            // El RichTextBox del Designer queda oculto: el manual ahora es HTML.
            rtbManual.Visible = false;
        }

        private void UcGuiaUso_Load(object sender, EventArgs e)
        {
            CargarManual();
        }

        /// <summary>
        /// Metodo publico invocado por Form1 al navegar a este apartado.
        /// </summary>
        public void Actualizar()
        {
            CargarManual();
        }

        // ------------------------------------------------------------------
        //  Helpers de data URI (imagenes incrustadas en Base64)
        // ------------------------------------------------------------------

        /// <summary>
        /// Devuelve el data URI de una imagen almacenada como Base64 en
        /// <see cref="RecursosBase64"/>. El mime debe coincidir con el formato real.
        /// </summary>
        private static string DataUri(string base64, string mime)
        {
            return "data:" + mime + ";base64," + base64;
        }

        private static string Img(string base64, string mime, int anchoPx)
        {
            // max-width:100% + height:auto + box-sizing:border-box:
            // la imagen nunca supera el ancho del contenedor (elimina la barra
            // horizontal) y el borde se descuenta dentro del ancho.
            return "<img src='" + DataUri(base64, mime) +
                   "' alt='Imagen del manual' width='" + anchoPx +
                   "' style='display:block;margin:0 auto;max-width:100%;height:auto;" +
                   "box-sizing:border-box;border:1px solid #d0d7de;border-radius:6px;' />";
        }

        // ------------------------------------------------------------------
        //  Render perezoso: el HTML se construye ya y se pinta una sola vez,
        //  en el siguiente ciclo de mensajes, cuando el navegador ya tiene
        //  handle y tamano. Esto corrige que el primer click salga en blanco.
        // ------------------------------------------------------------------

        private void CargarManual()
        {
            try
            {
                _htmlManual = ConstruirHtml();
                ProgramarRender();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando manual: {ex.Message}");
                rtbManual.Visible = true;
                rtbManual.Text = "Error al cargar el manual de usuario.";
            }
        }

        /// <summary>
        /// Encola el pintado una sola vez. Si Load y Actualizar se disparan en
        /// el mismo ciclo (primer click), solo se encola un render.
        /// </summary>
        private void ProgramarRender()
        {
            if (_renderPendiente) return;        // ya hay un render encolado
            if (!this.IsHandleCreated) return;   // Load lo reintentara al crear el handle

            _renderPendiente = true;
            this.BeginInvoke((Action)Renderizar);
        }

        private void Renderizar()
        {
            _renderPendiente = false;
            if (string.IsNullOrEmpty(_htmlManual)) return;
            if (this.IsDisposed) return;

            // Fuerza la creacion del handle del navegador si aun no existe
            if (!_navegador.IsHandleCreated)
            {
                var _ = _navegador.Handle;
            }

            _navegador.DocumentText = _htmlManual;
        }

        private string ConstruirHtml()
        {
            var h = new StringBuilder();

            // Cabecera HTML5 + estilos institucionales
            h.Append("<!DOCTYPE html><html><head><meta charset='utf-8'>");
            h.Append("<style>");
            h.Append("html,body{max-width:100%;overflow-x:hidden;}");
            h.Append("body{font-family:'Segoe UI',Arial,sans-serif;color:#162945;margin:0;padding:24px;background:#ffffff;line-height:1.5;}");
            h.Append("img{max-width:100%;height:auto;}");
            h.Append("h1{font-size:26px;color:#162945;margin:0 0 4px 0;}");
            h.Append("h2{font-size:20px;color:#166699;margin:28px 0 8px 0;border-bottom:2px solid #e3ecf3;padding-bottom:4px;}");
            h.Append("h3{font-size:16px;color:#162945;margin:20px 0 6px 0;}");
            h.Append("p.sub{color:#505050;font-style:italic;margin:0 0 8px 0;}");
            h.Append("ul{margin:6px 0 6px 0;padding-left:22px;}");
            h.Append("li{margin:4px 0;}");
            h.Append("li b{color:#166699;}");
            h.Append(".paso{background:#f2f7fb;border-left:4px solid #0078d7;padding:12px 16px;margin:16px 0;border-radius:0 6px 6px 0;}");
            h.Append(".admin{background:#fff4e5;border-left:4px solid #f59e0b;padding:12px 16px;margin:12px 0;border-radius:0 6px 6px 0;}");
            h.Append(".fig{margin:16px 0;text-align:center;}");
            h.Append(".cap{color:#505050;font-size:13px;margin-top:6px;}");
            h.Append("table{border-collapse:collapse;margin:12px 0;width:100%;table-layout:fixed;}");
            h.Append("th,td{border:1px solid #d0d7de;padding:8px 10px;text-align:left;font-size:14px;word-wrap:break-word;overflow-wrap:break-word;}");
            h.Append("th{background:#166699;color:#fff;}");
            h.Append("</style></head><body>");

            // Titulo
            h.Append("<h1>SISTEMA DE GESTIÓN BIBLIOTECARIA - CUBO ING. RIGOBERTO ORELLANA</h1>");
            h.Append("<p class='sub'>Manual Operativo para Bibliotecarios</p>");

            // ============================================================
            // 1. UBICACION FISICA
            // ============================================================
            h.Append("<h2>1. UBICACIÓN FÍSICA DE LOS LIBROS (MÓDULOS)</h2>");
            h.Append("<p>La biblioteca se organiza en <b>estantes completos llamados módulos</b>. A cada estante completo se le asigna la etiqueta <b>M seguida de un número</b>: en este Cubo hay <b>5 módulos</b>, identificados como <b>M1, M2, M3, M4 y M5</b>. Teniendo en cuenta que hay Cubos con menos o más módulos, el sistema siempre numera los estantes en orden a partir de M1 según la cantidad real de módulos del local.</p>");
            h.Append("<p>Dentro de cada módulo, las <b>letras del abecedario</b> (A, B, C, D, E) representan las <b>columnas</b>, y los <b>números 1 a 4</b> representan los <b>estantes o filas</b> de cada columna. La ubicación de un libro se escribe concatenando módulo, columna y fila: <b>M&lt;modulo&gt;&lt;columna&gt;&lt;fila&gt;</b>.</p>");
            h.Append("<p>Por ejemplo M1A4:</p>");
            h.Append("<div class='fig'>" + Img(RecursosBase64.ModuloCubo, "image/jpeg", 620) +
                     "<div class='cap'>Módulo 1: columnas A–E y estantes 1–4. La casilla resaltada corresponde a la ubicación M1A4.</div></div>");

            // ============================================================
            // 2. INVENTARIO DE LIBROS
            // ============================================================
            h.Append("<h2>2. INVENTARIO DE LIBROS</h2>");
            h.Append("<p>Catálogo digital completo de la biblioteca.</p>");
            h.Append("<ul>");
            h.Append("<li><b>Búsqueda:</b> filtre el catálogo escribiendo el código del ejemplar o el título en la barra superior.</li>");
            h.Append("<li><b>Préstamo rápido:</b> al hacer doble clic sobre un libro, el sistema lo lleva a Préstamos Externos con los datos ya cargados.</li>");
            h.Append("<li><b>Gestión de ejemplares:</b> desde esta pantalla también puede agregar, editar o eliminar libros de forma individual.</li>");
            h.Append("</ul>");

            // ---- Importacion masiva: rol Administrador ----
            h.Append("<h3>Importación masiva de inventario</h3>");
            h.Append("<div class='admin'><b>Atención:</b> la importación masiva es una función de uso <b>exclusivo para el rol Administrador</b>. Los usuarios con rol Operador no ven ni pueden ejecutar esta acción. Si necesita importar el catálogo, inicie sesión con una cuenta de Administrador.</div>");
            h.Append("<p>La importación masiva permite cargar miles de libros de una sola vez desde un archivo con las <b>6 columnas exactas</b> que se detallan a continuación.</p>");
            h.Append("<table>");
            h.Append("<tr><th>#</th><th>Columna</th><th>Descripción</th></tr>");
            h.Append("<tr><td>1</td><td><b>Código</b></td><td>Identificador único del ejemplar (ej. DRTSC.2.001).</td></tr>");
            h.Append("<tr><td>2</td><td><b>Título</b></td><td>Título completo del libro.</td></tr>");
            h.Append("<tr><td>3</td><td><b>Autor</b></td><td>Nombre del autor o autores.</td></tr>");
            h.Append("<tr><td>4</td><td><b>Editorial</b></td><td>Editorial del libro.</td></tr>");
            h.Append("<tr><td>5</td><td><b>Estado</b></td><td>Excelente, Buen Estado, Regular o Mal Estado.</td></tr>");
            h.Append("<tr><td>6</td><td><b>Ubicación</b></td><td>Código de ubicación M&lt;modulo&gt;&lt;columna&gt;&lt;fila&gt; (ej. M1A1).</td></tr>");
            h.Append("</table>");

            // ---- Secuencia de 5 pasos ----
            h.Append("<h3>Proceso de importación paso a paso</h3>");

            h.Append("<div class='paso'><b>Paso 1 — Preparar el archivo en Excel con sus 6 campos.</b> Escriba los encabezados (Código, Título, Autor, Editorial, Estado, Ubicación) y debajo los registros. Verifique que la columna Ubicación siga la convención de módulos.</div>");
            h.Append("<div class='fig'>" + Img(RecursosBase64.FormatoExcel, "image/jpeg", 760) +
                     "<div class='cap'>Paso 1: archivo Excel con los 6 campos y sus encabezados.</div></div>");

            h.Append("<div class='paso'><b>Paso 2 — Quitar los encabezados.</b> Antes de exportar, elimine la fila de títulos para que el archivo contenga únicamente los datos, tal como lo espera el sistema.</div>");
            h.Append("<div class='fig'>" + Img(RecursosBase64.FormatoCsv, "image/jpeg", 760) +
                     "<div class='cap'>Paso 2: los mismos datos sin la fila de encabezados.</div></div>");

            h.Append("<div class='paso'><b>Paso 3 — Guardar como CSV.</b> En Excel elija <i>Archivo &gt; Guardar como</i> y seleccione <b>CSV (separado por comas)</b>. El archivo conservará el orden de las 6 columnas.</div>");
            h.Append("<div class='fig'>" + Img(RecursosBase64.GuardarCsv, "image/jpeg", 620) +
                     "<div class='cap'>Paso 3: guardar el archivo en formato CSV (delimitado por comas).</div></div>");

            h.Append("<div class='paso'><b>Paso 4 — Clic en el botón Importar.</b> En el módulo Inventario (como Administrador) pulse <b>Importar CSV</b> y seleccione el archivo que acaba de guardar.</div>");
            h.Append("<div class='fig'>" + Img(RecursosBase64.BotonImportar, "image/jpeg", 760) +
                     "<div class='cap'>Paso 4: hacer clic en el botón Importar CSV de la pantalla de Inventario.</div></div>");

            h.Append("<div class='paso'><b>Paso 5 — Inventario cargado.</b> El sistema valida las 6 columnas y carga los ejemplares. El catálogo quedará poblado con los libros importados.</div>");
            h.Append("<div class='fig'>" + Img(RecursosBase64.InventarioCargado, "image/jpeg", 760) +
                     "<div class='cap'>Paso 5: inventario ya cargado en el sistema tras la importación.</div></div>");

            // ============================================================
            // 3. CONTROL DE SALA
            // ============================================================
            h.Append("<h2>3. CONTROL DE SALA</h2>");
            h.Append("<p>Registra las visitas diarias a las instalaciones del CUBO.</p>");
            h.Append("<ul>");
            h.Append("<li><b>Registro:</b> llene los datos del visitante (Nombre, Género, Edad) y el libro que leerá. Haga clic en <b>Registrar Lectura</b>.</li>");
            h.Append("<li><b>Devolución interna:</b> seleccione al usuario en la tabla y haga clic en <b>Marcar Devolución</b> cuando termine su lectura.</li>");
            h.Append("<li><b>Seguridad:</b> la eliminación de registros está protegida y pedirá sus credenciales de administrador.</li>");
            h.Append("</ul>");

            // ============================================================
            // 4. PRESTAMOS EXTERNOS
            // ============================================================
            h.Append("<h2>4. PRÉSTAMOS EXTERNOS</h2>");
            h.Append("<p>Gestión de libros que los usuarios llevan a casa. El proceso completo sigue este orden:</p>");
            h.Append("<ul>");
            h.Append("<li><b>1. Selección del libro en Inventario:</b> vaya al Inventario y haga doble clic en la fila del libro que se va a prestar; el sistema lo redirigirá a Préstamos Externos con el código y el título del libro ya cargados en el formulario.</li>");
            h.Append("<li><b>2. Registro de los datos del usuario:</b> complete los campos de contacto del usuario (nombre, teléfono, correo, género, etc.). En cuanto se registran, el sistema calcula automáticamente la <b>fecha de entrega sumando 8 días</b>; este plazo es orientativo y puede ampliarse manualmente según la situación del préstamo.</li>");
            h.Append("<li><b>3. Registrar el préstamo:</b> pulse el botón <b>Registrar Préstamo</b>. El registro se guarda con el estado <b>Pendiente</b>, que identifica el libro como prestado y aún no devuelto.</li>");
            h.Append("<li><b>4. Notificación a la persona:</b> pulse <b>Enviar</b> en la fila del préstamo; el sistema redactará automáticamente el correo de bienvenida dirigido a esa persona con los detalles del libro, listo para que usted lo envíe.</li>");
            h.Append("<li><b>5. Devoluciones y renovaciones:</b> cuando el usuario devuelva o renueve, busque el registro, cambie el <b>Estado</b> en la lista desplegable (Pendiente, Renovado o Entregado) y asigne el personal responsable.</li>");
            h.Append("</ul>");

            // ============================================================
            // 5. ALERTAS DE VENCIDOS
            // ============================================================
            h.Append("<h2>5. ALERTAS DE VENCIDOS</h2>");
            h.Append("<p>Módulo de seguimiento de préstamos vencidos y de la gestión de cobro con los usuarios morosos.</p>");
            h.Append("<ul>");
            h.Append("<li><b>Indicador en tiempo real:</b> el menú lateral muestra junto a <b>Alertas de Vencidos</b> la cantidad de préstamos vencidos que requieren atención inmediata.</li>");
            h.Append("<li><b>Listado de morosos:</b> la tabla presenta Usuario, Teléfono, Correo, Título del Libro, Entrega Esperada y Días de Retraso, con el cálculo dinámico de los días de atraso.</li>");
            h.Append("<li><b>Notificación por correo:</b> pulse <b>Enviar</b> (por usuario) o <b>Notificar morosos</b> (a todos) para abrir un correo personalizado de recordatorio de entrega al usuario moroso, con asunto y cuerpo ya redactados.</li>");
            h.Append("<li><b>Estado vacío:</b> cuando no hay atrasos, el sistema indica «No hay usuarios morosos».</li>");
            h.Append("</ul>");
            h.Append("<div class='fig'>" + Img(RecursosBase64.AlertasVencidos, "image/jpeg", 760) +
                     "<div class='cap'>Alertas de vencidos: indicador lateral, días de retraso y botón Enviar para notificar al usuario moroso.</div></div>");

            // ============================================================
            // 6. ESTADISTICAS
            // ============================================================
            h.Append("<h2>6. ESTADÍSTICAS</h2>");
            h.Append("<p>Panel de indicadores con dos gráficas de barras nativas y un filtro de fechas compartido. El módulo analiza por separado el <b>Control de Sala</b> y los <b>Préstamos Externos</b> dentro del rango de fechas seleccionado.</p>");
            h.Append("<ul>");
            h.Append("<li><b>Filtro de fechas:</b> en la parte superior, seleccione el rango <i>Desde</i> y <i>Hasta</i> y haga clic en <b>Filtrar</b>. El mismo rango se aplica a las dos gráficas a la vez.</li>");
            h.Append("<li><b>Gráfica izquierda (Control de Sala):</b> muestra las visitas a sala agrupadas por género en el rango elegido.</li>");
            h.Append("<li><b>Gráfica derecha (Préstamos Externos):</b> muestra los préstamos agrupados por género en ese mismo rango.</li>");
            h.Append("<li><b>Tarjetas de resumen:</b> 6 tarjetas en total. A la izquierda: Total de lectores en sala más los 2 géneros con más visitas. A la derecha: Total de préstamos más los 2 géneros con más préstamos.</li>");
            h.Append("<li><b>Sin datos:</b> si no hay registros en el periodo, las gráficas muestran «No hay registros» en lugar de barras.</li>");
            h.Append("</ul>");
            h.Append("<div class='fig'>" + Img(RecursosBase64.Estadistica, "image/jpeg", 760) +
                     "<div class='cap'>Estadísticas: filtro de fechas, 6 tarjetas de resumen y las dos gráficas de barras (Sala y Préstamos).</div></div>");

            // ============================================================
            // 7. CAMBIAR CONTRASENA
            // ============================================================
            h.Append("<h2>7. CAMBIAR CONTRASEÑA</h2>");
            h.Append("<p>Permite actualizar la contraseña de acceso del usuario que tiene la sesión abierta.</p>");
            h.Append("<ul>");
            h.Append("<li><b>Acceso:</b> en el menú lateral, haga clic en <b>Cambiar Contraseña</b>.</li>");
            h.Append("<li><b>Contraseña actual:</b> escriba su contraseña vigente para confirmar su identidad.</li>");
            h.Append("<li><b>Nueva contraseña:</b> escriba la nueva clave y confírmela en el segundo campo.</li>");
            h.Append("<li><b>Guardar:</b> presione <b>Aceptar</b> para aplicar el cambio. La nueva contraseña será válida desde el siguiente inicio de sesión.</li>");
            h.Append("</ul>");

            h.Append("</body></html>");

            return h.ToString();
        }
    }
}
