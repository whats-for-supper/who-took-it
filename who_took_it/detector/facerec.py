
import cv2
import numpy as np
from insightface.app import FaceAnalysis

### weird hacky thing to make everything work
np.int = int

### Facial Recognition Model
app = FaceAnalysis(name="buffalo_l")  # strong default model pack
app.prepare(ctx_id=0, det_size=(640, 640))  # ctx_id=0 uses GPU if available, CPU otherwise

### Image Captured by Frontend
capt_img_path = "70.jpg"
capt_img = cv2.imread(capt_img_path)
if capt_img is None:
    raise FileNotFoundError(
        f"Could not read '{capt_img_path}'. "
        f"Check the file path and your working directory."
    )

capt_faces = app.get(capt_img)
print(f"Found {len(capt_faces)} face(s) in {capt_img_path}")

capt_embedded_face = capt_faces[0].embedding

def face_similarity(embed_one: np.ndarray, embed_two: np.ndarray, threshold: float) -> tuple[float, bool]:
    similarity = np.dot(embed_one, embed_two) / (np.linalg.norm(embed_one) * np.linalg.norm(embed_two))

    return similarity, similarity > threshold

image_paths = ["two_faces.jpg"]

for path in image_paths:
    cur_image = cv2.imread(path)

    if cur_image is None:
        raise FileNotFoundError(
        f"Could not read '{img_path}'. "
        f"Check the file path and your working directory."
    )

    faces = app.get(cur_image)

    print(f"There are {len(faces)} faces in the picture.")

    if len(faces) <= 0:   # if there are no faces
        continue

    if len(faces) >= 1:
        embedded_face_arr = []
        for i in range(len(faces)):
            embedded_face_arr.append(faces[i].embedding)

    for embedded_face_i in embedded_face_arr:
        similarity, is_same_face = face_similarity(embedded_face_i, embedded_face, 0.50)
        print(similarity, is_same_face)

### Bounding Box
#out = app.draw_on(img, faces)
#out_path = "89_output.jpg"
#cv2.imwrite(out_path, out)
#print("Image saved.")