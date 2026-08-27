# Sistema de Gestión Bibliotecaria — CUBO Orellana

Aplicación de escritorio desarrollada en **C# (.NET / Windows Forms)** diseñada para la administración integral del inventario bibliotecario, registro de usuarios y control de préstamos externos en el centro CUBO.

---

## 🚀 Características Principales

* **Control de Inventario y Stock Físico:** Catálogo con más de 1,800 títulos indexados, diferenciación por ejemplar físico (códigos únicos) y actualización automática de disponibilidad en tiempo real.
* **Módulo de Préstamos Externos:** Registro de salidas y devoluciones asociadas al DUI del usuario, con cálculo automático de fechas estimadas de entrega y estados de renovación.
* **Búsqueda y Autocompletado Ágil:** Selector inteligente con filtrado predictivo (`SuggestAppend`) para localizar títulos sin redundancia visual.
* **Interfaz Institucional (UI/UX):** Diseño plano (*Flat Design*), paleta de colores institucional, soporte de marcas de agua (*placeholders*) nativas y efectos visuales al pasar el cursor (*hover*).

---

## Stack Tecnológico

| Componente | Tecnología |
| :--- | :--- |
| **Lenguaje** | C# (.NET Framework / .NET Core) |
| **Interfaz (GUI)** | Windows Forms (WinForms) |
| **Arquitectura** | Modular basada en `UserControls` (`UcPrestamosExternos`, etc.) |
| **Almacenamiento** | Base de datos relacional / Importación desde catálogos CSV |
| **Control de Versiones** | Git / GitHub |

---

## 📂 Estructura del Proyecto

```text
sistema-bibliotecario-cubo-orellana/
│
├── BibliotecaApp/
│   ├── EstiloUI.cs                 # Estilos visuales globales y componentes Flat
│   ├── FormPrincipal.cs            # Contenedor y navegación lateral
│   ├── UcPrestamosExternos.cs      # Lógica de préstamos, stock y devoluciones
│   └── UcInventario.cs             # Visualización y búsqueda del catálogo general
│
├── CATÁLOGO BIBLIOTECA CUBO...     # Archivo fuente de inventario
├── BibliotecaApp.slnx              # Solución de Visual Studio
└── README.md                       # Documentación del proyecto

⚙️ Requisitos e Instalación
Requisitos del Sistema:

Sistema Operativo: Windows 10 / 11

.NET SDK / Runtime correspondiente

Visual Studio 2022 o superior (con la carga de trabajo de Desarrollo de escritorio de .NET)

Clonar el repositorio:

Bash
git clone [https://github.com/Mirandasv001/sistema-bibliotecario-cubo-orellana.git](https://github.com/Mirandasv001/sistema-bibliotecario-cubo-orellana.git)
Ejecución:

Abre el archivo de solución BibliotecaApp.slnx en Visual Studio.

Compila el proyecto presionando Ctrl + Shift + B.

Inicia la aplicación con F5.

👤 Autor
Erick Alexander Sánchez Miranda — @Mirandasv001


**Para subirlo a GitHub desde tu terminal:**

```bash
git add README.md
git commit -m "docs: actualizar README principal con documentacion completa"
git push origin master
