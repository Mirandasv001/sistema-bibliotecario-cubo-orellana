namespace BibliotecaApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

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
