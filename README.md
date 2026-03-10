# SkillSync – AI Job Recommendation System

SkillSync is a job recommendation system that analyzes a user's CV and recommends the most relevant jobs using Natural Language Processing (NLP).

The system is built using a **microservice-style architecture** consisting of:

- **.NET Backend API**
- **Python AI Service (Flask)**
- **SQL Server Database**

The AI service processes CV text and compares it against job descriptions using **TF-IDF embeddings and cosine similarity** to recommend the most relevant jobs.

---

# System Architecture

```
User
 |
 | Register / Login
 v
Backend API (.NET)
 |
 | Upload CV
 v
SQL Server Database
 |
 | Retrieve CV
 v
AI Service (Flask)
 |
 | Compute embeddings
 | Cosine similarity
 v
Top 3 Job Recommendations
 |
 v
JSON Response
```

---

# Project Structure

```
SkillSync
│
├── backend
│   ├── backend.slnx
│   └── backend/
│
├── AI-service
│   ├── app.py
│   ├── tfidf_vectorizer.pkl
│   ├── tfidf_matrix.pkl
│   ├── vocab.pkl
│   └── idf.pkl
│
└── README.md
```

---

# Workflow

### 1. User Registration
Users create an account through the backend API.

### 2. Login
Authentication is handled by the backend.

### 3. Upload CV
Users upload their CV file.

The CV is stored in **SQL Server** as:

```
VARBINARY(MAX)
```

---

### 4. Compute Embeddings

The backend sends the CV text to the **AI service**.

The AI service:

- extracts text
- converts it into TF-IDF embeddings
- returns the embedding vector

The embedding is stored in the database.

---

### 5. Job Recommendation

When the user requests recommendations:

1. Retrieve the **user embedding**
2. Compare with **job embeddings**
3. Compute **cosine similarity**
4. Return **Top 3 most relevant jobs**

---

# Technologies Used

### Backend
- .NET
- ASP.NET Web API
- SQL Server

### AI Service
- Python
- Flask
- Scikit-learn
- TF-IDF Vectorization
- Cosine Similarity

### Database
- SQL Server

---

# Performance Optimization

The AI service loads the trained TF-IDF model **once at startup**:

```
tfidf_vectorizer.pkl
tfidf_matrix.pkl
idf.pkl
vocab.pkl
```

This avoids disk reads for every request and allows fast in-memory similarity computations.

---

# Future Improvements

Possible enhancements:

- Background job processing for embeddings
- Vector database
- Cloud file storage instead of database blobs
- Advanced embeddings (BERT / Sentence Transformers)

---

# Author

Mahmoud Mohamed  
Computer Science Graduate – Ain Shams University