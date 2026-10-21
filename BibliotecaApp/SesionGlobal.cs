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

        public static bool EsAdmin => Rol == "Administrador";

        public static bool EsOperador => Rol == "Operador";
    }
}