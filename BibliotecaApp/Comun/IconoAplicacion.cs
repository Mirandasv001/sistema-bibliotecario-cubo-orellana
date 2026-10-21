using System.Drawing;
using System.Windows.Forms;

namespace BibliotecaApp
{
    internal static class IconoAplicacion
    {
        private static Icon? _icono;

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
