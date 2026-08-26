<p align="center">
  <img src="Docs/banner.svg" alt="Citiz — Prepárate · Comunícate · Descubre · Participa" width="100%">
</p>

<div align="center">

[![Demo en vivo](https://img.shields.io/badge/demo-en%20vivo-brightgreen?logo=googlechrome&logoColor=white)](https://peopleworks.github.io/Citiz/)
[![CI](https://img.shields.io/github/actions/workflow/status/peopleworks/Citiz/ci.yml?branch=main&label=CI&logo=github)](https://github.com/peopleworks/Citiz/actions/workflows/ci.yml)
[![CodeQL](https://img.shields.io/github/actions/workflow/status/peopleworks/Citiz/codeql.yml?branch=main&label=CodeQL&logo=github)](https://github.com/peopleworks/Citiz/actions/workflows/codeql.yml)
[![Licencia: MIT](https://img.shields.io/badge/código-MIT-blue)](LICENSE)
[![Contenido: CC BY 4.0](https://img.shields.io/badge/contenido-CC%20BY%204.0-blue)](content/README.md#licensing)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Blazor WebAssembly](https://img.shields.io/badge/Blazor-WASM-512BD4?logo=blazor&logoColor=white)](https://learn.microsoft.com/aspnet/core/blazor/)
[![GitHub stars](https://img.shields.io/github/stars/peopleworks/Citiz?style=social)](https://github.com/peopleworks/Citiz/stargazers)

</div>

*English version → [README.md](README.md)*

**[Pruébalo ahora →](https://peopleworks.github.io/Citiz/)** — funciona en tu navegador, sin cuenta y sin subir nada. Instálalo como app desde el menú del navegador y funciona sin conexión.

**Citiz** es un acompañante gratuito, de código abierto, multilingüe y orientado a la privacidad para
quienes se preparan para la ciudadanía de Estados Unidos. Practica las preguntas cívicas oficiales tal
como las hace el oficial, ejercita el vocabulario del examen de inglés y enseña algo del país cada día;
y todo ocurre en tu navegador.

> 🔒 **Citiz funciona por completo en tu dispositivo.** No hay cuenta, no hay servidor que vea lo que
> estudias, no hay analítica. Tu progreso se guarda en tu navegador y puedes descargarlo o borrarlo cuando
> quieras. Lo único que Citiz te pregunta es *cuándo presentaste el Formulario N-400*, para elegir la
> versión correcta del examen, y hasta eso es opcional.

Citiz es una **herramienta educativa independiente**. No está afiliada a USCIS ni a ninguna agencia
gubernamental, no ofrece asesoría legal y no puede garantizar el resultado de una entrevista o solicitud.

<table>
<tr>
<td width="68%" valign="top">
<img src="Docs/screenshots/home-desktop.png" alt="El panel de Citiz: navegación lateral, la cápsula de descubrimiento del día, progreso real de preparación y la actividad semanal">
</td>
<td width="32%" valign="top">
<img src="Docs/screenshots/home-mobile.png" alt="El mismo panel en el celular, con una barra de navegación inferior en vez de un sidebar">
</td>
</tr>
</table>

<sub>La misma experiencia, adaptada a cada pantalla: sidebar en escritorio, barra inferior en
móvil — sin construir una app aparte para cada uno.</sub>

---

## Qué hace hoy (v0.3)

| Pilar | Construido | Cómo |
| --- | --- | --- |
| **Prepárate** | Los dos bancos cívicos oficiales: **2008** (100 preguntas) y **2025** (128 preguntas) | Tarjetas con repaso espaciado, opción múltiple, escribir la respuesta con un comprobador determinista, un **simulacro calificado exactamente como el examen real** (se detiene en cuanto el resultado queda decidido) y un banco explorable con fuentes |
| **Comunícate** | Las listas oficiales de vocabulario de **lectura** y **escritura** | Toca una palabra para escucharla (voz del navegador, en el dispositivo cuando el navegador lo permite), práctica de dictado |
| **Descubre** | Doce cápsulas "Hoy en Estados Unidos" | Piezas breves con fuentes, enlazadas a las preguntas a las que dan contexto |
| **Juega y Aprende** | *Reto cívico* | Rondas de diez preguntas de opción múltiple donde cada opción es una respuesta oficial real; los resultados cuentan como práctica |
| **Idiomas** | 7 idiomas de interfaz | Inglés, español, chino (simplificado y tradicional), filipino, vietnamita, árabe (de derecha a izquierda); el idioma de la interfaz, el de estudio y el de ayuda son independientes |

Además: una herramienta de línea de comandos `citiz` que valida cada archivo de contenido y cada paquete
de idioma (las mismas comprobaciones que ejecuta la integración continua), resuelve qué examen aplica a
una fecha de presentación y ejecuta un simulacro en la terminal; una API opcional; un worker que vigila
las fuentes oficiales; un Dockerfile; despliegue a GitHub Pages.

**Todavía no construido** (diseñado, en la [hoja de ruta](ROADMAP.md)): reconocimiento de voz y
simulación de entrevista, explicaciones con IA, las apps híbridas .NET MAUI, funciones comunitarias, el
resto de los juegos.

<p align="center">
  <img src="Docs/screenshots/practice-desktop.png" alt="Una pregunta de opción múltiple contestada correctamente, con la respuesta aceptada resaltada y una insignia 'Sin verificar aún' junto a la pregunta">
</p>

<sub>Cada pregunta muestra su estado de revisión al lado — <code>Sin verificar aún</code> hasta que
un responsable la coteja con el documento oficial de USCIS. Citiz nunca esconde qué tan seguro está.</sub>

## ¿Qué examen me toca?

USCIS administra dos versiones del examen cívico según cuándo se presentó el Formulario N-400. Citiz lo
modela como datos, no como código, en [`content/exams/versions.json`](content/exams/versions.json):

| N-400 presentado | Versión | Banco | Preguntas | Aprueba con | Termina con |
| --- | --- | --- | --- | --- | --- |
| **Antes del 20 de octubre de 2025** | 2008 Civics Test | 100 | hasta 10 | 6 correctas | 5 incorrectas |
| **El 20 de octubre de 2025 o después** | 2025 Civics Test | 128 | hasta 20 | 12 correctas | 9 incorrectas |

Quienes tienen 65 años o más y 20 o más años como residentes permanentes estudian un subconjunto
designado de 20 preguntas y responden hasta 10 (la *consideración especial 65/20*). Citiz tiene la lista
de 2008; la de 2025 falta copiarla del documento oficial, así que ese modo está desactivado para 2025
hasta entonces. Citiz no adivina.

## Contenido en el que se puede confiar, porque se puede comprobar

Todo lo que se le dice al estudiante como *hecho* vive en [`content/`](content/README.md) como JSON
sencillo, con tres reglas que el validador y las pruebas hacen cumplir:

1. **El texto oficial se transcribe, no se parafrasea**, incluidos los paréntesis que USCIS usa para
   palabras opcionales: `"(U.S.) Constitution"`. El comprobador de respuestas entiende esa notación.
2. **Nada se publica sin fuente y sin estado de revisión.** La interfaz etiqueta todo lo que no esté
   `approved`. Marcar algo como aprobado es un acto humano.
3. **Las respuestas que dependen de quién ocupa un cargo** (Presidente, Speaker, tu gobernador…) nunca
   se escriben en el banco. Viven en `dynamic-answers.json` y se vuelven a verificar con su propio
   calendario.

El informe `citiz content report` es honesto sobre dónde está el proyecto: los bancos se transcribieron
de las listas oficiales y todavía necesitan que un responsable los compare línea por línea con los
documentos de USCIS antes de marcarlos como aprobados.
[`content/exams/VERIFICATION.md`](content/exams/VERIFICATION.md) es la lista de comprobación. Hacer esa
comparación es hoy la contribución más valiosa, y también, no por casualidad, una muy buena forma de
estudiar.

## Ejecutarlo

Necesitas el [SDK de .NET 10](https://dotnet.microsoft.com/download). Nada más.

```bash
git clone https://github.com/peopleworks/Citiz.git
cd Citiz
dotnet run --project src/Citiz.Web            # la app, en http://localhost:5000
```

La herramienta del mantenedor:

```bash
dotnet run --project src/Citiz.Cli -- content validate          # cada archivo de contenido, cada regla
dotnet run --project src/Citiz.Cli -- content report            # qué falta por verificar
dotnet run --project src/Citiz.Cli -- localization validate     # cada paquete de idioma contra en.json
dotnet run --project src/Citiz.Cli -- exam resolve 2025-11-03   # qué examen aplica a esa fecha
dotnet run --project src/Citiz.Cli -- exam simulate --version 2025   # un simulacro en la terminal
```

Todo lo que ejecuta la integración continua, de una vez: `scripts/bootstrap.sh` (o `.ps1`).

## Contribuir

Las contribuciones más útiles no necesitan C#:

- **Verificar contenido** contra los documentos oficiales y marcarlo como aprobado:
  [`content/exams/VERIFICATION.md`](content/exams/VERIFICATION.md).
- **Revisar un paquete de idioma** si hablas ese idioma:
  [`Docs/Localization/README.md`](Docs/Localization/README.md). Cinco de los siete paquetes son
  borradores automáticos que esperan a un lector fluido.
- **Reportar una respuesta incorrecta o desactualizada** con la
  [plantilla de corrección de contenido](https://github.com/peopleworks/Citiz/issues/new/choose).

Las contribuciones de código, accesibilidad, diseño y documentación son igual de bienvenidas; empieza
por [CONTRIBUTING.md](CONTRIBUTING.md). Aquí todos seguimos el [Código de Conducta](CODE_OF_CONDUCT.md).

## Licencia y aviso

El código es [MIT](LICENSE). El contenido editorial escrito para Citiz es CC BY 4.0. El material de USCIS
es obra del Gobierno de Estados Unidos y está en el dominio público; toda otra fuente conserva su propia
licencia, registrada en cada entrada.

Citiz está construido con .NET 10 y Blazor WebAssembly por **Pedro Hernández (PeopleWorks)**, junto
con la comunidad: *por y para la comunidad*. Existe porque prepararse para la ciudadanía no debería
depender de poder pagar, de hablar ya inglés ni de entregarle tus datos a nadie.

---

<div align="center">

**Pedro Hernández** — PeopleWorks

[![Microsoft MVP](https://img.shields.io/badge/Microsoft-MVP%20for%20.NET-5E5E5E?logo=microsoft&logoColor=white)](https://mvp.microsoft.com/en-US/mvp/profile/24060a02-dbc6-44ec-bca5-c213ff9835c5)
[![GitHub](https://img.shields.io/badge/GitHub-peopleworks-181717?logo=github&logoColor=white)](https://github.com/peopleworks)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Pedro%20Hern%C3%A1ndez-0A66C2?logo=linkedin&logoColor=white)](https://www.linkedin.com/in/pedrohernandez2020/)
[![Email](https://img.shields.io/badge/Email-peopleworks%40gmail.com-D14836?logo=gmail&logoColor=white)](mailto:peopleworks@gmail.com)

<sub>También construido por Pedro: <a href="https://github.com/peopleworks/SignsofAI">Signs of AI Writing</a>, un kit gratuito para la integridad académica y de escritura.</sub>

</div>
