# Inventory Management System

A role-based web application built with ASP.NET Core MVC (.NET 8) that helps businesses manage their inventory efficiently. The system supports product management, sales and restock recording, low-stock alerts, and advanced search functionality.

## Technologies Used
- ASP.NET Core MVC (.NET 8)
- C#
- Entity Framework Core
- ASP.NET Core Identity
- Microsoft SQL Server
- HTML, CSS, Bootstrap

## Roles & Features

### Manager
- Manage products
- Record sales and restocks
- Generate reports
- Receive low-stock notifications when a product quantity falls below threshold
- Advanced search functionality

### Staff
- Record sales and restocks
- Advanced search functionality

## Advanced Search
Both managers and staff can search for any item using:
- Keywords
- Category
- Sale or restock dates

Search results display the item alongside its full sales and restock transaction logs.

## Low-Stock Notifications
The system automatically notifies the manager when any product's quantity drops below a defined threshold, ensuring timely restocking.
