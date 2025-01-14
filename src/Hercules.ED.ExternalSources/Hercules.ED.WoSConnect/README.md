![](../../../Docs/media/CabeceraDocumentosMD.png)

| Fecha         | 21/10/2022                                                  |
| ------------- | ------------------------------------------------------------ |
|Titulo|API Hercules.ED.WoSConnect| 
|Descripción|Manual del servicio API Hercules.ED.WoSConnect para la importación de recursos desde WoS|
|Versión|1.0|
|Módulo|Hercules.ED.ExternalSources|
|Tipo|Manual|
|Cambios de la Versión|Versión inicial |

## Sobre Hercules.ED.WoSConnect

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.WoSConnect)

[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.WoSConnect&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.WoSConnect)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.WoSConnect&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.WoSConnect)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.WoSConnect&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.WoSConnect)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.WoSConnect&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.WoSConnect)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.WoSConnect&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.WoSConnect)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Hercules.ED.WoSConnect&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Hercules.ED.WoSConnect)

## Descripción.
Servicio encargado de obtener la información de WoS. Documentación del API de [WoS](https://api.clarivate.com/swagger-ui/?url=https%3A%2F%2Fdeveloper.clarivate.com%2Fapis%2Fwos%2Fswagger). 

## Controladores

**APIController**  

[GET] GetROs -> Obtiene los datos de las publicaciones de un autor.  
orcid: Código ORCID del autor a buscar las publicaciones.  
date: Fecha a partir de la búsqueda de publicaciones.  
Resultado: Lista de objetos [Publication](https://github.com/HerculesCRUE/HerculesED/blob/main/src/Hercules.ED.ExternalSources/Hercules.ED.WoSConnect/ROs/WoS/Models/ROPublicationModel.cs).  

[GET] GetRoByWosId -> Obtiene los datos de una publicación mediante el ID de WoS.  
pIdWos: Código WoS de la publicación.  
Resultado: Objeto [Publication](https://github.com/HerculesCRUE/HerculesED/blob/main/src/Hercules.ED.ExternalSources/Hercules.ED.WoSConnect/ROs/WoS/Models/ROPublicationModel.cs).  

[GET] GetRoByDoi -> Obtiene los datos de una publicación.  
pDoi: Código DOI de la publicación.  
Resultado: Objeto [Publication](https://github.com/HerculesCRUE/HerculesED/blob/main/src/Hercules.ED.ExternalSources/Hercules.ED.WoSConnect/ROs/WoS/Models/ROPublicationModel.cs).  

[GET] GetCitesByWosId -> Obtiene las citas mediante el ID de WoS.  
pWosId: Código WoS de la publicación.  
Resultado: Lista de objetos [Publication](https://github.com/HerculesCRUE/HerculesED/blob/main/src/Hercules.ED.ExternalSources/Hercules.ED.WoSConnect/ROs/WoS/Models/ROPublicationModel.cs).    

## Configuración en el appsetting.json
```json{
{
	"Logging": {
		"LogLevel": {
			"Default": "",
			"Microsoft": "",
			"Microsoft.Hosting.Lifetime": ""
		}
	},
	"AllowedHosts": "*",
	"LogPath": ""
}
```

- LogLevel.Default: Nivel de error por defecto.
- LogLevel.Microsoft: Nivel de error para los errores propios de Microsoft.
- LogLevel.Microsoft.Hosting.Lifetime: Nivel de error para los errores de host.
- LogPath: Ruta de guardado del fichero de logs.

## Dependencias
- **ClosedXML**: v0.95.4
- **ExcelDataReader**: v3.6.0
- **ExcelDataReader.DataSet**: v3.6.0
- **GnossApiWrapper.NetCore**: v6.0.2
- **Newtonsoft.Json**: v13.0.1
- **Serilog.AspNetCore**: v4.1.0
- **Swashbuckle.AspNetCore**: v6.2.1
- **System.Net.Http.Json**: v5.0.0
