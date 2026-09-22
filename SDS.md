# College Complaint Management System

## System Design Document

### Version 1.0

---

## 1. Project Overview

The **College Complaint Management System (CCMS)** is a web-based complaint management platform designed for college students.

The system allows students to:

* Register using their College ID and College Email.
* Verify their email address.
* Log in securely.
* Submit complaints related to college facilities and services.
* Add complaint title, category, description, and location.
* Optionally upload supporting images.
* View their submitted complaints.
* Edit or delete complaints before administrative review.
* Track the current status of their complaints.

A designated administrator manages all complaints through a secure administrative dashboard.

The system will be developed as a **responsive ASP.NET Core MVC web application** and will also support installation as a **Progressive Web App (PWA)**.

---

# 2. Project Goals

The primary goals are:

1. Provide students with a simple way to report college-related problems.
2. Give administrators a centralized interface to manage complaints.
3. Make complaint status transparent to students.
4. Reduce manual complaint handling.
5. Maintain structured complaint records.
6. Provide secure authentication and authorization.
7. Support image evidence through Cloudinary.
8. Provide email notifications through Brevo.
9. Provide a mobile-friendly, installable PWA experience.
10. Keep the system simple enough for students and administrators to use without training.

---

# 3. User Roles

The initial system contains two primary roles.

## 3.1 Student

Students can:

* Register.
* Verify their email.
* Log in.
* Log out.
* View their dashboard.
* Submit complaints.
* Upload optional complaint images.
* View their own complaints.
* Edit Submitted complaints.
* Delete Submitted complaints.
* View the current status of complaints.
* Manage their profile.

Students cannot:

* Access other students' complaints.
* Change complaint status.
* Access administrator pages.
* Create administrator accounts.
* Modify system categories.
* Access administrative statistics.

---

## 3.2 Administrator

The administrator can:

* Log in securely.
* View the administrator dashboard.
* View all complaints.
* Search complaints.
* Filter complaints.
* View complaint details.
* View complaint images.
* Change complaint status.
* Resolve complaints.
* Close complaints.
* View registered students.
* Manage complaint categories.
* View complaint statistics.
* Manage administrator approval requests where applicable.

---

# 4. Authentication and Registration

## Student Registration

The registration form contains:

* Full Name
* College ID
* College Email
* Password
* Confirm Password

### Registration Flow

```text
Student
   │
   ▼
Registration Form
   │
   ▼
Validate Details
   │
   ▼
Create Account
   │
   ▼
Send Verification Email
   │
   ▼
Student Verifies Email
   │
   ▼
Account Activated
   │
   ▼
Login
   │
   ▼
Student Dashboard
```

Email verification will use **Brevo** for email delivery.

The exact college-ID validation mechanism can be configured according to the college's available student records.

---

# 5. Administrator Authentication

Administrator access must be separated from normal student registration.

Students must never be able to select:

```text
Role = Administrator
```

during registration.

Administrator accounts require controlled creation and approval.

The system must ensure that an administrator cannot approve their own administrator registration request.

The initial administrator should be provisioned through a secure deployment/setup process.

---

# 6. Complaint Management

## Complaint Submission

A student can submit a complaint using the following information:

| Field           | Required |
| --------------- | -------- |
| Complaint Title | Yes      |
| Category        | Yes      |
| Description     | Yes      |
| Location        | Yes      |
| Image           | No       |

### Location

The location will be a free-text field.

Example placeholder:

```text
e.g. Main Building, Room 204
```

This allows students to describe locations that may not exist in a predefined list.

---

# 7. Complaint Categories

Initial categories may include:

* Infrastructure
* Laboratory
* Library
* Washroom & Sanitation
* Electricity
* Water Supply
* Internet & Wi-Fi
* Administration
* Other

Categories should be administrator-managed.

The administrator can:

* Add categories.
* Edit categories.
* Activate categories.
* Deactivate categories.

Deactivated categories should not be available for new complaints but must remain attached to historical complaints.

---

# 8. Complaint Lifecycle

The system uses four primary statuses:

```text
SUBMITTED
    │
    ▼
UNDER REVIEW
    │
    ▼
RESOLVED
    │
    ▼
CLOSED
```

## Submitted

The student has successfully created the complaint.

Student permissions:

* View
* Edit
* Delete

## Under Review

The administrator has started reviewing the complaint.

Student permissions:

* View only

## Resolved

The administrator has resolved the reported issue.

Student permissions:

* View only

## Closed

The administrator has completed the complaint process.

Student permissions:

* View only

---

# 9. Complaint Permission Matrix

| Action               | Submitted | Under Review | Resolved | Closed |
| -------------------- | --------: | -----------: | -------: | -----: |
| Student View         |         ✓ |            ✓ |        ✓ |      ✓ |
| Student Edit         |         ✓ |            ✕ |        ✕ |      ✕ |
| Student Delete       |         ✓ |            ✕ |        ✕ |      ✕ |
| Admin View           |         ✓ |            ✓ |        ✓ |      ✓ |
| Admin → Under Review |         ✓ |            ✕ |        ✕ |      ✕ |
| Admin → Resolved     |         ✕ |            ✓ |        ✕ |      ✕ |
| Admin → Closed       |         ✕ |            ✕ |        ✓ |      ✕ |

The server must enforce these rules.

Hiding a button in the UI is not considered sufficient authorization.

---

# 10. Complaint Reference Number

Every complaint receives a unique reference number.

Example:

```text
CMP-2026-000124
```

The reference number can be displayed to the student immediately after successful submission.

It can also be used by administrators when searching for complaints.

---

# 11. Complaint Information

A complaint record should contain:

```text
Complaint ID
Reference Number
Student ID
Category ID
Title
Description
Location
Status
Created At
Updated At
Resolved At
Closed At
```

The timestamps are generated by the server.

Client-provided timestamps must not be trusted.

---

# 12. Image Management

Cloudinary will be used for complaint images.

## Upload Flow

```text
Student selects image
        │
        ▼
Client-side validation
        │
        ▼
ASP.NET Core server validation
        │
        ▼
Upload to Cloudinary
        │
        ▼
Cloudinary returns image information
        │
        ▼
Save Cloudinary reference in SQL Server
        │
        ▼
Complaint successfully stored
```

The database should not store image binaries.

It should store information such as:

```text
Image ID
Complaint ID
Cloudinary Public ID
Image URL
Uploaded At
```

## Image Security

The system should:

* Validate MIME type.
* Validate file extension.
* Enforce maximum file size.
* Reject unsupported files.
* Keep Cloudinary secrets server-side.
* Never expose Cloudinary API secrets to the browser.
* Handle upload failures safely.

Recommended initial formats:

```text
JPG / JPEG
PNG
WEBP
```

The final maximum file size can be configured in application settings.

---

# 13. Email System

**Brevo** will be used as the email delivery provider.

Emails will be used for:

### Registration

* Email verification.

### Complaint status

When the administrator changes the complaint status, the student receives an email.

Example:

```text
Your complaint CMP-2026-000124
has been marked as Under Review.
```

Possible status emails:

```text
Submitted
Under Review
Resolved
Closed
```

The system should send an email only after the corresponding database operation succeeds.

---

# 14. Student Portal

## Navigation

```text
Student Portal

├── Dashboard
├── Submit Complaint
├── My Complaints
├── Profile
└── Logout
```

---

# 15. Student Dashboard

The dashboard should be simple and action-oriented.

### Header

Contains:

* College Complaint Management branding.
* Student profile/menu.
* Logout.

### Main content

Display:

```text
Welcome back

[ Total Complaints ]
[ Under Review ]
[ Resolved ]
[ Closed ]

        + Submit Complaint

Recent Complaints
----------------------------------
Complaint ID
Title
Category
Date
Status
----------------------------------
```

The dashboard should prioritize recent complaints and the Submit Complaint action.

---

# 16. Submit Complaint Screen

The screen should contain:

```text
Submit a Complaint

Complaint Title
[____________________________]

Category
[ Select Category ▼ ]

Location
[ e.g. Main Building, Room 204 ]

Description
[                            ]
[                            ]
[                            ]

Supporting Image
[ Upload Image ]

[ Cancel ]       [ Submit Complaint ]
```

The image should be optional.

After successful submission:

```text
Complaint Submitted Successfully

Complaint Reference:
CMP-2026-000124

[ View Complaint ]
```

---

# 17. My Complaints

Desktop layout:

```text
My Complaints

Search
[________________]

-----------------------------------------------
ID          Title       Category    Status
-----------------------------------------------
CMP-001     Fan Issue   Electricity Submitted
CMP-002     WiFi Issue  Internet    Under Review
CMP-003     Lab Issue   Laboratory  Resolved
-----------------------------------------------
```

Mobile layout should use cards:

```text
CMP-2026-000124

Wi-Fi connectivity problem
Internet & Wi-Fi

Status
Under Review

Submitted
22 Sep 2026

[ View Details ]
```

---

# 18. Complaint Details

The student complaint details screen displays:

```text
CMP-2026-000124

Wi-Fi connectivity problem

Status
UNDER REVIEW

Category
Internet & Wi-Fi

Location
Main Building, Room 204

Description
The Wi-Fi connection is unavailable...

Attachment
[ Complaint Image ]

Submitted
22 Sep 2026
```

When the status is Submitted:

```text
[ Edit Complaint ] [ Delete Complaint ]
```

Once the administrator begins reviewing:

```text
No Edit/Delete controls
```

---

# 19. Administrator Portal

## Navigation

```text
Admin Portal

├── Dashboard
├── Complaints
├── Students
├── Categories
├── Reports
├── Settings
└── Logout
```

---

# 20. Administrator Dashboard

The administrator dashboard should provide a quick overview.

Example:

```text
Dashboard

Total Complaints       120
Submitted               18
Under Review            32
Resolved                45
Closed                  25

Recent Complaints
------------------------------------------
ID        Student       Category    Status
------------------------------------------
CMP-001   Student A     Electricity Submitted
CMP-002   Student B     Laboratory  Under Review
CMP-003   Student C     Internet    Resolved
```

All values will come from the database.

No mock statistics should be displayed in production.

---

# 21. Administrator Complaint Management

The administrator can search and filter complaints.

### Search

Search by:

* Complaint reference number.
* Complaint title.
* Student name.
* College ID.

### Filters

Filter by:

* Status.
* Category.
* Date range.

### Sorting

Sort by:

* Newest.
* Oldest.
* Recently updated.

Pagination should be used when the complaint count becomes large.

---

# 22. Administrator Complaint Details

The administrator sees:

```text
Complaint
--------------------------------

CMP-2026-000124

Student
Rahul Sharma
College ID: STU1024

Category
Internet & Wi-Fi

Location
Main Building, Room 204

Description
...

Attachment
[ Image ]

Current Status
UNDER REVIEW

Actions
[ Mark Resolved ]
```

After resolution:

```text
Current Status
RESOLVED

[ Close Complaint ]
```

The system should show only valid actions for the current state.

---

# 23. Student Management

Administrator can view:

* Student name.
* College ID.
* Email.
* Account verification status.
* Registration date.

Search:

```text
Search by name / College ID
```

Administrators must never see student passwords.

---

# 24. Category Management

Administrator interface:

```text
Complaint Categories

+ Add Category

------------------------------------
Category              Status    Action
------------------------------------
Infrastructure        Active    Edit
Laboratory            Active    Edit
Library               Active    Edit
Electricity           Active    Edit
Internet              Active    Edit
------------------------------------
```

Categories can be deactivated instead of physically deleting them.

This preserves historical complaint data.

---

# 25. Reports

The initial reporting system should remain simple.

Reports can include:

* Total complaints.
* Complaints by status.
* Complaints by category.
* Complaints during a selected date range.
* Resolved complaints.
* Closed complaints.

Possible future functionality:

* CSV/Excel export.
* PDF reports.
* Charts.
* Average resolution time.
* Monthly trends.

These are not required for the initial release.

---

# 26. PWA Design

The application will support installation as a Progressive Web App.

Required components:

```text
manifest.json
service-worker.js
application icons
theme metadata
responsive layouts
```

The PWA should support:

* Mobile installation.
* Desktop installation where supported.
* Standalone display mode.
* Responsive navigation.
* Appropriate caching.
* Offline status indication.

The system must not falsely claim that a complaint was submitted while the device is offline.

---

# 27. Responsive Design

The application must support:

### Mobile

```text
320px+
```

### Tablet

```text
768px+
```

### Desktop

```text
1024px+
```

The student interface should prioritize mobile usability.

The administrator interface should prioritize desktop productivity while remaining usable on tablets and smaller screens.

---

# 28. Design System

## Visual Style

The visual language should be:

* Minimal.
* Professional.
* Clean.
* Accessible.
* Uncluttered.
* Student-friendly.

## Colors

Primary:

```text
Black      #111827
White      #FFFFFF
```

Background:

```text
#F8FAFC
```

Border:

```text
#E5E7EB
```

Secondary text:

```text
#6B7280
```

Status colors should be used sparingly.

### Typography

Recommended:

```text
Inter
```

Typography hierarchy:

```text
Page heading
Section heading
Card heading
Body text
Secondary text
Helper text
```

Avoid excessive font sizes, shadows, gradients, and decorative elements.

---

# 29. Suggested ASP.NET Core Architecture

The application should use a modular monolith architecture.

```text
CollegeComplaintSystem
│
├── Controllers
│
├── Models
│
├── ViewModels
│
├── Services
│
├── Data
│
├── Identity
│
├── Infrastructure
│
├── Views
│
└── wwwroot
```

### Controllers

Responsible for:

* Receiving requests.
* Authorization.
* Calling services.
* Returning Views.

Controllers should not contain large amounts of business logic.

### Services

Examples:

```text
IComplaintService
ICloudinaryService
IEmailService
IStudentService
IAdminService
ICategoryService
```

### ViewModels

Use dedicated ViewModels for forms and pages rather than exposing database entities directly to Razor Views.

---

# 30. Proposed Project Structure

```text
CollegeComplaintSystem/
│
├── CollegeComplaintSystem.sln
│
├── CollegeComplaintSystem.Web/
│   │
│   ├── Controllers/
│   │   ├── AccountController.cs
│   │   ├── StudentController.cs
│   │   ├── ComplaintController.cs
│   │   ├── AdminController.cs
│   │   ├── CategoryController.cs
│   │   └── ReportController.cs
│   │
│   ├── Models/
│   │   ├── ApplicationUser.cs
│   │   ├── Complaint.cs
│   │   ├── ComplaintCategory.cs
│   │   ├── ComplaintImage.cs
│   │   ├── ComplaintStatusHistory.cs
│   │   └── AdministratorApproval.cs
│   │
│   ├── ViewModels/
│   │   ├── LoginViewModel.cs
│   │   ├── RegisterViewModel.cs
│   │   ├── ComplaintCreateViewModel.cs
│   │   ├── ComplaintEditViewModel.cs
│   │   └── ComplaintDetailsViewModel.cs
│   │
│   ├── Services/
│   │   ├── ComplaintService.cs
│   │   ├── CloudinaryService.cs
│   │   ├── EmailService.cs
│   │   └── CategoryService.cs
│   │
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── Configurations/
│   │
│   ├── Views/
│   │   ├── Account/
│   │   ├── Student/
│   │   ├── Complaints/
│   │   ├── Admin/
│   │   ├── Categories/
│   │   └── Shared/
│   │
│   ├── wwwroot/
│   │   ├── css/
│   │   ├── js/
│   │   ├── images/
│   │   ├── icons/
│   │   ├── manifest.json
│   │   └── service-worker.js
│   │
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
│
└── CollegeComplaintSystem.Tests/
```

---

# 31. Database Design

## ApplicationUser

ASP.NET Core Identity should be used for authentication.

Additional fields:

```text
Id
FullName
CollegeId
Email
EmailConfirmed
CreatedAt
IsActive
```

Roles:

```text
Student
Administrator
```

---

## Complaint

```text
ComplaintId
ReferenceNumber
StudentId
CategoryId
Title
Description
Location
Status
CreatedAt
UpdatedAt
ResolvedAt
ClosedAt
```

Relationships:

```text
Student 1 ──────── * Complaint

Category 1 ─────── * Complaint

Complaint 1 ────── * ComplaintImage

Complaint 1 ────── * ComplaintStatusHistory
```

---

## ComplaintCategory

```text
CategoryId
Name
Description
IsActive
CreatedAt
UpdatedAt
```

---

## ComplaintImage

```text
ImageId
ComplaintId
CloudinaryPublicId
ImageUrl
CreatedAt
```

---

## ComplaintStatusHistory

Recommended for reliable administrative history.

```text
HistoryId
ComplaintId
PreviousStatus
NewStatus
ChangedBy
ChangedAt
```

Even though students only see the current status, the backend can maintain historical status information.

---

# 32. Security Architecture

The system must implement:

### Authentication

ASP.NET Core Identity.

### Authorization

Role-based authorization.

Example:

```text
[Authorize(Roles = "Administrator")]
```

Administrator routes must be protected.

Student routes should require authenticated student access.

### Ownership authorization

For every complaint request:

```text
CurrentUserId == Complaint.StudentId
```

must be verified before allowing access.

### Input validation

Validation must happen both:

* Client-side for usability.
* Server-side for security.

Server-side validation is authoritative.

### CSRF protection

MVC form submissions should use ASP.NET Core's antiforgery protection.

### Secrets

The following must never be committed to source control:

```text
Database passwords
Cloudinary API secrets
Brevo API keys
Authentication secrets
Production connection strings
```

Use environment variables or secure deployment configuration.

---

# 33. Error Handling

The application should provide user-friendly errors.

Example:

```text
Something went wrong.

We couldn't submit your complaint.
Please try again.
```

Technical details such as:

```text
SQL exception
Cloudinary credentials
Stack traces
Internal server paths
```

must never be displayed to normal users.

Detailed errors should be available through server-side logging.

---

# 34. Validation Rules

### Registration

* Name required.
* College ID required.
* College ID unique.
* College email required.
* Valid email format.
* Password must meet configured security requirements.
* Email must be verified.

### Complaint

* Title required.
* Category required.
* Description required.
* Location required.
* Image optional.
* Image format validated.
* Image size validated.

### Category

* Name required.
* Category name should be unique.

---

# 35. Audit and Timestamp Strategy

Important operations should record server-generated timestamps.

Examples:

```text
Complaint Created
Complaint Updated
Complaint Reviewed
Complaint Resolved
Complaint Closed
```

This ensures the system has reliable historical information.

Status changes should be recorded in `ComplaintStatusHistory`.

---

# 36. Notification Architecture

The application should use a service abstraction:

```text
IEmailService
```

The application does not need to know the implementation details of Brevo.

Example:

```text
ComplaintService
      │
      ▼
IEmailService
      │
      ▼
Brevo
      │
      ▼
Student Email
```

This allows the email provider to be replaced later without rewriting complaint-management logic.

---

# 37. Development Phases

## Phase 1 — Project Setup

* Create ASP.NET Core MVC solution.
* Configure SQL Server.
* Configure Entity Framework Core.
* Configure ASP.NET Core Identity.
* Configure roles.
* Configure application settings.

## Phase 2 — Database

* Create models.
* Configure relationships.
* Create migrations.
* Seed initial categories.
* Provision initial administrator.

## Phase 3 — Authentication

* Student registration.
* Email verification.
* Login.
* Logout.
* Forgot password.
* Role authorization.

## Phase 4 — Student Portal

* Dashboard.
* Submit Complaint.
* My Complaints.
* Complaint Details.
* Edit Complaint.
* Delete Complaint.
* Profile.

## Phase 5 — Cloudinary

* Image upload.
* Validation.
* Cloudinary integration.
* Database image references.
* Image display.

## Phase 6 — Administrator Portal

* Admin dashboard.
* Complaint listing.
* Search.
* Filters.
* Complaint details.
* Status management.
* Student management.
* Category management.

## Phase 7 — Email Notifications

* Verification emails.
* Complaint status emails.
* Email templates.
* Error handling.

## Phase 8 — PWA

* Manifest.
* Icons.
* Service worker.
* Installability.
* Responsive layouts.
* Offline indicator.

## Phase 9 — Testing

Test:

* Registration.
* Email verification.
* Authentication.
* Authorization.
* Complaint creation.
* Complaint editing.
* Complaint deletion.
* Image upload.
* Status transitions.
* Email notifications.
* Student ownership restrictions.
* Administrator permissions.
* Responsive layouts.

## Phase 10 — Deployment

* Production database.
* Production configuration.
* Cloudinary production configuration.
* Brevo production configuration.
* HTTPS.
* Logging.
* Error handling.
* Database migrations.
* Production testing.

---

# 38. Testing Scenarios

### Student security

A Student A must not be able to access Student B's complaint by manually changing:

```text
/Complaint/Details/123
```

to another complaint ID.

### Editing

Submitted:

```text
Edit = Allowed
```

Under Review:

```text
Edit = Forbidden
```

Resolved:

```text
Edit = Forbidden
```

Closed:

```text
Edit = Forbidden
```

### Deletion

Submitted:

```text
Delete = Allowed
```

Under Review:

```text
Delete = Forbidden
```

Resolved:

```text
Delete = Forbidden
```

Closed:

```text
Delete = Forbidden
```

### Status

The application must reject invalid transitions.

For example:

```text
Submitted → Closed
```

must not be directly permitted.

---

# 39. Production Principles

The following principles should be followed throughout development:

1. Do not put business logic directly inside Razor Views.
2. Do not trust client-side validation.
3. Do not trust client-provided user IDs.
4. Do not trust client-provided timestamps.
5. Do not expose database entities unnecessarily.
6. Do not store passwords manually.
7. Do not store Cloudinary images inside SQL Server.
8. Do not expose API secrets.
9. Do not allow students to access other students' complaints.
10. Do not rely only on UI controls for authorization.
11. Do not allow invalid complaint status transitions.
12. Do not report successful operations when persistence has failed.
13. Keep configuration environment-specific.
14. Keep controllers lightweight.
15. Keep services responsible for business operations.
16. Use migrations for database changes.
17. Log unexpected production errors securely.
18. Keep the initial system simple and maintainable.

---

# 40. Final Product Structure

The finished application will essentially consist of two experiences.

## Student Experience

```text
Login
  ↓
Student Dashboard
  ├── Submit Complaint
  ├── My Complaints
  │      └── Complaint Details
  └── Profile
```

## Administrator Experience

```text
Admin Login
  ↓
Admin Dashboard
  ├── Complaints
  │      └── Complaint Details
  ├── Students
  ├── Categories
  ├── Reports
  └── Settings
```

Both experiences will use the same ASP.NET Core MVC application and authentication system, with role-based authorization separating their access.

---

# 41. Technology Stack

### Backend

```text
ASP.NET Core MVC
.NET 10
C#
```

### Database

```text
Microsoft SQL Server
Entity Framework Core
```

### Authentication

```text
ASP.NET Core Identity
Role-based Authorization
```

### Frontend

```text
Razor Views
HTML5
CSS3
JavaScript
Responsive UI
```

### External Services

```text
Cloudinary → Complaint Images
Brevo      → Email
```

### Application

```text
Responsive Web Application
+
Progressive Web App
```

---

# 42. MVP Definition

The first production-ready version is complete when a student can:

```text
Register
   ↓
Verify Email
   ↓
Login
   ↓
Submit Complaint
   ↓
Upload Optional Image
   ↓
View Complaint
   ↓
Edit/Delete while Submitted
   ↓
Track Status
```

And an administrator can:

```text
Login
   ↓
View Complaints
   ↓
Review Complaint
   ↓
Under Review
   ↓
Resolved
   ↓
Closed
```

with:

* SQL Server persistence.
* Cloudinary image storage.
* Brevo email notifications.
* Secure authentication.
* Role-based authorization.
* Complaint ownership protection.
* Responsive UI.
* PWA installation support.

This constitutes the baseline **Version 1.0 system design**. Future functionality such as advanced analytics, multiple departments, escalation workflows, chat, exports, and AI classification can be added without changing the fundamental complaint lifecycle.
