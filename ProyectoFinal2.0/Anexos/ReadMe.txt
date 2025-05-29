Para que el proyecto corra satisfactoriamente, debe tener en cuenta los siguientes aspectos:

1) El proyecto se desarrolla en un entorno local a partir de MySQL workbench, por lo tanto se recomienda hacer la conexión teniendo en cuenta estos parámetros: 

Connection Method: Standard (TCP/IP)
Hostname: localhost
Port: 3306
Username: root

2) Puede que genere errores al momento de abrirlo por un fallo en los .resx (recursos), este fallo es común al iniciar por primera vez el proyecto, para solucionarlo
 debe dirigirse a esta ruta "Proyecto_ElFercho\ProyectoFinal2.0\ProyectoFinal\Plantillas\PlantillaDiseñoProyectoFinal_JS",  y ubicar los .resx que le produzcan el error (generalmente el 4 y 8)
 acto seguido, deberá darle click derecho, propiedades, en la parte inferior derecha aparecerá una casilla que dice "desbloquear", marcarla, aplicar y aceptar para liberar el recurso.

3) En esta carpeta de anexos encontrara el código SQL de los productos, solo deberá abrirlo con workbench (cuando la conexión/puente ya se haya realizado) y ejecutarlo desde workbeanch para que cargue
el catalogo. 