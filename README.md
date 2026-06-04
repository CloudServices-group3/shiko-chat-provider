# Shiko Chat Provider API

Microservice responsible for managing a general live chat. Acts as a bridge between the Next.js frontend and Azure Communication Services (ACS), handling user identity creation and token generation for chat access. This service is deployed to Azure and runs as part of the Shiko microservices architecture. 

**API Documentation (Scalar):** [https://azure-chat-webapp-crf4ded2dzf0b5d0.swedencentral-01.azurewebsites.net/scalar](https://azure-chat-webapp-crf4ded2dzf0b5d0.swedencentral-01.azurewebsites.net/scalar)

## 🚀 Features


- **Global Chat Room**: A single general chat room shared by all users, no course-specific rooms or database required.
- **Token Management**: Generates unique ACS identities and time-limited access tokens for each user session.
- **Real-time Messaging**: Supports real-time notifications via Azure Communication Services.


## 🛠️ Technologies


- C# .NET 10 (ASP.NET Core Minimal API)
- Azure Communication Services (ACS) – Chat and Identity
- `Azure.Communication.Chat` – Chat client
- `Azure.Communication.Identity` – Identity and token management


## 🔗 Related Services


- **Shiko Auth API** - https://github.com/CloudServices-group3/shiko-auth-api – Handles authentication and provides the JWT used to authorize chat requests
- **Shiko Frontend** - https://github.com/CloudServices-group3/shiko-frontend – Next.js frontend that connects to the chat using the ACS token returned by this API

