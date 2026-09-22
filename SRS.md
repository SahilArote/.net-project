# College Complaint Management System

## Software Requirements Specification — Version 1.0 (Draft)

### 1. Project Purpose

The College Complaint Management System is a web-based application that allows college students to submit complaints regarding college facilities, infrastructure, laboratories, administration, and other college-related issues.

The system provides an administrative interface through which a designated administrator can review, resolve, and close complaints.

The application will be responsive and support installation as a Progressive Web App (PWA).

### 2. User Roles

#### 2.1 Student

Students can:

* Register using their college ID and college email address.
* Verify their email address.
* Log in and log out.
* View their dashboard.
* Submit complaints.
* Upload optional complaint images.
* View their own complaints.
* Edit or delete complaints while their status is Submitted.
* View the current status of their complaints.

#### 2.2 Administrator

The administrator can:

* Log in securely.
* View all complaints.
* Search and filter complaints.
* Review complaint details and attached images.
* Change complaint statuses.
* Resolve and close complaints.
* View registered students.
* Manage complaint categories.
* View complaint statistics.

### 3. Functional Requirements

#### FR-01: Student Registration

The system shall allow students to register using their college ID, name, college email address, and password.

#### FR-02: Email Verification

The system shall verify the student's email address before enabling normal account access.

#### FR-03: Authentication

The system shall provide secure login and logout functionality for students and administrators.

#### FR-04: Complaint Submission

The system shall allow authenticated students to submit complaints containing:

* Title
* Description
* Category
* Location
* Optional image attachment

#### FR-05: Complaint Reference

The system shall generate a unique reference number for each complaint.

#### FR-06: Complaint Ownership

Students shall be able to access only their own complaints.

#### FR-07: Complaint Editing

Students shall be allowed to edit their complaints only while the complaint status is Submitted.

#### FR-08: Complaint Deletion

Students shall be allowed to delete their complaints only while the complaint status is Submitted.

#### FR-09: Complaint Review

The administrator shall be able to review complaints and change their status to Under Review.

#### FR-10: Complaint Resolution

The administrator shall be able to mark complaints as Resolved.

#### FR-11: Complaint Closure

The administrator shall be able to close resolved complaints.

#### FR-12: Status Tracking

Students shall be able to view the current status of their complaints.

#### FR-13: Email Notifications

The system shall send email verification messages and complaint status-change notifications.

#### FR-14: Image Storage

The system shall upload complaint images to Cloudinary and store the relevant image references in the database.

#### FR-15: Administration

The administrator shall be able to manage complaint categories and view registered student information.

#### FR-16: Dashboard

The system shall provide student and administrator dashboards with information appropriate to each role.

### 4. Complaint Statuses

The system shall support the following statuses:

1. Submitted
2. Under Review
3. Resolved
4. Closed

The application shall enforce valid status transitions.

### 5. Non-Functional Requirements

#### Security

* Passwords shall be securely hashed.
* Role-based authorization shall protect administrative functionality.
* Students shall not access other students' complaint records.
* Server-side validation shall be applied to user inputs and uploaded files.
* Secrets shall not be hardcoded into source code.

#### Performance

* Pages should load efficiently under normal college usage.
* Database queries should be appropriately filtered and indexed.
* Images should be optimized for web delivery where appropriate.

#### Usability

* The interface shall be simple and easy to navigate.
* Forms shall provide understandable validation messages.
* The application shall support desktop, tablet, and mobile screens.

#### Reliability

* The system shall handle database and image-upload failures gracefully.
* Complaint status changes shall be persisted reliably.
* The application shall avoid reporting successful submissions when persistence fails.

#### PWA

* The application shall provide a web app manifest and service worker.
* The application shall support installation on compatible devices.
* Essential interface assets may be cached.
* Complaint submission and authentication shall require an appropriate server connection.

### 6. Proposed Technology Stack

* Backend: ASP.NET Core MVC
* Frontend: Razor Views, HTML, CSS, JavaScript
* Database: Microsoft SQL Server
* ORM: Entity Framework Core
* Authentication: ASP.NET Core Identity
* Image storage: Cloudinary
* PWA: Web app manifest and service worker

### 7. Out of Scope for Initial Release

Unless subsequently approved, the initial release will not include:

* Multiple administrative departments
* Complex complaint escalation
* In-app chat
* Student-to-student complaint visibility
* Mandatory resolution notes
* Mobile applications separate from the PWA
* Advanced analytics or AI-based complaint classification

### 8. Outstanding Decisions

The following items require confirmation before implementation:

* College ID validation source
* Email delivery provider
* Final complaint category list
* Exact location input rules
* Image upload size and format restrictions
* Administrator account creation process
* Final visual design and color palette
 