# functions-dotnet-liquidtransform

An Azure Function (v1) executing Liquid transforms using DotLiquid. The following transformation types are supported:
- JSON to JSON
- JSON to XML
- JSON to plain text / CSV
- XML to JSON
- XML to XML
- XML to plain text / CSV
- CSV to JSON
- CSV to XML

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

#### Input type
The transformation input type has to be specified with the HTTP **Content-Type** header. This can either be application/json or application/xml.

#### Output type
The transformation output type has to be specified with the HTTP **Accept** header. Examples could be application/json, application/XML or text/CSV.

### Examples
#### JSON transformation
JSON can be transformed to any output type. Use the Conent-Type application/json to specify the JSON input type. JSON is the default input type, so if no Content-Type is specified it automatically attempts to parse JSON.

##### JSON to JSON
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

##### JSON to XML
And with the following Liquid transform:
```xml
<fullName>{{content.name}}</fullName>
```

This will result in the the following XML output:
```xml
<fullName>olaf</fullName>
```

#### XML transformation
XML can be transformed to any output type. Use the Content-Type application/xml or text/xml to specify the XML input type.

##### XML to JSON
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

#### CSV transformation
Basic support for CSV file ingestion is supported. There's currently no 'smarts' around headers and datatypes, but with the use of Liquid constructs and custom filters there's no real limitations.

##### CSV to JSON
Let's take the following CSV input:
```csv
name;dob;score
john doe;03/12/1975;56.8
jane doe;17/03/1980;61.3
```
With the following Liquid transformation:
```
[
{%- for row in content -%}
{%- if forloop.first == false -%}
{
    "name": "{{ row[0] }}",
    "dob": "{{ row[1] | date: 'yyy-MM-dd' }}",
    "score": "{{ row[2] | parsedouble | times: 2 }}"
},
{%- endif -%}
{%- endfor %}
]
```
This will result in the following JSON output:
```
[
    {
        "name": "john doe",
        "dob": "1975-12-03",
        "score": "113.6"
    },
    {
        "name": "jane doe",
        "dob": "1980-03-17",
        "score": "122.6"
    }
]
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
