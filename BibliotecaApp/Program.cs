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
    }
}
