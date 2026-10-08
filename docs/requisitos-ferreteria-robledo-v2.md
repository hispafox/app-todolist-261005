# Documento de requisitos — Ferretería Robledo, S.L. (versión 2.0)

| Dato | Valor |
|---|---|
| Cliente | Ferretería Robledo, S.L. (nave de Alcalá de Henares y tienda de la calle Mayor) |
| Interlocutores | Marisa Robledo, Javier Ortega, Lucía Carvajal |
| Fecha del análisis | 08/10/2026 |
| Versión / estado | 2.0 — Borrador para validar con el cliente |
| Sustituye a | Nada: complementa a `docs/requisitos-ferreteria-robledo.docx` (v1.0), que **no se modifica** |
| Fuentes | Los 3 `.txt` de `docs/AnalistaRequisitos/` (ver sección 2) |
| Documento hermano | `docs/historias-usuario-ferreteria-robledo-v2.xlsx` (45 historias de usuario, requisitos y preguntas abiertas v2) |
| Elaborado por | Agente analista de requisitos |

> **Convenciones.** Siglas de fuente: **COR** = `correo_resumen_reunion.txt`, **PET** = `peticiones_cliente.txt`, **LIS** = `productos_cliente.txt`. **Explícito** = lo dice el cliente; **Supuesto** = lo deduce el analista. Fases: **F1** = acordada para la 1.ª fase; **F1\*** = propuesta del analista para F1, aplazable si el calendario aprieta; **F2** = acordada para la 2.ª fase; **F2\*** = propuesta para F2; **Aparcado** = acordado dejarlo para más adelante.

## 1. Resumen ejecutivo

Ferretería Robledo lleva el stock en dos Excel (uno por ubicación), cobra con un programa antiguo que nadie quiere usar y hace a mano facturas, albaranes de proveedor y devoluciones. En la reunión del 06/10 acordó una **1.ª fase con tres bloques**: stock por ubicación (nave y tienda), venta en mostrador con las pistolas USB de código de barras que ya tiene, y facturación conforme a Hacienda (Verifactu). Los perfiles de usuario (Dirección, Almacén y compras, Facturación y cobros, Mostrador) y la restricción de descuentos a Marisa y Lucía forman parte de esa fase.

Albaranes en PDF, tienda online e informe mensual para el gestor pasan a la **2.ª fase**; el asistente con IA queda **aparcado**. Fecha objetivo: **antes del inventario de enero de 2027**.

Esta versión consolida **42 requisitos funcionales** (21 Must, 15 Should, 5 Could, 1 Won't) y **12 no funcionales** (8 Must, 4 Should), añade un criterio de verificación a cada requisito y separa lo acordado de lo propuesto dentro de cada fase. Los cambios respecto a v1.0 están en la sección 11.

Bloquean el diseño tres decisiones del cliente: **vender sin stock** (Q-01), **pedidos a proveedor** (Q-02) y **qué pasó en 2023** (Q-03). Faltan además datos críticos: instalación y modo sin conexión (Q-04, Q-05), modalidad Verifactu (Q-06), códigos de barras (Q-10), Excel de la tienda (Q-09) y presupuesto (Q-19).

**Alerta de encaje:** nada de lo pedido está cubierto por el proyecto actual (lista de tareas). Es un producto distinto y mayor; además el modo sin conexión y Verifactu exceden el stack simple .NET + SQLite + React (R-01, R-02, Q-26).

## 2. Fuentes analizadas

| Fuente | Tipo | Estado | Observaciones |
|---|---|---|---|
| `correo_resumen_reunion.txt` [COR] | Correo de Pedro a Marisa, 07/10/2026 | Procesada | Fuente más reciente y de mayor autoridad: fija alcance por fases, perfiles, plazo y 3 puntos pendientes. |
| `peticiones_cliente.txt` [PET] | Notas de llamadas, WhatsApp, correos y visita del 06/10 | Procesada | 32 peticiones sin ordenar, con duplicados y 3 contradicciones declaradas. |
| `productos_cliente.txt` [LIS] | Exportación del Excel de la nave, 06/10/2026 | Procesada | 39 productos, 9 familias, 5 proveedores. Solo stock de la nave. |

Los otros ficheros de `docs/AnalistaRequisitos/` (`Guión base.docx`, `Propuesta comercial.docx`) no forman parte de esta versión, que se limita a los TXT. Ningún documento original se ha modificado.

**Regla de precedencia aplicada:** si COR y PET difieren, prevalece COR (más reciente y validado en reunión); la diferencia se anota como conflicto.

## 3. Contexto y objetivos de negocio

### 3.1 Situación actual

- Stock en dos Excel, uno por ubicación [COR, LIS].
- Cobro con un programa antiguo de "seis clics" [COR, PET].
- Facturación, albaranes de proveedor y devoluciones/abonos a mano ("con un papel") [COR, PET].
- Lucía prepara a mano el informe mensual por familia para el gestor y calcula aparte el recargo de equivalencia [COR, PET].
- Descuentos de clientes habituales (10–15 % según familia) llevados "de cabeza" [PET].
- Pistolas de código de barras USB; la línea de la nave se cae con frecuencia [PET].

### 3.2 Objetivos de negocio

| ID | Objetivo |
|---|---|
| O1 | Una única fuente fiable de stock para nave y tienda. |
| O2 | Cobro ágil en mostrador, apto para personal nuevo. |
| O3 | Cumplir con Hacienda (Verifactu) y calcular bien el recargo de equivalencia. |
| O4 | Eliminar trabajo manual: albaranes, informes, descuentos y devoluciones en papel. |
| O5 | Controlar quién hace qué (perfiles y descuentos restringidos). |
| O6 | Continuidad: cobrar sin internet y copias de seguridad automáticas. |

Único criterio de éxito dado por el cliente: funcionando antes del inventario de enero [COR, PET].

### 3.3 Alcance por fases

| Fase | Contenido | Fuente |
|---|---|---|
| **F1** (acordada) | Stock por ubicación; venta en mostrador con pistola USB; facturación conforme a Hacienda; perfiles de usuario; descuentos solo para Marisa y Lucía. | COR |
| **F2** (acordada) | Albaranes en PDF, conexión con la tienda online, informe mensual para el gestor. | COR |
| **Aparcado** | Asistente con IA para consultar stock desde la web. | COR |
| **Sin asignar** | Resto de peticiones: el analista propone fase (F1\* / F2\*). | PET |
| **Pendiente del cliente** | Vender sin stock, pedidos a proveedor, qué pasó en 2023, listado de clientes en recargo de equivalencia (antes del 17/10). | COR |

### 3.4 Glosario

| Término | Definición |
|---|---|
| Nave / Tienda | Almacén de Alcalá de Henares y tienda de la calle Mayor. |
| Familia | Agrupación de productos: Herramienta eléctrica, Herramienta manual, Tornillería, Abrasivos, Química, EPI, Medición, Electricidad, Varios. |
| Stock mínimo | Nivel a partir del cual Javier suele pedir al proveedor; según LIS, no está revisado desde hace tiempo. |
| PVP | Precio de venta al público; en LIS va sin IVA. |
| Albarán | Documento de entrega del proveedor; llega en PDF por correo (15–20 a la semana). |
| Verifactu | Sistema de la AEAT para software de facturación (integridad, trazabilidad, huella y QR). |
| Recargo de equivalencia (RE) | Régimen de IVA de ciertos autónomos minoristas que lleva un recargo adicional en factura. |
| Factura simplificada | Documento fiscal habitual de una venta de mostrador. Uso por confirmar (Q-07). |
| Abono / rectificativa | Documento que corrige o anula una factura (devoluciones). |
| Gestor | Asesor externo que recibe el informe mensual y lleva la contabilidad. |

### 3.5 Contraste con el proyecto actual

Según `docs/analisis-diseño.md`, el proyecto actual es una lista de tareas (`TodoItem`, `TodoCategory`, `TodoUser`) sobre .NET 10, EF Core, SQLite y React; sus usuarios son referencias organizativas sin autenticación ni roles. **Ningún requisito de Robledo está cubierto: todos son Nuevo.** Solo se reutilizan patrones (API REST con servicios, EF Core, migraciones, React/TypeScript).

## 4. Usuarios y roles

| Rol | Persona | Necesidades principales | Fuente |
|---|---|---|---|
| Dirección | Marisa Robledo | Acceso completo; descuentos; revisar pedidos a proveedor; ver desde el móvil lo vendido hoy. | COR, PET |
| Almacén y compras | Javier Ortega | Stock, productos, proveedores y pedidos; avisos de stock mínimo. | COR, PET, LIS |
| Facturación y cobros | Lucía Carvajal | Facturas, cobros, descuentos, exportación a Excel, informe para el gestor. | COR, PET |
| Mostrador | 2 empleados (dos son nuevos) | Solo vender: leer códigos, cobrar, devoluciones; sin acceso a descuentos. | COR, PET |
| Gestor (externo) | Sin nombre | No usa el sistema; recibe el informe mensual (F2). Programa de contabilidad desconocido (Q-21). | COR, PET |
| Cliente final | Habituales y autónomos en RE | No usa el sistema; recibe facturas con descuento y recargo. Chat con IA aparcado. | PET, COR |

## 5. Requisitos funcionales

Todos Nuevo respecto al proyecto actual. Columna **Verificación** = criterio con el que se comprobará el requisito (esbozo para las historias de aceptación).

### 5.1 Catálogo y stock

| ID | Requisito | Prior. | Fase | Fuente | Tipo | Verificación |
|---|---|---|---|---|---|---|
| RF-01 | El sistema debe mantener el stock de cada producto por separado en la nave y en la tienda. | Must | F1 | COR; PET | Explícito | Una venta en la tienda no cambia el stock de la nave. |
| RF-02 | El sistema debe mostrar en una sola pantalla el stock de ambas ubicaciones, con total y desglose. | Must | F1 | PET | Explícito | Pantalla con columnas Nave, Tienda y Total por producto. |
| RF-03 | El sistema debe gestionar un catálogo con referencia, descripción, familia, proveedor, precio de compra, PVP sin IVA, stock, stock mínimo y observaciones. | Must | F1 | LIS | Explícito | Alta, edición y consulta de un producto con todos los campos. |
| RF-04 | El sistema debe importar el listado de productos desde Excel/CSV (formato español: coma decimal, tildes) sin duplicar referencias. | Must | F1 | COR; LIS | Supuesto | Importar las 39 líneas de LIS dos veces deja 39 productos. |
| RF-05 | El sistema debe asociar uno o varios códigos de barras a cada producto. | Must | F1 | COR; LIS (sin columna) | Supuesto | Un código no puede estar en dos productos. |
| RF-06 | El sistema debe descontar el stock vendido de la ubicación de la venta y reponerlo en las devoluciones. | Must | F1 | COR | Supuesto | Vender 2 uds. reduce el stock en 2; devolverlas lo restaura. |
| RF-07 | El sistema debe registrar entradas de mercancía y ajustes manuales de stock por ubicación con motivo, usuario y fecha. | Must | F1 | COR; LIS | Supuesto | Todo cambio de stock deja un movimiento consultable. |
| RF-08 | El sistema debe traspasar unidades de un producto entre nave y tienda. | Should | F1\* | PET | Supuesto | Un traspaso resta en origen y suma en destino en una sola operación. |
| RF-09 | El sistema debe avisar en la pantalla principal de los productos con stock inferior al mínimo (correo opcional, Q-13). | Should | F1\* | PET (Javier) | Explícito | Con los datos de LIS aparecen FR-0024, FR-0033 y FR-0053. |
| RF-10 | El sistema debe marcar productos como descatalogados y excluirlos de avisos y pedidos. | Should | F1\* | LIS (FR-0080) | Supuesto | FR-0080 marcado no genera aviso ni propuesta de pedido. |
| RF-11 | El sistema debe listar los productos sin ventas en los últimos seis meses. | Could | F2\* | PET | Explícito | Listado filtrable por ubicación y familia. |
| RF-12 | El sistema debe asociar una foto a cada producto y mostrarla en la ficha y en la venta. | Should | F1\* | PET | Explícito | La foto aparece al añadir el producto a una venta. |
| RF-13 | El sistema debe dar de alta un producto desde el móvil con datos mínimos (referencia, descripción, familia, foto). | Could | F2\* | PET | Explícito | Alta completa desde un navegador de móvil. |
| RF-14 | El sistema debe imprimir etiquetas de estantería con precio y código de barras. | Could | F2\* | PET | Explícito | Etiqueta con precio y código legible por la pistola. |
| RF-43 | El sistema debe importar el listado de clientes (NIF, régimen de recargo de equivalencia, descuentos por familia) desde Excel/CSV. | Should | F1\* | COR (listado RE pendiente de entrega); PET | Supuesto | Importar el listado deja cada cliente con su régimen y descuentos. |

### 5.2 Venta en mostrador

| ID | Requisito | Prior. | Fase | Fuente | Tipo | Verificación |
|---|---|---|---|---|---|---|
| RF-15 | El sistema debe añadir a la venta el producto leído con la pistola USB (modo teclado) e incrementar la cantidad si se lee de nuevo. | Must | F1 | COR; PET | Explícito | Dos lecturas del mismo código dan cantidad 2 sin tocar el ratón. |
| RF-16 | El sistema debe permitir añadir productos buscando por referencia o descripción. | Must | F1 | COR | Supuesto | Búsqueda parcial devuelve resultados al teclear. |
| RF-17 | El sistema debe permitir cobrar una venta típica con el menor número de pasos; objetivo propuesto: como máximo 3 interacciones desde el último artículo hasta el cobro (Q-15). | Must | F1 | PET | Explícito (objetivo: Supuesto) | Prueba cronometrada con el personal de mostrador. |
| RF-18 | El sistema debe ofrecer una pantalla de venta con controles de tamaño táctil. | Could | F1\* | PET | Explícito | Botones principales de al menos 44 px. |
| RF-19 | El sistema debe aplicar descuentos a una línea o al total de la venta. | Must | F1 | COR; PET | Explícito | El descuento queda reflejado en línea, ticket y factura. |
| RF-20 | El sistema debe permitir aplicar y modificar descuentos solo a Dirección y Facturación. | Must | F1 | COR; PET | Explícito | Un usuario de Mostrador recibe rechazo del servidor (no solo opción oculta). |
| RF-21 | El sistema debe gestionar clientes con datos fiscales, régimen de recargo de equivalencia (sí/no) y descuentos asociados. | Must | F1 | PET; COR | Supuesto | Alta de un cliente en RE y consulta de su régimen. |
| RF-22 | El sistema debe aplicar al seleccionar un cliente su descuento por familia (hoy 10–15 %). | Should | F1\* | PET | Explícito | Un cliente con 10 % en Química lo recibe solo en esa familia. |
| RF-23 | El sistema debe aplicar la política de venta sin stock que decida el cliente (permitir, bloquear o permitir con autorización). **Pendiente de decisión (Q-01).** | Must | F1 | COR; PET | Explícito | Según la política elegida, la venta de un producto con stock 0 se completa, se bloquea o pide autorización. |
| RF-24 | El sistema debe registrar devoluciones y abonos generando el documento rectificativo y reponiendo el stock. | Must | F1\* | PET; COR | Explícito | Una devolución parcial genera abono y suma stock. |
| RF-25 | El sistema debe crear presupuestos y convertirlos en pedido y en factura sin reintroducir datos. | Should | F2\* | PET | Explícito | La factura resultante coincide línea a línea con el presupuesto. |

### 5.3 Facturación

| ID | Requisito | Prior. | Fase | Fuente | Tipo | Verificación |
|---|---|---|---|---|---|---|
| RF-26 | El sistema debe emitir facturas conforme a los requisitos de Hacienda (Verifactu). | Must | F1 | COR; PET (obligatorio) | Explícito | Cumple la lista de comprobación Verifactu validada con el gestor (R-01). |
| RF-27 | El sistema debe calcular y desglosar el recargo de equivalencia en facturas de clientes en ese régimen. | Must | F1 | PET; COR | Explícito | Una factura de cliente en RE muestra base, IVA y recargo; el importe coincide con el cálculo manual de Lucía. |
| RF-28 | El sistema debe generar para cada venta de mostrador el documento fiscal que corresponda (p. ej. factura simplificada). Tipo pendiente (Q-07). | Must | F1 | COR; PET | Supuesto | Toda venta cobrada tiene documento fiscal numerado. |
| RF-29 | El sistema debe registrar cobros de facturas y consultar las pendientes de cobro. | Should | F1\* | COR (Lucía) | Supuesto | Una factura cobrada deja de aparecer como pendiente. |

### 5.4 Compras y proveedores

| ID | Requisito | Prior. | Fase | Fuente | Tipo | Verificación |
|---|---|---|---|---|---|---|
| RF-30 | El sistema debe gestionar proveedores y asociar cada producto a su proveedor. | Must | F1 | LIS; COR | Explícito | Los 5 proveedores de LIS existen y enlazan con sus productos. |
| RF-31 | El sistema debe proponer pedidos a proveedor al bajar del mínimo y exigir confirmación manual antes de su salida. **Pendiente de decisión (Q-02).** | Should | F1\* | PET (Javier / Marisa); COR | Supuesto | Ningún pedido sale sin aprobación de un usuario autorizado. |
| RF-32 | El sistema debe registrar manualmente un albarán de proveedor y dar entrada a su stock en la ubicación indicada. | Should | F1\* | COR; PET | Supuesto | El albarán genera movimientos de entrada (apoyado en RF-07). |
| RF-33 | El sistema debe leer los albaranes PDF recibidos por correo (15–20 a la semana) y cargarlos como borrador para validar. | Should | F2 | PET; COR | Explícito | Con PDFs reales de los 5 proveedores, el borrador acierta líneas y cantidades (umbral a acordar). |
| RF-34 | El sistema debe guardar y consultar el historial de precios de compra por proveedor y producto. | Should | F2\* | PET; LIS (FR-0043) | Explícito | Cada cambio de precio de compra queda con fecha. |

### 5.5 Usuarios, permisos, informes e integraciones

| ID | Requisito | Prior. | Fase | Fuente | Tipo | Verificación |
|---|---|---|---|---|---|---|
| RF-35 | El sistema debe exigir autenticación antes de usar cualquier función. | Must | F1 | COR | Supuesto | Toda petición sin sesión recibe rechazo. |
| RF-36 | El sistema debe aplicar cuatro perfiles: Dirección (completo), Almacén y compras, Facturación y cobros, y Mostrador (solo venta). | Must | F1 | COR; PET | Explícito | Matriz perfil-función comprobada con un usuario por perfil. |
| RF-37 | El sistema debe exportar a Excel los listados e informes (alcance en Q-14). | Should | F1\* | PET (Lucía, 3 veces) | Explícito | Stock, ventas, facturas y clientes se exportan a `.xlsx` con las columnas visibles. |
| RF-38 | El sistema debe generar el informe mensual de ventas por familia para el gestor. | Should | F2 | COR; PET | Explícito | Coincide con el informe que Lucía monta a mano. |
| RF-39 | El sistema debe permitir a Dirección ver desde el móvil el total vendido en el día. | Should | F2\* | PET (Marisa) | Explícito | El total coincide con la suma de ventas del día de ambas ubicaciones. |
| RF-40 | El sistema debe sincronizar el stock con la tienda online (Prestashop). | Could | F2 | COR; PET | Explícito | Una venta en tienda física actualiza el stock publicado. |
| RF-41 | El sistema debe ofrecer un asistente con IA en la web para que los clientes consulten stock. | Won't | Aparcado | COR; PET | Explícito | No se desarrolla; se retoma cuando F1 esté en marcha. |

*RF-42 (v1.0, "sustituir el programa de contabilidad del gestor") se retira como requisito y pasa a fuera de alcance (sección 9.1).*

## 6. Requisitos no funcionales y restricciones

| ID | Requisito | Prior. | Fuente | Tipo | Verificación |
|---|---|---|---|---|---|
| RNF-01 | El sistema debe permitir cobrar en el mostrador sin conexión a internet y sincronizar al recuperarla. Alcance y duración en Q-05. | Must | PET | Explícito | Con la red cortada se completa una venta y se sincroniza al volver. |
| RNF-02 | El sistema debe garantizar integridad, trazabilidad e inalterabilidad de los registros de facturación (Verifactu). | Must | COR; PET | Explícito | Una factura emitida no puede editarse ni borrarse. |
| RNF-03 | El sistema debe hacer copias de seguridad automáticas. Frecuencia, retención y destino pendientes (Q-03). | Must | PET | Explícito | Restauración de una copia probada con éxito. |
| RNF-04 | El sistema debe comprobar los permisos del perfil en el servidor y proteger el acceso con credenciales individuales. | Must | COR; PET | Supuesto | Llamar a la API con un perfil no autorizado devuelve rechazo. |
| RNF-06 | El sistema debe funcionar con las pistolas USB ya existentes, sin hardware adicional de lectura. | Must | COR; PET | Explícito | Lectura correcta con las pistolas reales en el mostrador. |
| RNF-07 | El sistema debe estar operativo (alcance de F1) antes del inventario anual de enero de 2027. Fecha exacta pendiente (Q-20). | Must | COR; PET | Explícito | Puesta en marcha antes de la fecha acordada. |
| RNF-08 | El sistema debe ser usable desde navegadores de móvil al menos en consulta de ventas del día y alta rápida de producto. | Should | PET | Supuesto | Pantallas legibles y operables en 360 px de ancho. |
| RNF-09 | El sistema debe registrar quién y cuándo hace ventas, descuentos, ajustes de stock, devoluciones y facturas. | Should | Criterio del analista | Supuesto | Consulta de auditoría por usuario y fecha. |
| RNF-10 | El sistema debe dimensionarse para unos 5 usuarios, 2 ubicaciones, 15–20 albaranes/semana y un catálogo de cientos de referencias. | Should | COR; PET; LIS | Supuesto | Prueba con el catálogo completo y 5 sesiones simultáneas. |
| RNF-11 | El sistema debe estar en español, con euros, IVA por defecto del 21 % y coma decimal. | Should | LIS | Supuesto | Importes y fechas con formato español en pantalla y documentos. |
| RNF-12 | La solución debe encajar en el stack .NET 10 + EF Core + SQLite + React/TypeScript; modo sin conexión, Verifactu y Prestashop requieren análisis previo. | Must | Instrucciones del repositorio | Supuesto | Informe de viabilidad antes de planificar cada uno. |
| RNF-13 | **Restricción de proceso:** presupuesto no indicado ("primero dime qué me vais a hacer"); la propuesta de alcance y plazos de F1 se entrega antes del 17/10/2026. | Must | PET; COR | Explícito | Propuesta entregada en plazo. |

*RNF-05 (v1.0, "máximo de interacciones al cobrar") se fusiona con RF-17 por ser el mismo requisito.*

## 7. Reglas de negocio y modelo de datos preliminar

### 7.1 Reglas de negocio

| ID | Regla | Fuente | Tipo |
|---|---|---|---|
| RN-01 | Todos los artículos llevan IVA del 21 %; los PVP del listado son sin IVA. | LIS | Explícito |
| RN-02 | El stock total de un producto es la suma de sus ubicaciones. | COR; PET | Explícito |
| RN-03 | Un producto está bajo mínimo si stock < stock mínimo. Falta saber si el mínimo es por ubicación o global (Q-13). | LIS; PET | Supuesto |
| RN-04 | Los clientes habituales tienen 10–15 % de descuento según la familia. | PET | Explícito |
| RN-05 | Solo Dirección y Facturación aplican o modifican descuentos. | COR; PET | Explícito |
| RN-06 | Los clientes autónomos en RE reciben factura con el recargo desglosado. Tipo aplicable (5,2 % sobre base al 21 %) a confirmar con el gestor. | PET; COR | Explícito (tipo: Supuesto) |
| RN-07 | Venta sin stock: sin regla acordada (Marisa a favor, Javier en contra). | PET; COR | Pendiente (Q-01) |
| RN-08 | Pedidos a proveedor: sin regla acordada. Propuesta del consultor en COR: propuesta automática, salida manual. | PET; COR | Pendiente (Q-02) |
| RN-09 | Un producto descatalogado no se repone ni genera avisos ni pedidos. | LIS | Supuesto |
| RN-10 | Una factura emitida no se modifica ni borra; se corrige con rectificativa. | Criterio normativo (validar con el gestor) | Supuesto |
| RN-11 | Un presupuesto aceptado se convierte en pedido y factura sin reintroducir datos. | PET | Explícito |

### 7.2 Entidades preliminares

Modelo conceptual, no es el diseño definitivo (lo hace el planificador).

| Entidad | Campos principales | Relaciones | Fase |
|---|---|---|---|
| Producto | Id, Referencia (única), Descripción, FamiliaId, ProveedorId, PrecioCompra, PvpSinIva, StockMínimo, Descatalogado, Observaciones, FotoUrl | Familia, Proveedor, CódigoBarras (1:N), StockUbicación (1:N) | F1 |
| CódigoBarras | Id, ProductoId, Código (único) | Producto | F1 |
| Familia | Id, Nombre (única) | Producto, DescuentoClienteFamilia | F1 |
| Proveedor | Id, Nombre, Contacto | Producto, PedidoProveedor, AlbaránProveedor | F1 |
| Ubicación | Id, Nombre (Nave, Tienda) | StockUbicación, MovimientoStock | F1 |
| StockUbicación | ProductoId, UbicaciónId, Cantidad | Producto, Ubicación | F1 |
| MovimientoStock | Id, ProductoId, UbicaciónId, Cantidad (±), Tipo (venta, entrada, ajuste, traspaso, devolución), Motivo, UsuarioId, Fecha | Producto, Ubicación, Usuario | F1 |
| Cliente | Id, Nombre, NIF, Dirección, RecargoEquivalencia (bool) | DescuentoClienteFamilia, Venta, Factura, Presupuesto | F1 |
| DescuentoClienteFamilia | ClienteId, FamiliaId, Porcentaje | Cliente, Familia | F1 |
| Venta / LíneaVenta | Id, Fecha, UbicaciónId, UsuarioId, ClienteId?, Subtotal, Descuento, Iva, Total, MedioPago; línea: ProductoId, Cantidad, PrecioUnitario, Descuento % | Factura (1:1) | F1 |
| Factura / Línea | Id, Serie, Número, Fecha, ClienteId?, Tipo (completa, simplificada, rectificativa), Base, Iva, Recargo, Total, FacturaOriginalId? | Cliente, RegistroFacturación, Cobro | F1 |
| RegistroFacturación | Id, FacturaId, Huella, HuellaAnterior, FechaHora, CódigoQR, EstadoEnvío | Factura | F1 (por validar) |
| Cobro | Id, FacturaId, Fecha, Importe, MedioPago | Factura | F1\* |
| PedidoProveedor / Línea | Id, ProveedorId, Estado (propuesto, aprobado, enviado, recibido), líneas | Proveedor, Producto | F1\* |
| AlbaránProveedor / Línea | Id, ProveedorId, Número, Fecha, UbicaciónId, Origen (manual, PDF), líneas con PrecioCompra | Proveedor, MovimientoStock | F1\* manual / F2 PDF |
| HistorialPrecioCompra | ProductoId, ProveedorId, Fecha, Precio | Producto, Proveedor | F2\* |
| Presupuesto | Id, ClienteId, Fecha, Estado, líneas | Cliente, Factura | F2\* |
| Usuario | Id, Nombre, Credenciales, Rol (4 perfiles) | Auditoría | F1 |

### 7.3 Calidad de los datos del listado (verificado sobre LIS)

Datos: 39 productos, 9 familias, 5 proveedores, stock valorado a precio de compra 6.862,65 € (13.400,80 € a PVP sin IVA), solo nave.

| Hallazgo | Detalle | Relacionado | Impacto |
|---|---|---|---|
| Sin códigos de barras | LIS no tiene columna de EAN; sin ellos no hay venta con pistola. | Q-10 | Alto |
| Solo stock de la nave | El Excel de la tienda no se ha entregado. | Q-09 | Alto |
| Listado posiblemente incompleto | FR-0054 menciona tallas 40–45 en referencias no exportadas. | Q-09 | Medio |
| Bajo mínimo | FR-0024 (0/20), FR-0033 (8/10), FR-0053 (3/15). FR-0024 consta como rotura con pedido del 03/10 y no lleva la anotación "por debajo del mínimo". | RF-09 | Medio |
| Mínimos sin revisar | Las notas dicen que no se revisan desde hace tiempo. | Q-13 | Medio |
| Descatalogado | FR-0080 "no reponer" (stock 2, mínimo 2: no está bajo mínimo). | RF-10 | Medio |
| Precio de compra antiguo | FR-0043 sin actualizar desde 2024. | RF-34 | Bajo |
| Unidades mezcladas | 11 de 39 productos son cajas, packs, bolsas o rollos. | Q-11 | Medio |
| Formato numérico | Decimales con coma y tildes. | RF-04 | Bajo |

### 7.4 Notas normativas externas (a validar con el gestor)

No proceden del cliente sino del conocimiento del analista; deben confirmarse antes de diseñar.

- Verifactu exige registros inalterables con huella encadenada, QR en la factura, registro de eventos y, según modalidad, envío a la AEAT.
- El productor del software debe emitir una declaración responsable de conformidad.
- Las fechas de obligatoriedad se han aplazado; para sociedades se prevé el 1 de enero de 2027, muy cerca del plazo del cliente. **Confirmar la fecha vigente.**
- El recargo de equivalencia para el 21 % de IVA es el 5,2 %.
- Las ventas de mostrador suelen documentarse como factura simplificada; los abonos, como rectificativa.

## 8. Conflictos, ambigüedades y preguntas abiertas

Se mantienen los identificadores de v1.0. 26 entradas: 25 pendientes y 1 resuelta.

| ID | Descripción | Fuentes | Pregunta para el cliente | Impacto |
|---|---|---|---|---|
| Q-01 | **CONFLICTO.** Vender sin stock: Marisa sí (el camión llega por la tarde), Javier no (líos con clientes). | PET; COR | ¿Se permite? Si sí, ¿para todos o con autorización de Marisa/Lucía? ¿Se acepta stock negativo? | Crítico (RF-23) |
| Q-02 | **CONFLICTO.** Pedidos a proveedor: Javier automáticos al bajar del mínimo; Marisa revisar siempre. | PET; COR | ¿Aceptáis propuesta automática con aprobación manual? ¿Quién aprueba y cómo se envía? | Alto (RF-31) |
| Q-03 | **HUECO.** No se cuenta qué pasó en 2023. | PET; COR | ¿Qué ocurrió (fallo de disco, borrado, ransomware, error humano)? ¿Cuánta pérdida de datos es tolerable? | Alto (RNF-03) |
| Q-04 | **HUECO.** Dónde se instala (servidor en la nave, nube, un PC por sede), equipos y puestos de cobro. | COR; PET | ¿Qué equipos y conexión hay? ¿Servidor local o nube? | Crítico |
| Q-05 | **AMBIGÜEDAD.** "Que funcione sin internet": qué y cuánto tiempo. | PET | ¿Basta con cobrar en mostrador? ¿Hasta cuántas horas? | Crítico (RNF-01) |
| Q-06 | **HUECO.** Verifactu: modalidad, certificado, series, declaración responsable. | PET; COR | ¿Qué modalidad prefiere el gestor? ¿Hay certificado digital? | Crítico |
| Q-07 | **HUECO.** Documento fiscal del mostrador. | COR; PET | ¿Todo ticket es factura simplificada? ¿Cuándo pasa a factura completa? | Alto (RF-28) |
| Q-08 | **HUECO.** Falta el listado de clientes en RE y con descuento. | COR; PET | ¿Podéis enviar clientes con NIF, régimen y descuento por familia antes del 17/10? | Alto (RF-21, 22, 27, 43) |
| Q-09 | **HUECO.** Sin Excel de la tienda; listado de la nave posiblemente parcial. | LIS; COR | ¿Podéis enviar el Excel de la tienda y el listado completo? | Alto (RF-04) |
| Q-10 | **HUECO.** Sin códigos de barras en LIS. | LIS; COR | ¿Los productos traen EAN? ¿Quién etiqueta los que no? ¿Cajas y unidades tienen códigos distintos? | Alto (RF-05, 15) |
| Q-11 | **AMBIGÜEDAD.** Cajas/packs/rollos (11 de 39). | LIS | ¿Se venden también por unidad suelta? | Medio |
| Q-12 | **AMBIGÜEDAD.** "Albaranes en PDF" (F2): ¿cargar los de proveedor o generar propios? PET apunta a cargar los de proveedor. | COR; PET | ¿Confirmáis la lectura de PDFs de los 5 proveedores? ¿Podéis enviar ejemplos? | Medio (RF-33) |
| Q-13 | **AMBIGÜEDAD.** Aviso de mínimo: ¿pantalla o correo? ¿Mínimo por ubicación o global? | PET; LIS | ¿Dónde se ve el aviso y quién revisa los mínimos antes de arrancar? | Medio (RF-09) |
| Q-14 | **AMBIGÜEDAD.** "Exportar a Excel. Todo. Siempre." | PET | ¿Qué listados y columnas son imprescindibles? | Medio (RF-37) |
| Q-15 | **AMBIGÜEDAD.** "No seis clics" sin criterio medible. | PET | ¿Cuántos pasos y segundos son aceptables para una venta típica? | Medio (RF-17) |
| Q-16 | **HUECO.** Reglas de descuento: acumulación, máximo, ¿por cliente y familia? | PET | ¿Puede haber descuentos puntuales? ¿Máximo permitido? | Medio |
| Q-17 | **HUECO.** Medios de pago, pagos aplazados, datáfono. | COR | ¿Qué medios usáis? ¿Integración con datáfono? | Medio (RF-29) |
| Q-18 | **HUECO.** Hardware: impresoras de ticket y etiquetas, cajón, táctil. | PET | ¿Qué tenéis o pensáis comprar? ¿Táctil deseable o necesario? | Medio |
| Q-19 | **HUECO.** Presupuesto no definido. | PET | ¿Hay rango de inversión? ¿Se valora F1 por separado? | Alto |
| Q-20 | **AMBIGÜEDAD.** "Antes del inventario de enero" sin fecha; F1 es amplia. | COR; PET | ¿Fecha exacta? ¿Qué mínimo imprescindible habría en esa fecha? | Alto (RNF-07) |
| Q-21 | **HUECO.** ¿Sustituye al programa del gestor? ¿Cuál usa? | PET | ¿Qué programa usa y qué datos recibe? Se asume que no se sustituye. | Bajo/Medio |
| Q-22 | **HUECO.** El plazo lo fija el inventario, pero no hay requisito de recuento. | COR | ¿Debe la aplicación ayudar al recuento y los ajustes anuales? | Medio |
| Q-23 | **RESUELTO.** Chat con IA: ahora o más adelante. | PET; COR | Resuelto por COR: aparcado. | — |
| Q-24 | **HUECO.** Migración: datos del programa antiguo y cuándo se fija el stock inicial. | COR | ¿Qué datos hay que conservar? ¿Cuándo se congela el stock inicial? | Medio |
| Q-25 | **HUECO.** Cuentas individuales y precios visibles por perfil. | COR; PET | ¿Cuenta por persona? ¿Almacén ve márgenes? ¿Mostrador ve precios de compra? | Bajo/Medio (RF-36) |
| Q-26 | **INTERNA.** El proyecto actual es otra aplicación. | `docs/analisis-diseño.md` | Equipo: ¿proyecto nuevo o dentro de este repositorio? | Alto |

## 9. Fuera de alcance y riesgos

### 9.1 Fuera de alcance

- Asistente con IA en la web (aparcado, COR).
- Sustitución del programa de contabilidad del gestor (antes RF-42; supuesto, Q-21).
- En F1: albaranes PDF, conexión con Prestashop e informe mensual para el gestor (F2 acordada).
- Gestión de compatibilidades entre productos (FR-0004 con FR-0001): no pedida.
- Aplicación móvil nativa: se pide acceso desde el móvil, no una app.
- Integración con datáfono y banca: no mencionada (Q-17).

### 9.2 Riesgos

| ID | Riesgo | Prob. | Impacto | Mitigación |
|---|---|---|---|---|
| R-01 | Verifactu es obligación legal con requisitos técnicos y fecha cercana a la de entrega. | Alta | Alto | Confirmar modalidad y fecha con el gestor; evaluar componente conforme; validación específica en el plan. |
| R-02 | El modo sin conexión con dos ubicaciones excede el stack simple. | Alta | Alto | Cerrar Q-04/Q-05 y hacer un spike de arquitectura antes de planificar la venta. |
| R-03 | Alcance de F1 (tres bloques, uno legal) frente al plazo de enero. | Alta | Alto | Entregar por orden: perfiles y catálogo → stock y venta → facturación; usar F1\* como colchón aplazable (Q-20). |
| R-04 | Decisiones pendientes (Q-01, Q-02, Q-03) bloquean venta, compras y copias. | Alta | Medio | Cerrarlas antes del 17/10. |
| R-05 | Datos de partida incompletos (sin EAN, sin Excel de la tienda, mínimos sin revisar). | Alta | Medio | Pedirlos el 21/10; carga con validación. |
| R-06 | Presupuesto sin definir. | Media | Alto | Propuesta por fases con coste por fase (Q-19). |
| R-07 | Adopción: personal nuevo y rechazo del programa antiguo. | Media | Medio | Priorizar usabilidad (RF-17, RF-12) y probar con usuarios reales. |
| R-08 | Datos personales de clientes (RGPD) y su presencia en copias. | Media | Medio | Definir retención y cifrado de copias. |
| R-09 | Hardware no descrito. | Media | Medio | Inventariar hardware (Q-18). |
| R-10 | Repositorio e instrucciones describen otra aplicación. | Alta | Medio | Decidir ubicación del producto (Q-26) y actualizar `docs/analisis-diseño.md`. |

## 10. Trazabilidad y siguientes pasos

### 10.1 Matriz requisito → fuente

● = respaldado por la fuente. Sin marca = criterio del analista (Supuesto).

| Requisitos | COR | PET | LIS |
|---|---|---|---|
| RF-01 | ● | ● | |
| RF-02, RF-08, RF-11, RF-12, RF-13, RF-14, RF-18, RF-22, RF-25, RF-37, RF-39 | | ● | |
| RF-03, RF-10 | | | ● |
| RF-04, RF-05, RF-07, RF-30 | ● | | ● |
| RF-06, RF-16, RF-29 | ● | | |
| RF-09, RF-34 | | ● | ● |
| RF-15, RF-19, RF-20, RF-21, RF-23, RF-24, RF-26, RF-27, RF-28, RF-31, RF-32, RF-33, RF-36, RF-38, RF-40, RF-41, RF-43 | ● | ● | |
| RF-17 | | ● | |
| RF-35 | ● | | |
| RNF-01, RNF-03 | | ● | |
| RNF-02, RNF-04, RNF-06, RNF-07, RNF-13 | ● | ● | |
| RNF-08 | | ● | |
| RNF-10 | ● | ● | ● |
| RNF-11 | | | ● |
| RNF-09, RNF-12 | | | |

### 10.2 Recomendación para el `planificador-apptodolist`

Agrupar en épicas planificables por separado, en este orden de dependencia:

| Épica | Alcance | Requisitos | Depende de |
|---|---|---|---|
| E0. Decisión de encaje | Proyecto nuevo o repositorio actual; spike de offline y Verifactu. | RNF-12 | Q-26, Q-04, Q-05, Q-06 |
| E1. Seguridad y perfiles | Autenticación y 4 perfiles con permisos en servidor. | RF-35, RF-36, RNF-04 | Q-25 |
| E2. Catálogo y stock | Productos, importación, códigos, stock, entradas, ajustes. | RF-01 a RF-07, RF-30 | Q-09, Q-10, Q-11 |
| E3. Venta en mostrador | Pistola, búsqueda, cobro, clientes, descuentos, venta sin stock. | RF-15 a RF-17, RF-19 a RF-21, RF-23, RNF-06 | E1, E2, Q-01, Q-15, Q-16 |
| E4. Facturación | Verifactu, recargo de equivalencia, documento de venta. | RF-26 a RF-28, RNF-02 | E3, Q-06, Q-07, Q-08 |
| E5. Continuidad | Copias automáticas y modo sin conexión. | RNF-01, RNF-03 | Q-03, Q-04, Q-05 |
| E6. Complementos de F1 (aplazables) | Traspasos, avisos, descatalogados, fotos, táctil, descuentos por familia, devoluciones y abonos, cobros, compras, exportación, importación de clientes, auditoría. | RF-08, 09, 10, 12, 18, 22, 24, 29, 31, 32, 37, 43, RNF-09 | E2–E4, Q-02, Q-13, Q-14 |
| E7. Fase 2 | Albaranes PDF, informe mensual, Prestashop, móvil, presupuestos, historial de precios, etiquetas, alta rápida, sin rotación. | RF-11, 13, 14, 25, 33, 34, 38, 39, 40 | F1 en marcha |

Nota: RF-24 (devoluciones) está en E6 por ser propuesta, pero depende de E4; si Verifactu obliga a rectificativas desde el primer día, debe subir a F1 (confirmar en Q-06).

### 10.3 Siguientes pasos

1. **Antes del viernes 17/10/2026:** el cliente responde Q-01, Q-02, Q-03 y entrega el listado de clientes en RE; el equipo envía la propuesta de alcance y plazos de F1.
2. **Martes 21/10/2026, 10:00, en la nave:** revisar la propuesta y cerrar Q-04, Q-05, Q-06, Q-07, Q-10, Q-19 y Q-20.
3. **Decisión interna (Q-26)** y actualización de `docs/analisis-diseño.md`.
4. **Spike de arquitectura** (offline y Verifactu) antes de planificar venta y facturación.
5. Pasar al planificador las épicas E0–E5 primero; E6 y E7 después.

## 11. Cambios respecto a la versión 1.0

| Cambio | Motivo |
|---|---|
| Fases separadas en F1 (acordada) y F1\* (propuesta aplazable); idem F2 / F2\*. | Distinguir lo que el cliente acordó de lo que propone el analista y dar un colchón de calendario (R-03). |
| Columna **Verificación** en cada requisito. | Hacerlos comprobables (criterio de calidad). |
| RF-32 (albarán manual) baja de Must a Should. | La entrada de stock ya la cubre RF-07; los albaranes están en F2 según COR. |
| Nuevo RF-43 (importar listado de clientes). | COR pide el listado de clientes en RE y no había requisito que lo cargara. |
| RF-42 retirado y movido a fuera de alcance. | Era una pregunta, no una necesidad; Q-21 asume que no se sustituye. |
| RNF-05 fusionado con RF-17. | Duplicado. |
| RNF-13 marcado como restricción de proceso. | No es un requisito del sistema. |
| Regla de precedencia COR > PET y matriz de trazabilidad simplificada por fuente. | Resolver discrepancias de forma explícita; sin referencias a historias de usuario que cambien con la numeración. |
| Se añade valor del stock a PVP (13.400,80 €) y la lista de épicas incorpora E0 (encaje y spike). | Datos verificados sobre LIS y riesgos R-01/R-02/Q-26. |

Totales: **RF** 21 Must, 15 Should, 5 Could, 1 Won't = 42; **RNF** 8 Must, 4 Should = 12; **reglas** 11; **preguntas** 26 (25 abiertas, 1 resuelta).
