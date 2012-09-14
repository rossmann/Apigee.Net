Apigee.Net
==========

C# Client for the Apigee Usergrid : https://apigee.com/usergrid/

Client is in early Alpha - only works with basic User functionality 
Results May Vary 

Usage is as follows :


            ApigeeClient apiClient = new ApigeeClient("http://api.usergrid.com/xxx/sandbox/");

            //Get a collection of all users 
            var allUsers = apiClient.GetUsers();

            string un = "apigee_" + Guid.NewGuid();

            //Create a new Account
            apiClient.CreateAccount(new UserModel
            {
                Username = un,
                Password = "abc123",
                Email = un + "@sympletech.com"
            });

            //Update an Existing Account
            apiClient.UpdateAccount(new UserModel
            {
                Username = un,
                Password = "abc123456",
                Email = un + "@sympletech.com"
            });

            //Login User - Get Token 
            var token = apiClient.GetToken(un, "abc123456");

            //Lookup a user by token ID
            var username = apiClient.LookUpToken(token);