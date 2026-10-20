# OpenAPI client

The API project is the source of truth for HTTP request and response contracts. The checked-in `ExpenseWiseApiClient.generated.cs` is generated from the API's OpenAPI document; do not edit that file by hand.
The API marks `decimal` schemas with the `decimal` format so NSwag preserves monetary values as `decimal` in generated request and response types.

To regenerate it:

1. Run the API in its Development Docker container using the existing Compose setup described in the [operations guide](../../../docs/operations/README.md). The default host-published API port is `8081`; use the configured `API_PORT` if it differs. Do not use the local launch profile, which embeds its `localhost:5153` address into the generated contract.
2. From the repository root, fetch the contract served by the container:

   ```bash
   curl --fail --silent --show-error \
     http://localhost:8081/openapi/v1.json \
     --output docs/api/open-api.json
   ```

3. Restore the pinned NSwag tool and generate the client from the checked-in contract:

   ```bash
   dotnet tool restore
   dotnet tool run nswag openapi2csclient \
     /input:docs/api/open-api.json \
     /output:src/Infrastructure/OpenAPI/ExpenseWiseApiClient.generated.cs \
     /namespace:OpenAPI \
     /classname:ExpenseWiseClient \
     /GenerateClientInterfaces:true \
     /jsonLibrary:SystemTextJson \
     /operationGenerationMode:SingleClientFromPathSegments \
     /dateTimeType:System.DateTime
   ```

In the UI, use generated request/response types as the HTTP boundary. Keep UI view models for form validation, screen-specific fields, and composed display data; map between them at the controller/service boundary. This avoids duplicating the transport contract without coupling Razor forms to generated API models.
