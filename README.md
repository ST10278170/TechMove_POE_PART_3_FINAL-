YouTube Video Link - 


TechMove – POE Part 3

This repository contains the final implementation for TechMove, a multi‑project .NET solution developed for POE Part 3. The system includes an MVC front‑end, a Web API, and a full automated testing suite. The goal of the project is to demonstrate clean architecture, API integration, automated testing, and DevOps‑ready structure.

Project Structure
The solution is split into three main projects:

1. TechMove.GLMS (MVC Application)
This is the main front‑end of the system.
It handles:

User interface
Authentication (via API)
CRUD operations for Clients, Contracts, Service Requests, and more
Validation and error handling
Session‑based JWT storage
The MVC app communicates directly with the API using HttpClientFactory.

2. TechMove.GLMS.API (Web API)
This is the backend service that exposes all business logic and data operations.

It includes:
RESTful endpoints
Entity models
DTOs
Authentication endpoints
CRUD operations for all entities
File handling for contract documents
The API is designed to be clean, modular, and easy to integrate with the MVC front‑end.

3. TechMove.GLMS.Tests (Automated Tests)
This project contains the automated test suite for the system.

It includes:
Controller tests
API integration tests
Mocked HttpClient tests
Validation tests
Edge‑case handling

The tests help ensure that changes do not break existing functionality and support a CI/CD workflow.
Technologies Used
.NET 8
ASP.NET MVC
ASP.NET Web API
MSTest
Moq
Newtonsoft.Json
HttpClientFactory
Session‑based JWT authentication

How to Run the Solution
Open the solution in Visual Studio 2022.
Set TechMove.GLMS.API as the startup project and run it first.
Then run TechMove.GLMS (MVC).
Ensure both projects are running on different ports.
The MVC app will communicate with the API automatically.

Testing
To run the automated tests:
Open Test Explorer in Visual Studio.
Build the solution.
Run all tests.
The tests cover:

API responses
MVC controller behavior
Error handling
CRUD operations
Authentication flows

Purpose of This Repository
This repository is the final submission for POE Part 3, demonstrating:
Multi‑project architecture
API + MVC integration
Automated testing

Clean GitHub structure

DevOps‑ready layout
