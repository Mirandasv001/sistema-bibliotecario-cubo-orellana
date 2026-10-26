namespace BibliotecaApp
{
    public static class SesionGlobal
    {
        public static string NombreUsuario { get; set; } = string.Empty;

        public static string Rol { get; set; } = string.Empty;

        public static bool HaySesionActiva => !string.IsNullOrEmpty(NombreUsuario);

        public static void CerrarSesion()
        {
            NombreUsuario = string.Empty;
            Rol = string.Empty;
        }

        // Única definición de rol administrador en toda la aplicación.
        public static bool EsRolAdministrador(string? rol) =>
            string.Equals(rol, "Administrador", StringComparison.OrdinalIgnoreCase)
            || string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase);

        public static bool EsAdmin => EsRolAdministrador(Rol);

        public static bool EsOperador => Rol == "Operador";
    }
}