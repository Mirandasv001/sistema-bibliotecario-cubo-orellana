# Sistema de Gestión Bibliotecaria — CUBO Ing. Rigoberto Orellana

Sistema de escritorio integral desarrollado en **C# (.NET WinForms)** para la digitalización, control de inventario y trazabilidad operativa de préstamos en el Centro Urbano de Bienestar y Oportunidades (CUBO).

---

## 📌 Resumen del Proyecto

El sistema reemplaza el control manual en bitácoras físicas por una plataforma digital centralizada, optimizando los tiempos de atención, garantizando la consistencia de los datos y permitiendo la administración eficiente de más de **3,800 ejemplares**.

---

## 🚀 Características Principales

* **Inventario General en Tiempo Real:** Catálogo de más de 3,800 libros con búsqueda y filtrado instantáneo por título, código de ejemplar y disponibilidad.
* **Flujo Seguro de Préstamos Externos:** Transferencia automática de datos desde el inventario hacia el módulo de préstamos mediante interacción directa (doble clic), previniendo errores humanos de digitación.
* **Interfaz Dinámica (UI/UX Adaptativa):** Implementación de `SplitContainer` horizontal que permite expandir o contraer la vista de datos estilo terminal, con desplazamiento vertical nativo.
* **Control de Devoluciones y Renovaciones:** Registro granular de fechas, personal a cargo y actualización automática del estado del ejemplar.
* **Módulo de Alertas de Vencimiento:** Monitoreo y control de préstamos con fecha de entrega expirada.
* **Guía de Uso Integrada:** Documentación de procesos accesible directamente desde la barra lateral del sistema.

---

## 🛠️ Stack Tecnológico

* **Lenguaje:** C# (.NET)
* **Interfaz Gráfica:** Windows Forms (WinForms) con diseño modular basado en `UserControls`
* **Persistencia de Datos:** Base de datos relacional (SQLite / ADO.NET)
* **Control de Versiones:** Git / GitHub

---

## 📸 Capturas del Sistema

| Módulo de Inventario | Módulo de Préstamos Externos |
| :---: | :---: |
| <img width="1354" height="695" alt="image" src="https://github.com/user-attachments/assets/da690040-9882-43f9-beff-e413463016f8" />
* | *<img width="1362" height="541" alt="image" src="https://github.com/user-attachments/assets/5aa81144-af5c-4a9a-839c-6285c3a31d6e" />
* |

---

## ⚙️ Instalación y Ejecución

### Prerrequisitos
* Sistema Operativo: Windows 10 / 11
* [.NET Desktop Runtime](https://dotnet.microsoft.com/download) / Visual Studio 2022 con carga de trabajo de escritorio .NET

### Pasos
1. **Clonar el repositorio:**
   ```bash
   git clone [https://github.com/Mirandasv001/sistema-bibliotecario-cubo-orellana.git](https://github.com/Mirandasv001/sistema-bibliotecario-cubo-orellana.git)

Abrir la solución:
Abrir el archivo .sln en Visual Studio.

Compilar y Ejecutar:
Presionar F5 o seleccionar Compilar > Compilar Solución.

👤 Autor
Erick Miranda — @Mirandasv001

Estudiante de Ingeniería en Desarrollo de Software
