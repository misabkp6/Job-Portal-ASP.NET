# JobPortal - ASP.NET Core MVC Job Application System

A comprehensive job portal web application built with ASP.NET Core MVC, featuring role-based authentication and application tracking capabilities.

## 🚀 Features

### For Job Seekers (Applicants)
- **User Registration & Authentication** - Secure account creation and login
- **Job Search & Filtering** - Browse and search available job listings
- **Job Application** - Apply for jobs with resume upload
- **Application Tracking** - View application status and history
- **Profile Management** - Manage personal information and preferences

### For Employers
- **Employer Dashboard** - Centralized management interface
- **Job Posting** - Create and manage job listings
- **Application Management** - View and review job applications
- **Application Tracking** - Track application views and responses
- **Candidate Management** - Review applicant profiles and resumes

### For Administrators
- **Admin Dashboard** - System-wide management and analytics
- **User Management** - Manage all user accounts and roles
- **Job Management** - Oversee all job postings across the platform
- **Application Oversight** - Monitor all job applications and activities
- **System Analytics** - View platform usage and statistics

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core MVC 8.0
- **Language**: C# with .NET 8.0
- **Database**: SQL Server LocalDB
- **ORM**: Entity Framework Core 8.0
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, HTML5, CSS3, JavaScript
- **Icons**: Font Awesome
- **Pagination**: X.PagedList

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- [SQL Server Express LocalDB](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)

## 🔧 Installation & Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/misabkp6/Job-Portal-ASP.NET.git
   cd Job-Portal-ASP.NET
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection string** (if needed)
   - Open `appsettings.json`
   - Modify the `DefaultConnection` string if required

4. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Build the project**
   ```bash
   dotnet build
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

7. **Access the application**
   - Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`

## 🏗️ Project Structure

```
JobPortal/
├── Areas/Identity/          # ASP.NET Core Identity pages
├── Controllers/             # MVC Controllers
│   ├── AdminController.cs
│   ├── ApplicationController.cs
│   ├── EmployerController.cs
│   ├── HomeController.cs
│   └── JobController.cs
├── Data/                    # Database context
│   └── ApplicationDbContext.cs
├── Migrations/              # Entity Framework migrations
├── Models/                  # Data models and ViewModels
│   ├── Application.cs
│   ├── CompanyProfile.cs
│   ├── Jobs.cs
│   ├── JobSearchViewModel.cs
│   └── Enums/
├── Views/                   # Razor views
│   ├── Admin/
│   ├── Application/
│   ├── Employer/
│   ├── Home/
│   ├── Job/
│   └── Shared/
└── wwwroot/                 # Static files (CSS, JS, images)
```

## 🎯 Key Features Implementation

### Role-Based Access Control
- **Admin**: Full system access and management capabilities
- **Employer**: Job posting and application management
- **Applicant**: Job searching and application submission

### Application Tracking System
- Real-time application status updates
- Employer view notifications for applicants
- Application timeline and history tracking

### Resume Management
- Secure file upload and storage
- Resume viewing capabilities for employers
- File type validation and security measures

## 🔐 Default User Roles

The system supports three main user roles:
- **Admin** - System administrators
- **Employer** - Company representatives who post jobs
- **Applicant** - Job seekers who apply for positions

## 📊 Database Schema

### Core Entities
- **Users** - ASP.NET Core Identity users with role assignments
- **Jobs** - Job postings with details and requirements
- **Applications** - Job applications linking users to jobs
- **CompanyProfile** - Employer company information (optional)

### Key Relationships
- Users can have multiple Applications
- Jobs can have multiple Applications
- Applications track view status and timestamps

## 🚀 Deployment

### Local Deployment
The application is configured for local development with LocalDB. For production deployment:

1. Update connection strings for your production database
2. Configure appropriate security settings
3. Set up file storage for resumes
4. Configure email services (if implemented)

### Azure Deployment
The application can be deployed to Azure App Service with SQL Database.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is open source and available under the [MIT License](LICENSE).

## 🛠️ Future Enhancements

- [ ] Email notifications for application updates
- [ ] Advanced search filters and sorting
- [ ] Company profile management
- [ ] Interview scheduling system
- [ ] Real-time chat between employers and applicants
- [ ] Integration with external job boards
- [ ] Analytics dashboard with charts and reports
- [ ] Mobile application development

## 📞 Support

If you encounter any issues or have questions, please:
1. Check the existing issues on GitHub
2. Create a new issue with detailed description
3. Contact the maintainer: [misabkp6](https://github.com/misabkp6)

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Bootstrap team for the responsive UI framework
- Font Awesome for the icon library
- All contributors and users of this project

---

**Built with ❤️ using ASP.NET Core MVC**
