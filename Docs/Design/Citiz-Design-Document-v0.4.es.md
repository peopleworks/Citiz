# Documento de diseño

## Plataforma comunitaria para prepararse, comunicarse, descubrir y participar en Estados Unidos

**Nombre provisional:** Citiz  
**Módulo de preparación:** CitizenPrep  
**Tipo de documento:** Plan maestro y diseño técnico para evaluación  
**Versión:** 0.4  
**Fecha:** 28 de julio de 2026  
**Autor del concepto:** Pedro Hernandez

---

> **Nota sobre este documento.** Este es el documento de diseño original (v0.4, 28 de julio de 2026), conservado como la visión fundacional del proyecto. Las decisiones de implementación que lo refinan o lo reemplazan quedan registradas como ADR en `Docs/Architecture/`; en particular, ADR-0003 sustituye la topología orientada a servidor de las secciones 10.1 y 10.2 por un cliente local-first en Blazor WebAssembly con un servidor opcional. El `README.md` es la descripción vigente y más breve del proyecto.

## 1. Resumen ejecutivo

**Citiz** será una plataforma comunitaria, bilingüe y asistida por inteligencia artificial para acompañar a las personas en su camino hacia la ciudadanía estadounidense. Su propósito no se limitará a aprobar un examen: ayudará a desarrollar el inglés, comprender la nación, descubrir algo nuevo cada día y participar con mayor confianza en la comunidad.

La solución combinará:

- Preparación para las preguntas de educación cívica.
- Práctica de inglés hablado, escuchado, leído y escrito.
- Simulación de la entrevista de naturalización.
- Planes de estudio personalizados.
- Descubrimiento diario de la historia, geografía, personas, cultura, instituciones, naturaleza e innovación de Estados Unidos.
- Rutas educativas que conecten contenido nacional, inglés y preguntas del examen.
- Contenido oficial versionado, curado y auditable.
- Actualización automática o semiautomática desde fuentes gubernamentales y culturales confiables.
- Funciones comunitarias moderadas y oportunidades de participación cívica.
- Experiencia web, móvil, de escritorio y parcialmente sin conexión.

La propuesta de valor se organizará en cuatro pilares:

1. **Prepárate:** examen, entrevista y vocabulario del N-400.
2. **Comunícate:** inglés para la ciudadanía y la vida cotidiana.
3. **Descubre:** historia, geografía, personas, cultura, instituciones y patrimonio.
4. **Participa:** comunidad, civismo cotidiano, servicio y pertenencia.

La plataforma se construirá principalmente con tecnología Microsoft: **.NET, ASP.NET Core, Blazor Web App, .NET MAUI Blazor Hybrid, Entity Framework Core, .NET Aspire y Semantic Kernel**. La inteligencia artificial podrá ejecutarse en la nube o localmente mediante una abstracción que permita utilizar modelos pequeños —por ejemplo, modelos disponibles a través de Foundry Local— sin acoplar el producto a un modelo específico.

La decisión arquitectónica central será separar claramente seis responsabilidades:

1. **Contenido oficial:** preguntas, respuestas, fuentes y fechas de vigencia.
2. **Reglas del examen:** versión aplicable, criterios de aprobación y consideraciones especiales.
3. **Aprendizaje:** progreso, repaso espaciado y recomendaciones.
4. **Descubrimiento:** cápsulas diarias, rutas temáticas y conexiones entre lugares, personas, acontecimientos, inglés y examen.
5. **Inteligencia artificial:** conversación, explicación, evaluación asistida y creatividad educativa.
6. **Actualización editorial:** detección, revisión, aprobación y publicación de cambios.

> **Principio rector:** la IA puede enseñar, conversar, conectar ideas y personalizar, pero nunca debe inventar ni decidir por sí sola cuál es una respuesta oficial o un hecho histórico publicado.

> **Promesa educativa:** la persona puede entrar para prepararse para el examen y permanecer porque cada día aprende algo significativo sobre el país del que se dispone a formar parte.

---

## 2. Contexto y oportunidad

La preparación para la ciudadanía no consiste únicamente en memorizar respuestas. El estudiante necesita comprender las preguntas en inglés, responder oralmente, practicar lectura y escritura, familiarizarse con la entrevista y mantener actualizados ciertos datos que pueden cambiar por elecciones, nombramientos o modificaciones oficiales.

USCIS administra actualmente dos versiones del examen cívico, según la fecha de presentación del Formulario N-400:

- Quienes presentaron el N-400 **antes del 20 de octubre de 2025** toman la versión de 2008: hasta 10 preguntas de un banco de 100 y necesitan 6 respuestas correctas.
- Quienes lo presentaron **el 20 de octubre de 2025 o después** toman la versión de 2025: hasta 20 preguntas de un banco de 128 y necesitan 12 respuestas correctas.
- Algunas respuestas deben corresponder al funcionario que ocupe el cargo en el momento de la entrevista.

La aplicación debe modelar estas diferencias como configuraciones versionadas, no como condiciones rígidas incrustadas en el código.

---

## 3. Visión del producto

### 3.1 Objetivo principal

Acompañar al estudiante en su camino hacia la ciudadanía mediante una preparación completa para el examen y la entrevista, el desarrollo del inglés, el descubrimiento continuo de Estados Unidos y una participación comunitaria informada y segura.

### 3.2 Objetivos específicos

- Seleccionar automáticamente la versión correcta del examen.
- Enseñar las respuestas oficiales y su contexto.
- Practicar comprensión auditiva y expresión oral.
- Practicar lectura y escritura en inglés.
- Simular la entrevista de forma progresiva.
- Adaptar el estudio al nivel y los errores del alumno.
- Informar cuando una respuesta haya cambiado.
- Permitir el estudio con conectividad limitada.
- Facilitar grupos de estudio y apoyo comunitario.
- Mantener trazabilidad completa de cada cambio editorial.
- Presentar una experiencia diaria breve para conocer Estados Unidos.
- Relacionar preguntas del examen con historia, lugares, personas y vocabulario.
- Fomentar curiosidad, pertenencia, pensamiento crítico y participación cívica.
- Ofrecer rutas de descubrimiento adaptadas al nivel de inglés, intereses y ubicación del estudiante.

### 3.3 Principios de diseño

1. Experiencia móvil primero.
2. Inglés y español desde la primera versión.
3. Contenido oficial separado de explicaciones educativas.
4. Interfaz sencilla para usuarios con poca experiencia tecnológica.
5. Audio integrado en el flujo principal.
6. Aprendizaje progresivo, respetuoso y sin penalizaciones innecesarias.
7. Actualizaciones de contenido sin publicar una nueva versión de la aplicación.
8. Privacidad y minimización de datos desde el diseño.
9. Fuente oficial y fecha de verificación visibles.
10. Funcionamiento parcial sin conexión.
11. Accesibilidad para adultos mayores y usuarios con discapacidades.
12. Independencia respecto de un proveedor o modelo específico de IA.
13. Representación amplia, respetuosa y documentada de las comunidades que forman Estados Unidos.
14. Aprendizaje diario breve y opcional, sin castigos por interrupciones.
15. Diferenciación visible entre hechos oficiales, interpretación editorial y contribuciones comunitarias.

### 3.4 Alcance y aviso

CitizenPrep será una herramienta educativa independiente. No representará a USCIS, no garantizará la aprobación y no ofrecerá asesoría legal. Cualquier resultado generado por una simulación será orientativo.

---

## 4. Perfiles de usuario

### 4.1 Estudiante

Persona que se prepara para la naturalización y desea estudiar civismo, practicar inglés y simular la entrevista.

### 4.2 Instructor o voluntario

Persona autorizada para dirigir grupos de estudio, asignar actividades y observar progreso agregado con el consentimiento correspondiente.

### 4.3 Organización comunitaria

Biblioteca, organización sin fines de lucro, iglesia, centro comunitario o institución educativa que ofrece preparación para ciudadanía.

### 4.4 Editor de contenido

Persona responsable de revisar fuentes oficiales, aprobar cambios y mantener traducciones y explicaciones.

### 4.5 Moderador

Persona responsable de aplicar las normas comunitarias, revisar reportes y prevenir fraude o abuso.

### 4.6 Administrador técnico

Responsable de fuentes, importadores, modelos de IA, observabilidad, seguridad y configuración operativa.

---

## 5. Incorporación y diagnóstico inicial

Durante la configuración inicial, la aplicación solicitará solamente los datos necesarios:

1. Si el usuario ya presentó el Formulario N-400.
2. Fecha de presentación, si corresponde.
3. Estado y código postal.
4. Distrito congresional, cuando pueda determinarse.
5. Fecha aproximada de entrevista.
6. Posibles consideraciones especiales aplicables.
7. Idioma preferido.
8. Nivel aproximado de inglés.
9. Tiempo disponible para estudiar.
10. Preferencias de audio, texto y accesibilidad.

La plataforma usará estos datos para seleccionar:

- Versión aplicable del examen.
- Reglas de aprobación correspondientes.
- Respuestas federales, estatales o distritales vigentes.
- Modalidad especial, si corresponde.
- Nivel de apoyo en español.
- Frecuencia y duración del plan de estudio.

El diagnóstico inicial evaluará comprensión auditiva, habla, lectura, escritura y conocimiento cívico. El resultado será un punto de partida, no una calificación definitiva.

---

## 6. Módulos funcionales

### 6.1 Perfil y plan de estudio

- Perfil educativo mínimo.
- Meta o fecha aproximada de entrevista.
- Plan semanal personalizado.
- Recordatorios configurables.
- Actividad recomendada para el día.
- Resumen de progreso por habilidad.
- Sincronización entre dispositivos.

### 6.2 Banco de preguntas cívicas

Cada pregunta podrá contener:

- Texto oficial en inglés.
- Audio de la pregunta.
- Respuestas oficiales aceptadas.
- Audio de cada respuesta.
- Traducción educativa al español.
- Explicación sencilla.
- Palabras clave.
- Contexto histórico o institucional.
- Fuente oficial.
- Fecha de última verificación.
- Versión del examen.
- Indicador especial 65/20.
- Indicador de respuesta dinámica.
- Jurisdicción aplicable.
- Periodo de vigencia.

#### Modos de práctica

- Selección múltiple para principiantes.
- Tarjetas de memoria.
- Escuchar y escoger.
- Escuchar y responder oralmente.
- Escribir la respuesta.
- Preguntas aleatorias.
- Práctica por categoría.
- Repaso de errores.
- Repaso espaciado.
- Simulación oral sin opciones visibles.

### 6.3 Entrenador de inglés

#### Escuchar

- Audio a distintas velocidades.
- Repetición y pausas.
- Dictados cortos.
- Identificación de palabras clave.
- Variación controlada de voces y acentos comprensibles.

#### Hablar

- Grabación de respuestas.
- Conversión de voz a texto.
- Detección de conceptos y palabras esenciales.
- Reproducción de la grabación.
- Retroalimentación sobre comprensibilidad.
- Ejercicios de pronunciación sin exigir un acento perfecto.

#### Leer

- Vocabulario frecuente.
- Oraciones de práctica.
- Lectura guiada y cronometrada.
- Grabación y reproducción.
- Retroalimentación sobre palabras omitidas o difíciles.

#### Escribir

- Dictado.
- Ordenamiento de palabras.
- Completar oraciones.
- Escritura de una oración escuchada.
- Práctica de mayúsculas y términos esenciales.
- Actividades con teclado y materiales imprimibles.

### 6.4 Simulador de entrevista

#### Modos

- **Guiado:** traducciones y explicaciones visibles.
- **Práctica:** ayuda después de cada respuesta.
- **Examen:** sin pistas durante la sesión.
- **Entrevista completa:** conversación, N-400, inglés y civismo.

#### Secuencia posible

1. Saludo y preparación.
2. Verificación simulada de identidad.
3. Preguntas de práctica relacionadas con el N-400.
4. Evaluación de comprensión oral.
5. Práctica de lectura.
6. Práctica de escritura.
7. Examen cívico correspondiente.
8. Informe final y plan de repaso.

El informe separará:

- Conocimiento cívico.
- Comprensión auditiva.
- Claridad oral.
- Lectura.
- Escritura.
- Vocabulario del N-400.
- Confianza de la evaluación automática.

### 6.5 Comunidad

Funciones iniciales:

- Grupos de estudio moderados.
- Calendario de sesiones.
- Pregunta del día.
- Retos semanales.
- Foros por idioma o área geográfica.
- Directorio de organizaciones educativas.
- Sesiones dirigidas por voluntarios verificados.
- Reporte de contenido desactualizado.

Para el MVP no se recomiendan mensajes privados entre desconocidos. La prioridad será establecer grupos moderados y eventos programados.

### 6.6 Panel editorial y administrativo

- Importación de contenidos.
- Comparación entre revisiones.
- Gestión de fuentes.
- Edición de respuestas dinámicas.
- Adjuntar evidencia oficial.
- Programar fechas de vigencia.
- Aprobar o rechazar cambios.
- Restaurar revisiones anteriores.
- Generar paquetes offline.
- Notificar a usuarios afectados.
- Revisar reportes comunitarios.
- Consultar auditoría completa.

---

## 7. Descubre Estados Unidos

### 7.1 Propósito

`Descubre Estados Unidos` será un componente central de Citiz, no un apéndice del examen. Su propósito será empoderar al estudiante mediante el conocimiento de la nación: su historia, territorio, gente, instituciones, cultura, naturaleza, innovación y vida comunitaria.

El módulo utilizará experiencias breves, visuales, narradas e interactivas. Cada contenido podrá conectarse con una pregunta cívica, palabras en inglés y una aplicación práctica en la vida cotidiana.

### 7.2 Experiencia “Hoy en Estados Unidos”

La pantalla principal mostrará una cápsula diaria con opciones de duración:

- **1 minuto:** dato esencial.
- **3 minutos:** historia breve.
- **5 minutos:** mini lección con vocabulario.
- **10 minutos:** experiencia completa con audio, mapa, actividad y conversación.

Ejemplo conceptual:

```text
HOY EN ESTADOS UNIDOS

El Gran Cañón

Conoce dónde se encuentra, cómo se formó y por qué
es una parte importante del patrimonio natural del país.

Palabras de hoy:
- canyon
- river
- national park

[Escuchar] [Explorar] [Practicar inglés] [Guardar]
```

La cápsula será opcional, podrá guardarse y no generará penalizaciones si el usuario interrumpe su racha.

### 7.3 Áreas de conocimiento

#### Historia

- Pueblos indígenas y primeras naciones.
- Exploración y colonización.
- Trece Colonias, independencia y fundación.
- Constitución y Bill of Rights.
- Expansión territorial.
- Esclavitud, abolición, Guerra Civil y Reconstrucción.
- Industrialización, inmigración y movimientos laborales.
- Guerras mundiales y Gran Depresión.
- Derechos civiles.
- Ciencia, tecnología e historia contemporánea.
- Evolución de la ciudadanía.

Formatos: líneas de tiempo, documentos primarios, fotografías, mapas, narraciones, comparaciones y conexiones con el examen.

#### Geografía

- Los 50 estados, Washington, D. C. y territorios.
- Capitales y regiones.
- Ríos, lagos, montañas, océanos y fronteras.
- Climas, paisajes y parques nacionales.
- Ciudades, agricultura e industrias regionales.
- Estado, condado, ciudad y comunidad del estudiante.

El recorrido partirá de lo cercano hacia lo nacional:

```text
Mi comunidad → Mi ciudad → Mi condado → Mi estado
→ Mi región → Estados Unidos
```

#### Personas que construyeron la nación

La selección no se limitará a presidentes o figuras políticas. Incluirá líderes históricos, científicos, inventores, educadores, artistas, escritores, atletas, trabajadores, empresarios, inmigrantes, veteranos, líderes indígenas y héroes comunitarios.

Cada perfil breve responderá:

1. ¿Quién fue o es esta persona?
2. ¿Dónde vivió o trabajó?
3. ¿Qué desafío enfrentó?
4. ¿Cuál fue su contribución?
5. ¿Qué valor o idea representa?
6. ¿Qué palabras en inglés se pueden aprender?

#### Instituciones y democracia cotidiana

- Gobierno federal, estatal, de condado y municipal.
- Congreso, Presidencia y tribunales.
- Constitución, derechos y responsabilidades.
- Elecciones y participación cívica no partidista.
- Bibliotecas, escuelas, emergencias, parques y servicios públicos.
- Uso de sitios oficiales y evaluación de fuentes.
- Formas apropiadas de comunicarse con una agencia pública.

#### Cultura y vida cotidiana

- Días festivos y conmemoraciones.
- Tradiciones regionales.
- Museos, bibliotecas y espacios públicos.
- Gastronomía, música, literatura, deportes y arquitectura.
- Historias de vecindarios.
- Servicio comunitario.
- Contribuciones de comunidades diversas.

El contenido comunicará que no existe una sola manera de verse, hablar o vivir como estadounidense.

#### Innovación y logros

- Exploración espacial y aviación.
- Medicina, ciencia y tecnología.
- Agricultura, transporte e ingeniería.
- Universidades e investigación.
- Inventos, emprendimiento e infraestructura.
- Conservación ambiental.

#### Naturaleza y patrimonio

- Parques nacionales.
- Monumentos y sitios históricos.
- Bosques, desiertos, costas y ríos.
- Fauna y conservación.
- Patrimonio indígena, industrial, migratorio y de derechos civiles.

### 7.4 Conexión entre descubrir, examen e inglés

Cada unidad tendrá tres vínculos explícitos:

```text
PARA CONOCER
Contexto histórico, geográfico, cultural o institucional.

PARA EL EXAMEN
Pregunta oficial relacionada y respuesta vigente.

PARA COMUNICARSE
Vocabulario, audio, pronunciación y una frase en inglés.
```

Ejemplo:

```text
Tema: La Constitución

Conoce:
Por qué fue escrita y cómo organiza el gobierno.

Examen:
What is the supreme law of the land?
The Constitution.

Inglés:
law · government · rights · amendment
The Constitution protects important rights.
```

### 7.5 Estructura de una cápsula educativa

Cada cápsula podrá contener:

1. Título, categoría y duración.
2. Imagen, mapa, objeto o documento.
3. Resumen editorial aprobado.
4. Explicación en inglés sencillo.
5. Apoyo opcional en español.
6. Audio, transcripción y velocidad ajustable.
7. Vocabulario y pronunciación.
8. Ubicación y periodo histórico.
9. Personas, acontecimientos y lugares relacionados.
10. Conexiones con preguntas del examen.
11. Actividad breve.
12. Pregunta de reflexión.
13. Fuentes, derechos de uso y fecha de verificación.

Ejemplo de contenido estructurado:

```yaml
title: "The Mississippi River"
contentType: "geography"
difficulty: "beginner"
estimatedMinutes: 5
simpleEnglishSummary: "..."
spanishSupport: "..."
vocabulary:
  - river
  - border
  - transportation
relatedPlaces:
  - Minnesota
  - Louisiana
relatedExamQuestions:
  - CIVICS-2008-088
activities:
  - listen
  - read
  - speak
  - map
  - quiz
reviewStatus: "approved"
lastVerifiedAt: "2026-07-28"
```

### 7.6 Rutas educativas

#### Mi primera ruta por Estados Unidos

1. Símbolos nacionales.
2. Regiones y territorio.
3. Constitución.
4. Tres poderes.
5. Pueblos y comunidades.
6. Grandes ríos y paisajes.
7. Cambios territoriales.
8. Inmigración.
9. Derechos civiles.
10. Mi estado y mi comunidad.

#### 50 estados, 50 historias

Cada estado incluirá ubicación, capital, geografía, historia, personas, lugares, economía, cultura, vocabulario y preguntas relacionadas.

#### Caminos de libertad

Independencia, abolición, sufragio, derechos laborales, derechos civiles, derechos de las personas con discapacidades y participación ciudadana.

#### Nación de comunidades

Historias documentadas de personas y grupos que han contribuido al país, presentadas con contexto, respeto y fuentes confiables.

### 7.7 Gamificación con propósito

- Pasaporte virtual de Estados Unidos.
- Sellos por estados, regiones, parques, museos y temas.
- Mapa de descubrimientos.
- Álbum de personas y lugares.
- Línea de tiempo progresiva.
- Insignias por escuchar, leer, hablar, explorar y ayudar.
- Retos comunitarios opcionales.

Los logros no se perderán por dejar de estudiar. La gamificación premiará la curiosidad y la constancia sin crear presión.

### 7.8 Participación comunitaria

La comunidad podrá proponer historias locales, fotografías autorizadas, museos, eventos, oportunidades de voluntariado y testimonios de naturalización. Toda contribución mostrará una etiqueta clara:

```text
Contenido editorial verificado
```

o

```text
Contenido aportado por la comunidad
```

Las contribuciones no sustituirán hechos oficiales y estarán sujetas a permisos, moderación y revisión.

### 7.9 Fuentes y derechos de uso

El catálogo podrá incluir USCIS, Library of Congress, Smithsonian, National Archives, National Park Service, U.S. Census Bureau y sitios oficiales estatales o locales. Antes de copiar, transformar o redistribuir cualquier material se deberán verificar licencia, dominio público, atribución y condiciones de uso.

### 7.10 Nuevo motor `DiscoveryEngine`

```text
DiscoveryEngine
├── DailyDiscovery
├── TopicCatalog
├── LearningPaths
├── Places
├── People
├── HistoricalEvents
├── CulturalStories
├── VocabularyConnections
├── ExamConnections
└── PersonalizedRecommendations
```

El motor seleccionará contenido según nivel de inglés, temas ya estudiados, intereses declarados, ubicación aproximada, versión del examen y tiempo disponible. Las recomendaciones evitarán crear perfiles sensibles o inferencias políticas, religiosas o identitarias.


---

## 8. Estrategia multilingüe y Juega y Aprende

### 8.1 Interfaz multilingüe

Citiz se diseñará desde el inicio como una aplicación internacionalizada. El idioma de la interfaz será independiente del idioma de aprendizaje: una persona podrá usar la navegación en español y practicar respuestas en inglés, o activar apoyo bilingüe por actividad.

El lanzamiento inicial priorizará cinco idiomas de interfaz, tomando como referencia los principales idiomas usados en los hogares de Estados Unidos y las necesidades de las comunidades inmigrantes:

1. Inglés.
2. Español.
3. Chino, con estrategia editorial para chino simplificado y tradicional.
4. Tagalog/Filipino.
5. Vietnamita.

La selección debe validarse con datos de uso, organizaciones comunitarias y demanda por región. Árabe, francés/criollo haitiano, coreano, ruso y portugués serán candidatos para fases posteriores.

#### Requisitos técnicos

- Recursos `.resx` en .NET para la interfaz.
- Cultura de interfaz separada de la cultura de contenido y del idioma de práctica.
- Soporte de pluralización, fechas, números y formatos culturales.
- Fuentes Unicode y pruebas de expansión de texto.
- Preparación para idiomas de derecha a izquierda.
- Audio y transcripciones por idioma.
- Fallback explícito: idioma regional → idioma base → inglés.
- Traducciones humanas para contenido oficial y de alta importancia.
- IA únicamente para borradores, con revisión editorial antes de publicar.
- Glosario central para términos de naturalización, civismo e instituciones.
- Indicador de estado por traducción: borrador, revisada, aprobada o desactualizada.

#### Modelo de contenido localizado

Cada contenido distinguirá:

- Texto oficial en inglés.
- Traducción educativa.
- Explicación en lenguaje sencillo.
- Audio.
- Transcripción.
- Vocabulario bilingüe.
- Fecha de revisión lingüística.
- Traductor y aprobador.

### 8.2 Juega y Aprende

`Juega y Aprende` será una capa transversal sobre Prepárate, Comunícate y Descubre. Los juegos reforzarán objetivos educativos concretos y no sustituirán el modo de simulación oficial.

Juegos iniciales:

- **Mapa relámpago:** localizar estados, capitales, ríos y regiones.
- **¿Quién soy?:** identificar personas por sus contribuciones.
- **Palabra correcta:** asociar términos en inglés con significados y audio.
- **Ordena la historia:** ubicar acontecimientos en una línea de tiempo.
- **Verdadero, falso o necesito una fuente:** practicar pensamiento crítico.
- **Reto cívico:** responder preguntas oficiales en sesiones breves.
- **Escucha y encuentra:** reconocer una respuesta hablada.
- **Pasaporte Citiz:** completar rutas, lugares y temas.

#### Principios de diseño de juegos

- Sesiones de 2 a 10 minutos.
- Dificultad adaptativa.
- Retroalimentación inmediata y explicativa.
- Sin pérdida de logros por ausencia.
- Sin mecánicas de azar monetizadas.
- Competencias amistosas opcionales y con privacidad.
- Accesibilidad para teclado, lectores de pantalla, baja visión y dificultades auditivas.
- Modo individual y modo comunitario moderado.
- Separación visible entre juego de práctica y simulación oficial.

#### Motor técnico

```text
GameEngine
├── GameCatalog
├── GameSession
├── DifficultyAdapter
├── QuestionProvider
├── RewardRules
├── AccessibilityProfile
├── MultiplayerChallenge
└── LearningOutcomeTracker
```

El `GameEngine` consumirá contenido publicado y registrará resultados en `LearningEngine`. Nunca mantendrá copias independientes de respuestas oficiales.

#### Entidades iniciales

- `GameDefinition`
- `GameSession`
- `GameAttempt`
- `GameReward`
- `Challenge`
- `LearningOutcome`
- `LocalizedGameText`


---

## 9. Filosofía open source, privacidad y sostenibilidad comunitaria

### 9.1 Propósito abierto

Citiz será un proyecto de código abierto, gratuito y orientado al servicio comunitario. La apertura no se limitará a publicar el código fuente: incluirá documentación, contenido estructurado, herramientas de validación, procesos editoriales, traducciones y mecanismos transparentes de gobernanza.

La meta será que una persona, biblioteca, organización educativa o centro comunitario pueda:

- Usar Citiz sin costo.
- Estudiar sin crear obligatoriamente una cuenta.
- Instalar o desplegar su propia instancia.
- Auditar las reglas del examen y sus fuentes.
- Traducir la interfaz y el contenido educativo.
- Proponer correcciones mediante pull requests.
- Crear integraciones y herramientas complementarias.
- Generar paquetes offline.
- Participar en la sostenibilidad del proyecto.

### 9.2 Manifiesto de Citiz

> **Citiz es libre** porque el acceso al conocimiento cívico no debe depender de la capacidad de pago.
>
> **Citiz es multilingüe** porque aprender inglés no debe impedir comprender las instrucciones.
>
> **Citiz es multiplataforma** porque cada persona debe poder estudiar con el dispositivo que ya posee.
>
> **Citiz protege la privacidad** porque prepararse para la ciudadanía no requiere entregar información migratoria sensible.
>
> **Citiz es transparente** porque la IA debe explicar, no imponer conclusiones.
>
> **Citiz es comunitario** porque conocer una nación también significa conocer y apoyar a sus comunidades.
>
> **Citiz existe para ser útil.**

### 9.3 Principios del proyecto

1. **Utilidad antes que espectacularidad.** Cada función debe resolver una necesidad educativa verificable.
2. **Privacidad por defecto.** Las funciones básicas deben operar localmente siempre que sea razonable.
3. **Cuenta opcional.** Estudiar y explorar no requerirá registro.
4. **Evidencia antes que veredicto.** El sistema explicará qué detectó, qué respuesta esperaba y cómo mejorar.
5. **Contenido verificable.** Cada hecho oficial o histórico publicado tendrá fuentes y fecha de revisión.
6. **IA reemplazable y opcional.** El producto funcionará de forma útil aunque no haya un modelo disponible.
7. **Accesibilidad desde el inicio.** La accesibilidad será una condición de aceptación, no una mejora posterior.
8. **Localización comunitaria con control editorial.** Las traducciones tendrán estado, revisor y vigencia.
9. **Interoperabilidad.** Los motores principales podrán consumirse desde web, Hybrid, API, CLI y futuras integraciones.
10. **Gobernanza transparente.** Las decisiones importantes quedarán documentadas públicamente.

### 9.4 Estrategia local-first y privacy-first

Las capacidades se clasificarán según dónde se ejecutan y qué información procesan.

#### Funciones locales o sin cuenta

- Selección de versión del examen.
- Banco de preguntas y respuestas descargado.
- Tarjetas de estudio.
- Juega y Aprende.
- Contenido de Descubre Estados Unidos.
- Progreso local.
- Repetición espaciada.
- Preferencias de idioma y accesibilidad.
- Evaluación determinista de respuestas escritas.
- Audio y paquetes offline.
- IA local cuando el dispositivo sea compatible.

#### Funciones opcionales en la nube

- Sincronización entre dispositivos.
- Reconocimiento de voz avanzado.
- Síntesis de voz avanzada.
- Simulación conversacional con modelos remotos.
- Participación en grupos y eventos.
- Paneles de organizaciones e instructores.
- Recomendaciones avanzadas.
- Administración editorial centralizada.

Antes de utilizar una función remota, la interfaz mostrará de manera comprensible:

- Qué información se enviará.
- A qué tipo de servicio se enviará.
- Para qué se utilizará.
- Cuánto tiempo podrá conservarse.
- Qué alternativa local existe.

### 9.5 Sin registro obligatorio

El flujo inicial recomendado será:

```text
Abrir Citiz
    ↓
Elegir idioma de interfaz
    ↓
Seleccionar o determinar versión del examen
    ↓
Descargar contenido básico opcional
    ↓
Comenzar a estudiar
```

La cuenta será necesaria únicamente para funciones que realmente requieran identidad o sincronización, como grupos, progreso en la nube, contribuciones, administración o paneles institucionales.

### 9.6 Licencias

La licencia deberá definirse por tipo de activo:

```text
Código fuente                      MIT, propuesta inicial
Contenido editorial original       CC BY 4.0, propuesta inicial
Traducciones comunitarias           CC BY 4.0, propuesta inicial
Contenido oficial de terceros       Condiciones de la fuente
Imágenes, audio y mapas              Licencia individual registrada
Modelos y pesos de IA                Licencia de cada modelo
Marcas y logotipos                   Política de marca separada
```

La decisión final requerirá una revisión específica de compatibilidad, atribución y redistribución. Ningún recurso se publicará sin metadatos de licencia o una excepción editorial documentada.

### 9.7 Estructura pública del repositorio

```text
Citiz/
├── .github/
│   ├── ISSUE_TEMPLATE/
│   ├── PULL_REQUEST_TEMPLATE.md
│   ├── CODEOWNERS
│   └── workflows/
├── content/
│   ├── exams/
│   ├── discovery/
│   ├── games/
│   ├── vocabulary/
│   ├── sources/
│   └── localization/
├── docs/
│   ├── architecture/
│   ├── editorial/
│   ├── localization/
│   ├── privacy/
│   └── decisions/
├── src/
├── tests/
├── tools/
├── LICENSE
├── README.md
├── CONTRIBUTING.md
├── CODE_OF_CONDUCT.md
├── SECURITY.md
├── GOVERNANCE.md
├── PRIVACY.md
├── TRADEMARKS.md
├── ROADMAP.md
└── CHANGELOG.md
```

### 9.8 Arquitectura de paquetes reutilizables

```text
Citiz.Core
    Dominio, reglas y contratos esenciales

Citiz.Content
    Versiones, fuentes, respuestas y publicación

Citiz.Learning
    Progreso, dominio y repetición espaciada

Citiz.Discovery
    Historia, geografía, personas y rutas

Citiz.Games
    Juega y Aprende

Citiz.Localization
    Recursos, glosarios y estado de traducciones

Citiz.AI
    Abstracciones, guardrails y proveedores

Citiz.Web
    PWA y experiencia de navegador

Citiz.Hybrid
    .NET MAUI Blazor Hybrid

Citiz.Api
    Sincronización y servicios remotos opcionales

Citiz.Cli
    Importación, validación y empaquetado

Citiz.Mcp
    Integración opcional con asistentes compatibles

Citiz.ContentWorker
    Detección y procesamiento de cambios
```

El dominio no dependerá de Blazor, MAUI ni de un proveedor de IA. Los proyectos de interfaz consumirán servicios y contratos del núcleo.

### 9.9 Plataformas y distribución

#### Web y PWA

Será la experiencia de entrada principal. Deberá funcionar en navegadores modernos, soportar instalación como PWA, almacenamiento local y paquetes offline.

#### .NET MAUI Blazor Hybrid

Permitirá reutilizar componentes Razor y acceder a funciones nativas en Android, iOS, Windows y macOS, de acuerdo con las prioridades de cada fase.

#### Servidor opcional

La instancia oficial podrá proporcionar sincronización, comunidad, IA avanzada y administración. El proyecto documentará cómo desplegar una instancia propia y qué funciones pueden desactivarse.

#### CLI

La herramienta de línea de comandos permitirá a mantenedores y organizaciones validar y empaquetar contenido:

```bash
citiz content validate
citiz content import uscis
citiz content diff
citiz localization status --culture es
citiz exam simulate --version 2025
citiz package build --offline
```

#### Integraciones

Los motores podrán exponerse posteriormente mediante API, paquetes NuGet y un servidor MCP opcional. Las integraciones deberán conservar las mismas reglas de privacidad, atribución y contenido oficial.

### 9.10 Repositorio abierto de contenido

El contenido será legible por máquinas, revisable mediante control de versiones y validado con esquemas.

```yaml
id: us-history-constitution-001
type: discovery-topic
status: published
sourceLanguage: en
reviewedAt: 2026-07-28
sources:
  - authority: National Archives
    url: https://example.gov/source
license:
  type: public-domain
translations:
  es: approved
  fil: draft
  vi: missing
```

Las respuestas oficiales dinámicas podrán mantenerse en un repositorio o servicio separado cuando su ciclo de publicación o seguridad lo requiera, pero conservarán historial auditable.

### 9.11 Automatización y calidad

Cada pull request ejecutará, según corresponda:

```text
Compilación .NET
        ↓
Pruebas unitarias e integración
        ↓
Validación de esquemas de contenido
        ↓
Validación de enlaces y fuentes
        ↓
Verificación de licencias
        ↓
Estado de traducciones
        ↓
Pruebas de accesibilidad
        ↓
Pruebas Playwright
        ↓
Análisis de seguridad y dependencias
        ↓
Vista previa desplegable
```

Controles recomendados:

- GitHub Actions.
- CodeQL.
- Dependabot.
- Protección de ramas.
- Revisión obligatoria mediante `CODEOWNERS`.
- JSON Schema o equivalente para contenido.
- Reportes de cobertura.
- Escaneo de secretos.
- Pruebas de enlaces.
- Artefactos firmados para versiones oficiales.
- Lista de materiales de software, cuando sea viable.

### 9.12 Gobernanza

#### Maintainers técnicos

Responsables de arquitectura, código, seguridad, rendimiento y versiones.

#### Maintainers de contenido

Responsables de exactitud, fuentes, vigencia y publicación.

#### Maintainers lingüísticos

Responsables de glosarios, traducciones, claridad y adecuación cultural.

#### Moderadores comunitarios

Responsables de grupos, eventos, contribuciones locales y reportes.

#### Consejo educativo asesor

Podrá incluir instructores de ciudadanía, docentes de inglés, bibliotecarios, organizaciones comunitarias y personas con experiencia directa en naturalización.

Las decisiones arquitectónicas o editoriales importantes se registrarán mediante documentos ADR y EDR:

```text
docs/decisions/ADR-0001-core-boundaries.md
docs/decisions/EDR-0001-official-content-policy.md
```

### 9.13 Comunidad y contribuciones

El repositorio incluirá rutas de contribución para distintos perfiles:

- Código.
- Traducción.
- Accesibilidad.
- Diseño.
- Contenido educativo.
- Corrección de fuentes.
- Pruebas de usuario.
- Documentación.
- Moderación.

Las incidencias deberán estar etiquetadas por dificultad, área, idioma y necesidad comunitaria. Se crearán tareas `good first issue` que aporten valor real y no sean trabajo artificial.

### 9.14 Sostenibilidad

El acceso básico permanecerá gratuito. Posibles mecanismos de sostenibilidad:

- Donaciones.
- Patrocinios transparentes.
- Subvenciones educativas o comunitarias.
- Apoyo de fundaciones.
- Servicios administrados para organizaciones.
- Capacitación y soporte institucional.
- Infraestructura patrocinada.

Nunca se comercializarán datos migratorios o grabaciones personales. La financiación no dará a un patrocinador autoridad para alterar respuestas oficiales, resultados educativos ni políticas editoriales.

### 9.15 Definición de utilidad

Citiz será útil si una persona puede:

1. Abrir la aplicación gratuitamente.
2. Comprender la interfaz en un idioma accesible.
3. Estudiar sin crear una cuenta.
4. Determinar la versión correcta del examen.
5. Practicar inglés con audio y apoyo bilingüe.
6. Comprender por qué una respuesta es correcta.
7. Simular partes de la entrevista.
8. Descubrir algo nuevo sobre Estados Unidos.
9. Aprender mediante juegos breves.
10. Utilizar funciones esenciales sin conexión.
11. Ver la fuente y vigencia del contenido.
12. Mantener control sobre sus datos.
13. Compartir, traducir o desplegar la herramienta.

La métrica rectora será:

> **¿Cuántas personas pudieron aprender algo útil sin barreras innecesarias?**


---

## 10. Arquitectura tecnológica recomendada

### 10.1 Stack principal

- **.NET 10** para nuevos desarrollos.
- **ASP.NET Core Web API** para servicios de backend.
- **Blazor Web App** para la experiencia en navegador.
- **.NET MAUI Blazor Hybrid** para Android, iOS, macOS y Windows, según el alcance de cada fase.
- **Razor Class Library** para compartir interfaz y componentes.
- **Entity Framework Core** para acceso a datos.
- **PostgreSQL** o **Azure SQL** como base de datos principal.
- **SQLite** para almacenamiento local y modo offline.
- **.NET Aspire** para composición, configuración, telemetría y experiencia local distribuida.
- **Semantic Kernel** como capa de integración y orquestación de IA.
- **Azure AI Foundry/Azure OpenAI** para modelos en la nube.
- **Foundry Local** para modelos ejecutados localmente cuando el dispositivo lo permita.
- **Azure AI Speech** o capacidades nativas para voz.
- **Redis** opcional para caché, sesiones, límites de uso y coordinación.
- **Blob Storage** para audio, documentos y paquetes de contenido.
- **SignalR** para sesiones interactivas, notificaciones y eventos en tiempo real.

### 10.2 Topología general

```text
┌─────────────────────────────────────────────────────────┐
│                    Aplicaciones cliente                 │
├──────────────────────────┬──────────────────────────────┤
│ Blazor Web App           │ .NET MAUI Blazor Hybrid     │
│ Navegador / PWA          │ Android / iOS / Windows     │
└──────────────┬───────────┴──────────────┬───────────────┘
               │                          │
               └──────── HTTPS / SignalR ─┘
                          │
┌─────────────────────────▼───────────────────────────────┐
│                   ASP.NET Core API                      │
├─────────────────────────────────────────────────────────┤
│ Identidad y perfiles                                   │
│ Motor de exámenes                                      │
│ Motor de aprendizaje                                   │
│ Progreso y estadísticas                                │
│ Contenido versionado                                   │
│ Comunidad y moderación                                 │
│ Orquestador de IA                                      │
└────────────┬────────────────┬──────────────────┬────────┘
             │                │                  │
     ┌───────▼──────┐ ┌──────▼──────┐  ┌────────▼────────┐
     │ PostgreSQL / │ │ Blob Storage│  │ Redis / Cache   │
     │ Azure SQL    │ │ Audio/PDF   │  │ sesiones        │
     └──────────────┘ └─────────────┘  └─────────────────┘

┌─────────────────────────────────────────────────────────┐
│                Procesamiento automatizado               │
├─────────────────────────────────────────────────────────┤
│ Worker Service / Azure Functions / Container Apps Jobs │
│ Importadores API, HTML, PDF y fuentes oficiales        │
│ Detección de cambios y cola de revisión humana         │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                    Proveedores de IA                    │
├──────────────────────┬──────────────────────────────────┤
│ IA local             │ IA en la nube                   │
│ Foundry Local        │ Azure AI Foundry/OpenAI         │
│ Modelos pequeños     │ Modelos de mayor capacidad      │
└──────────────────────┴──────────────────────────────────┘
```

### 10.3 Organización de la solución

```text
Citiz.sln

src/
├── Citiz.Core/
│   ├── Exams/
│   ├── Questions/
│   ├── Content/
│   ├── Learning/
│   ├── Discovery/
│   ├── Places/
│   ├── People/
│   └── Users/
│
├── Citiz.Application/
│   ├── Exams/
│   ├── PracticeSessions/
│   ├── Simulations/
│   ├── Discovery/
│   ├── DailyLearning/
│   ├── Games/
│   ├── Localization/
│   ├── ContentUpdates/
│   └── Abstractions/
│
├── Citiz.Infrastructure/
│   ├── Persistence/
│   ├── Identity/
│   ├── ContentSources/
│   ├── Maps/
│   ├── MediaRights/
│   ├── Speech/
│   ├── AI/
│   └── Notifications/
│
├── Citiz.Api/
│   ├── Endpoints/
│   ├── Authentication/
│   └── Program.cs
│
├── Citiz.Web/
│   └── Blazor Web App
│
├── Citiz.Hybrid/
│   └── .NET MAUI Blazor Hybrid
│
├── Citiz.SharedUI/
│   └── Razor Class Library
│
├── Citiz.Contracts/
│   ├── Requests/
│   ├── Responses/
│   └── Events/
│
├── Citiz.Cli/
│   ├── Commands/
│   ├── Validation/
│   └── Packaging/
│
├── Citiz.Mcp/
│   ├── Tools/
│   └── Resources/
│
├── Citiz.ContentWorker/
│   ├── USCIS/
│   ├── Congress/
│   ├── StateSources/
│   └── ChangeDetection/
│
└── Citiz.AppHost/
    └── .NET Aspire

tests/
├── Citiz.Domain.Tests/
├── Citiz.Application.Tests/
├── Citiz.IntegrationTests/
├── Citiz.ContentTests/
└── Citiz.PlaywrightTests/
```

La estructura seguirá una Clean Architecture pragmática. Se mantendrán límites claros, pero se evitará fragmentar la solución en proyectos innecesarios.

### 10.4 Interfaz compartida

La Razor Class Library incluirá:

- Pantallas de preguntas.
- Tarjetas de estudio.
- Flujo de aprendizaje.
- Resultados de simulaciones.
- Componentes de progreso.
- Formularios reutilizables.
- Diseño accesible.
- Localización inglés/español.

Las funciones específicas de cada plataforma se proporcionarán mediante interfaces e inyección de dependencias. Por ejemplo:

```csharp
public interface IAudioRecorder
{
    Task StartAsync(CancellationToken cancellationToken);
    Task<AudioRecording> StopAsync(CancellationToken cancellationToken);
}

public interface IOfflineContentStore
{
    Task SavePackageAsync(
        ContentPackage package,
        CancellationToken cancellationToken);

    Task<ContentPackage?> GetCurrentPackageAsync(
        CancellationToken cancellationToken);
}
```

En la web, `IAudioRecorder` podrá usar JavaScript y las API del navegador. En .NET MAUI utilizará capacidades nativas. Los componentes Razor no necesitarán conocer la plataforma subyacente.

---

## 11. Modelo de dominio y contenido versionado

### 11.1 Principio de versionado

Las preguntas y respuestas no deben estar incrustadas en la aplicación. El backend administrará versiones, periodos de vigencia, jurisdicciones, fuentes y revisiones.

### 11.2 Entidades principales

#### `ExamVersion`

- Identificador.
- Nombre de versión.
- Fecha de entrada en vigor.
- Regla de elegibilidad.
- Cantidad total de preguntas.
- Cantidad administrada.
- Respuestas necesarias para aprobar.
- Límite de respuestas incorrectas.
- Estado: borrador, activo o retirado.

#### `Question`

- Identificador permanente.
- Texto oficial.
- Categoría.
- Idioma original.
- Orden oficial.
- Versiones en las que aparece.
- Indicador 65/20.
- Indicador de respuesta dinámica.
- Fuente y fecha de verificación.

#### `AcceptedAnswer`

- Pregunta asociada.
- Texto aceptado.
- Variaciones aceptadas.
- Fecha de inicio y fin de vigencia.
- Jurisdicción aplicable.
- Fuente oficial.
- Estado editorial.

#### `DynamicOffice`

- Nombre del cargo.
- Tipo de jurisdicción.
- Jurisdicción.
- Nombre del funcionario.
- Fecha de inicio.
- Fecha de finalización.
- Fuente oficial.
- Fecha de última verificación.

#### `ContentRevision`

- Entidad afectada.
- Valor anterior.
- Valor nuevo.
- Motivo del cambio.
- Fuente.
- Autor y aprobador.
- Fecha de publicación.
- Usuarios o versiones afectadas.

#### `DiscoveryTopic`

- Título, categoría y resumen.
- Nivel de inglés y duración.
- Estado editorial.
- Idiomas disponibles.
- Importancia educativa.
- Fechas de publicación y verificación.

#### `HistoricalEvent`

- Fecha o periodo.
- Descripción aprobada.
- Lugares y personas relacionados.
- Fuentes primarias y secundarias.
- Temas y rutas asociadas.

#### `PersonProfile`

- Nombre y periodo histórico.
- Áreas de contribución.
- Biografía breve y documentada.
- Lugares, acontecimientos y vocabulario relacionados.
- Estado editorial y fuentes.

#### `Place`

- Nombre, tipo y región.
- Estado o territorio.
- Coordenadas aproximadas.
- Importancia geográfica, histórica o cultural.
- Recursos visuales y condiciones de uso.

#### `LearningConnection`

Relacionará una pregunta, cápsula, persona, lugar, acontecimiento, palabra, actividad o ruta sin duplicar el contenido fuente.

#### `DailyDiscovery`

- Fecha y audiencia.
- Tema principal.
- Nivel, idioma y duración.
- Región opcional.
- Orden de presentación.
- Estado editorial.

### 11.3 Configuración de una versión

La lógica del examen se almacenará como datos configurables:

```json
{
  "version": "2025",
  "filingDateFrom": "2025-10-20",
  "questionsInBank": 128,
  "questionsAsked": 20,
  "passingAnswers": 12,
  "failingAnswers": 9
}
```

Esto permitirá incorporar una versión futura sin modificar el motor principal.

### 11.4 Resolución de respuestas dinámicas

```text
Versión del examen
+ Fecha de presentación del N-400
+ Estado del usuario
+ Distrito congresional
+ Fecha estimada de entrevista
+ Reglas o consideraciones especiales
= Respuesta oficial aplicable
```

Si los datos no permiten resolver una respuesta de manera confiable, el sistema no adivinará. Solicitará la información necesaria o mostrará una advertencia.

---

## 12. Estrategia de inteligencia artificial

### 12.1 Responsabilidades permitidas

La IA podrá:

- Representar a un entrevistador virtual.
- Formular transiciones naturales.
- Ajustar el nivel del lenguaje.
- Explicar conceptos usando contexto aprobado.
- Generar ejercicios adicionales.
- Crear ayudas mnemotécnicas.
- Detectar áreas débiles.
- Proponer planes de estudio.
- Evaluar respuestas abiertas de forma asistida.
- Ayudar a comparar documentos oficiales.
- Proponer traducciones y explicaciones para revisión editorial.
- Narrar cápsulas de descubrimiento con diferentes niveles de inglés.
- Crear preguntas, analogías y ayudas de memoria basadas únicamente en contenido aprobado.
- Conectar una persona, lugar o acontecimiento con temas ya estudiados.
- Guiar recorridos virtuales y conversaciones educativas.

### 12.2 Responsabilidades prohibidas

La IA no podrá:

- Crear una respuesta oficial nueva.
- Presentar como hecho histórico una afirmación no respaldada por el contenido aprobado.
- Inventar citas, fuentes, fechas, biografías o atribuciones.
- Modificar contenido publicado.
- Cambiar reglas del examen.
- Publicar cambios sin aprobación humana.
- Garantizar que USCIS aceptará una respuesta.
- Proporcionar asesoría legal.
- Exponer datos sensibles a modelos sin consentimiento y controles.

### 12.3 Flujo de evaluación de una respuesta

```text
1. El motor selecciona una pregunta oficial.
2. La base de datos entrega respuestas vigentes.
3. La IA presenta la pregunta.
4. El estudiante responde.
5. Speech-to-text produce una transcripción.
6. Un evaluador determinista busca coincidencias claras.
7. La IA analiza solamente los casos ambiguos.
8. Se aplica un umbral de confianza.
9. La aplicación explica el resultado con contenido aprobado.
```

Ejemplo de salida estructurada:

```json
{
  "conceptuallyCorrect": true,
  "officialAnswerMatched": "The Constitution",
  "confidence": 0.94,
  "keyWordsDetected": [
    "constitution"
  ],
  "pronunciationPractice": [
    "constitution"
  ],
  "feedbackLevel": "beginner"
}
```

### 12.4 Modos de IA

#### Modo oficial

- Solo contenido publicado.
- Solo respuestas aceptadas.
- Temperatura baja.
- Salida estructurada.
- Fuente visible.
- Sin creatividad que altere el contenido.

#### Modo creativo

- Historias mnemotécnicas.
- Juegos y retos.
- Analogías.
- Conversaciones de práctica.
- Ejercicios personalizados.
- Contenido claramente marcado como educativo.

### 12.5 Semantic Kernel

Semantic Kernel funcionará como capa entre la aplicación y los modelos. Permitirá:

- Cambiar de proveedor sin reescribir el dominio.
- Exponer funciones C# controladas como plugins.
- Aplicar filtros y telemetría.
- Restringir las funciones disponibles a cada agente.
- Utilizar salidas estructuradas.
- Incorporar servicios de IA gradualmente.

Plugins iniciales:

```text
ExamPlugin
├── GetCurrentExamVersion
├── GetNextOfficialQuestion
├── GetAcceptedAnswers
└── RecordPracticeResult

StudentPlugin
├── GetEnglishLevel
├── GetWeakTopics
├── GetStudyPreferences
└── SaveLearningObservation

ContentPlugin
├── GetApprovedExplanation
├── GetOfficialSource
└── GetCurrentDynamicAnswer

DiscoveryPlugin
├── GetDailyDiscovery
├── GetApprovedTopic
├── GetRelatedPeopleAndPlaces
├── GetExamConnections
└── RecordDiscoveryProgress

SpeechPlugin
├── TranscribeAudio
├── AnalyzePronunciation
└── GenerateSpeech
```

Para el MVP se recomienda un agente controlado mediante un flujo explícito. No se requiere inicialmente una arquitectura multiagente autónoma.

### 12.6 Modelos locales y en la nube

#### IA local

Adecuada para:

- Explicaciones breves.
- Clasificación.
- Ejercicios sencillos.
- Conversación limitada sin conexión.
- Corrección básica.
- Resúmenes de progreso.

#### IA en la nube

Adecuada para:

- Simulaciones más naturales.
- Evaluación compleja de respuestas abiertas.
- Interacciones largas.
- Planes personalizados avanzados.
- Análisis lingüístico más preciso.

La aplicación dependerá de una abstracción propia:

```csharp
public interface ICitizenshipAiService
{
    Task<InterviewTurnResult> ContinueInterviewAsync(
        InterviewContext context,
        CancellationToken cancellationToken);

    Task<AnswerEvaluation> EvaluateAnswerAsync(
        AnswerEvaluationRequest request,
        CancellationToken cancellationToken);

    Task<StudentExplanation> ExplainAsync(
        ExplanationRequest request,
        CancellationToken cancellationToken);
}
```

Implementaciones posibles:

```text
FoundryLocalCitizenshipAiService
AzureFoundryCitizenshipAiService
WindowsLocalLanguageModelService
NoAiFallbackService
```

No se debe acoplar la arquitectura exclusivamente a Phi Silica ni a otro modelo particular. El modelo disponible podrá cambiar según plataforma, capacidad del dispositivo, costo, privacidad y evolución tecnológica.

---

## 13. Obtención automática de datos oficiales

### 13.1 Jerarquía de fuentes

1. API oficial.
2. Documento estructurado oficial.
3. Página HTML oficial.
4. PDF oficial.
5. Sitios oficiales federales o estatales.
6. Extracción asistida y revisión humana.

El portal público de USCIS ofrece APIs, pero el catálogo identificado se concentra en estado de casos y FOIA. Por tanto, el diseño no debe suponer que existe una API pública para las preguntas y respuestas del examen.

### 13.2 Catálogo de fuentes

Cada fuente tendrá:

- Nombre.
- Autoridad.
- URL.
- Formato.
- Frecuencia de revisión.
- Política de extracción.
- Último hash.
- Fecha de última consulta.
- Estado operativo.
- Requisito de revisión humana.

### 13.3 Pipeline de actualización

```text
Temporizador
    │
    ▼
Consultar catálogo de fuentes
    │
    ▼
Descargar API, HTML o PDF
    │
    ▼
Normalizar y calcular hash
    │
    ├── Sin cambios ──> Registrar comprobación
    │
    ▼
Detectar diferencias
    │
    ▼
Extraer candidatos
    │
    ▼
Validar estructura y reglas
    │
    ▼
Revisión editorial humana
    │
    ├── Rechazar ──> Registrar motivo
    │
    ▼
Aprobar con fecha de vigencia
    │
    ▼
Publicar nueva revisión
    │
    ▼
Invalidar caché y generar paquete offline
    │
    ▼
Notificar a usuarios afectados
```

### 13.4 Estados editoriales

```text
Detected
Extracted
NeedsReview
Approved
Scheduled
Published
Rejected
Superseded
```

### 13.5 Papel de la IA en la ingestión

La IA podrá destacar diferencias, extraer candidatos y redactar propuestas. La publicación requerirá reglas deterministas y aprobación humana.

Cada cambio guardará:

- URL de origen.
- Fecha de consulta.
- Hash del documento.
- Fragmento de evidencia.
- Valor anterior y nuevo.
- Fecha de vigencia.
- Persona que aprobó.
- Modelo utilizado, si participó.
- Versión del extractor o prompt.
- Confianza estimada.

### 13.6 Fuentes federales, estatales y locales

```text
USCIS
├── Versiones del examen
├── Preguntas oficiales
├── Reglas de administración
└── Actualizaciones generales

Federal
├── White House
├── Congress.gov
├── House.gov
├── Senate.gov
├── SupremeCourt.gov
├── Library of Congress
├── National Archives
├── National Park Service
├── U.S. Census Bureau
└── Smithsonian Institution

Estatal
├── Sitio oficial del gobernador
├── Legislatura estatal
└── Secretaría de Estado

Geográfica
├── Código postal
├── Dirección aproximada
└── Distrito congresional
```

Un código postal puede corresponder a varios distritos. Si existe ambigüedad, el sistema solicitará una ubicación más precisa, resolverá el distrito y conservará preferiblemente el distrito resultante en lugar de una dirección completa.

---

## 14. Experiencia offline y sincronización

La aplicación Hybrid podrá descargar un paquete firmado con:

- Versión aplicable del examen.
- Preguntas y respuestas estables.
- Respuestas dinámicas vigentes.
- Audios esenciales.
- Lecciones seleccionadas.
- Plan de estudio.
- Manifiesto, versión y fecha de expiración.

Proceso de sincronización:

```text
1. Comprobar el manifiesto remoto.
2. Descargar solo las diferencias.
3. Validar firma y hashes.
4. Actualizar SQLite dentro de una transacción.
5. Sincronizar progreso pendiente.
6. Resolver conflictos.
7. Advertir si una respuesta dinámica cambió.
```

La práctica básica y ciertas funciones de IA local podrán operar sin conexión. Las simulaciones avanzadas usarán la nube cuando esté disponible.

---

## 15. Seguridad, privacidad y uso responsable

### 15.1 Minimización de datos

La aplicación no necesita recopilar:

- Número de Seguro Social.
- Número de registro de extranjero.
- Copia de la residencia permanente.
- Contraseña de USCIS.
- Copia completa del N-400.
- Documentos de identidad.

Datos opcionales útiles:

- Fecha de presentación.
- Mes estimado de entrevista.
- Estado y código postal.
- Distrito congresional.
- Idioma preferido.
- Nivel de inglés.
- Progreso educativo.

### 15.2 Grabaciones

- Serán opcionales.
- Tendrán controles de eliminación.
- Usarán retención limitada.
- No se reutilizarán sin consentimiento explícito.
- Se cifrarán en tránsito y en reposo cuando se almacenen.

### 15.3 Controles para IA

- Recuperación limitada a contenido aprobado.
- Temperatura baja en evaluaciones.
- Salidas estructuradas.
- Protección contra prompt injection.
- Separación de información personal y prompts.
- Registro de modelo y proveedor.
- Límites de uso y costos.
- Evaluaciones de calidad y sesgo.
- Fallback determinista cuando la IA no esté disponible.

### 15.4 Seguridad comunitaria

- Prohibición de publicar documentos o números migratorios.
- Moderación y reportes.
- Prevención de fraude y solicitudes de dinero.
- Verificación de profesionales cuando corresponda.
- Etiquetas visibles para contenido comunitario.
- Ninguna contribución comunitaria reemplazará contenido oficial.

---

## 16. Accesibilidad y experiencia de usuario

- Botones grandes.
- Tamaño de texto ajustable.
- Alto contraste.
- Compatibilidad con lectores de pantalla.
- Navegación por teclado.
- Instrucciones de una sola acción por pantalla.
- Repetición sin penalización.
- Velocidad de audio configurable.
- Subtítulos opcionales.
- Indicadores que no dependan solamente del color.
- Modo 65/20 claramente identificado.
- Descargas para estudiar sin conexión.
- Materiales imprimibles.
- Historias locales moderadas.
- Eventos culturales, museos y oportunidades de servicio comunitario.

### Navegación propuesta

```text
Inicio
├── Mi plan de estudio
├── Preguntas de ciudadanía
│   ├── Aprender
│   ├── Tarjetas
│   ├── Escuchar
│   └── Simulación
├── Inglés
│   ├── Hablar
│   ├── Leer
│   ├── Escribir
│   └── Vocabulario
├── Entrevista de práctica
├── Mi progreso
├── Comunidad
└── Actualizaciones oficiales
```

La pantalla principal mostrará:

- Próxima actividad.
- Progreso semanal.
- Tiempo aproximado hasta la entrevista.
- Contenido que necesita repaso.
- Alertas de cambios oficiales.
- Botón principal: **Continuar estudiando**.

---

## 17. Motores y límites arquitectónicos

```text
OfficialContentEngine
    determina los hechos oficiales del examen

ExamEngine
    determina reglas, selección y resultado

LearningEngine
    determina qué practicar después

GameEngine
    convierte objetivos educativos en experiencias de juego

DiscoveryEngine
    recomienda qué conocer y conecta temas

AIOrchestrator
    conversa, explica y crea actividades controladas

ContentIngestion
    detecta cambios externos

EditorialWorkflow
    verifica, aprueba y publica
```

Una dependencia importante es que `AIOrchestrator` consulte al `OfficialContentEngine` y al `DiscoveryEngine`; nunca debe sustituirlos. `DiscoveryEngine` solo recomendará contenido con estado publicado y respetará derechos de uso, nivel educativo y trazabilidad.

---

## 18. Requisitos no funcionales

### Rendimiento

- Inicio rápido en dispositivos móviles.
- Caché de contenido estable.
- Respuestas de API paginadas.
- Audio transmitido o descargado según preferencia.

### Disponibilidad

- Funciones básicas disponibles aunque falle la IA.
- Práctica offline en Hybrid.
- Reintentos controlados para las fuentes oficiales.

### Escalabilidad

- Servicios sin estado cuando sea posible.
- Procesamiento de documentos en colas.
- Almacenamiento de audio fuera de la base relacional.
- Límites por usuario para IA y voz.

### Observabilidad

- OpenTelemetry.
- Correlation IDs.
- Métricas de importación y actualización.
- Registro de fallos de proveedores de IA.
- Auditoría editorial separada de logs técnicos.
- Paneles de costo, latencia y calidad.

### Open source y portabilidad

- Compilación reproducible y documentada.
- Configuración mediante variables de entorno y archivos de ejemplo.
- Ninguna dependencia obligatoria de una nube específica para funciones básicas.
- Despliegue documentado para la instancia oficial y para self-hosting.
- Versiones semánticas y changelog público.
- Migraciones de datos documentadas.
- Exportación del progreso del usuario en formato portable.

### Internacionalización

- Recursos localizados.
- Contenido editorial por idioma.
- Separación entre traducción y texto oficial.
- Preparación para agregar idiomas adicionales.

---

## 19. Estrategia de pruebas

### Pruebas unitarias

- Selección de versión.
- Reglas de aprobación.
- Vigencia de respuestas.
- Cálculo de repaso.
- Resolución de jurisdicción.

### Pruebas de integración

- EF Core y base de datos.
- API y autenticación.
- Importadores.
- Proveedores de IA.
- Sincronización offline.

### Pruebas de contenido

- Todas las preguntas tienen fuente.
- No existen periodos de vigencia solapados injustificados.
- Las versiones tienen reglas completas.
- Las traducciones no sustituyen el texto oficial.
- Las respuestas dinámicas no están vencidas.

### Pruebas de IA

- Conjunto de respuestas correctas, incorrectas y ambiguas.
- Evaluación bilingüe.
- Resistencia a prompt injection.
- Consistencia de salida JSON.
- Medición de falsos positivos y falsos negativos.
- Pruebas de fallback.

### Pruebas de distribución y portabilidad

- PWA en navegadores principales.
- Instalación y actualización offline.
- Android, Windows y plataformas priorizadas de Hybrid.
- Despliegue self-hosted con configuración mínima.
- Compatibilidad de paquetes NuGet y CLI.
- Verificación de que las funciones básicas no requieren cuenta ni servidor.

### Pruebas de interfaz

- Playwright para flujos críticos.
- Accesibilidad automatizada y manual.
- Tamaños de pantalla.
- Navegación por teclado.
- Rendimiento en equipos de bajos recursos.

---

## 20. Fases del proyecto

### Fase 1 — Investigación y definición

- Entrevistas con estudiantes e instructores.
- Mapa del recorrido de preparación.
- Reglas y versiones del examen.
- Inventario de fuentes oficiales.
- Modelo de dominio.
- Política editorial, de privacidad y de IA.
- Selección de licencias y política de marca.
- README, CONTRIBUTING, SECURITY, GOVERNANCE y CODE_OF_CONDUCT iniciales.
- Prototipos de navegación.
- Taxonomía inicial de historia, geografía, personas, cultura, patrimonio e instituciones.
- Piloto de cápsulas “Hoy en Estados Unidos”.

### Fase 2 — Producto mínimo viable

- Blazor Web App.
- API ASP.NET Core.
- Android y Windows con .NET MAUI Blazor Hybrid.
- Razor Class Library compartida.
- Versiones 2008 y 2025.
- Preguntas bilingües.
- Audio.
- Tarjetas y práctica por categoría.
- Motor determinista de examen.
- Progreso.
- SQLite offline.
- Panel administrativo.
- Importación de fuentes.
- Reporte de contenido.
- Catálogo inicial de descubrimiento.
- Cápsula diaria.
- Conexiones entre temas, vocabulario y preguntas del examen.
- Primera ruta “Mi primera ruta por Estados Unidos”.
- PWA utilizable sin registro.
- Pipeline público de compilación, pruebas y validación de contenido.

### Fase 3 — Inglés e inteligencia artificial

- Speech-to-text y text-to-speech.
- Práctica oral.
- Lectura y escritura.
- Semantic Kernel.
- Simulación conversacional controlada.
- Explicaciones personalizadas.
- Evaluación asistida de respuestas.
- Narrador educativo para rutas de descubrimiento.
- Recomendaciones personalizadas del DiscoveryEngine.

### Fase 4 — Comunidad

- Grupos moderados.
- Eventos.
- Voluntarios verificados.
- Panel para organizaciones.
- Estadísticas grupales anónimas.
- Materiales imprimibles.
- Historias locales moderadas.
- Eventos culturales, museos y oportunidades de servicio comunitario.

### Fase 5 — Expansión

- iOS y macOS, según prioridad.
- Más idiomas, priorizados mediante datos de uso y alianzas comunitarias.
- Modelos locales adicionales.
- Panel avanzado para instructores.
- Integraciones oficiales nuevas.
- Calificación avanzada de pronunciación.
- Automatización editorial adicional, manteniendo aprobación humana.
- Ruta “50 estados, 50 historias”.
- Pasaporte virtual y mapas interactivos.
- Integración ampliada con colecciones y fuentes públicas autorizadas.
- CLI estable, paquetes NuGet e integración MCP opcional.
- Guía de self-hosting para organizaciones.

---

## 21. Backlog inicial del MVP

### Épica: incorporación

- Como estudiante, quiero indicar cuándo presenté el N-400 para recibir la versión correcta.
- Como estudiante, quiero elegir español o inglés.
- Como estudiante, quiero realizar un diagnóstico breve.

### Épica: contenido

- Como editor, quiero importar preguntas oficiales.
- Como editor, quiero registrar fuente y vigencia.
- Como estudiante, quiero ver cuándo se verificó una respuesta.

### Épica: práctica

- Como estudiante, quiero practicar por categoría.
- Como estudiante, quiero escuchar preguntas.
- Como estudiante, quiero repasar mis errores.

### Épica: simulación

- Como estudiante, quiero practicar con las reglas de mi versión.
- Como estudiante, quiero recibir un reporte al finalizar.
- Como sistema, quiero detener la sesión de acuerdo con las reglas configuradas.

### Épica: actualización

- Como administrador, quiero detectar cambios en una fuente.
- Como editor, quiero comparar valores anteriores y nuevos.
- Como editor, quiero aprobar y programar un cambio.

### Épica: proyecto abierto y privacidad

- Como visitante, quiero estudiar sin crear una cuenta.
- Como usuario, quiero saber qué funciones se ejecutan localmente y cuáles usan la nube.
- Como organización, quiero desplegar una instancia propia.
- Como contribuidor, quiero validar contenido desde la CLI.
- Como maintainer, quiero que cada pull request ejecute controles de calidad y seguridad.
- Como usuario, quiero exportar o eliminar mi progreso.

### Épica: localización

- Como usuario, quiero elegir el idioma de la interfaz sin cambiar el idioma del examen.
- Como estudiante, quiero activar apoyo bilingüe por actividad.
- Como editor, quiero conocer el estado de revisión de cada traducción.
- Como administrador, quiero agregar un idioma sin modificar la lógica principal.

### Épica: Juega y Aprende

- Como estudiante, quiero practicar en juegos de 2 a 10 minutos.
- Como estudiante, quiero que la dificultad se adapte a mis resultados.
- Como instructor, quiero asignar retos educativos a un grupo.
- Como sistema, quiero registrar resultados de juego como evidencia de práctica, no como resultado oficial.

### Épica: descubrir Estados Unidos

- Como estudiante, quiero conocer algo nuevo cada día en menos de cinco minutos.
- Como estudiante, quiero relacionar una lección con el examen y con palabras en inglés.
- Como estudiante, quiero explorar mi estado, región y comunidad.
- Como estudiante, quiero guardar temas para estudiarlos después.
- Como editor, quiero relacionar personas, lugares, acontecimientos y fuentes.
- Como editor, quiero registrar derechos de uso de cada recurso visual o documental.
- Como sistema, quiero recomendar contenido publicado según nivel, intereses y progreso sin inferir características sensibles.

### Épica: offline

- Como estudiante, quiero descargar mi contenido.
- Como estudiante, quiero practicar sin conexión.
- Como estudiante, quiero sincronizar mi progreso más tarde.

---

## 22. Indicadores de éxito

### Educativos

- Mejora entre diagnóstico y simulaciones.
- Dominio por categoría.
- Reducción de errores repetidos.
- Comprensión auditiva.
- Respuestas orales comprensibles.
- Frecuencia y consistencia de estudio.
- Cápsulas de descubrimiento completadas.
- Capacidad de relacionar examen, inglés y contexto nacional.
- Diversidad de regiones y categorías exploradas.

### Producto

- Retención semanal.
- Finalización del plan.
- Uso del modo offline.
- Éxito de sincronización.
- Satisfacción de estudiantes e instructores.
- Uso de rutas, mapas y pasaporte virtual.
- Participación y retorno a Juega y Aprende.
- Tasa de finalización por idioma y paridad entre traducciones.
- Retención originada por la experiencia diaria de descubrimiento.
- Porcentaje de sesiones completadas sin registro.
- Uso offline y despliegues comunitarios conocidos.
- Tiempo para que una nueva persona ejecute el proyecto localmente.

### Contenido

- Tiempo desde detección hasta publicación.
- Porcentaje de contenido con fuente vigente.
- Incidencias reportadas y resueltas.
- Importaciones fallidas.
- Respuestas dinámicas próximas a vencer.
- Cobertura de fuentes, atribuciones y derechos de uso.
- Porcentaje de contenido traducido, revisado y vigente por idioma.
- Representación equilibrada de regiones, periodos y tipos de contribución.

### IA

- Precisión de evaluación sobre un conjunto validado.
- Tasa de casos ambiguos.
- Latencia.
- Costo por sesión.
- Disponibilidad del fallback.
- Incidentes de alucinación o incumplimiento.
- Porcentaje de operaciones realizadas localmente frente a servicios remotos.
- Contribuidores activos, idiomas mantenidos y tiempo de revisión de pull requests.

---

## 23. Riesgos y mitigaciones

### Alucinaciones de IA

**Mitigación:** contenido aprobado, RAG limitado, salidas estructuradas, evaluación determinista y avisos claros.

### Cambios en sitios oficiales

**Mitigación:** adaptadores por fuente, hashes, alertas operativas, pruebas de contrato y revisión manual.

### Falta de API oficial

**Mitigación:** arquitectura de importadores para API, HTML y PDF; catálogo de fuentes y procesos editoriales.

### Evaluación oral imprecisa

**Mitigación:** umbrales conservadores, transcripción visible, posibilidad de repetir y lenguaje no concluyente.

### Costos de IA y voz

**Mitigación:** cuotas, caché, modelos pequeños, procesamiento local, telemetría y degradación controlada.

### Privacidad

**Mitigación:** minimización de datos, consentimiento, retención limitada, cifrado y eliminación.

### Fraude comunitario

**Mitigación:** moderación, reportes, verificación y restricciones de contacto.

### Fragmentación del proyecto abierto

**Mitigación:** contratos estables, versionado semántico, gobernanza pública, ADR, pruebas de compatibilidad y política clara de extensiones.

### Costos de infraestructura comunitaria

**Mitigación:** funciones locales por defecto, límites transparentes, donaciones, patrocinios, subvenciones y posibilidad de self-hosting.

### Uso engañoso de la marca Citiz

**Mitigación:** licencia de código separada de una política de marca que proteja la identidad de las versiones oficiales sin impedir forks legítimos.

### Traducciones inexactas o desactualizadas

**Mitigación:** glosario central, revisión humana, versionado lingüístico, alertas cuando cambie el contenido fuente y bloqueo de traducciones vencidas en modos oficiales.

### Gamificación que distraiga del aprendizaje

**Mitigación:** objetivos educativos por juego, sesiones breves, métricas de dominio y separación del modo de examen.

### Sesgo, omisiones o simplificación histórica

**Mitigación:** política editorial, múltiples fuentes confiables, revisión especializada, contexto visible, registro de cambios y mecanismos para reportar problemas.

### Derechos de uso de imágenes y documentos

**Mitigación:** registro de licencia por recurso, preferencia por dominio público o permisos claros, atribución y bloqueo de publicación si faltan derechos.

### Sobrecarga de contenido

**Mitigación:** cápsulas breves, rutas progresivas, nivel configurable y prioridad a conexiones significativas con el examen y el inglés.

### Dependencia tecnológica

**Mitigación:** interfaces propias, Semantic Kernel, proveedores intercambiables y fallback sin IA.

---

## 24. Decisiones pendientes para evaluación

1. Nombre y marca definitiva.
2. PostgreSQL frente a Azure SQL.
3. Proveedor inicial de identidad.
4. Plataforma móvil prioritaria después de Android y Windows.
5. Proveedor de voz inicial.
6. Modelo local inicial y dispositivos soportados.
7. Política exacta de retención de grabaciones.
8. Alcance del vocabulario N-400 en el MVP.
9. Proceso de validación legal y editorial.
10. Organizaciones comunitarias para el piloto.
11. Modelo de sostenibilidad: gratuito, donaciones, subvenciones o servicios institucionales.
12. Frecuencia de actualización por tipo de fuente.
13. Taxonomía editorial definitiva del módulo Descubre.
14. Instituciones culturales y educativas para alianzas.
15. Política de representación histórica y revisión especializada.
16. Alcance inicial del mapa y del pasaporte virtual.
17. Política de licencias, dominio público y atribución de recursos.
18. Idiomas definitivos del lanzamiento y variante de chino.
19. Proceso y presupuesto de revisión lingüística.
20. Juegos incluidos en el MVP y reglas de recompensas.
21. Licencia definitiva del código.
22. Licencia del contenido editorial y traducciones.
23. Política de marca y uso del nombre Citiz.
24. Alcance de self-hosting y soporte comunitario.
25. Formato público del contenido y esquemas de validación.
26. Alcance inicial de CLI, NuGet y MCP.

---

## 25. Recomendación final

La primera implementación debe priorizar confiabilidad antes que complejidad. El MVP debe demostrar diez capacidades centrales:

1. Seleccionar correctamente la versión del examen.
2. Entregar contenido oficial versionado, con fuente y vigencia.
3. Permitir práctica bilingüe y parcialmente offline.
4. Ejecutar simulaciones mediante un motor determinista.
5. Incorporar IA de manera controlada para conversación y explicación.
6. Ofrecer una experiencia diaria de descubrimiento conectada con inglés y civismo.
7. Permitir una interfaz multilingüe separada del idioma de práctica.
8. Incluir al menos tres juegos educativos breves con resultados conectados al LearningEngine.
9. Funcionar sin registro para las actividades esenciales y explicar claramente los límites de privacidad.
10. Publicarse con repositorio, licencias, documentación comunitaria y pipeline de calidad.

Una vez estabilizados estos fundamentos, podrán añadirse evaluación oral avanzada, comunidad, mapas interactivos, pasaporte virtual, modelos locales y automatización editorial más sofisticada.

La arquitectura propuesta permite cambiar el modelo de IA, el proveedor de nube, la modalidad de Blazor, la fuente de datos o la versión del examen sin reescribir el núcleo del producto. Esa flexibilidad es esencial para una aplicación educativa que debe mantenerse vigente durante muchos años.

---

## 26. Referencias oficiales consultadas

1. USCIS, **2025 Civics Test**: <https://www.uscis.gov/citizenship-resource-center/naturalization-test-and-study-resources/2025-civics-test>
2. USCIS, **The Naturalization Interview and Test**: <https://www.uscis.gov/citizenship/learn-about-citizenship/the-naturalization-interview-and-test>
3. USCIS, **Study for the Test**: <https://www.uscis.gov/citizenship/find-study-materials-and-resources/study-for-the-test>
4. USCIS, **Check for Test Updates**: <https://www.uscis.gov/citizenship/find-study-materials-and-resources/check-for-test-updates>
5. USCIS, **Developer Portal**: <https://developer.uscis.gov/>
6. Microsoft Learn, **ASP.NET Core Blazor Hybrid**: <https://learn.microsoft.com/aspnet/core/blazor/hybrid/>
7. Microsoft Learn, **Build a .NET MAUI Blazor Hybrid app**: <https://learn.microsoft.com/aspnet/core/blazor/hybrid/tutorials/maui>
8. Microsoft Learn, **Semantic Kernel documentation**: <https://learn.microsoft.com/semantic-kernel/>
9. Microsoft Learn, **Get started with Foundry Local**: <https://learn.microsoft.com/azure/foundry-local/get-started>
10. Microsoft Learn, **Local AI on Windows**: <https://learn.microsoft.com/windows/ai/>
11. Smithsonian Institution, **Social Studies and Civics Learning Resources**: <https://www.si.edu/educators/social-studies-resources>
12. Library of Congress, **Classroom Materials and Primary Sources**: <https://www.loc.gov/programs/teachers/classroom-materials/>
13. National Museum of American History, **Classroom Resources**: <https://americanhistory.si.edu/education/resources>
14. National Archives, **Educator Resources**: <https://www.archives.gov/education>
15. National Park Service, **Discover History**: <https://www.nps.gov/subjects/history/index.htm>
16. U.S. Census Bureau, **Geography**: <https://www.census.gov/programs-surveys/geography.html>
17. U.S. Census Bureau, **ACS C16001: Language Spoken at Home**: <https://data.census.gov/table/ACSDT1Y2024.C16001>
18. U.S. Census Bureau, **Language Use Data**: <https://www.census.gov/topics/population/language-use/data.html>
19. PeopleWorks, **Signs of AI Writing — repositorio open source**: <https://github.com/peopleworks/SignsofAI>
20. PeopleWorks, **Signs of AI Writing — Code of Conduct**: <https://github.com/peopleworks/SignsofAI/blob/main/CODE_OF_CONDUCT.md>

---

## 27. Historial de versiones del documento

- **0.4 — 28 de julio de 2026:** incorporación de filosofía open source, manifiesto, estrategia privacy-first y local-first, cuenta opcional, licencias propuestas, estructura pública del repositorio, paquetes reutilizables, CLI, MCP opcional, self-hosting, gobernanza, automatización, sostenibilidad y métricas de utilidad.
- **0.3 — 28 de julio de 2026:** incorporación de estrategia multilingüe, idiomas iniciales, arquitectura de localización, módulo Juega y Aprende, `GameEngine`, backlog, métricas y riesgos asociados.
- **0.2 — 28 de julio de 2026:** incorporación de la visión ampliada de Citiz, los cuatro pilares, el módulo Descubre Estados Unidos, `DiscoveryEngine`, cápsulas diarias, rutas educativas, nuevas entidades, fuentes culturales, métricas, riesgos y backlog.
- **0.1 — 28 de julio de 2026:** consolidación inicial del plan maestro, arquitectura .NET, estrategia de IA, actualización de fuentes y alcance del MVP.
