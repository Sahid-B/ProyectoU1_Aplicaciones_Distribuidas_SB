# Market Rubik en Kubernetes local

Ejecuta estos pasos desde la carpeta que contiene docker-compose.yml, con el motor Linux de Docker Desktop y su clúster Kubernetes activos. El script utiliza el contexto docker-desktop y el espacio market-rubik. El clúster debe poder utilizar las imágenes locales de Docker Desktop y su clase de almacenamiento por defecto.

En PowerShell define las claves de inicialización antes del primer despliegue. Usa contraseñas diferentes con mayúsculas, minúsculas, números y un símbolo.

```powershell
$env:MARKET_ADMIN_PASSWORD = Read-Host 'Clave inicial de Admin' -MaskInput
$env:MARKET_DEMO_PASSWORD = Read-Host 'Clave de las dos cuentas de demostración' -MaskInput
.\k8s\deploy.ps1
kubectl -n market-rubik port-forward service/web 8080:8080
```

Abre http://localhost:8080. La cuenta administrativa es admin@marketrubik.com. Si configuraste la clave de demostración, hay dos clientes confirmados, maria@market.test y jose@market.test. Las cuentas se crean durante la inicialización. Cambiar la variable posteriormente no cambia una contraseña existente. El catálogo de un entorno nuevo se puede llenar desde la administración de categorías y productos. La carga masiva destructiva está desactivada por defecto.

La web tiene tres réplicas. Un Job prepara el esquema antes de que se inicien. PostgreSQL y Redis usan volúmenes persistentes. Redis distribuye el chat y la señalización de llamadas y comparte sesiones y claves de autenticación. Los cambios confirmados en PostgreSQL notifican a cada réplica sobre stock, ventas y pedidos. El servicio de IA tiene su propio Deployment y Service. Configura los servicios externos de pagos, correo e IA según el entorno.

Para probar entre réplicas, lista los Pods con `kubectl -n market-rubik get pods -l app=web`. En dos terminales ejecuta `kubectl -n market-rubik port-forward pod/NOMBRE_DEL_PRIMER_POD 8081:8080` y el equivalente para un Pod distinto en 8082. Abre cada URL en un perfil de navegador diferente, inicia sesión con una cuenta distinta y abre **Mensajes** en ambos. Busca al otro cliente por su correo, envía un mensaje con producto y comprueba el precio en ambos historiales. Inicia una llamada y acepta en la segunda ventana. Para una demostración local se necesitan permisos de cámara y micrófono. Un administrador puede abrir el inicio en un tercer perfil y observar las ventas y las alertas de stock de cinco unidades o menos.

Para cámaras en dispositivos remotos utiliza HTTPS y un acceso web que admita WebSocket. Configura servidores ICE propios con Rtc__Stun, Rtc__Turn, Rtc__Username y Rtc__Password como variables del despliegue web cuando las redes requieran STUN/TURN. La aplicación lee esas opciones desde la configuración del servidor. Audio y video circulan por WebRTC.

Para revisar un despliegue:

```powershell
kubectl -n market-rubik get deployments,pods,services,pvc
kubectl -n market-rubik logs job/market-init
kubectl -n market-rubik logs deployment/web --tail=50
```
