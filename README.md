# 🎫 Ticket Management System (TMS)

A complete **Ticket Management System** built using **ASP.NET MVC**, designed to manage tickets, employees, clients, and notifications efficiently.

---

## 🚀 Features

* ✅ User Authentication (Login / Logout)
* 🎫 Ticket Creation & Management
* 👨‍💼 Employee Management
* 🏢 Client Management
* 🔔 Real-time Notifications (SignalR)
* 📊 Dashboard with statistics
* 📁 Ticket Backup & Restore
* 📥 Excel Import Support
* 📄 PDF Export Support

---

## 🛠️ Tech Stack

* **Backend:** ASP.NET MVC (.NET)
* **Frontend:** Razor Views, Bootstrap
* **Database:** SQL Server
* **ORM:** Entity Framework
* **Real-time:** SignalR
* **Libraries:**

  * ClosedXML (Excel)
  * iText7 (PDF)

---

## 📂 Project Structure

```
TMS201/
│── Controllers/
│── Models/
│── Views/
│── Data/
│── Services/
│── Hubs/
│── wwwroot/
│── appsettings.json
│── Program.cs
```

---

## ⚙️ Setup Instructions

### 1️⃣ Clone the repository

```
git clone https://github.com/HelloUser-ai/Ticket-Management-System.git
```

### 2️⃣ Navigate to project

```
cd TMS201
```

### 3️⃣ Restore packages

```
dotnet restore
```

### 4️⃣ Run the project

```
dotnet run
```

---

## 🗄️ Database Setup

* Update your **connection string** in:

  ```
  appsettings.json
  ```
* Run migrations:

  ```
  dotnet ef database update
  ```

---

## 📸 Screenshots

> Add screenshots here (Dashboard, Ticket Page, etc.)

---

## 🤝 Contributing

Contributions are welcome!
Feel free to fork the repo and submit a pull request.

---

## 📜 License

This project is licensed under the **MIT License**.

---

## 👨‍💻 Author

**HelloUser-ai**

---

## ⭐ Support

If you like this project, give it a ⭐ on GitHub!
