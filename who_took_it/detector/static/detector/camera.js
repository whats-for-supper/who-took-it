document.addEventListener("DOMContentLoaded", () => {
  const video = document.querySelector("#camera");
  const record = document.querySelector("#record-dot");

  // constants
  let isDetecting = false;
  let watchInterval = null;

  // ensure camera is always running
  async function startCamera() {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: true,
        audio: false,
      });

      video.srcObject = stream;
    } catch (err) {
      console.error("Camera error:", err);

      // show message on screen
      const stage = document.querySelector(".camera-stage");
      stage.innerHTML = `<div class="stage-placeholder">
        Could not access camera. Check browser permissions.
      </div>`;
    }
  }

  function toggleDetector() {
    if (!isDetecting) {
      console.log("Starting detection");
      watchActivity();
      isDetecting = true;
    } else {
      console.log("Stopping detection");
      stopWatching();
      isDetecting = false;
    }

    record?.classList.toggle("active");
  }

  function watchActivity() {
    // check if existing interval
    if (watchInterval) {
      console.log("Tick already running");
      return;
    }

    // collect frames every second
    watchInterval = setInterval(() => {
      console.log("Capturing...");
      const framePromise = captureFrame();
      if (!framePromise) {
        return;
      }

      framePromise.then((frameBlob) => {
        if (!frameBlob) {
          return;
        }
        console.log("Captured frame");
        // Later send frameBlob to Django / ML
        analyseFrame(frameBlob);
      });
    }, 2000);

  }

  function stopWatching() {
    if (watchInterval) {
      clearInterval(watchInterval);
      watchInterval = null;
    }
  }

  // capture the frames into a jpeg blob for processing
  async function captureFrame() {
    // check whether video frame can be processed
    if (video.readyState < 2) {
      console.warn("Video not ready for capturing frame");
      return null;
    }

    const canvas = document.createElement("canvas");
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    const context = canvas.getContext("2d");
    context.drawImage(video, 0, 0, canvas.width, canvas.height);

    return new Promise((resolve) => {
      canvas.toBlob((blob) => {
        resolve(blob);
      }, "image/jpeg", 0.85);
    });
  }

  // jaye's code
  async function analyseFrame(frameBlob) {
    const formData = new FormData();
    formData.append("frame", frameBlob, "frame.jpg");

    try {
      const res = await fetch("/api/facerec/", {
        method: "POST",
        body: formData,
      });

      if (!res.ok) return;

      const data = await res.json();
      if (data.ok && data.alert) {
        triggerAlert();
      }
    } catch (e) {
      console.error(e);
    }
  }

  record?.addEventListener("click", toggleDetector);
  startCamera();
});