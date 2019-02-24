# functions-dotnet-liquidtransform

An Azure Function (v1) executing Liquid transforms using DotLiquid. Currently only transformations from JSON are supported. Support for XML transformations will be added in a future release. In the meantime XML is only supported as output format. In summary the following transformation types are supported:
- JSON to JSON
- JSON to XML
- JSON to plain text


The actual Liquid transforms should be stored in an Azure storage account. The Liquid transform uses the HTTP request body as input.

## Getting Started

### Prerequisites
- Visual Studio
- Azure subscription

### Usage

Post a JSON payload to the URL where your function app is hosted, e.g.:

```http
https://liquidtransformfunctionappv1291962835504.azurewebsites.net/api/jsontransformer/sample.liquid?code=1tQ/TjrC0Fu7F/Ca585ArMV45mO53x/gaBG2aPbuNiID7wODytSVoB==
```

Note how the name of the Liquid transform as stored in the storage account is part of the URL path. This allows for a function binding to the blob.

The sample.liquid transformation should be stored in the associated Azure storage account (used for AzureWebJobsStorage) in the blob container liquid-transforms.

Let's take the following JSON input:
```json
{
	"name": "olaf"
}
```
With the following Liquid transform:
```
{
    "fullName": "{{content.name}}"
}
```
This will produce a JSON payload with the following output:
```json
{
	"fullName": "olaf"
}
```

And with the following Liquid transform:
```xml
<fullName>{{content.name}}</fullName>
```

This will result in the the following output:
```xml
<fullName>olaf</fullName>
```

## Deployment

Open the Visual Studio solution file and deploy the included project to an Azure subscription. The Azure function (V1) can be hosted in a consumption tier plan. 

Create a container called liquid-transforms in the storage account that's associated with your Azure function. This allows the liquid transforms to be passed in as a blob with function bindings.

### Swagger definition
Azure Functions V1 support exposing API definitions, allowing for easy consumption of your API. The Swagger file [liquidtransformfunctionapp.swagger](liquidtransformfunctionapp.swagger) can be used for this purpose.

Refer to [Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-functions/functions-openapi-definition) on how to enable Swagger for Azure Functions.

Expose the Swagger definition if you want to consume the API in Logic Apps.  More info on how to consume Azure Functions in Logic Apps using Swagger can be found [here](https://docs.microsoft.com/en-us/azure/logic-apps/logic-apps-azure-functions). A sample Logic Apps action would look like this:
```json
"apijsontransformerliquidtransformfilenamepost": {
	"inputs": {
		"body": {
			"name": "olaf"
		},
		"functionApp": {
			"id": "/subscriptions/ab6c9011-d1be-48c1-a58b-87c12cbb3434/resourceGroups/rg-liquidtransform/providers/Microsoft.Web/sites/LiquidTransformfunctionappv12038622085504"
		},
		"headers": {
			"Accept": "application/json"
		},
		"method": "post",
		"uri": "https://liquidTransformfunctionappv12038622085504.azurewebsites.net/api/jsontransformer/@{encodeURIComponent('sample.liquid')}"
	},
	"runAfter": {},
	"type": "Function"
}

```

## Built With

* [DotLiquid](http://dotliquidmarkup.org) - The Liquid transformation engine for .NET
* [Visual Studio](https://visualstudio.microsoft.com/) - IDE


## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
