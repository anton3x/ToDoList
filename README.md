# ToDoList

ToDoList is a comprehensive task management application that allows users to register, login, create tasks, and organize them into custom lists. This application helps users stay organized and productive by providing features like task reminders and periodic tasks.

## Features

- **User Registration and Login:**
  - Secure user registration and login functionality.
  - Profile management with support for profile pictures.

- **Task Management:**
  - Create, edit, and delete tasks.
  - Detailed task properties including title, description, start date, end date, start time, end time, importance level, and state (e.g., not started, in progress, completed).
  - Set tasks to repeat on specific days of the week.

- **Custom Lists:**
  - Create custom lists to categorize and organize tasks.
  - Custom navigation items to help manage different projects or priorities.

- **Alerts and Notifications:**
  - Set up task alerts to remind users of upcoming tasks or deadlines via email or system notifications.
  - Support for different types of alerts like anticipation or execution alerts.

- **Data Persistence:**
  - Uses SQL Server for data storage ensuring data is persisted across sessions.

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/todolist.git
   cd todolist```

2. Set up the database:
  Ensure you have SQL Server installed and running.
  Create a database and run the provided SQL scripts to set up the necessary tables.
  Update the connection string:

3. In App.config or Web.config, update the connection string to point to your SQL Server database.
  Build the application:

4. Open the project in Visual Studio.
  Build the solution to restore the necessary packages and compile the code.
