# Finanzas Fáciles - .NET MAUI + MVVM + EF Core 9

Aplicación **Finanzas Fáciles** para trabajo final de Tecnicatura en Programación, siguiendo estrictamente el patrón MVVM con .NET 9 y Entity Framework Core 9.

## Hitos implementados

### Hito 1 - Gestión de Bases
- **RF1** – Costos Fijos: alta, listado, total mensual y umbral de rentabilidad.
- **RF2** – Catálogo de Actividades: alta, listado, precio sugerido y utilidad por unidad.

### Hito 2 - Núcleo operativo
- **RF3** – Registro de Ingresos: alta con actividad, cantidad y fecha. Segmentación automática en Fondo de Operación y Utilidad Bruta.
- **RF4** – Tablero de Control: punto de equilibrio, excedente/faltante, indicadores financieros, alertas.
- **RF5** – Gestión de Retiros: monto, fecha, concepto. Clasificación GananciaReal/AdelantoUtilidad. Validación de efectivo y advertencia de capital.
- **RF6** – Historial de Retiros: trazabilidad cronológica, filtros por período (semana, mes, trimestre), total retirado y proporción vs utilidad.
- **RF7** – Tablero de Control Visual: Dashboard principal con Utilidad Real (bruta - retiros), ProgressBar hacia punto de equilibrio, estados "Fase de Cobertura" / "Ganancia Neta Disponible", efectivo destacado, colores semánticos.

## Requisitos

- .NET 9 SDK
- Visual Studio 2022 con workload "Desarrollo para .NET Multi-Platform App UI" (o VS Code con extensión .NET MAUI)

## Ejecutar la aplicación

```bash
# Windows
dotnet build -f net9.0-windows10.0.19041.0
dotnet run -f net9.0-windows10.0.19041.0

# Android (con emulador o dispositivo conectado)
dotnet build -f net9.0-android
dotnet run -f net9.0-android
```

## Estructura de la solución

```
FinanzasFaciles/
├── Models/           # GastoFijo, Actividad, Ingreso, enums
├── Views/            # TableroPage, RegistroIngresosPage, GastoFijosPage, ActividadesPage
├── ViewModels/       # TableroViewModel, RegistroIngresosViewModel, + Hito 1
├── Services/         # IGastoFijo, IActividad, IIngreso, IRetiro + SQLite + SegmentacionIngresoService
├── Helpers/          # RelayCommand, AsyncRelayCommand, validación, convertidores
├── Resources/        # Estilos, colores, fuentes
└── Platforms/        # Código específico por plataforma
```

## Tecnologías utilizadas

- **.NET 9**
- **.NET MAUI** - Interfaz multiplataforma
- **Entity Framework Core 9** - ORM con SQLite (preparado para RF1/RF2)
- **Patrón MVVM** - Separación de responsabilidades
- **Dependency Injection** - Servicios inyectados en MauiProgram
