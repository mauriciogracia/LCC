# Livefront Coding Challenge

This repository contains a Referral endpoint that allows to
- GetUserReferralCode
- IsValidReferralCode
- GetUserReferrals
- AddReferral
- PrepareMessage
- GetReferral

Those are NOT the name of the endpoints, but the functional names

## SETUP

Requirements - TODO MGG
- dotnet version X.Y
- restore packages
- run 

## OVERVIEW
There are two projects
- **LCC** - is where the actual endpoint logic lives
- **LCC-Tests** - unit tests for the services


## LCC - Tech Notes

- A **UserService** could have been implemented specially to get the ReferralCode everything kept on the same controller to simply implementation and code review
- Logging has been implemented with usage of **Ilog** interface and a concrete **ConsoleLogger**
- **ReferralService** keeps the referral data in memory to simplify implementation and focus on the actual API, than on storying the data in a database or other persistence
- Since **Dependency injection** is being used and the controller depends on **IReferralFeatures** 
this means that another Service could be implemented and injected to actually store and retrieve referrals on a SQL Database or NoSQL database
- That also means that another type of logger could be used (PlainTextLogger, EmailLogger, DatabaseLogger, CloudLogger)