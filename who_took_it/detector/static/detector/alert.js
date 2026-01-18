// if familiar person detected, blare alarm
let alertLoopCount = 0;
let alertIsRunning = false;

function triggerAlert() {
  const overlay = document.getElementById("alert-overlay");
  overlay?.classList.remove("hidden");

  const alertSound = document.getElementById("alert-sound");
  if (!alertSound) return;

  // start sound only once
  if (alertIsRunning) return;
  alertIsRunning = true;

  // volume gradually increases
  alertLoopCount = 0;
  alertSound.volume = 0.5; // 0.0 to 1.0
  alertSound.currentTime = 0;

  alertSound.loop = false;
  // increase volume each loop (cap at 1.0)
  alertSound.onended = () => {
    alertLoopCount += 1;
    const nextVol = Math.min(1.0, 0.5 + alertLoopCount * 0.15);
    alertSound.volume = nextVol;

    alertSound.currentTime = 0;
    alertSound.play().catch(() => {});
  };

  alertSound.play().catch(() => {
      alertIsRunning = false;
  });
}

function showAlert() {
  document.getElementById("alert-overlay")?.classList.remove("hidden");
}

function stopAlert() {
  document.getElementById("alert-overlay")?.classList.add("hidden");

  const alertSound = document.getElementById("alert-sound");
  if (alertSound) {
    alertSound.onended = null;
    alertSound.pause();
    alertSound.currentTime = 0;
  }

  alertIsRunning = false;
  alertLoopCount = 0;
}