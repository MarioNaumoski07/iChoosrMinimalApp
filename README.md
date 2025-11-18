iChoosr assignment plan  
Author: [Mario Naumoski]

################################################

This assignment focuses on developing a Proxy API using the .NET Minimal API pattern. This API will function as middleware and will be secured using JWT authentication and role-based access control (RBAC).

Key configurations will include:

⦁	    API Explorer and Documentation: AddEndpointsApiExplorer, MapOpenApi, AddSwaggerGen, UseSwagger, and UseSwaggerUI.
⦁	    Security Pipeline: UseAuthentication and UseAuthorization. 


1. For first part I will create the main skeleton of the application, setting up the minimal API architecture.

2. Second part i will implement swagger, so it will allow practical and easy approach to see and test API endpoints.

3. In third part i will create endpoint to be visible on swagger and check if it work properly and ping the endpoint successfully.

4. If everything works fine with endpoints, in fourth part i will set up the proxy api, so first client will ping proxy endpoint, and inside the endpoint i will ping spacexdata endpoints.

5. Next thing that I will implement in fifth part is access control, so there should be user the should log in to use the api calls and i will be protected to not be public if you are not authenticated. Depending on time in this part i will use JWT token for authorization and use claims and i will use roles so depending on the role for example Admin, User, they will have different access to the to execute the api calls.
Admin will have permission to execute every api call for example /payloads and /payloads/{payload_id} and the user can only retrieve /payloads/{payload_id}
