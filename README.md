🍽️ Restaurant Management System (MVC Core)

A full-stack web application built using ASP.NET Core MVC 8 that simulates a restaurant ordering and management platform similar to Talabat. The system supports multiple roles, restaurant browsing, menu management, reservations, and user interactions.

🚀 Features
👤 User Features
Register & login system
Browse restaurants and menus
View restaurant details
Add dishes and restaurants to favorites
Rate dishes
Make reservations
🧑‍💼 Admin Features
Manage restaurants (Add / Edit / Delete)
Manage menu items
Add or manage restaurant managers
Full control over system data
🏪 Restaurant Manager Features
Manage restaurant profile
Handle reservations
Manage menu items
Track customer activity
🏗️ Tech Stack
ASP.NET Core MVC 8
C#
Entity Framework Core
SQL Server
Identity Authentication
LINQ
Bootstrap (UI)
HTML / CSS / JavaScript
📁 Project Structure
Restaurant/
│
├── Controllers/
├── Models/
├── Views/
├── Data/
├── Services/
├── wwwroot/
└── appsettings.json
🧠 Key Concepts Used
Clean Architecture (MVC Pattern)
Role-Based Authorization
Database Relationships (One-to-Many, Many-to-Many)
CRUD Operations
Authentication & Authorization (ASP.NET Identity)
Dependency Injection
LINQ Queries
🛠️ Setup Instructions
1. Clone the repository
git clone https://github.com/your-username/restaurant-mvc.git
2. Open project

Open solution in Visual Studio 2022

3. Configure database

Update connection string in:

appsettings.json
4. Run migrations
dotnet ef database update
5. Run project

Press F5 or:

dotnet run
📸 Screenshots (Optional)

Add screenshots of:

Home page
Restaurant list
Menu page
Admin dashboard
🎯 Future Improvements
Online payment integration
Real-time chat between users and restaurants
Mobile app (Flutter integration)
Advanced recommendation system
👨‍💻 Author

Mahmoud Saber Attia

GitHub: github.com/mahmoudssaber
LinkedIn: www.linkedin.com/in/mahmoud-saber-7a11a9200
