using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    /// <summary>
    /// Capa de acceso a datos (SQLite). Crea la base 'biblioteca.db', las tablas
    /// del sistema e importa el catálogo de libros desde el CSV al arrancar.
    /// </summary>
    public static class ConexionDB
    {
        private const string NombreBaseDatos = "biblioteca.db";

        public static string RutaBaseDatos =>
            Path.Combine(AppContext.BaseDirectory, NombreBaseDatos);

        public static string CadenaConexion =>
            new SqliteConnectionStringBuilder
            {
                DataSource = RutaBaseDatos,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

        /// <summary>Abre y devuelve una conexión SQLite ya abierta.</summary>
        public static SqliteConnection ObtenerConexion()
        {
            var conexion = new SqliteConnection(CadenaConexion);
            conexion.Open();
            return conexion;
        }

        /// <summary>Inicializa la base de datos: tablas + importación del catálogo.</summary>
        public static void Inicializar()
        {
            using var conexion = ObtenerConexion();
            CrearTablas(conexion);
            MigrarEsquemaControlSala(conexion);
            ImportarCatalogoDesdeCsv(conexion);
        }

        /// <summary>
        /// Migra bases creadas con el esquema anterior: si la tabla
        /// ControlUsuariosSala aún tiene la columna 'Taller', la elimina
        /// preservando todas las filas.
        /// </summary>
        private static void MigrarEsquemaControlSala(SqliteConnection conexion)
        {
            bool existeTaller = false;
            using (var info = conexion.CreateCommand())
            {
                info.CommandText = "PRAGMA table_info(ControlUsuariosSala);";
                using var lector = info.ExecuteReader();
                while (lector.Read())
                {
                    if (string.Equals(lector.GetString(1), "Taller", StringComparison.OrdinalIgnoreCase))
                    {
                        existeTaller = true;
                        break;
                    }
                }
            }

            if (!existeTaller) return;

            try
            {
                // SQLite 3.35+ admite DROP COLUMN directamente.
                using var eliminar = conexion.CreateCommand();
                eliminar.CommandText = "ALTER TABLE ControlUsuariosSala DROP COLUMN Taller;";
                eliminar.ExecuteNonQuery();
            }
            catch (SqliteException)
            {
                // Plan B para versiones antiguas de SQLite: reconstruir la tabla.
                using var transaccion = conexion.BeginTransaction();
                try
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
                            PersonalTurno  TEXT
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
                                 TituloLibro, HoraEntrega, HoraRecibido, PersonalTurno)
                            SELECT ID, Fecha, NombreUsuario, Genero, Edad,
                                   TituloLibro, HoraEntrega, HoraRecibido, PersonalTurno
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

                    transaccion.Commit();
                }
                catch
                {
                    transaccion.Rollback();
                    throw;
                }
            }
        }

        // ------------------------------------------------------------------
        //  Estructura
        // ------------------------------------------------------------------
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
                    EstadoLibro      TEXT DEFAULT 'Pendiente'
                );";

            using (var cmd = conexion.CreateCommand())
            {
                cmd.CommandText = sqlLibros + sqlSala + sqlPrestamos;
                cmd.ExecuteNonQuery();
            }

            using var indice = conexion.CreateCommand();
            indice.CommandText = "CREATE INDEX IF NOT EXISTS IX_Libros_Titulo ON Libros(Titulo);";
            indice.ExecuteNonQuery();
        }

        // ------------------------------------------------------------------
        //  Importación del catálogo CSV
        // ------------------------------------------------------------------
        private static void ImportarCatalogoDesdeCsv(SqliteConnection conexion)
        {
            string? rutaCsv = BuscarArchivoCsv();
            if (rutaCsv == null) return;

            var filas = LectorCsv.Leer(rutaCsv);
            if (filas.Count == 0) return;

            using var transaccion = conexion.BeginTransaction();
            try
            {
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

                transaccion.Commit();
                System.Diagnostics.Debug.WriteLine($"Catálogo CUBO: {insertados} libros insertados desde CSV.");
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
        }

        private static string? BuscarArchivoCsv()
        {
            // Busca el CSV en el directorio de salida y en las carpetas superiores
            // (solución/proyecto) para cubrir también la ejecución en desarrollo.
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (int i = 0; i < 6 && dir != null; i++, dir = dir.Parent!)
            {
                string? ruta = Directory
                    .EnumerateFiles(dir.FullName, "*.csv")
                    .FirstOrDefault(f => Path.GetFileName(f)
                        .Contains("INVENTARIO", StringComparison.OrdinalIgnoreCase))
                    ?? Directory.EnumerateFiles(dir.FullName, "*.csv").FirstOrDefault();

                if (ruta != null) return ruta;
            }
            return null;
        }
    }
}
