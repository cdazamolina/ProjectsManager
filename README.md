## PROJECTS MANAGER - SLABCODE

### EJECUCIÓN: 
  1. Clona el projecto.
  2. En el archivo appsetting.json está especificada la cadena de conexión al servidor de base de datos SQL Server.
    - Asignar la cadena de conexión según la instancia de bases de datos a utilizar.
  4. Desde la herramienta de PowerShield de desarrollo ejecutar las migraciones de base de datos.
    - *dotnet ef database update* [Esto creará la base de datos y insertará data inicial (Roles y usuario administrador por defecto)].
    - Al momento de realizar la migración, se crea un usuario por defecto con rol Administrador, para comenzar a interactuar con la aplicación.
      - **Username:** Administrator
      - **Password:** Administrator

### DESCRIPCIÓN
Aplicación web para que los usuarios puedan crear proyectos, donde cada proyecto tiene un listado de tareas asociadas. Los roles de usuario del sistema son los siguientes:

  - **Administrador:** Puede deshabilitar a otros usuarios para evitar que inicien sesión en la aplicación. Además tiene permisos de consulta a toda la información en la plataforma. Además puede borrar tanto tareas o proyectos de cualquier usuario.
  - **Operador:** Tiene la potestad de crear proyectos y de crear tareas vinculadas a cada proyecto. Además puede cambiar el estado de una tarea de Pendiente a Realizada, y de
cambiar el Estado de un proyecto de En Proceso a Finalizado.

Este proyecto tiene el siguiente conjunto de funcionalidades:
  -**Autenticación de usuarios:** Los usuarios se autentican con un nombre de usuario y contraseña, y el backend retorna un token JTW.
  -**Creación de usuario:** El rol administrador podrá crear usuarios operadores, con una contraseña, esto debe disparar un correo al usuario indicando las credenciales de
autenticación.
  - **Cambiar la contraseña:** Endpoint para cambiar la contraseña del usuario, necesita de la contraseña actual para realizar el cambio de contraseña.
  - **Crear proyecto:** Los operadores consumen un endpoint para crear un proyecto con la siguiente información:
    1. Nombre del proyecto (campo obligatorio)
    2. Descripción (campo obligatorio)
    3. Fecha de inicio (debe ser posterior o igual al día presente, campo obligatorio)
    4. Fecha de finalización (debe ser posterior al día inicial)
  -**Editar proyecto:** Los operadores consumen un endpoint para modificar alguno de los siguientes campos de un proyecto:
    1. Nombre del proyecto
    2. Descripción
    3. Fecha de finalización (no se puede modificar si existen tareas con fechas de ejecución posteriores a la nueva fecha de finalización).
  - **Borrar proyecto:** Los operadores consumen un endpoint para borrar un proyecto y todas sus tareas asociadas.
  - **Completar proyecto:** Los operadores consumen un endpoint para completar un proyecto, con lo cual su estado cambia de En Proceso a Finalizado. La petición solo debe ser exitosa si todas las tareas del proyecto ya están en estado Realizada, esto dispara un correo a los administradores indicando la finalización del proyecto.
  - **Crear tarea:** Los operadores consumen un endpoint para crear una tarea asociada a un proyecto con la siguiente información:
    1. Nombre de la tarea (campo obligatorio)
    2. Descripción (campo opcional)
    3. Fecha de ejecución (debe estar en el rango de fechas de inicio y final del proyecto)
  - **Editar tarea:** Los operadores consumen un endpoint para modificar alguno de los siguientes campos de un proyecto:
    1. Nombre de la tarea
    2. Descripción
    3. Fecha de ejecución (debe estar en el rango de fechas de inicio y final del proyecto)
  - **Borrar tarea:** Los operadores consumen un endpoint para borrar una tarea.
  - **Completar tarea:** Los operadores consumen un endpoint para completar una tarea, con lo cual su estado cambia de En Proceso a Finalizado.
  - **Habilitar/Deshabilitar operadores:** Los administradores consumen un endpoint para habilitar o deshabilitar operadores del sistema. Un operador deshabilitado no debe poder autenticarse

### CONTACTO: 
  - Carlos Eduardo Daza Molina.
  - Tel: +57 30591112227
  - Codigo fuente: https://github.com/cdazamolina/ProjectsManager
  
