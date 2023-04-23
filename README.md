# Asp Net Core - Rest API Authorization with JWT (Roles Vs Claims Vs Policy)

### Install packages 
- in GrpcServer
```
System.IdentityModel.Tokens.Jwt
Microsoft.AspNetCore.Authentication.JwtBearer
```

- in GrpcClient
```
Grpc.Net.Client
Google.Protobuf
Grpc.Tools
```

### Result

<img src="/pictures/calculations.png" title="calculations"  width="800">


## AspNetCore gRPC Deadline

A **Deadline** allows a gRPC client to specify how long it will wait for a call to complete. When a deadline is exceeded, the call will get cancelled. Setting a deadline is important because it provides an upper limit on how long a call can run for. It stops misbehaving services from running forever and exhausting server resources. Deadlines are a useful tool for building reliable apps and should be configured.

<img src="/pictures/delay.png" title="delay"  width="400">
