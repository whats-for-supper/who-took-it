
import os
import cv2
import numpy as np
from insightface.app import FaceAnalysis
from supabase import create_client, Client
import datetime
from dotenv import load_dotenv

### weird hacky thing to make everything work
np.int = int

### Supabase Config
load_dotenv()

SUPABASE_URL = os.environ.get("SUPABASE_URL")
SUPABASE_KEY = os.environ.get("SUPABASE_KEY")

if not SUPABASE_URL or not SUPABASE_KEY:
    raise RuntimeError(
        "Missing SUPABASE URL or SUPABASE KEY"
    )

supabase: Client = create_client(SUPABASE_URL, SUPABASE_KEY)

PERSON_TABLE = "Person"
EMBED_TABLE = "Embedding"

### Facial Recognition Model
MODEL_NAME = "buffalo_l"
app = FaceAnalysis(name = MODEL_NAME)  # strong default model pack
app.prepare(ctx_id=0, det_size=(640, 640))  # ctx_id=0 uses GPU if available, CPU otherwise

### Facial Similarity Function
def face_similarity(embed_one: np.ndarray, embed_two: np.ndarray, threshold: float) -> tuple[float, bool]:
    similarity = np.dot(embed_one, embed_two) / (np.linalg.norm(embed_one) * np.linalg.norm(embed_two))
    return similarity, similarity > threshold

### Create a person
def create_person() -> str:
    res = supabase.table(PERSON_TABLE).insert({}).execute()

    if getattr(res, "error", None):
        raise RuntimeError(f"Supabase insert(Person) error: {res.error}")

    return res.data[0]["id"]

### Save the Embedding
def save_embedding(person_id: str, embedding: np.ndarray) -> None:
    payload = {
        "person_id": person_id, 
        "vector": embedding.astype(np.float32).tolist(),
    }

    res = supabase.table(EMBED_TABLE).insert(payload).execute()

    if getattr(res, "error", None):
        raise RuntimeError(f"Supabase insert error: {res.error}")

### Load the Embeddings
def load_embeddings() -> list[tuple[str, np.ndarray]]:
    res = supabase.table(EMBED_TABLE).select("person_id, vector").execute()
    if getattr(res, "error", None):
        raise RuntimeError(f"Supabase select(Embedding) error: {res.error}")
    
    out: list[tuple[str, np.ndarray]] = []
    
    for row in (res.data or []):
        pid = row["person_id"]
        vect = row["vector"]

        if vect is None:
            continue

        out.append((pid, np.array(vect, dtype = np.float32)))

    return out

### Match the Faces
def best_match(cur_embedding: np.ndarray, db: list[tuple[str, np.ndarray]], threshold: float) -> tuple[str | None, float]:
    best_person_id = None
    best_sim = -1.0

    for person_id, db_embedding in db:
        similarity, _ = face_similarity(cur_embedding, db_embedding, threshold)

        if similarity > best_sim:
            best_sim = similarity
            best_person_id = person_id
        
    if best_sim >= threshold:
        return best_person_id, best_sim
        
    return None, best_sim

def decode_frame_to_bgr(img_bytes: bytes):
    arr = np.frombuffer(img_bytes, dtype = np.uint8)
    bgr = cv2.imdecode(arr, cv2.IMREAD_COLOR)
    return bgr

def process_captured_image(capt_img, enroll_unknown = True):
    capt_faces = app.get(capt_img)
    print(f"Found {len(capt_faces)} face(s).")

    # Load DB embeddings + compare
    db = load_embeddings()
    THRESHOLD = 0.50

    seen_person_ids = []
    results = []

    for i, face in enumerate(capt_faces):
        probe = face.embedding
        person_id, similarity = best_match(probe, db, THRESHOLD)  

        if person_id is None:
            if not enroll_unknown:
                continue

            person_id = create_person()
            save_embedding(person_id, probe)
            matched = False
            first_seen = True
            
        else: 
            matched = True
            first_seen = False

        seen_person_ids.append(person_id)

        results.append({
            "face_index": i,
            "person_id": person_id,
            "matched": matched,
            "first_seen": first_seen,
            "similarity": float(similarity),
        })

    return {"ok": True, "face_count": len(capt_faces), "seen_person_ids": seen_person_ids, "results": results}


### Bounding Box
#out = app.draw_on(img, faces)
#out_path = "89_output.jpg"
#cv2.imwrite(out_path, out)
#print("Image saved.")