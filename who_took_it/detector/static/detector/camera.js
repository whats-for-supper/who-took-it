document.addEventListener("DOMContentLoaded", () => {
  const video = document.querySelector("#camera");

  async function startCamera() {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: true,
        audio: false,
      });

      video.srcObject = stream;
    } catch (err) {
      console.error("Camera error:", err);

      // Optional: show message on screen
      const stage = document.querySelector(".camera-stage");
      stage.innerHTML = `<div class="stage-placeholder">
        Could not access camera. Check browser permissions.
      </div>`;
    }
  }

  startCamera();
});