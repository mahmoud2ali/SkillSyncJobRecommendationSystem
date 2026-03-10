# SkillSync Backend API

This is the backend service for the **SkillSync Job Recommendation System**.

It is responsible for:

- User authentication
- CV upload
- Storing CV data
- Managing user information
- Communicating with the AI service

---

# Tech Stack

- ASP.NET Web API
- C#
- SQL Server
- REST API

---

# Backend Responsibilities

The backend handles the core application logic.

### Features

- User Registration
- User Login
- Upload CV
- Store CV in database
- Call AI service to compute embeddings
- Retrieve job recommendations

---

# Database Design

The system stores uploaded CV files in SQL Server.

Example structure:

```
Users
-----
Id
Email
PasswordHash

UserCV
------
Id
UserId
CvFile VARBINARY(MAX)
Embedding VECTOR / VARBINARY(MAX)
```

---

# CV Upload Process

1. User uploads CV
2. Backend receives the file
3. The file is encoded and stored in SQL Server as:

```
VARBINARY(MAX)
```

---

# Embedding Computation

The backend exposes an endpoint that:

1. Retrieves the CV from the database
2. Sends the text to the **AI service**
3. Receives the embedding
4. Stores the embedding in the database

---

# Recommendation Flow

When a recommendation request is made:

1. Retrieve user embedding
2. Send embedding to AI service
3. AI service calculates cosine similarity
4. Return Top 3 job recommendations

---

# API Endpoints

The system exposes APIs through two services:

- **Backend API (.NET)** → authentication, CV storage
- **AI Service (Flask)** → embeddings and recommendations

---

# Backend API

Base URL

```
http://localhost:5143
```

---

# Authentication

## Register User

```
POST /api/Account/register
```

Request

```json
{
  "UserName": "MahmoudMohamed",
  "Email": "mahmoud@gmail.com",
  "Password": "ab1234!Z"
}
```

---

## Login

```
POST /api/Account/login
```

Request

```json
{
  "UserName": "MahmoudMohamed",
  "Password": "ab1234!Z"
}
```

Response

```
JWT Token
```

The token must be included in the header for protected endpoints.

```
Authorization: Bearer <token>
```

---

# Document Management

## Upload CV

```
POST /api/Documents/upload
```

Headers

```
Authorization: Bearer <token>
```

Description

Uploads a user CV and stores it in SQL Server as binary data.

---

## Delete CV

```
DELETE /api/Documents/{id}
```

Example

```
DELETE /api/Documents/27
```

Headers

```
Authorization: Bearer <token>
```

---

# Recommendation API

## Compute CV Embeddings

```
GET /api/Recommendation/Embeddings/{documentId}
```

Example

```
GET /api/Recommendation/Embeddings/28
```

Headers

```
Authorization: Bearer <token>
```

Description

1. Retrieves the CV from the database
2. Sends CV text to the AI service
3. Computes TF-IDF embedding
4. Stores embedding in the database

---

## Get Top Job Recommendations

```
GET /api/Recommendation/TopJobs/{documentId}
```

Example

```
GET /api/Recommendation/TopJobs/27
```

Headers

```
Authorization: Bearer <token>
```

Description

Returns the top 3 job recommendations for the given CV.

---

# AI Service API

Base URL

```
http://127.0.0.1:5000
```

---

# Generate Embeddings

```
POST /get_embeddings
```

Request

```json
{
  "cvData": "Full CV text"
}
```

Response

```json
{
  "embedding": [0.1, 0.02, 0.5, ...]
}
```

Description

Converts CV text into a TF-IDF embedding vector.

---

# Get Top Jobs

```
POST /get_top_jobs
```

Request

```json
{
  "embeddings": [0, 0, 0, 0, ...]
}
```

Response

```json
{
  {
    "top_jobs": [
        {
            "Job Description": "",
            "Job Title": "",
            "similarity": 
        },
        {
            "Job Description": "",
            "Job Title": "",
            "similarity":
        },
        {
            "Job Description": "",
            "Job Title": "",
            "similarity": 
        }
    ]
}
}
```

Description

1. Receives user embedding
2. Computes cosine similarity with job embeddings
3. Returns the top 3 most similar jobs.

---

# Request Flow Example

Example end-to-end workflow:

```
1 Register
2 Login
3 Upload CV
4 Compute Embeddings
5 Get Job Recommendations
```

---

# Authentication

Protected endpoints require JWT token.

Example header

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```


# Configuration

Before running the backend, you must configure the `appsettings.json` file.

Open:

```
backend/appsettings.json
```

Update the following fields.

---

# Database Configuration

Set the SQL Server connection string.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SkillSyncDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

You can modify the server and database name depending on your environment.

Example alternatives:

```
Server=.;Database=SkillSyncDB;Trusted_Connection=True;
```

or

```
Server=localhost;Database=SkillSyncDB;User Id=sa;Password=yourpassword;
```

---

# JWT Configuration

The system uses **JWT authentication** for securing API endpoints.

Update the JWT settings:

```json
"JWT": {
  "SecretKey": "your-super-secret-key",
  "Issuer": "SkillSyncAPI",
  "Audience": "SkillSyncUsers"
}
```

### Description

| Field | Description |
|-----|-----|
| SecretKey | Used to sign JWT tokens |
| Issuer | The application issuing the token |
| Audience | The intended users of the token |

Example SecretKey:

```
this_is_a_super_secret_key_for_skillsync_project
```

---

# Final Example `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SkillSyncDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JWT": {
    "SecretKey": "this_is_a_super_secret_key_for_skillsync_project",
    "Issuer": "SkillSyncAPI",
    "Audience": "SkillSyncUsers"
  }
}
```
