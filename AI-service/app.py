from flask import Flask, request, jsonify
import pickle
import joblib
from sklearn.metrics.pairwise import cosine_similarity
from sklearn.feature_extraction.text import TfidfVectorizer
import pandas as pd
import regex as re
import spacy
# ---------------------------
# Step 1: Load saved TF-IDF model
# ---------------------------

# Load saved parts
vocab = joblib.load("vocab.pkl")
idf = joblib.load("idf.pkl")

# Step 1: Initialize with vocabulary
vectorizer = TfidfVectorizer(vocabulary=vocab)

# Step 2: Fake-fit to initialize internal structures
vectorizer.fit(["dummy"])  # required to create _tfidf

# Step 3: Now override idf safely
vectorizer.idf_ = idf
vectorizer._tfidf.idf_ = idf

tfidf_matrix = joblib.load('tfidf_matrix.pkl')   

data = pd.read_csv('./dataset/job_description.csv') 


nlp = spacy.load("en_core_web_sm", disable=["parser","ner"])

app = Flask(__name__)



# clean functions 
def clean_lemmatize(text):
    text = text.lower()
    text = re.sub(r'<br\s*/?>', ' ', text)
    text = re.sub(r'\n', ' ', text)
    text = re.sub(r'\b[\w\.-]+@[\w\.-]+\.\w+\b', '', text)
    text = re.sub(r'\+?\d[\d\s\-\(\)]{6,}\d', '', text)
    text = re.sub(r'[^a-z0-9\s]', '', text)
    text = re.sub(r'\s+', ' ', text)
    text = text.strip()
    doc = nlp(text)
    lemmatized_text = ' '.join([token.lemma_ for token in doc if not token.is_stop])

    return lemmatized_text


@app.route('/get_embeddings', methods=['POST'])
def get_embeddings():
    data = request.get_json()

    if not data or 'cvData' not in data:
        return jsonify({"error": "Missing 'cvData' field"}), 400

    cv_text = data['cvData']
    
    cv_text = clean_lemmatize(cv_text)

    embedding_vector = vectorizer.transform([cv_text])

    embedding_list = embedding_vector.toarray().tolist()[0]

    return jsonify({"embedding": embedding_list})


@app.route('/get_top_jobs', methods=['POST'])
def get_top_jobs():
    req = request.get_json()
    if not req or 'embeddings' not in req:
        return jsonify({"error": "Missing 'embeddings' field"}), 400
    try:
        import numpy as np
        cv_vector = np.array(req['embeddings']).reshape(1, -1)
    except Exception as e:
        return jsonify({"error": str(e)}), 400
  
    similarity_scores = cosine_similarity(cv_vector, tfidf_matrix).flatten()

    top_indices = similarity_scores.argsort()[-3:][::-1] 
    top_jobs = data.iloc[top_indices].copy()
    top_jobs['similarity'] = similarity_scores[top_indices]

    # Return minimal info as JSON
    # top_indices = similarity_scores.argsort()[-3:][::-1]

    # return jsonify({
    #     "job_indexes": top_indices.tolist()
    # })
    result = top_jobs[['Job Title', 'Job Description', 'similarity']].to_dict(orient='records')


    return jsonify({"top_jobs": result})



if __name__ == "__main__":
    app.run(debug=True)