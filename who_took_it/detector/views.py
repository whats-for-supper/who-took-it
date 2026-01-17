from django.shortcuts import render
from django.http import JsonResponse
from django.views.decorators.csrf import csrf_exempt
from .updated_face_rec import decode_frame_to_bgr, process_captured_image

# Create your views here.
def index(request):
    return render(request, "detector/index.html")

# ---- Presence tracking state (in-memory) ----
present = set()
ever_seen = set()
missing_counts = {}

LEAVE_FRAMES = 2

@csrf_exempt
def facerec_api(request):
    if request.method != "POST":
        return JsonResponse({"ok": False, "error": "POST only"}, status=405)

    frame = request.FILES.get("frame")
    if not frame:
        return JsonResponse({"ok": False, "error": "Missing 'frame'"}, status=400)

    img_bytes = frame.read()
    capt_img = decode_frame_to_bgr(img_bytes)
    if capt_img is None:
        return JsonResponse({"ok": False, "error": "Invalid image"}, status=400)

    rec = process_captured_image(capt_img, enroll_unknown=True)

    seen_now = set(rec.get("seen_person_ids", []))

    # Handle leaving
    for pid in list(present):
        if pid not in seen_now:
            missing_counts[pid] = missing_counts.get(pid, 0) + 1
            if missing_counts[pid] >= LEAVE_FRAMES:
                present.remove(pid)
        else:
            missing_counts[pid] = 0

    alert = False
    alerted_person_ids = []

    # Handle arrivals / re-entries
    for pid in seen_now:
        if pid not in ever_seen:
            ever_seen.add(pid)
            present.add(pid)
            missing_counts[pid] = 0
            continue

        if pid not in present:
            alert = True
            alerted_person_ids.append(pid)

        present.add(pid)
        missing_counts[pid] = 0

    return JsonResponse({
        "ok": True,
        "alert": alert,
        "alerted_person_ids": alerted_person_ids,
        "face_count": rec["face_count"],
        "results": rec["results"],
    })
