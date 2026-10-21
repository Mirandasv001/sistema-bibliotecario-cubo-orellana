using Microsoft.Data.Sqlite;
using System.Configuration; // Para ConfigurationManager

namespace BibliotecaApp
{
    public static class ConexionDB
    {
        private const string NombreBaseDatos = "biblioteca.db";

        private static readonly string CarpetaDatos =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BibliotecaCUBO");

        public static string RutaBaseDatos
        {
            get
            {
                Directory.CreateDirectory(CarpetaDatos);
                return Path.Combine(CarpetaDatos, NombreBaseDatos);
            }
        }

        public static string CadenaConexion =>
            new SqliteConnectionStringBuilder
            {
                DataSource = RutaBaseDatos,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

        public static string? CatalogoCsvPath { get; set; } = null;

        public static SqliteConnection ObtenerConexion()
        {
            var conexion = new SqliteConnection(CadenaConexion);
            conexion.Open();
            return conexion;
        }

        public static void Inicializar()
        {
            using var conexion = ObtenerConexion();
            using var transaccion = conexion.BeginTransaction();
            try
            {
                CrearTablas(conexion);
                MigrarEsquemaControlSala(conexion, transaccion);
                MigrarEsquemaPrestamos(conexion);
                ImportarCatalogoDesdeCsv(conexion, transaccion);
                SeedUsuarios(conexion);
                transaccion.Commit();
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
        }

        private static void SeedUsuarios(SqliteConnection conexion)
        {
            using var check = conexion.CreateCommand();
            check.CommandText = "SELECT COUNT(*) FROM Usuarios;";
            long count = (long)(check.ExecuteScalar() ?? 0);
            if (count > 0) return;

            using var cmd = conexion.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Usuarios (Nombre, Usuario, Contrasena, Rol) VALUES
                (@nombre1, @usuario1, @pass1, @rol1),
                (@nombre2, @usuario2, @pass2, @rol2);";
            cmd.Parameters.AddWithValue("@nombre1", "Usuario Operador");
            cmd.Parameters.AddWithValue("@usuario1", "UserCubo");
            cmd.Parameters.AddWithValue("@pass1", PasswordHasher.Hash("1234$"));
            cmd.Parameters.AddWithValue("@rol1", "Operador");
            cmd.Parameters.AddWithValue("@nombre2", "Administrador");
            cmd.Parameters.AddWithValue("@usuario2", "AdminCubo");
            cmd.Parameters.AddWithValue("@pass2", PasswordHasher.Hash("Admin123$"));
            cmd.Parameters.AddWithValue("@rol2", "Administrador");
            cmd.ExecuteNonQuery();
        }

        private static void MigrarEsquemaControlSala(SqliteConnection conexion, SqliteTransaction transaccion)
        {
            var columnas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var info = conexion.CreateCommand())
            {
                info.CommandText = "PRAGMA table_info(ControlUsuariosSala);";
                using var lector = info.ExecuteReader();
                while (lector.Read())
                {
                    columnas.Add(lector.GetString(1));
                }
            }

            if (columnas.Contains("Taller"))
            {
                try
                {
                    using var eliminar = conexion.CreateCommand();
                    eliminar.CommandText = "ALTER TABLE ControlUsuariosSala DROP COLUMN Taller;";
                    eliminar.ExecuteNonQuery();
                }
                catch (SqliteException)
                {
                    const string nuevaTabla = @"
                        CREATE TABLE ControlUsuariosSala_Nueva (
                            ID             INTEGER PRIMARY KEY AUTOINCREMENT,
                            Fecha          TEXT,
                            NombreUsuario  TEXT,
                            Genero         TEXT,
                            Edad           INTEGER,
                            TituloLibro    TEXT,
                            HoraEntrega    TEXT,
                            HoraRecibido   TEXT,
                            PersonalTurno  TEXT,
                            Estado         TEXT DEFAULT 'Pendiente'
                        );";

                    using (var crear = conexion.CreateCommand())
                    {
                        crear.Transaction = transaccion;
                        crear.CommandText = nuevaTabla;
                        crear.ExecuteNonQuery();
                    }

                    using (var copiar = conexion.CreateCommand())
                    {
                        copiar.Transaction = transaccion;
                        copiar.CommandText = @"
                            INSERT INTO ControlUsuariosSala_Nueva
                                (ID, Fecha, NombreUsuario, Genero, Edad,
                                 TituloLibro, HoraEntrega, HoraRecibido, PersonalTurno, Estado)
                            SELECT ID, Fecha, NombreUsuario, Genero, Edad,
                                   TituloLibro, HoraEntrega, HoraRecibido, PersonalTurno,
                                   CASE WHEN HoraRecibido = 'En lectura' THEN 'Pendiente' ELSE 'Entregado' END
                            FROM ControlUsuariosSala;";
                        copiar.ExecuteNonQuery();
                    }

                    using (var borrarVieja = conexion.CreateCommand())
                    {
                        borrarVieja.Transaction = transaccion;
                        borrarVieja.CommandText = "DROP TABLE ControlUsuariosSala;";
                        borrarVieja.ExecuteNonQuery();
                    }

                    using (var renombrar = conexion.CreateCommand())
                    {
                        renombrar.Transaction = transaccion;
                        renombrar.CommandText =
                            "ALTER TABLE ControlUsuariosSala_Nueva RENAME TO ControlUsuariosSala;";
                        renombrar.ExecuteNonQuery();
                    }
                }
            }

            if (!columnas.Contains("Estado"))
            {
                try
                {
                    using var agregar = conexion.CreateCommand();
                    agregar.CommandText = "ALTER TABLE ControlUsuariosSala ADD COLUMN Estado TEXT DEFAULT 'Pendiente';";
                    agregar.ExecuteNonQuery();

                    using var actualizar = conexion.CreateCommand();
                    actualizar.CommandText = @"
                        UPDATE ControlUsuariosSala
                        SET Estado = CASE
                            WHEN HoraRecibido = 'En lectura' THEN 'Pendiente'
                            ELSE 'Entregado'
                        END
                        WHERE Estado IS NULL OR Estado = '';";
                    actualizar.ExecuteNonQuery();
                }
                catch (SqliteException)
                {
                    const string nuevaTabla = @"
                        CREATE TABLE ControlUsuariosSala_Nueva (
                            ID             INTEGER PRIMARY KEY AUTOINCREMENT,
                            Fecha          TEXT,
                            NombreUsuario  TEXT,
                            Genero         TEXT,
                            Edad           INTEGER,
                            TituloLibro    TEXT,
                            HoraEntrega    TEXT,
                            HoraRecibido   TEXT,
                            PersonalTurno  TEXT,
                            Estado         TEXT DEFAULT 'Pendiente'
                        );";

                    using (var crear = conexion.CreateCommand())
                    {
                        crear.Transaction = transaccion;
                        crear.CommandText = nuevaTabla;
                        crear.ExecuteNonQuery();
                    }

                    using (var copiar = conexion.CreateCommand())
                    {
                        copiar.Transaction = transaccion;
                        copiar.CommandText = @"
                            INSERT INTO ControlUsuariosSala_Nueva
                                (ID, Fecha, NombreUsuario, Genero, Edad,
                                 TituloLibro, HoraEntrega, HoraRecibido, PersonalTurno, Estado)
                            SELECT ID, Fecha, NombreUsuario, Genero, Edad,
                                   TituloLibro, HoraEntrega, HoraRecibido, PersonalTurno,
                                   CASE WHEN HoraRecibido = 'En lectura' THEN 'Pendiente' ELSE 'Entregado' END
                            FROM ControlUsuariosSala;";
                        copiar.ExecuteNonQuery();
                    }

                    using (var borrarVieja = conexion.CreateCommand())
                    {
                        borrarVieja.Transaction = transaccion;
                        borrarVieja.CommandText = "DROP TABLE ControlUsuariosSala;";
                        borrarVieja.ExecuteNonQuery();
                    }

                    using (var renombrar = conexion.CreateCommand())
                    {
                        renombrar.Transaction = transaccion;
                        renombrar.CommandText =
                            "ALTER TABLE ControlUsuariosSala_Nueva RENAME TO ControlUsuariosSala;";
                        renombrar.ExecuteNonQuery();
                    }
                }
            }
        }

        private static void MigrarEsquemaPrestamos(SqliteConnection conexion)
        {
            var columnas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var info = conexion.CreateCommand())
            {
                info.CommandText = "PRAGMA table_info(PrestamosExternos);";
                using var lector = info.ExecuteReader();
                while (lector.Read())
                    columnas.Add(lector.GetString(1));
            }

            if (!columnas.Contains("CodigoLibro"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN CodigoLibro TEXT;";
                agregar.ExecuteNonQuery();
            }

            if (!columnas.Contains("FechaDevolucion"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN FechaDevolucion TEXT;";
                agregar.ExecuteNonQuery();
            }

            // Trazabilidad de quién ejecutó cada renovación / devolución
            if (!columnas.Contains("PersonalRenovo"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN PersonalRenovo TEXT;";
                agregar.ExecuteNonQuery();
            }

            if (!columnas.Contains("PersonalDevolvio"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN PersonalDevolvio TEXT;";
                agregar.ExecuteNonQuery();
            }

            if (!columnas.Contains("Genero"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN Genero TEXT DEFAULT '';";
                agregar.ExecuteNonQuery();
            }

            if (!columnas.Contains("CorreoBienvenidaEnviado"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN CorreoBienvenidaEnviado INTEGER DEFAULT 0;";
                agregar.ExecuteNonQuery();
            }

            if (!columnas.Contains("CorreoAlertaEnviado"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN CorreoAlertaEnviado INTEGER DEFAULT 0;";
                agregar.ExecuteNonQuery();
            }

            using var indice = conexion.CreateCommand();
            indice.CommandText = "CREATE INDEX IF NOT EXISTS IX_Prestamos_CodigoLibro ON PrestamosExternos(CodigoLibro);";
            indice.ExecuteNonQuery();
        }

        private static void CrearTablas(SqliteConnection conexion)
        {
            const string sqlLibros = @"
                CREATE TABLE IF NOT EXISTS Libros (
                    Codigo         TEXT PRIMARY KEY,
                    Titulo         TEXT NOT NULL,
                    Autor          TEXT,
                    Editorial      TEXT,
                    Estado         TEXT,
                    Ubicacion      TEXT,
                    Disponibilidad TEXT DEFAULT 'Disponible'
                );";

            const string sqlSala = @"
                CREATE TABLE IF NOT EXISTS ControlUsuariosSala (
                    ID             INTEGER PRIMARY KEY AUTOINCREMENT,
                    Fecha          TEXT,
                    NombreUsuario  TEXT,
                    Genero         TEXT,
                    Edad           INTEGER,
                    TituloLibro    TEXT,
                    HoraEntrega    TEXT,
                    HoraRecibido   TEXT,
                    PersonalTurno  TEXT
                );";

            const string sqlPrestamos = @"
                CREATE TABLE IF NOT EXISTS PrestamosExternos (
                    ID               INTEGER PRIMARY KEY AUTOINCREMENT,
                    NombreUsuario    TEXT,
                    Correo           TEXT,
                    DUI              TEXT,
                    Telefono         TEXT,
                    Direccion        TEXT,
                    TituloLibro      TEXT,
                    FechaPrestamo    TEXT,
                    PersonalPresto   TEXT,
                    FechaRenovacion  TEXT,
                    PersonalRenovo   TEXT,
                    FechaEntrega     TEXT,
                    PersonalRecibio  TEXT,
                    EstadoLibro      TEXT DEFAULT 'Pendiente',
                    CodigoLibro      TEXT,
                    FechaDevolucion  TEXT,
                    PersonalDevolvio TEXT,
                    Genero          TEXT DEFAULT '',
                    CorreoBienvenidaEnviado INTEGER DEFAULT 0,
                    CorreoAlertaEnviado     INTEGER DEFAULT 0
                );";

            const string sqlUsuarios = @"
                CREATE TABLE IF NOT EXISTS Usuarios (
                    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre         TEXT NOT NULL,
                    Usuario        TEXT NOT NULL UNIQUE,
                    Contrasena     TEXT NOT NULL,
                    Rol            TEXT NOT NULL DEFAULT 'Operador'
                );";

            const string sqlConfiguracion = @"
                CREATE TABLE IF NOT EXISTS Configuracion (
                    Clave TEXT PRIMARY KEY,
                    Valor TEXT
                );";

            const string sqlUbicacionesConfig = @"
                CREATE TABLE IF NOT EXISTS UbicacionesConfig (
                    NombreUbicacion TEXT PRIMARY KEY
                );";

            using (var cmd = conexion.CreateCommand())
            {
                cmd.CommandText = sqlLibros + sqlSala + sqlPrestamos + sqlUsuarios
                    + sqlConfiguracion + sqlUbicacionesConfig;
                cmd.ExecuteNonQuery();
            }

            using var indice = conexion.CreateCommand();
            indice.CommandText = "CREATE INDEX IF NOT EXISTS IX_Libros_Titulo ON Libros(Titulo);";
            indice.ExecuteNonQuery();

            using (var cmdConteo = conexion.CreateCommand())
            {
                cmdConteo.CommandText = "SELECT COUNT(*) FROM UbicacionesConfig;";
                long total = Convert.ToInt64(cmdConteo.ExecuteScalar());

                if (total == 0)
                {
                    foreach (string modulo in new[] { "M1", "M2", "M3", "M4", "M5" })
                    {
                        using var cmdSemilla = conexion.CreateCommand();
                        cmdSemilla.CommandText =
                            "INSERT INTO UbicacionesConfig (NombreUbicacion) VALUES (@nombre);";
                        cmdSemilla.Parameters.AddWithValue("@nombre", modulo);
                        cmdSemilla.ExecuteNonQuery();
                    }
                }
            }
        }

        private const string ClaveInventarioVaciado = "InventarioVaciado";

        public static void MarcarInventarioVaciado(bool vaciado)
        {
            using var conexion = ObtenerConexion();
            using var cmd = conexion.CreateCommand();
            cmd.CommandText = "INSERT OR REPLACE INTO Configuracion (Clave, Valor) VALUES (@clave, @valor);";
            cmd.Parameters.AddWithValue("@clave", ClaveInventarioVaciado);
            cmd.Parameters.AddWithValue("@valor", vaciado ? "1" : "0");
            cmd.ExecuteNonQuery();
        }

        private static bool InventarioVaciadoPorUsuario(SqliteConnection conexion)
        {
            using var cmd = conexion.CreateCommand();
            cmd.CommandText = "SELECT Valor FROM Configuracion WHERE Clave = @clave;";
            cmd.Parameters.AddWithValue("@clave", ClaveInventarioVaciado);

            object? valor = cmd.ExecuteScalar();
            return string.Equals(valor as string, "1", StringComparison.OrdinalIgnoreCase);
        }

        private static void ImportarCatalogoDesdeCsv(SqliteConnection conexion, SqliteTransaction transaccion)
        {
            if (InventarioVaciadoPorUsuario(conexion)) return;

            string? rutaCsv = BuscarArchivoCsv();
            if (rutaCsv == null) return;

            var filas = LectorCsv.Leer(rutaCsv);
            if (filas.Count == 0) return;

            using var comando = conexion.CreateCommand();
            comando.Transaction = transaccion;
            comando.CommandText = @"
                INSERT OR IGNORE INTO Libros (Codigo, Titulo, Autor, Editorial, Estado, Ubicacion)
                VALUES ($codigo, $titulo, $autor, $editorial, $estado, $ubicacion);";

            var pCodigo = comando.Parameters.Add("$codigo", SqliteType.Text);
            var pTitulo = comando.Parameters.Add("$titulo", SqliteType.Text);
            var pAutor = comando.Parameters.Add("$autor", SqliteType.Text);
            var pEditorial = comando.Parameters.Add("$editorial", SqliteType.Text);
            var pEstado = comando.Parameters.Add("$estado", SqliteType.Text);
            var pUbicacion = comando.Parameters.Add("$ubicacion", SqliteType.Text);

            int insertados = 0;
            foreach (var fila in filas)
            {
                // Omite encabezado y filas incompletas o sin código.
                if (fila.Length < 6) continue;
                if (fila[0].Equals("CODIGO", StringComparison.OrdinalIgnoreCase)) continue;

                string codigo = fila[0].Trim();
                if (codigo.Length == 0) continue;

                pCodigo.Value = codigo;
                pTitulo.Value = fila[1];
                pAutor.Value = fila[2];
                pEditorial.Value = fila[3];
                pEstado.Value = fila[4];
                pUbicacion.Value = fila[5];

                insertados += comando.ExecuteNonQuery();
            }

            System.Diagnostics.Debug.WriteLine($"Catálogo CUBO: {insertados} libros insertados desde CSV.");
        }

        private const string NombreCatalogoCsv =
            "CATÁLOGO BIBLIOTECA CUBO SITIO DEL NIÑO - ORIGINAL-2026 .xlsx - INVENTARIO TOTAL (1).csv";

        private static string? BuscarArchivoCsv()
        {
            if (!string.IsNullOrWhiteSpace(CatalogoCsvPath) && File.Exists(CatalogoCsvPath))
                return CatalogoCsvPath;

            // C-03: Prioridad 2 - Ruta fija junto al .exe, sin búsquedas ascendentes
            string ruta = Path.Combine(AppContext.BaseDirectory, NombreCatalogoCsv);
            return File.Exists(ruta) ? ruta : null;
        }
    }
}
