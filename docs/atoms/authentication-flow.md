# Authentication Flow

## JWT Authentication Architecture

```
1. Client → POST /api/auth/login → AuthService
2. AuthService validates credentials → returns JWT token  
3. Client → API requests with Authorization: Bearer {token} → API Gateway
4. API Gateway → forwards request to target service
5. Target Service → validates JWT → processes request
```

## JWT Token Structure
```json
{
  "sub": "123",                    // User ID
  "unique_name": "admin",          // Username  
  "role": "Admin",                 // User role
  "customer_tier": "Gold",         // Business tier (for rate limiting)
  "user_id": "123",               // Explicit user ID claim
  "iss": "LibraryApp.AuthService", // Issuer
  "aud": "LibraryApp.ApiClients",  // Audience
  "exp": 1234567890               // Expiration
}
```

## AuthService Implementation
```csharp
[HttpPost("login")]
public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
{
    var user = await _authService.ValidateUserAsync(request.Username, request.Password);
    if (user == null)
    {
        return Unauthorized(new ApiResponse<object>
        {
            Success = false,
            Message = "Invalid credentials"
        });
    }

    var token = await _jwtTokenService.GenerateTokenAsync(user);
    return Ok(new ApiResponse<LoginResponse>
    {
        Success = true,
        Data = new LoginResponse
        {
            Token = token,
            User = _mapper.Map<UserDto>(user),
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        }
    });
}
```

## JWT Token Generation
```csharp
public async Task<string> GenerateTokenAsync(User user)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim("customer_tier", user.CustomerTier ?? "Bronze"),
        new Claim("user_id", user.Id.ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _jwtSettings.Issuer,
        audience: _jwtSettings.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

## Service-Level JWT Validation
Each service validates JWT independently:

```csharp
// In Program.cs of Book/Member services
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
        };
    });
```

## Custom Authorization Attribute
```csharp
public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}

// Usage:
[AuthorizeRoles("Admin")]
public async Task<IActionResult> CreateBook([FromBody] CreateBookRequest request)
{
    // Only Admin users can create books
}

[AuthorizeRoles("Admin", "Member")]  
public async Task<IActionResult> GetBooks()
{
    // Both Admin and Members can view books
}
```

## JWT Middleware Pattern
```csharp
public class JwtAuthenticationMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            await AttachUserToContext(context, token);
        }

        await next(context);
    }

    private async Task AttachUserToContext(HttpContext context, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.First(x => x.Type == "user_id").Value;
            
            // Attach user to context for use in controllers
            context.Items["UserId"] = userId;
            context.Items["User"] = jwtToken.Claims;
        }
        catch
        {
            // Token validation failed - user is not authenticated
        }
    }
}
```

## API Gateway Authentication
API Gateway validates JWT for routing decisions but passes token through:

```csharp
// API Gateway Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* same JWT validation */ });

// Middleware order is critical:
app.UseAuthentication();  // Validate JWT
app.UseAuthorization();   // Check permissions
await app.UseOcelot();    // Route to services (with token forwarded)
```

## Configuration Pattern
```yaml
# Environment variables for all services
JWT_SECRET_KEY: "YourSuperSecretKeyThatIsAtLeast32CharactersLong123456789"
JWT_ISSUER: "LibraryApp.AuthService"  
JWT_AUDIENCE: "LibraryApp.ApiClients"
JWT_EXPIRY_MINUTES: 60
```

```json
// appsettings.json
{
  "JwtSettings": {
    "SecretKey": "${JWT_SECRET_KEY}",
    "Issuer": "${JWT_ISSUER}",
    "Audience": "${JWT_AUDIENCE}",
    "ExpiryMinutes": "${JWT_EXPIRY_MINUTES}"
  }
}
```

## Frontend Integration
```typescript
// Store token after login
const response = await fetch('/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ username, password })
});

const { data } = await response.json();
localStorage.setItem('token', data.token);

// Use token in subsequent requests
const token = localStorage.getItem('token');
const response = await fetch('/api/books', {
  headers: { 
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});
```

## Security Considerations
- **Secret Key**: Must be at least 256 bits (32+ characters)
- **HTTPS Only**: Never transmit JWT over HTTP in production
- **Token Expiry**: Short-lived tokens (1 hour) with refresh mechanism
- **Secure Storage**: HttpOnly cookies preferred over localStorage
- **Validation**: Every service validates independently (no single point of failure)