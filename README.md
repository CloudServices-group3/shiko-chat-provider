# Shiko Chat Provider API

Microservice designed to manage group chats for courses. It acts as a bridge between the internal database and Azure Communication Services (ACS).


🚀 Features


Lazy Creation: Automatically creates chat rooms in Azure the first time a specific course is requested.

Token Management: Generates unique identities and time-limited Access Tokens for users.

Course Mapping: Maps Azure Thread IDs to specific CourseIds within the SQL database

