# SkillSync AI Service

This service provides the **machine learning functionality** for the SkillSync job recommendation system.

It is implemented using **Python and Flask**.

The service computes **TF-IDF embeddings** and performs **cosine similarity** to recommend relevant jobs based on a user's CV.

---

# Tech Stack

- Python
- Flask
- Scikit-learn
- NumPy
- TF-IDF Vectorization

---

# AI Model Overview

The recommendation system is based on **content-based filtering**.

It compares the user's CV text with job descriptions using:

1. TF-IDF vectorization
2. Cosine similarity

---

# Model Files

The service loads precomputed model files when the application starts.

```
tfidf_vectorizer.pkl
tfidf_matrix.pkl
idf.pkl
vocab.pkl
```

### Purpose

| File | Description |
|----|----|
| tfidf_vectorizer.pkl | trained TF-IDF vectorizer |
| tfidf_matrix.pkl | TF-IDF embeddings for job descriptions |
| vocab.pkl | vocabulary used by the vectorizer |
| idf.pkl | inverse document frequency values |

---

# Performance Optimization

The model files are **loaded once when the Flask application starts**.

```
Flask Start
   |
   | load tfidf_vectorizer.pkl
   | load tfidf_matrix.pkl
   | load vocab.pkl
   | load idf.pkl
   |
Ready for requests
```

This avoids expensive disk operations during each API request.

---

# API Endpoints

### Compute Embedding

```
POST /get_embeddings
```

Request:

```
{
  "text": "CV text content"
}
```

Response:

```
{
  "embedding": [...]
}
```

---

### Recommend Jobs

```
POST /get_top_jobs
```

Request:

```
{
  "embedding": [...]
}
```

Response:

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

---

# Recommendation Algorithm

Steps:

1. Convert CV text into TF-IDF vector
2. Compare with job TF-IDF matrix
3. Compute cosine similarity
4. Rank jobs
5. Return top 3 matches

---

# Future Improvements

Possible enhancements include:

- Sentence Transformers embeddings
- Vector search with FAISS