# functions-dotnet-liquidtransform

An Azure Function (v1) executing Liquid transforms using DotLiquid. In summary the following transformation types are supported:
- JSON to JSON
- JSON to XML
- JSON to plain text
- XML to JSON
- XML to XML
- XML to plain text

The actual Liquid transforms should be stored in an Azure storage account. The Liquid transform uses the HTTP request body as input.

## Getting Started

### Prerequisites
- Visual Studio
- Azure subscription

### Usage

Post a JSON or XML payload to the URL where your function app is hosted, e.g.:

```http
POST /api/liquidtransformer/xmlsample.liquid HTTP/1.1
Host: localhost:7071
Content-Type: application/json
Accept: application/xml
{
	"name": "olaf"
}

```

Note how the name of the Liquid transform as stored in the storage account is part of the URL path. This allows for a function binding to the blob.

The Liquid transformation should be stored in the Azure storage account associated with the Azure function (used for AzureWebJobsStorage) in a blob container named liquid-transforms.

The transformation input type has to be specified with the HTTP Content-Type header. This can either be application/json or application/xml.

The transformation output type has to be specified with the HTTP Accept header. Examples could be application/json, application/XML or text/CSV.

#### JSON transformation
Let's take the following JSON input:
```json
{
	"name": "olaf"
}
```
With the following Liquid transform:
```
{
    "fullName": "{{content.name | upcase }}"
}
```
This will produce a JSON payload with the following output:
```json
{
	"fullName": "OLAF"
}
```

And with the following Liquid transform:
```xml
<fullName>{{content.name}}</fullName>
```

This will result in the the following XML output:
```xml
<fullName>olaf</fullName>
```

#### XML transformation
Let's take the following XML input:

```xml
<root>
    <name>olaf</name>
    <cities>
        <city>sydney</city>
        <city>amsterdam</city>
        <test>test</test>
    </cities>
</root>
```
With the following Liquid transformation:
```
{
    "fullName": "{{content.root.name}}",
    "cities": [
    {%- for item in content.root.cities.city -%}
        {
            "city": "{{ item }}"
        },
    {%- endfor -%}
    ],
    "test": "{{content.root.cities.test}}"
}
```
This will result in the following JSON output:
```json
{
    "fullName": "olaf",
    "cities": [
        {
            "city": "sydney"
        },
        {
            "city": "amsterdam"
        }
    ],
    "test": "test"
}
```

## Deployment

Open the Visual Studio solution file and deploy the included project to an Azure subscription. The Azure function (V1) can be hosted in a consumption tier plan. 

Create a container called liquid-transforms in the storage account that's associated with your Azure function. This allows the liquid transforms to be passed in as a blob with function bindings.

### Swagger definition
Azure Functions V1 support exposing API definitions, allowing for easy consumption of your API. The Swagger file [liquidtransformfunctionapp.swagger](liquidtransformfunctionapp.swagger) can be used for this purpose.

Refer to [Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-functions/functions-openapi-definition) on how to enable Swagger for Azure Functions.

Expose the Swagger definition if you want to consume the API in Logic Apps.  More info on how to consume Azure Functions in Logic Apps using Swagger can be found [here](https://docs.microsoft.com/en-us/azure/logic-apps/logic-apps-azure-functions). A sample Logic Apps action would look like this:
```json
"apiliquidtransformerliquidtransformfilenamepost": {
	"inputs": {
		"body": {
			"name": "olaf"
		},
		"functionApp": {
			"id": "/subscriptions/ab6c9011-d1be-48c1-a58b-87c12cbb3434/resourceGroups/rg-liquidtransform/providers/Microsoft.Web/sites/LiquidTransformfunctionappv12038622085504"
		},
		"headers": {
			"Content-Type": "application/json",
			"Accept": "application/json"
		},
		"method": "post",
		"uri": "https://liquidTransformfunctionappv12038622085504.azurewebsites.net/api/liquidtransformer/@{encodeURIComponent('sample.liquid')}"
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
