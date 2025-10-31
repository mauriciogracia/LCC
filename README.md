# Livefront Coding Challenge

This repository contains a Referral endpoint that allows to
- GetUserReferralCode
- IsValidReferralCode
- GetUserReferrals
- AddReferral
- PrepareMessage
- GetReferral
- UpdateReferral

Those are NOT the name of the endpoints, but the functional names, when you run the project in development mode you will get the actual endpoint details

## SETUP

Requirements
- .NET 8.0

Clone this repo
- git clone https://github.com/mauriciogracia/LCC.git

Go to repo folder
- cd LLC (or the folder where you clone it)

Restore packages and build
- dotnet restore
- dotnet build

To run tests 
- dotnet test
- dotnet run --project LCC/LCC.csproj

Open 
- http://localhost:5097/swagger/index.html

## OVERVIEW
There are two projects
- **LCC** - is where the actual endpoint logic lives (controller, services, models)
- **LCC-Tests** - unit tests for the services


## LCC - Tech Notes

- Async/Await and Try/Catch with logger implementation

- Idiomatica RESTful API that covers all the logic needed by the mobile application

- A **UserService** could have been implemented specially to get the ReferralCode everything kept on the same controller to simply implementation and code review
- Logging has been implemented with usage of **Ilog** interface and a concrete **ConsoleLogger**
- **ReferralService** keeps the referral data in memory to simplify implementation and focus on the actual API, than on storying the data in a database or other persistence
- Since **Dependency injection** is being used and the controller depends on **IReferralFeatures** 
this means that another Service could be implemented and injected to actually store and retrieve referrals on a SQL Database or NoSQL database
- That also means that another type of logger could be used (PlainTextLogger, EmailLogger, DatabaseLogger, CloudLogger)