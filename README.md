# Asp Net Core - Rest API Authorization with JWT (Roles Vs Claims Vs Policy)

ASP.NET Core authorization provides a simple, declarative role and a rich policy-based model. Authorization is expressed in requirements, and handlers evaluate a user's claims against requirements. Imperative checks can be based on simple policies or policies which evaluate both the user identity and properties of the resource that the user is attempting to access.

## Install packages

- in GrpcServer
```
System.IdentityModel.Tokens.Jwt
Microsoft.AspNetCore.Authentication.JwtBearer
```

## Setup Controller

### Get Roles

<img src="/pictures/get_roles.png" title="get roles"  width="800">

### Get Users

<img src="/pictures/get_all_users.png" title="get all users"  width="800">

### Add User to Role

<img src="/pictures/add_user_to_role.png" title="add user to role"  width="800">
<img src="/pictures/add_user_to_role2.png" title="add user to role"  width="800">
<img src="/pictures/add_user_to_role3.png" title="add user to role"  width="800">

### Get Users

<img src="/pictures/get_user_roles.png" title="get user roles"  width="800">


