# College Complaint Management System (.NET 10 MVC)

A modern, robust College Complaint Management System built with ASP.NET Core MVC (.NET 10), Entity Framework Core, ASP.NET Core Identity, and Tailwind CSS.

## 📌 Features

- **Authentication & Role-Based Access Control**:
  - Secure student registration with institutional email verification.
  - Role-based authorization for **Students** and **Administrators**.
  - Password reset with email verification tokens.
- **Complaint Management**:
  - Lodge complaints with category classification, title, detailed description, and image attachments.
  - Live status tracking: `Submitted`, `UnderReview`, `InProgress`, `Resolved`, `Rejected`.
  - Filter and search complaints by status, category, and date.
- **Admin Dashboard**:
  - Analytics & summary metrics (total, pending, in progress, resolved).
  - Review, update status, and add official admin remarks.
  - Student and category management.
- **Cloudinary Integration**:
  - Cloud storage for complaint photo attachments with local fallback.
- **Email Notifications**:
  - Brevo (Sendinblue) transactional email API integration with simulated development mode fallback.
- **Offline & PWA Support**:
  - Service Worker and Web App Manifest for progressive web app capabilities.

## 🛠️ Tech Stack

- **Framework**: .NET 10 (ASP.NET Core MVC)
- **Database**: SQLite (Development) / SQL Server (Production)
- **ORM**: Entity Framework Core 10
- **Security**: ASP.NET Core Identity, Anti-CSRF protection, Security Headers
- **Cloud / APIs**: Cloudinary (Image storage), Brevo (Transactional Email)
- **Frontend**: Tailwind CSS, Vanilla CSS, Vanilla JavaScript, jQuery Validation

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2026 / VS Code / JetBrains Rider

### Setup Instructions

1. **Clone the repository**:
   ```bash
   git clone https://github.com/SahilArote/.net-project.git
   cd .net-project
   ```

2. **Configure Settings**:
   Add your development keys in `appsettings.Development.json` or using `dotnet user-secrets`:
   ```json
   {
     "Cloudinary": {
       "CloudName": "YOUR_CLOUD_NAME",
       "ApiKey": "YOUR_API_KEY",
       "ApiSecret": "YOUR_API_SECRET"
     },
     "Brevo": {
       "ApiKey": "YOUR_BREVO_API_KEY",
       "SenderEmail": "your-email@college.edu",
       "SenderName": "College Complaint Portal"
     }
   }
   ```

3. **Run the project**:
   ```bash
   dotnet run --project CollegeComplaintSystem.Web
   ```

4. **Access the application**:
   Open [http://localhost:5207](http://localhost:5207) in your browser.

## 📄 License

This project is licensed under the MIT License.
