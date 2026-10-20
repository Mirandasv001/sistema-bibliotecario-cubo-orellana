using System;
using System.Text;
using System.Windows.Forms;

namespace BibliotecaApp
{
    /// <summary>
    /// Guia de Uso a pantalla completa para bibliotecarios.
    /// Construye el RTF del manual en memoria e incrusta las imagenes de la guia
    /// desde <see cref="RecursosBase64"/> (formato hexadecimal, que es lo que el
    /// control RTF exige para \pict), sin depender de archivos externos en disco.
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
        /// Metodo publico invocado por Form1 al navegar a este apartado.
        /// </summary>
        public void Actualizar()
        {
            CargarManual();
        }

        // ------------------------------------------------------------------
        //  Construccion del RTF
        // ------------------------------------------------------------------

        /// <summary>
        /// Convierte una cadena Base64 a la representacion hexadecimal continua
        /// que RTF espera dentro de un control \pict. A diferencia de la
        /// codificacion usada en <c>Convert.FromBase64String</c>, RTF no admite
        /// Base64 en el stream de imagen: si se le pasa, el control falla al
        /// cargar el documento (ese era el origen del error).
        /// </summary>
        private static string Base64AHex(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        /// <summary>
        /// Devuelve el fragmento RTF \pict de una imagen JPEG incrustada,
        /// dimensionada en twips (1 px = 15 twips a 96 DPI).
        /// </summary>
        private static string PictJpeg(string base64, int pxAncho, int pxAlto, int twipsAncho)
        {
            int twipsAlto = (int)(twipsAncho * (double)pxAlto / pxAncho);
            return "{\\pict\\jpeg\\picw" + pxAncho + "\\pich" + pxAlto +
                   "\\picwgoal" + twipsAncho + "\\pichgoal" + twipsAlto + " " +
                   Base64AHex(base64) + "}";
        }

        private void CargarManual()
        {
            try
            {
                var rtf = new StringBuilder();

                // Cabecera: fuentes y tabla de colores institucionales
                rtf.Append(@"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Segoe UI Semibold;}{\f2\fnil\fcharset0 Segoe UI Black;}}");
                rtf.Append(@"{\colortbl ;\red22\green41\blue69;\red51\green102\blue153;\red80\green80\blue80;\red0\green120\blue215;}");
                rtf.Append(@"{\*\generator Riched20 10.0.19041}\viewkind4\uc1 ");

                // Titulo general
                rtf.Append(@"\pard\sa200\sl276\slmult1\cf1\f2\fs32 SISTEMA DE GESTI\'d3N BIBLIOTECARIA - CUBO ING. RIGOBERTO ORELLANA\par");
                rtf.Append(@"\cf3\f0\fs22\i Manual Operativo para Bibliotecarios\i0\par\par");

                // ---- 1. UBICACION FISICA ----
                Titulo(rtf, @"UBICACI\'d3N F\'cdSICA DE LOS LIBROS (M\'d3DULOS)");
                Texto(rtf, @"La biblioteca est\'e1 organizada en 5 m\'f3dulos (M1 a M5). Cada m\'f3dulo tiene 5 columnas (A, B, C, D, E) y 4 estantes (filas 1, 2, 3, 4). La ubicaci\'f3n de un libro se escribe como M<modulo><columna><fila>, por ejemplo M1A4.");
                Viñeta(rtf, @"M\'f3dulo", @"n\'famero del m\'f3dulo, de M1 a M5.");
                Viñeta(rtf, @"Columna", @"letra de la columna dentro del m\'f3dulo, de A a E.");
                Viñeta(rtf, @"Fila", @"n\'famero del estante, de 1 a 4.");
                Texto(rtf, @"Ejemplo visual del m\'f3dulo 1 y su codificaci\'f3n:");
                Imagen(rtf, RecursosBase64.ModuloCubo, 1100, 614, 6800);
                rtf.Append(@"\par");

                // ---- 2. CONTROL DE SALA ----
                Titulo(rtf, @"CONTROL DE SALA");
                Texto(rtf, @"Este m\'f3dulo registra las visitas diarias a las instalaciones del CUBO.");
                Viñeta(rtf, @"Registro", @"llene los datos del visitante (Nombre, Genero, Edad) y el libro que leer\'e1. Haga clic en Registrar Lectura.");
                Viñeta(rtf, @"Devoluci\'f3n interna", @"seleccione al usuario en la tabla y haga clic en Marcar Devoluci\'f3n cuando termine su lectura.");
                Viñeta(rtf, @"Seguridad", @"la eliminaci\'f3n de registros est\'e1 protegida y pedir\'e1 sus credenciales de administrador.");
                rtf.Append(@"\par");

                // ---- 3. INVENTARIO ----
                Titulo(rtf, @"INVENTARIO DE LIBROS");
                Texto(rtf, @"Cat\'e1logo digital completo de la biblioteca.");
                Viñeta(rtf, @"B\'fasqueda", @"filtre el cat\'e1logo escribiendo el c\'f3digo del ejemplar o el t\'edtulo en la barra superior.");
                Viñeta(rtf, @"Pr\'e9stamo r\'e1pido", @"al hacer doble clic sobre un libro, el sistema lo lleva a Pr\'e9stamos Externos con los datos ya cargados.");
                Viñeta(rtf, @"Importaci\'f3n masiva (Admin)", @"permite cargar miles de libros desde un archivo Excel o CSV.");
                rtf.Append(@"\par");

                // ---- 3.1 FORMATO EXCEL ----
                Subtitulo(rtf, @"Formato para insertar el inventario (Excel)");
                Texto(rtf, @"Prepare un archivo con estas 6 columnas en este orden exacto:");
                Viñeta(rtf, @"CODIGO", @"identificador \'fanico del ejemplar (ej. DRTSC.2.001).");
                Viñeta(rtf, @"TITULO", @"t\'edtulo completo del libro.");
                Viñeta(rtf, @"AUTOR", @"nombre del autor o autores.");
                Viñeta(rtf, @"EDITORIAL", @"editorial del libro.");
                Viñeta(rtf, @"ESTADO", @"estado f\'edsico: Excelente, Buen Estado, Regular o Mal Estado.");
                Viñeta(rtf, @"UBICACI\'d3N", @"c\'f3digo de ubicaci\'f3n M<modulo><columna><fila> (ej. M1A1).");
                Texto(rtf, @"Ejemplo de archivo con encabezados (fila 1 con los t\'edtulos de columna):");
                Imagen(rtf, RecursosBase64.FormatoExcel, 1400, 588, 8600);
                rtf.Append(@"\par");

                // ---- 3.2 FORMATO CSV ----
                Subtitulo(rtf, @"Formato del archivo CSV (sin encabezados)");
                Texto(rtf, @"Al guardar el archivo como CSV, el sistema espera \'fanicamente los datos, \f1 sin la fila de encabezados\f0 :");
                Viñeta(rtf, @"Fila 1", @"el primer registro de datos (ej. DRTSC.2.001), no los t\'edtulos de columna.");
                Viñeta(rtf, @"Orden", @"se conserva el mismo orden de columnas del ejemplo anterior.");
                Texto(rtf, @"Ejemplo del archivo sin encabezados (los datos comienzan en la fila 1):");
                Imagen(rtf, RecursosBase64.FormatoCsv, 1400, 513, 8600);
                rtf.Append(@"\par");

                // ---- 4. PRESTAMOS EXTERNOS ----
                Titulo(rtf, @"PR\'c9STAMOS EXTERNOS");
                Texto(rtf, @"Gesti\'f3n de libros que los usuarios llevan a casa.");
                Viñeta(rtf, @"Nuevo pr\'e9stamo", @"ingrese los datos de contacto del usuario. El sistema calcular\'e1 autom\'e1ticamente 8 d\'edas como fecha l\'edmite de entrega.");
                Viñeta(rtf, @"Notificaci\'f3n de bienvenida", @"una vez registrado el pr\'e9stamo, haga clic en Enviar para mandar un correo con los detalles del libro.");
                Viñeta(rtf, @"Devoluciones y renovaciones", @"busque el registro, cambie el Estado en la lista desplegable y asigne el personal responsable.");
                rtf.Append(@"\par");

                // ---- 5. ALERTAS ----
                Titulo(rtf, @"ALERTAS DE VENCIDOS");
                Texto(rtf, @"Control de morosidad y recuperaci\'f3n de libros.");
                Viñeta(rtf, @"Notificaciones individuales", @"la tabla muestra los usuarios que excedieron su fecha l\'edmite. Al hacer clic en Enviar se redacta un correo recordando la devoluci\'f3n.");
                Viñeta(rtf, @"Seguimiento visual", @"los botones cambian a verde (Enviado) para saber a qui\'e9n ya se le notific\'f3.");
                rtf.Append(@"\par");

                // ---- 6. ESTADISTICAS ----
                Titulo(rtf, @"ESTAD\'cdSTICAS");
                Texto(rtf, @"Panel de an\'e1lisis para reportes de alcance.");
                Viñeta(rtf, @"Uso", @"seleccione un rango de fechas (Desde y Hasta) y presione Filtrar. El sistema generar\'e1 gr\'e1ficos del total de visitas por g\'e9nero.");
                rtf.Append("}");

                rtbManual.Rtf = rtf.ToString();

                // Restaurar cursor al inicio
                rtbManual.SelectionStart = 0;
                rtbManual.SelectionLength = 0;
                rtbManual.ScrollToCaret();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando manual: {ex.Message}");
                rtbManual.Text = "Error al cargar el manual de usuario.";
            }
        }

        // ------------------------------------------------------------------
        //  Helpers de formato RTF (reciben cadenas verbatim con \' literal)
        // ------------------------------------------------------------------

        private static void Titulo(StringBuilder sb, string texto)
        {
            sb.Append(@"\pard\sa200\sl276\slmult1\cf2\f1\fs26 " + texto + @"\par");
        }

        private static void Subtitulo(StringBuilder sb, string texto)
        {
            sb.Append(@"\pard\sa120\sl276\slmult1\cf1\f1\fs22 " + texto + @"\par");
        }

        private static void Texto(StringBuilder sb, string texto)
        {
            sb.Append(@"\pard\sa120\sl276\slmult1\cf3\f0\fs20 " + texto + @"\par");
        }

        private static void Viñeta(StringBuilder sb, string etiqueta, string detalle)
        {
            sb.Append(@"\pard\li360\sa80\sl276\slmult1\cf0\f1\fs20\'95 " + etiqueta + @": \f0 " + detalle + @"\par");
        }

        private static void Imagen(StringBuilder sb, string base64, int pxAncho, int pxAlto, int twipsAncho)
        {
            sb.Append(@"\pard\sa120\sl276\slmult1\pard\qc ");
            sb.Append(PictJpeg(base64, pxAncho, pxAlto, twipsAncho));
            sb.Append(@"\par");
        }
    }
}
