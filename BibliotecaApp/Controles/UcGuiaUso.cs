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
                // RTF estructurado con jerarquía visual profesional
                rtbManual.Rtf = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil\fcharset0 Segoe UI;}{\f1\fnil\fcharset0 Segoe UI Semibold;}{\f2\fnil\fcharset0 Segoe UI Black;}}
{\colortbl ;\red22\green41\blue69;\red51\green102\blue153;\red80\green80\blue80;\red0\green120\blue215;}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1 
\pard\sa200\sl276\slmult1\cf1\f2\fs32 SISTEMA DE GESTI\'d3N BIBLIOTECARIA - CUBO ING. RIGOBERTO ORELLANA\par
\cf3\f0\fs22\i Manual Operativo para Bibliotecarios\i0\par
\par
\cf2\f1\fs26 CONTROL DE SALA\par
\cf3\f0\fs20 Este m\'f3dulo registra las visitas diarias a las instalaciones del CUBO.\par
\pard\li360\sa200\sl276\slmult1\cf0\f1\fs20\'95 Registro: \f0 Llene los datos del visitante (Nombre, G\'e9nero, Edad) y el libro que leer\'e1 dentro de las instalaciones. Haga clic en \cf4\f1 ""Registrar Lectura""\cf0\f0 .\par
\cf0\f1\'95 Devoluci\'f3n Interna: \f0 Seleccione al usuario en la tabla y haga clic en \cf4\f1 ""Marcar Devoluci\'f3n""\cf0\f0  cuando termine su lectura.\par
\cf0\f1\'95 Seguridad: \f0 La eliminaci\'f3n de registros est\'e1 protegida. Requerir\'e1 ingresar nuevamente sus credenciales de administrador para evitar borrados accidentales.\par
\pard\sa200\sl276\slmult1\par
\cf2\f1\fs26 INVENTARIO DE LIBROS\par
\cf3\f0\fs20 Cat\'e1logo digital completo de la biblioteca.\par
\pard\li360\sa200\sl276\slmult1\cf0\f1\fs20\'95 B\'fasqueda: \f0 Puede filtrar el cat\'e1logo escribiendo el c\'f3digo del ejemplar o el t\'edtulo del libro en la barra superior.\par
\cf0\f1\'95 Pr\'e9stamo R\'e1pido: \f0 Si hace ""Doble Clic"" sobre cualquier libro de la lista, el sistema lo llevar\'e1 autom\'e1ticamente al m\'f3dulo de Pr\'e9stamos Externos con los datos del libro ya cargados.\par
\cf0\f1\'95 Importaci\'f3n Masiva (Admin): \f0 Permite cargar miles de libros simult\'e1neamente desde un archivo CSV.\par
\pard\sa200\sl276\slmult1\par
\cf2\f1\fs26 PR\'c9STAMOS EXTERNOS\par
\cf3\f0\fs20 Gesti\'f3n de libros que los usuarios llevan a casa.\par
\pard\li360\sa200\sl276\slmult1\cf0\f1\fs20\'95 Nuevo Pr\'e9stamo: \f0 Ingrese los datos de contacto del usuario. El sistema calcular\'e1 autom\'e1ticamente 8 d\'edas como fecha l\'edmite de entrega.\par
\cf0\f1\'95 Notificaci\'f3n de Bienvenida: \f0 Una vez registrado el pr\'e9stamo, haga clic en el bot\'f3n \cf4\f1 ""Enviar""\cf0\f0  en la tabla para mandarle un correo personalizado al usuario con los detalles de su libro.\par
\cf0\f1\'95 Devoluciones y Renovaciones: \f0 Busque el registro, cambie el Estado en la lista desplegable y asigne el nombre del personal responsable. El sistema validar\'e1 autom\'e1ticamente que los campos requeridos est\'e9n llenos seg\'fan la acci\'f3n.\par
\pard\sa200\sl276\slmult1\par
\cf2\f1\fs26 ALERTAS DE VENCIDOS\par
\cf3\f0\fs20 Control de morosidad y recuperaci\'f3n de libros.\par
\pard\li360\sa200\sl276\slmult1\cf0\f1\fs20\'95 Notificaciones Individuales: \f0 La tabla muestra los usuarios que han excedido su fecha l\'edmite. Al hacer clic en el bot\'f3n \cf4\f1 ""Enviar""\cf0\f0  junto al nombre del moroso, el sistema redactar\'e1 autom\'e1ticamente un correo personalizado recordando la devoluci\'f3n del libro.\par
\cf0\f1\'95 Seguimiento Visual: \f0 Los botones cambiar\'e1n a color verde (\cf4\f1 ""Enviado""\cf0\f0 ) para que el equipo de bibliotecarios sepa a qui\'e9n ya se le ha notificado durante el turno.\par
\pard\sa200\sl276\slmult1\par
\cf2\f1\fs26 ESTAD\'cdSTICAS\par
\cf3\f0\fs20 Panel de an\'e1lisis para reportes de alcance.\par
\pard\li360\sa200\sl276\slmult1\cf0\f1\fs20\'95 Uso: \f0 Seleccione un rango de fechas (""Desde"" y ""Hasta"") y presione \cf4\f1 ""Filtrar""\cf0\f0 . El sistema generar\'e1 gr\'e1ficos exactos del total de visitas divididas por g\'e9nero para facilitar los informes administrativos.\par
}";

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
    }
}