<p align="center">
  <img src="https://github.com/izipay-pe/Imagenes/blob/main/logos_izipay/logo-izipay-banner-1140x100.png?raw=true" alt="Formulario" width=100%/>
</p>

# Popin-PaymentForm-.NET

## Índice

➡️ [1. Introducción](https://github.com/izipay-pe/Popin-PaymentForm-.NET/tree/main?tab=readme-ov-file#%EF%B8%8F-1-introducci%C3%B3n)  
🔑 [2. Requisitos previos](https://github.com/izipay-pe/Popin-PaymentForm-.NET/tree/main?tab=readme-ov-file#-2-requisitos-previos)  
🚀 [3. Ejecutar ejemplo](https://github.com/izipay-pe/Popin-PaymentForm-.NET/tree/main?tab=readme-ov-file#-3-ejecutar-ejemplo)  
🔗 [4. Pasos de integración](https://github.com/izipay-pe/Popin-PaymentForm-.NET/tree/main?tab=readme-ov-file#4-pasos-de-integraci%C3%B3n)  
💻 [4.1. Desplegar pasarela](https://github.com/izipay-pe/Popin-PaymentForm-.NET/tree/main?tab=readme-ov-file#41-desplegar-pasarela)  
💳 [4.2. Analizar resultado de pago](https://github.com/izipay-pe/Popin-PaymentForm-.NET/tree/main?tab=readme-ov-file#42-analizar-resultado-del-pago)  
📡 [4.3. Pase a producción](https://github.com/izipay-pe/Popin-PaymentForm-.NET/tree/main?tab=readme-ov-file#43pase-a-producci%C3%B3n)  
🎨 [5. Personalización](https://github.com/izipay-pe/Popin-PaymentForm-.NET/tree/main?tab=readme-ov-file#-5-personalizaci%C3%B3n)  
📚 [6. Consideraciones](https://github.com/izipay-pe/Popin-PaymentForm-.NET/tree/main?tab=readme-ov-file#-6-consideraciones)

## ➡️ 1. Introducción

En este manual podrás encontrar una guía paso a paso para configurar un proyecto de **[.NET]** con la pasarela de pagos de IZIPAY. Te proporcionaremos instrucciones detalladas y credenciales de prueba para la instalación y configuración del proyecto, permitiéndote trabajar y experimentar de manera segura en tu propio entorno local.
Este manual está diseñado para ayudarte a comprender el flujo de la integración de la pasarela para ayudarte a aprovechar al máximo tu proyecto y facilitar tu experiencia de desarrollo.

> [!IMPORTANT]
> En la última actualización se agregaron los campos: **nombre del tarjetahabiente** y **correo electrónico** (Este último campo se visualizará solo si el dato no se envía en la creación del formtoken).

<p align="center">
  <img src="https://github.com/izipay-pe/Imagenes/blob/main/formulario_popin/Imagen-Formulario-Popin.png?raw=true" alt="Formulario" width="350"/>
</p>

## 🔑 2. Requisitos Previos

- Comprender el flujo de comunicación de la pasarela. [Información Aquí](https://secure.micuentaweb.pe/doc/es-PE/rest/V4.0/javascript/guide/start.html)
- Extraer credenciales del Back Office Vendedor. [Guía Aquí](https://github.com/izipay-pe/obtener-credenciales-de-conexion)
- Para este proyecto utilizamos .NET 8.0.
- Visual Studio

> [!NOTE]
> Tener en cuenta que, para que el desarrollo de tu proyecto, eres libre de emplear tus herramientas preferidas.

## 🚀 3. Ejecutar ejemplo

### Instalar Visual Studio

Visual Studio IDE compatible con .NET.

1. Dirigirse a la página web de [Microsoft](https://visualstudio.microsoft.com/es/).
2. Descargarlo e instalarlo.

### Clonar el proyecto
```sh
git clone https://github.com/izipay-pe/Embedded-PaymentForm-.NET
``` 

### Datos de conexión 

Reemplace **[CHANGE_ME]** con sus credenciales de `API REST` extraídas desde el Back Office Vendedor, revisar [Requisitos previos](https://github.com/izipay-pe/Embedded-PaymentForm-.NET/tree/main?tab=readme-ov-file#-2-requisitos-previos).

- Editar el archivo `appsettings.json` en la ruta raíz:
```json
"ApiCredentials": {
        "USERNAME": "CHANGE_ME_USER_ID",
        "PASSWORD": "CHANGE_ME_PASSWORD",
        "PUBLIC_KEY": "CHANGE_ME_PUBLIC_KEY",
        "HMACSHA256": "CHANGE_ME_HMAC_SHA_256"
    }
```

### Ejecutar proyecto

1. Una vez dentro del código ejecutamos el proyecto con el comando F5, se abrirá en tu navegador predeterminado y accedera en la siguiente ruta:

```sh
https://localhost:7095/
``` 

<p align="center">
  <img src="https://i.postimg.cc/Bb11T1J7/ejecutarproyecto.jpg" alt="Formulario" width="350"/>
</p>


## 🔗4. Pasos de integración

<p align="center">
  <img src="https://i.postimg.cc/pT6SRjxZ/3-pasos.png" alt="Formulario" />
</p>

## 💻4.1. Desplegar pasarela
### Autentificación
Extraer las claves de `usuario` y `contraseña` del Backoffice Vendedor, concatenar `usuario:contraseña` y agregarlo en la solicitud del encabezado `Authorization`. Podrás encontrarlo en el archivo `Controllers/McwController.cs`.
```c#
string username = apiCredentials["USERNAME"];
string password = apiCredentials["PASSWORD"];
...
...
string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
```
ℹ️ Para más información: [Autentificación](https://secure.micuentaweb.pe/doc/es-PE/rest/V4.0/javascript/guide/embedded/keys.html)
### Crear formtoken
Para configurar la pasarela se necesita generar un formtoken. Se realizará una solicitud API REST a la api de creación de pagos:  `https://api.micuentaweb.pe/api-payment/V4/Charge/CreatePayment` con los datos de la compra para generar el formtoken. Podrás encontrarlo en el archivo `Controllers/McwController.cs`.

```c#
string url = "https://api.micuentaweb.pe/api-payment/V4/Charge/CreatePayment";

var data = new
  {
    currency = paymentRequest.Currency,
    ...
    ...
    orderId = paymentRequest.OrderId
  };

// Crear la conexión a la API para la creación del FormToken
using (HttpClient client = new HttpClient())
  {
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    var response = await client.PostAsync(url, new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json"));
    ...
    ...
    // Extrae el FormToken y PublicKey
    if (responseData.Status == "SUCCESS")
    {
      ViewData["FormToken"] = responseData?.Answer?.FormToken;
      ViewData["PublicKey"] = publicKey;
      // Renderiza el Template
      return View("Checkout");
    }
```
ℹ️ Para más información: [Formtoken](https://secure.micuentaweb.pe/doc/es-PE/rest/V4.0/javascript/guide/embedded/formToken.html)
### Visualizar formulario
Para desplegar la pasarela, configura la llave `public key` en el encabezado (Header) del archivo `checkout.html`. Esta llave debe ser extraída desde el Back Office del Vendedor.

Header: 
Se coloca el script de la libreria necesaria para importar las funciones y clases principales de la pasarela. Podrás encontrarlo en el archivo `Views/Mcw/Checkout.cshtml`.
```javascript
@section extrahead {
    <script type="text/javascript"
            src="https://static.micuentaweb.pe/static/js/krypton-client/V4.0/stable/kr-payment-form.min.js"
            kr-public-key="@ViewData["PublicKey"]"
            kr-post-url-success="result">
    </script>
    <link rel="stylesheet" href="https://static.micuentaweb.pe/static/js/krypton-client/V4.0/ext/classic.css">
    <script type="text/javascript" src="https://static.micuentaweb.pe/static/js/krypton-client/V4.0/ext/classic.js"></script>
}
```
Además, se inserta en el body una etiqueta div con la clase `kr-embedded` que deberá tener el atributo `kr-form-token` e incrustarle el `formtoken` generado en la etapa anterior.

Body:
```javascript
<div id="micuentawebstd_rest_wrapper">
        <div class="kr-embedded" kr-popin kr-form-token="@ViewData["FormToken"]"></div>
</div>
```
ℹ️ Para más información: [Visualizar formulario](https://secure.micuentaweb.pe/doc/es-PE/rest/V4.0/javascript/guide/embedded/formToken.html)

## 💳4.2. Analizar resultado del pago

### Validación de firma
Se configura la función `checkHash` que realizará la validación de los datos del parámetro `kr-answer` utilizando una clave de encriptación definida por el parámetro `kr-hash-key`. Podrás encontrarlo en el archivo `Controllers/McwController.cs`.

```c#
private bool CheckHash(IFormCollection form, string key)
        {
            var answer = form["kr-answer"].ToString();
            var hash = form["kr-hash"].ToString();

            // Genera un hash HMAC-SHA256
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                byte[] answerBytes = Encoding.UTF8.GetBytes(answer);
                byte[] computedHashBytes = hmac.ComputeHash(answerBytes);
                string computedHashString = Convert.ToHexString(computedHashBytes).ToLowerInvariant();
                
                // Retorna la integridad
                return string.Equals(computedHashString, hash, StringComparison.OrdinalIgnoreCase);
            }
}
```

Se valida que la firma recibida es correcta. Podrás encontrarlo en el archivo `Controllers/McwController.cs`.

```c#
        // @@ Manejo de solicitudes POST para result @@
        [HttpPost("/result")]
        public IActionResult Result([FromForm] IFormCollection form)
        {
            string hmacKey = _configuration.GetSection("ApiCredentials")["HMACSHA256"];

            // Válida que la respuesta sea íntegra comprando el hash recibido en el 'kr-hash' con el generado con el 'kr-answer'
            if (!CheckHash(form, hmacKey))
            {
                Console.WriteLine("Invalid signature");
                return View("Error");
            }
            ...
            ...
        }
```
En caso que la validación sea exitosa, se puede mostrara los datos de `kr-answer` a través de un JSON y mostrar los datos del pago realizado. Podrás encontrarlo en el archivo `Controllers/McwController.cs`.

```c#
        // @@ Manejo de solicitudes POST para result @@
        [HttpPost("/result")]
        public IActionResult Result([FromForm] IFormCollection form)
        {
            ...
            ...
            var formData = new
            {
                ...
                ...
                KrHashKey = form["kr-hash-key"].ToString()
            };

            // Renderiza el Template
            return View("Result", formData);
        }
```
ℹ️ Para más información: [Analizar resultado del pago](https://secure.micuentaweb.pe/doc/es-PE/rest/V4.0/kb/payment_done.html)

### IPN
La IPN es una notificación de servidor a servidor (servidor de Izipay hacia el servidor del comercio) que facilita información en tiempo real y de manera automática cuando se produce un evento, por ejemplo, al registrar una transacción.


Se realiza la verificación de la firma utilizando la función `checkhash` y se devuelve al servidor de izipay un mensaje confirmando el estado del pago. Podrás encontrarlo en el archivo `Controllers/McwController.cs`.

```c#
 [HttpPost("/ipn")]
        public IActionResult Ipn([FromForm] IFormCollection form)
        {
            string privateKey = _configuration.GetSection("ApiCredentials")["PASSWORD"];

            // Válida que la respuesta sea íntegra comprando el hash recibido en el 'kr-hash' con el generado con el 'kr-answer'
            if (!CheckHash(form, privateKey))
            {
                Console.WriteLine("Invalid signature");
                return View("Error");
            }
            ...
            ...
            // Extrae datos de la transacción
            string orderStatus = root.GetProperty("orderStatus").GetString();

            // Retorna el valor de OrderStatus
            return Ok($"OK! Order Status: {orderStatus}");
            
        }
```

La IPN debe ir configurada en el Backoffice Vendedor, en `Configuración -> Reglas de notificación -> URL de notificación al final del pago`

<p align="center">
  <img src="https://i.postimg.cc/zfx5JbQP/ipn.png" alt="Formulario" width=80%/>
</p>

ℹ️ Para más información: [Analizar IPN](https://secure.micuentaweb.pe/doc/es-PE/rest/V4.0/api/kb/ipn_usage.html)

### Transacción de prueba

Antes de poner en marcha su pasarela de pago en un entorno de producción, es esencial realizar pruebas para garantizar su correcto funcionamiento.

Puede intentar realizar una transacción utilizando una tarjeta de prueba con la barra de herramientas de depuración (en la parte inferior de la página).

<p align="center">
  <img src="https://i.postimg.cc/3xXChGp2/tarjetas-prueba.png" alt="Formulario"/>
</p>

- También puede encontrar tarjetas de prueba en el siguiente enlace. [Tarjetas de prueba](https://secure.micuentaweb.pe/doc/es-PE/rest/V4.0/api/kb/test_cards.html)

## 📡4.3.Pase a producción

Reemplace **[CHANGE_ME]** con sus credenciales de PRODUCCIÓN de `API REST` extraídas desde el Back Office Vendedor, revisar [Requisitos Previos](https://github.com/izipay-pe/Popin-PaymentForm-.NET/tree/main?tab=readme-ov-file#-2-requisitos-previos).

- Editar el archivo `appsettings.json` en la ruta raíz:
```json
"ApiCredentials": {
        "USERNAME": "CHANGE_ME_USER_ID",
        "PASSWORD": "CHANGE_ME_PASSWORD",
        "PUBLIC_KEY": "CHANGE_ME_PUBLIC_KEY",
        "HMACSHA256": "CHANGE_ME_HMAC_SHA_256"
    }
```

## 🎨 5. Personalización

Si deseas aplicar cambios específicos en la apariencia de la pasarela de pago, puedes lograrlo mediante la modificación de código CSS. En este enlace [Código CSS - Popin](https://github.com/izipay-pe/Personalizacion/blob/main/Formulario%20Popin/Style-Personalization-PopIn.css) podrá encontrar nuestro script para un formulario incrustado.

<p align="center">
  <img src="https://github.com/izipay-pe/Imagenes/blob/main/formulario_popin/Imagen-Formulario-Custom-Popin.png" alt="Formulario"/>
</p>

## 📚 6. Consideraciones

Para obtener más información, echa un vistazo a:

- [Formulario incrustado: prueba rápida](https://secure.micuentaweb.pe/doc/es-PE/rest/V4.0/javascript/quick_start_js.html)
- [Primeros pasos: pago simple](https://secure.micuentaweb.pe/doc/es-PE/rest/V4.0/javascript/guide/start.html)
- [Servicios web - referencia de la API REST](https://secure.micuentaweb.pe/doc/es-PE/rest/V4.0/api/reference.html)
