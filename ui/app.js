/**
 * ManimPilot Studio - Windows 11 Fluent App Controller
 * Implements full interaction model matching Windows 11 Settings (Dark Mode)
 * Accent: #4CC2FF | Zero emojis | Professional Vector Graphics
 */

document.addEventListener("DOMContentLoaded", () => {
  // Application State
  const state = {
    binary: "10110110",
    decimal: 182,
    activeTab: "update",
    viewMode: "video" // "video" or "canvas"
  };

  // DOM Elements
  const inputBinary = document.getElementById("input-binary-val");
  const badgeDecimal = document.getElementById("badge-decimal");
  const heroMathText = document.getElementById("hero-math-text");
  const btnCheckUpdates = document.getElementById("btn-check-updates");

  const toggleLatest = document.getElementById("toggle-latest-updates");
  const toggleLabel = document.getElementById("toggle-label-text");

  const pillVideo = document.getElementById("pill-video");
  const pillCanvas = document.getElementById("pill-canvas");
  const videoWrapper = document.getElementById("video-wrapper");
  const canvasWrapper = document.getElementById("canvas-wrapper");
  const manimVideo = document.getElementById("manim-video-element");

  const bitsRow = document.getElementById("bits-row");
  const powersRow = document.getElementById("powers-row");
  const sumEquation = document.getElementById("sum-equation");
  const canvasBinaryTag = document.getElementById("canvas-binary-tag");

  const modalOverlay = document.getElementById("modal-overlay");
  const btnDialogClose = document.getElementById("btn-dialog-close");
  const btnDialogCancel = document.getElementById("btn-dialog-cancel");
  const btnDialogExecute = document.getElementById("btn-dialog-execute");
  const dialogProgressBar = document.getElementById("dialog-progress-bar");
  const dialogProgressFill = document.getElementById("dialog-progress-fill");
  const dialogProgressCaption = document.getElementById("dialog-progress-caption");

  const navItems = document.querySelectorAll(".nav-item");
  const aiChips = document.querySelectorAll(".win11-chip");

  // Recompute Binary Math Function
  function updateBinaryMath() {
    let raw = inputBinary.value.replace(/[^01]/g, "");
    if (!raw) raw = "0";

    state.binary = raw;
    const len = raw.length;
    let dec = 0;
    const activeTerms = [];

    // Clear dynamic grids
    bitsRow.innerHTML = "";
    powersRow.innerHTML = "";

    for (let i = 0; i < len; i++) {
      const bit = raw[i];
      const power = len - 1 - i;
      const placeValue = Math.pow(2, power);

      const isBitActive = bit === "1";
      if (isBitActive) {
        dec += placeValue;
        activeTerms.push(placeValue);
      }

      // Bit chip
      const bitEl = document.createElement("div");
      bitEl.className = `bit-chip ${isBitActive ? "active" : ""}`;
      bitEl.textContent = bit;
      bitEl.title = `Bit ${power}: 2^${power} = ${placeValue}`;
      bitsRow.appendChild(bitEl);

      // Power chip
      const powEl = document.createElement("div");
      powEl.className = `power-chip ${isBitActive ? "active" : ""}`;
      powEl.textContent = `2^${power} (${placeValue})`;
      powersRow.appendChild(powEl);
    }

    state.decimal = dec;
    badgeDecimal.textContent = `= ${dec}`;
    heroMathText.textContent = `${raw}₂ = ${dec}₁₀ (Verified)`;
    canvasBinaryTag.textContent = `${raw}₂`;

    if (activeTerms.length > 0) {
      sumEquation.textContent = `${activeTerms.join(" + ")} = ${dec}`;
    } else {
      sumEquation.textContent = "0 = 0";
    }
  }

  // Bind Input Event
  inputBinary.addEventListener("input", updateBinaryMath);

  // Toggle Switch Event
  toggleLatest.addEventListener("change", (e) => {
    toggleLabel.textContent = e.target.checked ? "On" : "Off";
  });

  // Pill Toggles (Video vs Canvas)
  pillVideo.addEventListener("click", () => {
    pillVideo.classList.add("active");
    pillCanvas.classList.remove("active");
    videoWrapper.style.display = "flex";
    canvasWrapper.style.display = "none";
  });

  pillCanvas.addEventListener("click", () => {
    pillCanvas.classList.add("active");
    pillVideo.classList.remove("active");
    videoWrapper.style.display = "none";
    canvasWrapper.style.display = "block";
    updateBinaryMath();
  });

  // Modal Dialog Handlers
  function openModal() {
    modalOverlay.classList.add("open");
    dialogProgressBar.style.display = "none";
    dialogProgressFill.style.width = "0%";
    btnDialogExecute.disabled = false;
    btnDialogExecute.textContent = "Start Render";
  }

  function closeModal() {
    modalOverlay.classList.remove("open");
  }

  btnCheckUpdates.addEventListener("click", openModal);
  btnDialogClose.addEventListener("click", closeModal);
  btnDialogCancel.addEventListener("click", closeModal);

  modalOverlay.addEventListener("click", (e) => {
    if (e.target === modalOverlay) closeModal();
  });

  // Execute Render Simulation / Verification
  btnDialogExecute.addEventListener("click", () => {
    btnDialogExecute.disabled = true;
    dialogProgressBar.style.display = "flex";
    dialogProgressFill.style.width = "0%";

    const stages = [
      { pct: 20, text: "Step 1/5: Validating LessonModel schemas..." },
      { pct: 45, text: "Step 2/5: Verifying place-value mathematical proofs..." },
      { pct: 70, text: "Step 3/5: Checking Sinhala speech overlap guard (si-LK)..." },
      { pct: 90, text: "Step 4/5: Compiling output/scene.py Manim script..." },
      { pct: 100, text: "Complete! Ready to play 1080p video." }
    ];

    let currentStage = 0;
    const interval = setInterval(() => {
      if (currentStage < stages.length) {
        dialogProgressFill.style.width = stages[currentStage].pct + "%";
        dialogProgressCaption.textContent = stages[currentStage].text;
        currentStage++;
      } else {
        clearInterval(interval);
        setTimeout(() => {
          closeModal();
          // Ensure video tab is active and start playback
          pillVideo.click();
          if (manimVideo) {
            manimVideo.currentTime = 0;
            manimVideo.play().catch(() => {});
          }
        }, 600);
      }
    }, 400);
  });

  // Navigation Items
  navItems.forEach(item => {
    item.addEventListener("click", (e) => {
      e.preventDefault();
      navItems.forEach(n => n.classList.remove("active"));
      item.classList.add("active");
    });
  });

  // AI Copilot Chips
  aiChips.forEach(chip => {
    chip.addEventListener("click", () => {
      aiChips.forEach(c => c.classList.remove("active"));
      chip.classList.add("active");
      const action = chip.getAttribute("data-ai");
      if (action === "simplify") {
        inputBinary.value = "1010";
      } else if (action === "natural") {
        inputBinary.value = "10110110";
      }
      updateBinaryMath();
    });
  });

  // Initialize
  updateBinaryMath();
});
