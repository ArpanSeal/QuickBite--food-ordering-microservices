# QuickBite 🍔
### Food Ordering Platform using ASP.NET Core Microservices

QuickBite is a distributed food ordering platform built using ASP.NET Core microservices architecture.  
The system demonstrates scalable backend design using multiple independent services communicating via REST APIs and Azure Service Bus.

---

## Features

- User authentication & authorization using JWT
- Microservices-based architecture
- Centralized API routing using Ocelot API Gateway
- Asynchronous communication using Azure Service Bus
- Secure Stripe payment integration
- Global exception handling middleware
- Cloud media storage using Azure Blob Storage
- Food browsing and ordering workflow
- Shopping cart management
- Order processing pipeline

---

## Architecture

This project follows a distributed microservices architecture with independent deployable services.

### Services Included

- Authentication Service
- Product Service
- Cart Service
- Coupon Service
- Order Service
- Reward Service

---

## Architecture Diagram
<img width="1681" height="935" alt="ChatGPT Image May 22, 2026, 01_15_23 AM" src="https://github.com/user-attachments/assets/21c70bcd-69b5-435a-bc17-f88954ed53ba" />

---

## Tech Stack

### Backend
- ASP.NET Core Web API
- ASP.NET Core MVC
- Microservices Architecture
- Entity Framework Core
- SQL Server
- Ocelot API Gateway
- Azure Service Bus
- Azure Blob Storage
- Stripe Payment Gateway
- JWT Authentication
- Custom Middleware

### Frontend
- ASP.NET Core MVC

### Cloud / Messaging
- Azure Service Bus
- Azure Blob Storage

---

## System Flow

1. User registers/logs in via Authentication Service
2. JWT token is generated
3. Requests flow through Ocelot API Gateway
4. Product service manages food catalog
5. Cart service handles cart operations
6. Coupon service applies discounts
7. Payment service integrates Stripe checkout
8. Order service processes final orders
9. Azure Service Bus handles async communication

---

## Screenshots

### Home Page
<img width="1882" height="902" alt="image" src="https://github.com/user-attachments/assets/6816a2cd-649e-46e0-8cbc-8cf9fa4a1238" />

### Login
<img width="1897" height="880" alt="image" src="https://github.com/user-attachments/assets/8db75a21-4345-46e0-b256-bff3f720dffd" />

### Cart
<img width="1886" height="907" alt="image" src="https://github.com/user-attachments/assets/d7b99007-2591-4365-a1b2-51a1615a170b" />
<img width="1902" height="702" alt="image" src="https://github.com/user-attachments/assets/178530b5-83e2-4318-b8ce-42edc62d3154" />

### Payment
<img width="998" height="842" alt="image" src="https://github.com/user-attachments/assets/411c30fc-0358-48c8-b7d8-de366c966bfe" />

### Manage Order
<img width="1888" height="848" alt="image" src="https://github.com/user-attachments/assets/fbb800f1-088b-4c19-8576-f696f4fb256f" />

---

## Project Structure

```text
QuickBite
│
├── QuickBite.AuthAPI
├── QuickBite.ProductAPI
├── QuickBite.CartAPI
├── QuickBite.CouponAPI
├── QuickBite.OrderAPI
├── QuickBite.RewardsAPI
├── QuickBite.Web
├── Ocelot Gateway
```

---

## Setup Instructions

### Clone Repository

```bash
git clone https://github.com/yourusername/QuickBite--food-ordering-microservices.git
```

---

### Backend Setup

```bash
dotnet restore
dotnet build
```

---

### Run Services

Example:

```bash
dotnet run --project QuickBite.AuthAPI
dotnet run --project QuickBite.ProductAPI
dotnet run --project QuickBite.CartAPI
dotnet run --project QuickBite.CouponAPI
dotnet run --project QuickBite.OrderAPI
dotnet run --project QuickBite.RewardsAPI
```

---

### Run MVC Frontend

```bash
dotnet run --project QuickBite.Web
```

---

## Future Enhancements

- Serching by food name
- Docker containerization
- Redis distributed caching
- Monitoring with Application Insights
