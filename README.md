# Asp Net Core - Rest API Authorization with JWT (Roles Vs Claims Vs Policy)

ASP.NET Core authorization provides a simple, declarative role and a rich policy-based model. Authorization is expressed in requirements, and handlers evaluate a user's claims against requirements. Imperative checks can be based on simple policies or policies which evaluate both the user identity and properties of the resource that the user is attempting to access.

!!! TO DO : Add connection to SQL DB

## Migration

In **Package Manager Console** :
```
Add-Migration Add_refresh_token_table
Update-Database
```


## Refresh Token

### Register

<img src="/pictures/refresh_token.png" title="refresh token"  width="800">

### Login

<img src="/pictures/refresh_token2.png" title="refresh token"  width="800">

We can retrieve the meaning of the token. Currently we don't have the roles included in the token :

<img src="/pictures/token_decode.png" title="token decode"  width="800">

### Refresh

<img src="/pictures/token_used.png" title="token has been used error"  width="800">


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

### Remove User from Role

<img src="/pictures/remove_user_from_role.png" title="remove user from role"  width="800">


## Authentication Controller

### Register new User

- Register :

<img src="/pictures/register_new_user.png" title="register new user"  width="800">

- Get user role :

<img src="/pictures/register_new_user2.png" title="register new user"  width="800">

- Token not yet expired :

<img src="/pictures/token_not_yet_expired.png" title="token not yet expired"  width="800">

- Token has been used :

<img src="/pictures/token_has_been_used.png" title="token has been used"  width="800">

- Token successfully refreshed :

<img src="/pictures/token_successfully_refreshed.png" title="token successfully refreshed"  width="800">

