# RaceDay API Endpoint Plan

This document outlines the RESTful API endpoints for the RaceDay system. It enforces role-based access control (Organiser vs. Participant) as required by the system specifications.

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **AUTHENTICATION** | | | | | |
| POST | `/api/auth/register` | Registers a new user (Participant or Organiser) in the system. | None (Public) | `{ "FirstName": "", "LastName": "", "Email": "", "Password": "", "Role": "" }` | **201 Created**: User successfully registered.<br>**400 Bad Request**: Invalid data. |
| POST | `/api/auth/login` | Authenticates a user and generates a JWT token for session management. | None (Public) | `{ "Email": "", "Password": "" }` | **200 OK**: Login successful, returns JWT token.<br>**401 Unauthorized**: Invalid credentials. |
| **USER PROFILE** | | | | | |
| GET | `/api/users/profile` | Retrieves the profile details of the currently logged-in user. | Any (Logged in) | None | **200 OK**: Returns user details.<br>**401 Unauthorized**: Missing/invalid token. |
| PUT | `/api/users/profile` | Updates the logged-in user's profile details. | Any (Logged in) | `{ "FirstName": "", "LastName": "" }` | **200 OK**: Profile updated successfully. |
| POST | `/api/users/profile/picture` | Saves the logged-in user's profile picture through the API into the local SQL database. | Any (Logged in) | `multipart/form-data` image file | **200 OK**: Returns the updated profile and image URL. |
| **CATEGORIES** | | | | | |
| GET | `/api/categories` | Retrieves a list of all available event categories. | Any (Logged in) | None | **200 OK**: JSON array of categories. |
| POST | `/api/categories` | Creates a new category type (e.g., Ultra Marathon, 10km Run). | Organiser | `{ "CategoryName": "", "Description": "" }` | **201 Created**: Category added successfully.<br>**403 Forbidden**: Not an organiser. |
| **EVENTS** | | | | | |
| GET | `/api/events` | Retrieves a list of all upcoming events. | Any (Logged in) | None | **200 OK**: JSON array of events. |
| POST | `/api/events` | Creates a new event. | Organiser | `{ "EventName": "", "Description": "", "EventDate": "", "Location": "", "DistanceKm": 10, "EventType": "Run" }` | **201 Created**: Event created successfully.<br>**403 Forbidden**: Access denied. |
| PUT | `/api/events/{id}` | Edits an existing event's details. | Organiser | `{ "EventName": "", "EventDate": "", "Location": "", "DistanceKm": 10, "EventType": "Run", "Status": "" }` | **200 OK**: Event updated.<br>**404 Not Found**: Event ID does not exist. |
| DELETE | `/api/events/{id}` | Deletes an event. | Organiser | None | **204 No Content**: Event deleted successfully. |
| POST | `/api/events/{id}/categories` | Links a category to an event with specific fees and capacities. | Organiser | `{ "CategoryID": "", "EntryFee": 0.00, "MaxParticipants": 100 }` | **201 Created**: Category added to event. |
| POST | `/api/events/{id}/banner` | Saves or replaces an event banner through the API in the local SQL database. | Organiser (event owner) | `multipart/form-data` image file | **200 OK**: Returns the event and banner image URL. |
| GET | `/api/events/{id}/enrolments` | Views all participants enrolled in an organiser's event. | Organiser | None | **200 OK**: Enrolment list.<br>**403 Forbidden**: Event belongs to another organiser. |
| **EVENT ENROLMENTS** | | | | | |
| POST | `/api/event-categories/{id}/enrol` | Enters a participant into a specific event category. | Participant | None (Uses token for UserID) | **201 Created**: Enrolment successful.<br>**409 Conflict**: Already enrolled / Event full. |
| GET | `/api/enrolments/my-enrolments` | Views a list of the logged-in participant's enrolments. | Participant | None | **200 OK**: JSON array of enrolments. |
| **RESULTS** | | | | | |
| POST | `/api/enrolments/{id}/result` | Captures a participant's result for an enrolment. | Organiser | `{ "FinishTime": "HH:MM:SS", "Position": 1, "ResultStatus": "Finished" }` | **201 Created**: Result captured successfully.<br>**404 Not Found**: Enrolment not found. |
| GET | `/api/results/my-results` | Tracks and views the logged-in participant's personal results history. | Participant | None | **200 OK**: JSON array of personal results. |
| GET | `/api/images/{container}/{blobName}` | Retrieves an event banner or profile picture from the local SQL database through the API. | Public image URL | None | **200 OK**: Image content. **404 Not Found**: Image not found. |
