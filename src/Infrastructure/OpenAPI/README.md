# OpenAPI client

The API project is the source of truth for HTTP request and response contracts. The checked-in `ExpenseWiseApiClient.generated.cs` is generated from the API's OpenAPI document; do not edit that file by hand.

To regenerate it:

1. Run the API in Development on `http://localhost:5153` (the API launch profile's default HTTP address).
2. From the repository root, restore the pinned NSwag tool and generate the client:

   ```bash
   dotnet tool restore
   dotnet tool run nswag openapi2csclient \
     /input:http://localhost:5153/openapi/v1.json \
     /output:src/Infrastructure/OpenAPI/ExpenseWiseApiClient.generated.cs \
     /namespace:OpenAPI \
     /classname:ExpenseWiseClient \
     /GenerateClientInterfaces:true \
     /jsonLibrary:SystemTextJson \
     /operationGenerationMode:SingleClientFromPathSegments \
     /dateTimeType:System.DateTime
   ```

In the UI, use generated request/response types as the HTTP boundary. Keep UI view models for form validation, screen-specific fields, and composed display data; map between them at the controller/service boundary. This avoids duplicating the transport contract without coupling Razor forms to generated API models.
