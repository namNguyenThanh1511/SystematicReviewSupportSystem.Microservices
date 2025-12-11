# 🔐 SRSS IAM Authentication Flow Review

## 📋 Executive Summary

The authentication system implements a **JWT + Refresh Token + Blacklist** architecture. The flow handles user registration, login, token refresh, and logout with access token blacklisting via Redis.

**Current Architecture Overview:**
```
User Registration → Login → Access Token (JWT) 
                         ↓
                  Refresh Token (HttpOnly Cookie)
                         ↓
                  Token Refresh → New Access Token
                         ↓
                  Logout → Blacklist Token (Redis) + Revoke Refresh Token
```

---

## 🔄 Detailed Flow Analysis

### 1️⃣ **REGISTRATION** (`RegisterAsync`)

**Location:** `AuthService.cs` (Lines 39-58)

```
POST /api/auth/register
{
    "KeyRegister": "user@email.com or username",
    "FullName": "User Full Name",
    "Password": "password",
    "Role": "User|Admin"
}
```

**Flow:**
1. ✅ Extracts username and email from `KeyRegister` (same value for both)
2. ✅ Validates no duplicate username/email exists
3. ✅ Hashes password using `IPasswordHasher<User>`
4. ✅ Creates user with `IsActive = true`
5. ✅ Persists to database

**Issues Found:**
- ❌ **Critical Security Issue:** `Username` and `Email` are set to the same value from `KeyRegister`
  - Should allow separate email and username inputs
  - Email should be validated as valid email format
- ❌ **No email validation** - Invalid email formats are accepted
- ❌ **No password validation** - No strength requirements enforced
- ✅ Password hashing looks good (using Microsoft.AspNetCore.Identity)

**Recommendation:**(done)
```csharp
// Should be:
public async Task RegisterAsync(RegisterRequest request)
{
    // Validate email format
    if (!IsValidEmail(request.Email))
        throw new BadRequestException("Invalid email format");
    
    // Validate password strength
    ValidatePasswordStrength(request.Password);
    
    // Separate username and email
    var existingUser = await _unitOfWork.Users.FindSingleAsync(u => 
        u.Username.Equals(request.Username, StringComparison.CurrentCultureIgnoreCase) || 
        u.Email.Equals(request.Email, StringComparison.CurrentCultureIgnoreCase));
    
    if (existingUser != null)
        throw new BadRequestException("Username or email already registered");
}
```

---

### 2️⃣ **LOGIN** (`LoginAsync`)

**Location:** `AuthService.cs` (Lines 33-54)

```
POST /api/auth/login
{
    "KeyLogin": "user@email.com or username",
    "Password": "password"
}
```

**Flow:**
1. ✅ Finds user by email OR username (case-insensitive)
2. ✅ Checks `IsActive` status
3. ✅ Verifies password hash
4. ✅ Generates access token
5. ✅ Updates user in database
6. ✅ Returns `LoginResponse` with token and expiry

**Issues Found:**
- ❌ **Missing refresh token issuance** - Access token is returned without refresh token
  - Refresh token is issued in **controller** (`IssueRefreshTokenAsync`), not service
  - Creates separation of concerns issue
- ❌ **Database update** - Why update user on login? (Updates last login time?)
  - Not clear what's being updated
  - Should document this behavior
- ⚠️ **No login attempt tracking** - No brute force protection
- ✅ Password verification uses proper ASP.NET Identity API

**Recommendation:**
```csharp
// Move refresh token issuance to service
public async Task<LoginResponse> LoginAsync(LoginRequest request)
{
    // ... existing validation ...
    
    // Issue refresh token
    var refreshTokenResult = await _refreshTokenService.IssueAsync(user.Id);
    var response = CreateLoginResponse(user, accessToken);
    response.RefreshToken = refreshTokenResult.RefreshToken; // If needed to return
    
    return response;
}
```

---

### 3️⃣ **TOKEN GENERATION** (`GenerateAccessToken`)

**Location:** `JwtService.cs` (Lines 24-45)

**Claims Generated:**
- `NameIdentifier` - User ID (GUID)
- `Email` - User email
- `Role` - User role
- `FullName` - User full name
- `IsActive` - User active status
- `Jti` - JWT ID (unique)
- `Iat` - Issued at time (Unix)

**Issues Found:**
- ❌ **CRITICAL TIMING BUG:** 
  ```csharp
  Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes * 24)
  ```
  - Multiplies by 24! If `AccessTokenExpirationMinutes = 60`, token expires in 1440 minutes (24 hours)
  - Should be just `AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)`
  
- ❌ **Mismatch in CreateLoginResponse:**
  ```csharp
  // AuthService.cs line 101
  AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
  ```
  - JwtService uses `* 24` multiplier
  - AuthService uses no multiplier
  - These should match!

- ✅ Strong HMAC-SHA256 signing
- ✅ Proper claim structure

**Critical Fix Needed:**
```csharp
// JwtService.cs - Remove the * 24 multiplier
Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
```

---

### 4️⃣ **TOKEN VALIDATION** (`ValidateAccessToken`)

**Location:** `JwtService.cs` (Lines 47-60)

**Validation Parameters:**
- ✅ Validates issuer
- ✅ Validates audience
- ✅ Validates lifetime
- ✅ Validates signing key
- ✅ Zero clock skew (strict time validation)
- ✅ Validates algorithm (HmacSha256)

**Issues Found:**
- ⚠️ **Silent failure** - Returns `false` on any exception
  - Should log validation errors for debugging
- ⚠️ **Missing:** Integration with Redis blacklist isn't in JwtService
  - Blacklist check happens in `JwtBlacklistMiddleware` (see #6)

**Recommendation:** (done)
```csharp
public bool ValidateAccessToken(string token)
{
    try
    {
        var validationParameters = GetTokenValidationParameters();
        _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
        
        var isValid = validatedToken is JwtSecurityToken jwtToken &&
               jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
        
        if (!isValid)
            _logger.LogWarning("JWT validation failed for token");
            
        return isValid;
    }
    catch (Exception ex)
    {
        _logger.LogWarning($"JWT validation exception: {ex.Message}");
        return false;
    }
}
```

---

### 5️⃣ **REFRESH TOKEN MANAGEMENT** (`RefreshTokenService`)

**Location:** `RefreshTokenService.cs`

#### 5a. **Issue (`IssueAsync`)**
```
1. Generates cryptographically secure random token (Base64, 64 bytes)
2. Sets expiry to current time + 30 days (from config)
3. Stores in DB with user
4. Returns token and expiry
```

**Quality Assessment:**
- ✅ Uses `RandomNumberGenerator.Create()` (cryptographically secure)
- ✅ 64 bytes = 512 bits (excellent entropy)
- ✅ Base64 encoding for transport
- ✅ UTC time handling
- ✅ Stored in database (persistent)

#### 5b. **Validate (`ValidateAsync`)**
```
1. Finds user by refresh token
2. Checks if token is not expired
3. Returns UserId + ExpiresAt
```

**Issues Found:**
- ❌ **Does NOT check `IsRefreshTokenRevoked` flag** - Missing revocation check!
  - If user has revoked token but still tries to use old token, validation passes
  - Should add: `if (user.IsRefreshTokenRevoked) return null;`

- ❌ **No check for null ExpiryTime** - Database inconsistency could cause null ref

**Fix Needed:** (done)
```csharp
public async Task<RefreshTokenValidationResult?> ValidateAsync(string refreshToken)
{
    var user = await _unitOfWork.Users.FindSingleAsync(u => u.RefreshToken == refreshToken);
    if (user == null) return null;
    
    // ✅ ADD THIS:
    if (user.IsRefreshTokenRevoked) return null;
    
    if (user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
    {
        return null;
    }
    
    return new RefreshTokenValidationResult
    {
        UserId = user.Id,
        ExpiresAt = user.RefreshTokenExpiryTime.Value
    };
}
```

#### 5c. **Revoke (`RevokeAsync`)**
```
1. Finds user by refresh token
2. Sets IsRefreshTokenRevoked = true
3. Clears RefreshToken and RefreshTokenExpiryTime
4. Persists to DB
```

**Quality:** ✅ Looks good - proper cleanup

#### 5d. **IsRevoke (`IsRevokeAsync`)**
```
1. Checks if user exists
2. Returns IsRefreshTokenRevoked flag
```

**Issues Found:**
- ⚠️ Redundant method - Should just be integrated into `ValidateAsync`

---

### 6️⃣ **LOGOUT** (`LogoutAsync`)

**Location:** `AuthService.cs` (Lines 77-91)

```
POST /api/auth/logout
Authorization: Bearer <accessToken>
```

**Flow:**
1. ✅ Finds user by userId
2. ✅ Revokes refresh token (sets to null, flags revoked)
3. ✅ Blacklists access token in Redis with TTL

**Issues Found:**
- ✅ Good approach - two-layer revocation (DB + Redis)
- ✅ Proper TTL handling for Redis

**Minor Improvements:**
- ⚠️ No error if user not found (silently continues)
  - Should throw exception or log warning

---

### 7️⃣ **TOKEN REFRESH** (`RefreshAsync`)

**Location:** `AuthController.cs` (Lines 48-63)

```
POST /api/auth/refresh
Cookie: SRSS_IAM_refreshToken=<token>
```

**Flow:**
1. ✅ Extracts refresh token from HttpOnly cookie
2. ✅ Validates token (checks expiry + validity)
3. ✅ Gets new access token
4. ✅ Issues new refresh token
5. ✅ Returns new access token + sets new refresh cookie

**Issues Found:**
- ⚠️ **Missing revocation check in middleware flow:**
  - `ValidateAsync` doesn't check `IsRefreshTokenRevoked` (see issue #5b)
  - If old refresh token is somehow reused, it passes validation

- ❌ **Cookie security:** `Secure = false`
  ```csharp
  new CookieOptions
  {
      HttpOnly = true,  // ✅ Good
      Secure = false,   // ❌ BAD - Should be true in production
      SameSite = SameSiteMode.None,  // ⚠️ Risky, requires Secure=true
      Expires = expiresUtc
  }
  ```

**Fix:**
```csharp
private static CookieOptions BuildCookieOptions(DateTime expiresUtc)
{
    return new CookieOptions
    {
        HttpOnly = true,
        Secure = true,  // ✅ Only HTTPS
        SameSite = SameSiteMode.Strict, // ✅ Or Lax, depending on requirements
        Expires = expiresUtc
    };
}
```

---

### 8️⃣ **BLACKLIST MIDDLEWARE** (`JwtBlacklistMiddleware`)

**Location:** `JwtBlacklistMiddleware.cs` + Registered in `Program.cs`

```
Middleware → Extract token from Authorization header
          → Call IJwtService.IsRevokeAsync(token)
          → If revoked → Return 401
          → Otherwise → Continue
```

**Issues Found:**
- ❌ **Method doesn't exist in IJwtService!**
  - `IJwtService.IsRevokeAsync()` is called but not defined in interface
  - This is a **compilation error**

- ❌ **Logic problem:** Checking Redis blacklist should be done here, not in JwtService
  - Redis operations belong in middleware or service layer
  - Currently trying to check `IRedisService` from `IJwtService`?

- ⚠️ **Performance issue:** Every request hits Redis
  - Should implement request caching
  - Or use JWT revocation list with periodic sync

**Architecture Issue:**
The middleware should directly use `IRedisService`:
```csharp
public async Task InvokeAsync(HttpContext context, IRedisService redisService)
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    
    if (!string.IsNullOrEmpty(token))
    {
        var isBlacklisted = await redisService.ExistsAsync($"blacklist:{token}");
        if (isBlacklisted)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { message = "Token has been revoked" });
            return;
        }
    }

    await _next(context);
}
```

---

## 📊 Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    AUTHENTICATION FLOW                           │
└─────────────────────────────────────────────────────────────────┘

1. REGISTER
   POST /api/auth/register
   └─→ AuthService.RegisterAsync()
       ├─ Validate: No duplicate user ✅
       ├─ Hash password ✅
       └─ Save user to DB ✅
           ❌ ISSUE: Username = Email (same value)
           ❌ ISSUE: No email/password validation

2. LOGIN
   POST /api/auth/login
   └─→ AuthService.LoginAsync()
       ├─ Find user by email/username ✅
       ├─ Check IsActive ✅
       ├─ Verify password ✅
       ├─ Generate access token ✅
       │   ❌ ISSUE: Expiry * 24 bug in JwtService
       └─→ AuthController.IssueRefreshTokenAsync()
           └─ RefreshTokenService.IssueAsync()
              ├─ Generate random token ✅
              ├─ Store in DB ✅
              └─ Set HttpOnly cookie
                  ❌ ISSUE: Secure = false

3. REQUEST WITH TOKEN
   GET /api/auth/profile
   Authorization: Bearer <accessToken>
   └─→ JwtBlacklistMiddleware
       ├─ Extract token from header ✅
       └─ Check if blacklisted
           ❌ ISSUE: IJwtService.IsRevokeAsync() doesn't exist
           ❌ ISSUE: Should use IRedisService directly

4. REFRESH TOKEN
   POST /api/auth/refresh
   Cookie: SRSS_IAM_refreshToken=<token>
   └─→ AuthController.RefreshToken()
       ├─ Extract token from cookie ✅
       └─→ RefreshTokenService.ValidateAsync()
           ├─ Check token exists ✅
           ├─ Check not expired ✅
           ├─ Check IsActive ✅
           └─ ❌ ISSUE: Missing IsRefreshTokenRevoked check
       └─→ AuthService.RefreshAsync()
           └─ Generate new access token ✅
       └─→ AuthController.IssueRefreshTokenAsync()
           └─ Issue new refresh token ✅

5. LOGOUT
   POST /api/auth/logout
   Authorization: Bearer <accessToken>
   └─→ AuthService.LogoutAsync()
       ├─ Revoke refresh token in DB ✅
       └─ Blacklist access token in Redis ✅
           └─→ Key: "blacklist:{token}"
               Value: "revoked"
               TTL: accessTokenTtl
```

---

## 🚨 Critical Issues Summary

### 🔴 **CRITICAL** (Must Fix)

1. **Access Token Expiry Bug** (JwtService.cs)
   - File: `JwtService.cs` Line 39
   - `Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes * 24)`
   - Should be: `AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)` without `* 24`
   - Impact: Tokens expire 24x longer than intended
   - Severity: 🔴 Critical

2. **Missing IsRefreshTokenRevoked Check** (RefreshTokenService.cs)
   - File: `RefreshTokenService.cs` Line 69 (ValidateAsync)
   - Should check `if (user.IsRefreshTokenRevoked) return null;`
   - Impact: Revoked tokens can still be validated
   - Severity: 🔴 Critical

3. **Blacklist Middleware Broken** (JwtBlacklistMiddleware.cs)
   - File: `JwtBlacklistMiddleware.cs`
   - Calls `IJwtService.IsRevokeAsync()` which doesn't exist
   - Should use `IRedisService` directly
   - Impact: Blacklist check will fail at runtime
   - Severity: 🔴 Critical (Compilation error)

---

### 🟠 **HIGH** (Should Fix)

4. **Cookie Security** (AuthController.cs)
   - File: `BuildCookieOptions()` Line ~92
   - `Secure = false` should be `true` for production
   - `SameSite = None` requires `Secure = true`
   - Impact: HTTPS enforcement missing
   - Severity: 🟠 High

5. **Weak Input Validation** (AuthService.cs)
   - File: `RegisterAsync` Line ~39
   - No email format validation
   - No password strength requirements
   - Username = Email (same value, should be separate)
   - Impact: Invalid data accepted
   - Severity: 🟠 High

6. **JWT Expiry Mismatch** (AuthService.cs + JwtService.cs)
   - JwtService uses `* 24` multiplier
   - AuthService response doesn't match token actual expiry
   - Impact: Client receives wrong expiry time
   - Severity: 🟠 High

---

### 🟡 **MEDIUM** (Nice to Have)

7. **No Brute Force Protection**
   - No login attempt tracking
   - No rate limiting
   - Impact: Account takeover risk
   - Severity: 🟡 Medium

8. **Refresh Token Revocation Not Checked**
   - `IsRevokeAsync` method exists but not used in `ValidateAsync`
   - Severity: 🟡 Medium

9. **No Error Logging**
   - JWT validation errors not logged
   - Severity: 🟡 Medium

---

## ✅ What's Working Well

- ✅ Cryptographically secure random token generation (64 bytes)
- ✅ Proper password hashing with ASP.NET Identity
- ✅ Comprehensive JWT validation (issuer, audience, lifetime, signature)
- ✅ HttpOnly cookies for refresh token
- ✅ Two-layer logout (DB revocation + Redis blacklist)
- ✅ UTC time handling throughout
- ✅ Separate refresh token lifetime configuration
- ✅ User active status check on login/refresh

---

## 📝 Recommended Changes Priority

### Phase 1 (Critical - Fix Immediately)
- [ ] Fix access token expiry `* 24` bug
- [ ] Add `IsRefreshTokenRevoked` check in `ValidateAsync`
- [ ] Fix JwtBlacklistMiddleware to use correct service method

### Phase 2 (High - Fix Soon)
- [ ] Implement email format validation in Register
- [ ] Add password strength validation
- [ ] Set `Secure = true` in cookie options (environment-aware)
- [ ] Align JWT expiry calculation in both places

### Phase 3 (Medium - Consider)
- [ ] Add login attempt tracking/rate limiting
- [ ] Add error logging to JWT validation
- [ ] Add brute force protection
- [ ] Implement JWT revocation caching strategy

---

## 🎯 Architecture Recommendations

### 1. Separate Concerns Better
- Move refresh token issuance to AuthService instead of Controller
- Move Redis blacklist checking to middleware (not JwtService)

### 2. Improve Error Handling
- Log all JWT validation failures
- Provide detailed validation result instead of boolean
- Add structured exception handling

### 3. Add Request Validation
- Create FluentValidation validators for Register/Login DTOs
- Validate email format
- Enforce password requirements
- Validate username format

### 4. Security Hardening
- Implement rate limiting
- Add login attempt tracking
- Add CSRF protection
- Consider JWT revocation list/cache for better performance

### 5. Configuration Review
- Make cookie security settings environment-aware
- Review token lifetime defaults
- Consider shorter token lifetime (15 min access, 7 day refresh)

