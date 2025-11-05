CertificationTracker README
=======================================

Project overview
----------------
CertificationTracker is a web application for managing employee certifications and surfacing expiry notifications for HR and management. It computes notifications at runtime from EmployeeCertification records, displays a notifications badge, and provides views for Employees, Certifications, and Assignments. The project uses ASP.NET Core MVC, Entity Framework Core, Bootstrap 5 for UI, and optional background services for notifications or other hosted tasks.

Goals and features
------------------
- Track employees and their assigned certifications with issue and expiry dates.
- Show notifications derived from expiring EmployeeCertification records within a configurable window.
- Provide polished, consistent Bootstrap 5 UI across all pages without changing back-end logic.
- Minimal friction integration into existing .NET projects: presentation-only changes and optional services.
- Configurable expiry window and easily extensible notification persistence or push mechanisms.

Repository layout
-----------------
- /Controllers
  - Controllers for Employees, Certifications, EmployeeCertifications, Notifications, Account, etc.
- /Models
  - Domain models: Employee, Certification, EmployeeCertification, CertificationNotificationViewModel, LoginViewModel
- /Data
  - ApplicationDbContext and EF Core configuration
- /Services
  - Blob service, optional ExpiryNotifierService hosted worker, SeedData
- /Views
  - Razor views organized by controller and shared components:
    - /Views/Shared/_Layout.cshtml
    - /Views/Shared/_LoginPartial.cshtml
    - /Views/Shared/Components/NotificationsBadge/Default.cshtml
    - Controller-specific views: Employees, Certifications, EmployeeCertifications, Notifications, Account
- /wwwroot
  - CSS and static assets: /css/site.css, /images
- Program.cs
  - Application startup, DI registrations and middleware
- README.txt (this file)

Prerequisites
-------------
- .NET SDK 7.0 or later installed locally
- SQL Server or other EF Core supported database accessible by a connection string
- Optional: tools for EF Core migrations: dotnet-ef
- Recommended development environment: Visual Studio 2022+, Visual Studio Code, or Rider

Configuration
-------------
Open appsettings.json and add or verify the following sections:

- Connection string
  - "ConnectionStrings": { "DefaultConnection": "Server=.;Database=CertificationTracker;Trusted_Connection=True;" }

- Expiry window
  - "ExpiryJob": { "DaysBeforeExpiry": 30 }

- Cookie and other options are configured in Program.cs; adjust as necessary.

Database and EF Core
--------------------
- ApplicationDbContext should register your DbSet properties for Employee, Certification, EmployeeCertification and include Identity tables only if you choose to enable Identity.
- If you enable Identity, ApplicationDbContext must inherit from IdentityDbContext or be configured appropriately.
- Typical EF steps:
  - Add migration: dotnet ef migrations add InitialCreate
  - Apply migration: dotnet ef database update
- SeedData.InitializeAsync exists to populate demo data on startup. Ensure it is idempotent before running in production.

Running locally
---------------
1. Set the connection string in appsettings.json to point to your database.
2. Ensure migrations are applied or run them manually:
   - dotnet ef database update
3. Build and run:
   - dotnet build
   - dotnet run
4. Open the site in a browser at https://localhost:5001 or the port shown in console.

If you prefer automatic migration on startup, uncomment the Database.MigrateAsync call in Program.cs. Use caution in production.

UI and Views
------------
- Views use Bootstrap 5 via CDN and a custom site.css for visual accents.
- _Layout.cshtml contains the main navigation and invokes the NotificationsBadge view component.
- _LoginPartial.cshtml uses IHttpContextAccessor only so Identity registration is optional.
- Employee details view includes status badges for certification expiry and a responsive table.
- All presentation files are safe to replace; they do not change controllers, models, or DI behavior.

Notifications behavior
----------------------
- Notifications are derived at runtime from EmployeeCertification.DateExpiry using the configured DaysBeforeExpiry window.
- Badge component queries EmployeeCertifications to count distinct certifications with expiring assignments.
- No Notifications table or persisted read/unread state by default. If you want persistence:
  - Add a lightweight NotificationRead table to store user read timestamps.
  - Update the badge and controller queries to exclude read entries per user.
  - Optionally add a hosted service or SignalR to push notifications to clients.

Background services and optional components
-------------------------------------------
- ExpiryNotifierService
  - Optional hosted worker that can run periodic background checks and persist or email notifications.
  - Register only if you need persisted notifications or scheduled outbound alerts.
- BlobService
  - Abstraction for document storage. Replace or implement IBlobService to store and serve uploaded documents.

Security
--------
- Cookie authentication is configured by default in Program.cs; adapt to your authentication scheme as required.
- If you enable Identity, register Identity in Program.cs and run migrations to create identity tables.
- Use HTTPS in production and ensure cookies are Secure and HttpOnly.
- Protect sensitive production configuration via environment variables or secret stores.

Testing
-------
- Unit test strategy
  - Create tests for controller logic that do not depend on EF by mocking repositories or using an in-memory DbContext for EF Core.
  - View rendering can be validated with Razor testing or integration tests using TestServer.
- Integration test strategy
  - Use WebApplicationFactory for end-to-end tests that verify routing, views, and controller results.
- Suggested test cases
  - Badge count logic with various EmployeeCertification expiry dates
  - Notifications controller grouping and details behavior
  - Employee details rendering with and without certifications
  - Authentication guard checks on protected controllers

Troubleshooting
---------------
- Error No service for type UserManager IdentityUser
  - Solution: either register Identity in Program.cs or update _LoginPartial to use HttpContext only. The project uses the latter by default to avoid forcing Identity registration.
- TempData InvalidCastException in views
  - Use safe string conversion before rendering TempData values as shown in _Layout sample.
- EF translation errors or runtime expression issues
  - Use AsNoTracking and only EF-translatable constructs in LINQ queries. Avoid unsupported client-side methods in IQueryable expressions.

Contributing
------------
- Follow repository coding conventions and keep UI changes isolated from controller and model logic unless necessary.
- Add unit tests for any nontrivial behavior.
- When adding database migrations, include them in the pull request and document any required deployment steps.

Deployment checklist
--------------------
- Ensure production connection string and secrets are set through secure configuration (Azure Key Vault, environment variables).
- Confirm migrations are applied or provide a migration plan.
- Enable HTTPS and configure reverse proxy or load balancer if necessary.
- Scale the database and web tier based on expected load; notification queries may need indexing on EmployeeCertifications.DateExpiry and CertificationId.

License
-------
Add a LICENSE file to the repository with your chosen license. If none is present the project is considered unlicensed by default.

Appendix - Optional next steps
------------------------------
- Provide README.md markdown version.
- Provide Dockerfile or Azure App Service deployment script.
- Provide CONTRIBUTING.md with contributor guidelines.

--------------------------------------------------
LOGIN
--------------------------------------------------

HR Login
Username: hr.admin@company
Password: Pa$$word123!

Manager
Username: manager@company.local
Password: Pa$$word123!

=====================================================
End of file