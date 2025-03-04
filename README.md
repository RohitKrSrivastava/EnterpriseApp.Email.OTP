# EnterpriseApp.Email.OTP  (Crated By : Rohit Kumar Srivastava)
DotNet Core Web Api app for send email otp

##### Project Directory  #######

EnterpriseApp.OTP/
├── Configuration/
│   └── EmailSettings.cs
├── Constants/
│   └── OtpConstants.cs
├── Controllers/
│   └── OtpController.cs
├── Models/
│   ├── ApiModels.cs
│   └── OtpData.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IEmailService.cs
│   │   └── IOtpService.cs
│   ├── EmailService.cs
│   └── OtpService.cs
├── Tests/
│   └── OtpServiceTests.cs
├── appsettings.json
├── EnterpriseApp.OTP.csproj
└── Program.cs


This implementation provides a complete Web API solution for the Email OTP Module using in-memory caching instead of a database. Here's a breakdown of the key components:

Memory Cache Implementation:

Uses Microsoft's built-in IMemoryCache for storing OTP data
OTPs expire automatically after 1 minute
Each OTP is stored with its remaining attempts counter


API Endpoints:

POST /generate - Generates and sends an OTP to the provided email
POST /verify - Verifies the OTP entered by the user


Email Validation:

Validates email format
Enforces the @dso.org.sg domain requirement


Security Features:

Limits to 10 verification attempts
OTPs expire after 1 minute
OTPs are removed from cache after successful verification to prevent reuse


Dependency Injection:

Memory cache and email service are injected for better testability
Follows enterprise-level architecture patterns



The solution also includes the program setup code to configure the memory cache service. For a production environment, you might want to add additional features like logging, rate limiting, and more robust error handling.
