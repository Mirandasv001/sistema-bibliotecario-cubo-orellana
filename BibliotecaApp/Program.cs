namespace BibliotecaApp
{
    internal static class Program
    {
        /// <summary>
        ///  Punto de entrada principal de la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Para personalizar la configuración de la aplicación (High DPI, fuente, etc.).
            ApplicationConfiguration.Initialize();

            // ==============================================================
            // BLINDAJE GLOBAL DE EXCEPCIONES (auditoría C4/C5)
            // Toda excepción no capturada en la UI se muestra en un
            // MessageBox amigable en lugar de cerrar el proceso abruptamente
            // (p. ej. "database is locked", fallos de disco o de red).
            // Se registra antes de crear cualquier formulario.
            // ==============================================================
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += ManejadorExcepcionNoControlada;
            AppDomain.CurrentDomain.UnhandledException += ManejadorExcepcionDominio;

            // Crea 'biblioteca.db', las tablas e importa el catálogo CSV al arrancar.
            try
            {
                ConexionDB.Inicializar();
            }
             // Manejo de excepciones
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo inicializar la base de datos:\n" + ex.Message +
                    "\n\nLa aplicación se cerrará.",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new FormLogin());
        }

        /// <summary>
        /// Captura las excepciones no controladas del hilo de la interfaz (UI).
        /// Muestra un mensaje genérico y mantiene viva la aplicación si la
        /// excepción es recuperable.
        /// </summary>
        private static void ManejadorExcepcionNoControlada(object? sender, System.Threading.ThreadExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[Excepción UI no controlada] {e.Exception}");
            MessageBox.Show(
                "Ocurrió un error inesperado. Por favor, intente la operación nuevamente.",
                "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Última línea de defensa: excepciones fuera del hilo de la UI.
        /// Registra el detalle e informa al usuario antes de que el proceso termine.
        /// </summary>
        private static void ManejadorExcepcionDominio(object? sender, UnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[Excepción fatal de dominio] {e.ExceptionObject}");
            MessageBox.Show(
                "Ocurrió un error inesperado. Por favor, intente la operación nuevamente.",
                "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
