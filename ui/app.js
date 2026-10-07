/**
 * ManimPilot Studio - Windows 11 Fluent App Controller
 * Pure professional implementation with zero emojis and authentic Windows 11 behavior.
 */

// Application State
const appState = {
  binary: "10110110",
  decimal: 182,
  overlapGuardActive: true,
  steps: [
    {
      id: "step_01",
      title: "ද්විමය සංඛ්‍යාව ඉදිරිපත් කිරීම (Show Binary)",
      narration: "අපිට ලබාදීලා තියෙන්නේ 10110110 කියන binary සංඛ්‍යාවයි.",
      duration: 6.5
    },
    {
      id: "step_02",
      title: "ස්ථානීය අගයන් දැක්වීම (Place Values)",
      narration: "දකුණේ සිට වමට දෙකෙහි බලයන් අනුව ස්ථානීය අගයන් පිළිවෙළින් ලියා ගනිමු.",
      duration: 5.0
    },
    {
      id: "step_03",
      title: "අගය 1 වන ස්ථාන තෝරාගැනීම (Select Active Bits)",
      narration: "මෙහි අගය එක වන ස්ථානවලට අදාළ ස්ථානීය අගයන් පමණක් අපි එකතු කිරීමට තෝරාගන්නවා.",
      duration: 6.0
    },
    {
      id: "step_04",
      title: "එකතු කිරීමේ ප්‍රකාශනය (Addition Expression)",
      narration: "දැන් තෝරාගත් අගයන් වන 128 + 32 + 16 + 4 + 2 එකතු කරමු.",
      duration: 7.0
    },
    {
      id: "step_05",
      title: "අවසාන පිළිතුර (Final Result)",
      narration: "එමගින් අපට අවසාන පිළිතුර ලෙස 182 ලැබෙනවා.",
      duration: 3.2
    }
  ]
};

// DOM Elements
const inputBinary = document.getElementById("input-binary-val");
const badgeDecimal = document.getElementById("badge-decimal");
const heroMathText = document.getElementById("hero-math-text");
const btnRenderPrimary = document.getElementById("btn-render-primary");

const btnModeVideo = document.getElementById("btn-mode-video");
const btnModeCanvas = document.getElementById("btn-mode-canvas");
const videoContainer = document.getElementById("video-container");
const canvasContainer = document.getElementById("canvas-container");
const manimPlayer = document.getElementById("manim-player");

const binaryBitsRow = document.getElementById("binary-bits-row");
const placeValuesRow = document.getElementById("place-values-row");
const eqVal = document.getElementById("eq-val");
const canvasSubText = document.getElementById("canvas-sub-text");

const toggleOverlapGuard = document.getElementById("toggle-overlap-guard");
const toggleStateText = document.querySelector(".toggle-state-text");

const renderModal = document.getElementById("render-modal");
const btnModalClose = document.getElementById("btn-modal-close");
const btnModalCancel = document.getElementById("btn-modal-cancel");
const btnModalExec = document.getElementById("btn-modal-exec");
const modalRenderProgress = document.getElementById("modal-render-progress");
const winProgFill = document.getElementById("win-prog-fill");
const winProgText = document.getElementById("win-prog-text");

// Initialization
document.addEventListener("DOMContentLoaded", () => {
  recomputeBinaryMath();
  bindWin11Events();
});

// Binary Math Recomputation
function recomputeBinaryMath() {
  const raw = inputBinary.value.replace(/[^01]/g, "");
  if (!raw) return;

  appState.binary = raw;
  let dec = 0;
  const len = raw.length;
  const activePlaceValues = [];

  for (let i = 0; i < len; i++) {
    const power = len - 1 - i;
    const placeVal = Math.pow(2, power);
    if (raw[i] === "1") {
      dec += placeVal;
      activePlaceValues.push(placeVal);
    }
  }

  appState.decimal = dec;
  badgeDecimal.textContent = `= ${dec}`;
  heroMathText.textContent = `${raw}₂ = ${dec}₁₀`;

  // Update Canvas Stage Bit Boxes
  binaryBitsRow.innerHTML = "";
  placeValuesRow.innerHTML = "";

  for (let i = 0; i < len; i++) {
    const power = len - 1 - i;
    const placeVal = Math.pow(2, power);
    const isOne = raw[i] === "1";

    const bitBox = document.createElement("div");
    bitBox.className = `win-bit-box ${isOne ? "active" : "dimmed"}`;
    bitBox.textContent = raw[i];
    binaryBitsRow.appendChild(bitBox);

    const pvItem = document.createElement("div");
    pvItem.className = `win-pv-item ${isOne ? "active" : ""}`;
    pvItem.textContent = placeVal;
    placeValuesRow.appendChild(pvItem);
  }

  const exprStr = activePlaceValues.join(" + ");
  eqVal.textContent = `${exprStr} = ${dec}`;
  appState.steps[3].narration = `දැන් තෝරාගත් අගයන් වන ${exprStr} එකතු කරමු.`;
  appState.steps[4].narration = `එමගින් අපට අවසාන පිළිතුර ලෙස ${dec} ලැබෙනවා.`;
}

// Event Bindings
function bindWin11Events() {
  // Binary Input change
  inputBinary.addEventListener("input", recomputeBinaryMath);

  // View Mode Switcher
  btnModeVideo.addEventListener("click", () => {
    btnModeVideo.classList.add("active");
    btnModeCanvas.classList.remove("active");
    videoContainer.style.display = "flex";
    canvasContainer.style.display = "none";
  });

  btnModeCanvas.addEventListener("click", () => {
    btnModeCanvas.classList.add("active");
    btnModeVideo.classList.remove("active");
    videoContainer.style.display = "none";
    canvasContainer.style.display = "flex";
    if (manimPlayer) manimPlayer.pause();
  });

  // Toggle Overlap Guard
  toggleOverlapGuard.addEventListener("change", (e) => {
    appState.overlapGuardActive = e.target.checked;
    toggleStateText.textContent = e.target.checked ? "On" : "Off";
  });

  // AI Copilot Action Chips
  document.querySelectorAll(".win-chip").forEach(chip => {
    chip.addEventListener("click", () => {
      const type = chip.dataset.ai;
      handleAiRefinement(type);
    });
  });

  // Modal Open & Close
  btnRenderPrimary.addEventListener("click", () => {
    renderModal.classList.add("open");
  });
  btnModalClose.addEventListener("click", () => {
    renderModal.classList.remove("open");
  });
  btnModalCancel.addEventListener("click", () => {
    renderModal.classList.remove("open");
  });

  // Execute Render from Modal
  btnModalExec.addEventListener("click", () => {
    modalRenderProgress.style.display = "flex";
    btnModalExec.disabled = true;

    const stages = [
      { pct: 25, label: "Validating schema & mathematical proof..." },
      { pct: 50, label: "Synthesizing audio timeline & padding..." },
      { pct: 75, label: "Executing Python Manim Community Engine..." },
      { pct: 100, label: "Render complete: D:\\ManimPilot\\media\\videos\\scene\\1080p30\\ManimPilotLessonScene.mp4" }
    ];

    let current = 0;
    const progressTimer = setInterval(() => {
      if (current < stages.length) {
        winProgFill.style.width = `${stages[current].pct}%`;
        winProgText.textContent = stages[current].label;
        current++;
      } else {
        clearInterval(progressTimer);
        setTimeout(() => {
          renderModal.classList.remove("open");
          modalRenderProgress.style.display = "none";
          btnModalExec.disabled = false;
          // Reload video
          if (manimPlayer) {
            manimPlayer.src = `video.mp4?t=${Date.now()}`;
            manimPlayer.load();
            manimPlayer.play();
          }
        }, 1200);
      }
    }, 600);
  });

  // Navigation Items Selection
  document.querySelectorAll(".win-nav-item").forEach(item => {
    item.addEventListener("click", () => {
      document.querySelectorAll(".win-nav-item").forEach(i => i.classList.remove("active"));
      item.classList.add("active");
    });
  });
}

function handleAiRefinement(action) {
  if (action === "simplify") {
    canvasSubText.textContent = `"අපි 1 තියෙන තැන්වල අගයන් විතරක් අරගෙන එකතු කරමු."`;
    appState.steps[2].narration = "අපි 1 තියෙන තැන්වල අගයන් විතරක් අරගෙන එකතු කරමු.";
  } else if (action === "natural") {
    canvasSubText.textContent = `"ඔන්න දැන් අපි කලින් ආපු ස්ථානීය අගයන් ටික එකතු කරමු."`;
    appState.steps[3].narration = "ඔන්න දැන් අපි කලින් ආපු ස්ථානීය අගයන් ටික එකතු කරමු.";
  } else if (action === "grade8") {
    canvasSubText.textContent = `"8 ශ්‍රේණියේ විෂය නිර්දේශයට අනුව ද්විමය සංඛ්‍යා දශම බවට හැරවීම බලමු."`;
    appState.steps[0].narration = "8 ශ්‍රේණියේ විෂය නිර්දේශයට අනුව ද්විමය සංඛ්‍යා දශම බවට හැරවීම බලමු.";
  }
}
