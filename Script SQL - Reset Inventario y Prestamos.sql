-- =====================================================================
--  RESET TOTAL: Inventario y Préstamos Externos - Biblioteca CUBO
--  SQLite (Microsoft.Data.Sqlite)
--
--  Propósito: limpiar el 'estado residual' que dejó copias marcadas
--  como "Prestado" sin préstamos activos de por medio. Devuelve TODO
--  el inventario a 'Disponible' y vacía la tabla de préstamos,
--  reiniciando el contador de identidad (ID) para empezar de cero.
--
--  NOTA: SQLite NO soporta TRUNCATE TABLE. Se usa DELETE FROM + el
--  borrado de la secuencia interna (sqlite_sequence) para reiniciar
--  los AUTOINCREMENT a 1.
-- =====================================================================

-- 1) Devolver TODOS los ejemplares a 'Disponible' (limpieza de estado residual)
UPDATE Libros
SET Disponibilidad = 'Disponible';

-- 2) Vaciar la tabla de préstamos
DELETE FROM PrestamosExternos;

-- 3) Reiniciar la identidad (AUTOINCREMENT) a 1
--    Borra el registro de la secuencia de cada tabla AUTOINCREMENT.
DELETE FROM sqlite_sequence
WHERE name IN ('PrestamosExternos', 'ControlUsuariosSala', 'Libros');

-- 4) (Opcional) Verificación: cuántos libros quedaron disponibles y cuántos préstamos
SELECT 'Disponibles' AS Concepto, COUNT(*) AS Total FROM Libros WHERE Disponibilidad = 'Disponible'
UNION ALL
SELECT 'Préstamos activos', COUNT(*) FROM PrestamosExternos;
