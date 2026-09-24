namespace BibliotecaApp
{
    /// <summary>
    /// Clase estática para mantener la información de la sesión del usuario autenticado
    /// accesible globalmente en toda la aplicación.
    /// </summary>
    public static class SesionGlobal
    {
        /// <summary>
        /// Nombre del usuario que ha iniciado sesión.
        /// </summary>
        public static string NombreUsuario { get; set; } = string.Empty;

        /// <summary>
        /// Rol del usuario autenticado: "Operador" o "Admin".
        /// </summary>
        public static string Rol { get; set; } = string.Empty;

        /// <summary>
        /// Indica si hay una sesión activa.
        /// </summary>
        public static bool HaySesionActiva => !string.IsNullOrEmpty(NombreUsuario);

        /// <summary>
        /// Limpia los datos de la sesión (logout).
        /// </summary>
        public static void CerrarSesion()
        {
            NombreUsuario = string.Empty;
            Rol = string.Empty;
        }

        /// <summary>
        /// Verifica si el usuario actual tiene rol de administrador.
        /// </summary>
        public static bool EsAdmin => Rol == "Admin";

        /// <summary>
        /// Verifica si el usuario actual tiene rol de operador.
        /// </summary>
        public static bool EsOperador => Rol == "Operador";
    }
}