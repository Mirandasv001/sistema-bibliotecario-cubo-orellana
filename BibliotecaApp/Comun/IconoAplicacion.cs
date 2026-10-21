using System.Drawing;
using System.Windows.Forms;

namespace BibliotecaApp
{
    /// <summary>
    /// Icono de la aplicación (el logo de inicio) extraído del propio .exe.
    /// WinForms NO hereda el icono del .exe: sin esta asignación la barra de
    /// título y la barra de tareas mostraban el icono genérico de Windows.
    /// Se cachea una sola instancia porque cada Icon es un recurso nativo.
    /// </summary>
    internal static class IconoAplicacion
    {
        private static Icon? _icono;

        /// <summary>Devuelve el icono incrustado en el .exe, o null si fallara.</summary>
        public static Icon? Obtener()
        {
            if (_icono is not null) return _icono;

            try
            {
                _icono = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                // Sin icono la aplicación funciona igual.
                _icono = null;
            }

            return _icono;
        }
    }
}
