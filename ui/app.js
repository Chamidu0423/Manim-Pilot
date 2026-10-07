/**
 * ManimPilot Studio - Interactive Client Controller
 * Powers live canvas playback, multi-track timeline, AI Copilot, and math verification.
 */

// Application State
const state = {
  binary: "10110110",
  decimal: 182,
  currentTime: 0.0,
  totalDuration: 31.26,
  isPlaying: false,
  playbackSpeed: 1.0,
  activeStepIndex: 0,
  steps: [
    {
      id: "step_01",
      order: 1,
      title: "ද්විමය සංඛ්‍යාව ඉදිරිපත් කිරීම",
      type: "show_binary",
      startTime: 0.0,
      endTime: 7.1,
      duration: 7.1,
      narration: "අපිට දීලා තියෙන්නේ 10110110 කියන binary සංඛ්‍යාවයි. අපි දැන් මෙය දශම සංඛ්‍යාවක් බවට හරවමු.",
      spoken: "අපිට දීලා තියෙන්නේ එක, බිංදුව, එක, එක, බිංදුව, එක, එක, බිංදුව කියන ද්විමය සංඛ්‍යාවයි."
    },
    {
      id: "step_02",
      order: 2,
      title: "ස්ථානීය අගයන් දැක්වීම",
      type: "show_place_values",
      startTime: 7.2,
      endTime: 12.8,
      duration: 5.6,
      narration: "දකුණේ සිට වමට දෙකෙහි බලයන් අනුව ස්ථානීය අගයන් පිළිවෙළින් ලියා ගනිමු.",
      spoken: "දකුණේ සිට වමට දෙකේ බලයන් අනුව ස්ථානීය අගයන් පිළිවෙළින් සටහන් කරමු."
    },
    {
      id: "step_03",
      order: 3,
      title: "අගය එක වන ස්ථාන තෝරාගැනීම",
      type: "select_bits",
      startTime: 12.9,
      endTime: 19.5,
      duration: 6.6,
      narration: "මෙහි අගය එක වන ස්ථානවලට අදාළ ස්ථානීය අගයන් පමණක් අපි එකතු කිරීමට තෝරාගන්නවා.",
      spoken: "මෙහි අගය එක වන ස්ථානවලට අදාළ ස්ථානීය අගයන් පමණක් අපි එකතු කිරීම සඳහා වෙන් කරගන්නවා."
    },
    {
      id: "step_04",
      order: 4,
      title: "එකතු කිරීමේ ප්‍රකාශනය",
      type: "addition_expression",
      startTime: 19.6,
      endTime: 27.2,
      duration: 7.6,
      narration: "දැන් තෝරාගත් අගයන් වන 128 + 32 + 16 + 4 + 2 එකතු කරමු.",
      spoken: "එකසිය විසි අට, තිස් දෙක, දහසය, හතර සහ දෙක එකතු කළ විට..."
    },
    {
      id: "step_05",
      order: 5,
      title: "අවසාන පිළිතුර",
      type: "final_answer",
      startTime: 27.3,
      endTime: 31.26,
      duration: 4.0,
      narration: "එමගින් අපට අවසාන පිළිතුර ලෙස 182 ලැබෙනවා.",
      spoken: "එමගින් අපට අවසාන පිළිතුර ලෙස එකසිය අසූ දෙකක් ලැබෙනවා."
    }
  ]
};

// DOM References
const binaryInput = document.getElementById("binary-input");
const decimalPreview = document.getElementById("decimal-preview");
const binaryGrid = document.getElementById("binary-grid");
const placeValuesGrid = document.getElementById("place-values-grid");
const mathExpression = document.getElementById("math-expression");
const eqContent = document.getElementById("eq-content");
const subtitleText = document.getElementById("subtitle-text");
const subtitleBox = document.getElementById("subtitle-box");
const stageTitle = document.getElementById("stage-title");
const stageStepPill = document.getElementById("stage-step-pill");
const btnViewVideo = document.getElementById("btn-view-video");
const btnViewCanvas = document.getElementById("btn-view-canvas");
const manimVideoPlayer = document.getElementById("manim-video-player");
const canvasBg = document.getElementById("canvas-bg");
const animationStage = document.getElementById("animation-stage");

const btnPlayPause = document.getElementById("btn-play-pause");
const playIcon = document.getElementById("play-icon");
const btnRewind = document.getElementById("btn-rewind");
const timelineScrubber = document.getElementById("timeline-scrubber");
const currentTimeLabel = document.getElementById("current-time");
const totalTimeLabel = document.getElementById("total-time");
const playhead = document.getElementById("timeline-playhead");

const visualLane = document.getElementById("visual-lane");
const narrationLane = document.getElementById("narration-lane");

const inspStepId = document.getElementById("insp-step-id");
const inspType = document.getElementById("insp-type");
const inspDuration = document.getElementById("insp-duration");
const inspNarration = document.getElementById("insp-narration");
const inspSpoken = document.getElementById("insp-spoken");
const btnUpdateStep = document.getElementById("btn-update-step");

const chatLogs = document.getElementById("chat-logs");
const copilotInput = document.getElementById("copilot-prompt-input");
const btnSendPrompt = document.getElementById("btn-send-prompt");

const renderModal = document.getElementById("render-modal");
const btnRenderModal = document.getElementById("btn-render-modal");
const btnCloseModal = document.getElementById("btn-close-modal");
const btnCancelRender = document.getElementById("btn-cancel-render");
const btnStartRender = document.getElementById("btn-start-render");
const renderProgressBox = document.getElementById("render-progress-box");
const progressFill = document.getElementById("progress-fill");
const progressStatusText = document.getElementById("progress-status-text");

// Initialization
document.addEventListener("DOMContentLoaded", () => {
  recomputeMath();
  renderTimelineTracks();
  updateStageVisuals(0.0);
  bindEvents();
});

// Binary Math Recomputation
function recomputeMath() {
  const raw = binaryInput.value.replace(/[^01]/g, "");
  if (!raw) return;

  state.binary = raw;
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

  state.decimal = dec;
  decimalPreview.textContent = `= ${dec}`;
  stageTitle.textContent = `ද්විමය සංඛ්‍යා දශම බවට හැරවීම (${raw})`;

  // Render bit cards & place values
  binaryGrid.innerHTML = "";
  placeValuesGrid.innerHTML = "";

  for (let i = 0; i < len; i++) {
    const power = len - 1 - i;
    const placeVal = Math.pow(2, power);

    // Bit card
    const card = document.createElement("div");
    card.className = "bit-card";
    card.id = `bit-${i}`;
    card.textContent = raw[i];
    binaryGrid.appendChild(card);

    // Place value item
    const pv = document.createElement("div");
    pv.className = "pv-item";
    pv.id = `pv-${i}`;
    pv.textContent = placeVal;
    placeValuesGrid.appendChild(pv);
  }

  const exprStr = activePlaceValues.join(" + ");
  eqContent.textContent = `${exprStr} = ${dec}`;

  // Update step 4 & 5 references
  state.steps[3].narration = `දැන් තෝරාගත් අගයන් වන ${exprStr} එකතු කරමු.`;
  state.steps[4].narration = `එමගින් අපට අවසාන පිළිතුර ලෙස ${dec} ලැබෙනවා.`;
}

// Render Multi-track Timeline Blocks
function renderTimelineTracks() {
  visualLane.innerHTML = "";
  narrationLane.innerHTML = "";

  const total = state.totalDuration;

  state.steps.forEach((step, idx) => {
    const leftPct = (step.startTime / total) * 100;
    const widthPct = ((step.endTime - step.startTime) / total) * 100;

    // Visual Block
    const vBlock = document.createElement("div");
    vBlock.className = "event-block visual-event";
    vBlock.style.left = `${leftPct}%`;
    vBlock.style.width = `${Math.max(widthPct, 2)}%`;
    vBlock.textContent = `[🎬] ${step.title}`;
    vBlock.title = `${step.title} (${step.startTime}s - ${step.endTime}s)`;
    vBlock.addEventListener("click", () => seekTo(step.startTime));
    visualLane.appendChild(vBlock);

    // Narration Block (with small offset)
    const nStart = step.startTime + 0.3;
    const nEnd = Math.min(step.endTime - 0.2, nStart + step.duration - 0.5);
    const nLeft = (nStart / total) * 100;
    const nWidth = ((nEnd - nStart) / total) * 100;

    const nBlock = document.createElement("div");
    nBlock.className = "event-block narration-event";
    nBlock.style.left = `${nLeft}%`;
    nBlock.style.width = `${Math.max(nWidth, 2)}%`;
    nBlock.textContent = `[🔊] Speech ${idx + 1}`;
    nBlock.title = `Narration: ${step.narration}`;
    nBlock.addEventListener("click", () => seekTo(step.startTime));
    narrationLane.appendChild(nBlock);
  });
}

// Update Stage State according to current playback time
function updateStageVisuals(time) {
  state.currentTime = time;
  timelineScrubber.value = time;
  currentTimeLabel.textContent = formatTime(time);

  // Update Playhead position on timeline
  const playheadPct = (time / state.totalDuration) * 100;
  playhead.style.left = `calc(90px + (100% - 110px) * ${playheadPct / 100})`;

  // Determine current active step
  let activeStep = state.steps[0];
  let stepIdx = 0;

  for (let i = 0; i < state.steps.length; i++) {
    if (time >= state.steps[i].startTime && time <= state.steps[i].endTime) {
      activeStep = state.steps[i];
      stepIdx = i;
      break;
    }
  }

  state.activeStepIndex = stepIdx;
  stageStepPill.textContent = `Step ${stepIdx + 1} of 5 • ${activeStep.title}`;
  subtitleText.textContent = `"${activeStep.narration}"`;

  // Update visual elements on canvas according to step type
  const len = state.binary.length;

  for (let i = 0; i < len; i++) {
    const bitEl = document.getElementById(`bit-${i}`);
    const pvEl = document.getElementById(`pv-${i}`);
    if (!bitEl || !pvEl) continue;

    const isBitOne = state.binary[i] === "1";

    if (stepIdx === 0) {
      // Step 1: Just show binary
      bitEl.className = "bit-card";
      pvEl.className = "pv-item";
      pvEl.style.opacity = "0.2";
      mathExpression.style.opacity = "0.2";
      mathExpression.classList.remove("highlight-final");
    } else if (stepIdx === 1) {
      // Step 2: Show Place values
      bitEl.className = "bit-card";
      pvEl.className = "pv-item";
      pvEl.style.opacity = "1";
      mathExpression.style.opacity = "0.2";
      mathExpression.classList.remove("highlight-final");
    } else if (stepIdx === 2) {
      // Step 3: Highlight active bits
      pvEl.style.opacity = "1";
      mathExpression.style.opacity = "0.4";
      mathExpression.classList.remove("highlight-final");
      if (isBitOne) {
        bitEl.className = "bit-card active-bit";
        pvEl.className = "pv-item active-pv";
      } else {
        bitEl.className = "bit-card dimmed-bit";
        pvEl.className = "pv-item dimmed-bit";
      }
    } else if (stepIdx === 3) {
      // Step 4: Addition Expression
      pvEl.style.opacity = "1";
      mathExpression.style.opacity = "1";
      mathExpression.classList.remove("highlight-final");
      if (isBitOne) {
        bitEl.className = "bit-card active-bit";
        pvEl.className = "pv-item active-pv";
      } else {
        bitEl.className = "bit-card dimmed-bit";
        pvEl.className = "pv-item dimmed-bit";
      }
    } else if (stepIdx >= 4) {
      // Step 5: Final Result highlight
      pvEl.style.opacity = "1";
      mathExpression.style.opacity = "1";
      mathExpression.classList.add("highlight-final");
      if (isBitOne) {
        bitEl.className = "bit-card active-bit";
        pvEl.className = "pv-item active-pv";
      } else {
        bitEl.className = "bit-card dimmed-bit";
        pvEl.className = "pv-item dimmed-bit";
      }
    }
  }

  // Update Inspector properties
  inspStepId.textContent = activeStep.id;
  inspType.value = activeStep.type;
  inspDuration.value = activeStep.duration.toFixed(1);
  inspNarration.value = activeStep.narration;
  inspSpoken.value = activeStep.spoken;
}

// Playback Engine
let playInterval = null;

function togglePlay() {
  if (state.isPlaying) {
    pause();
  } else {
    play();
  }
}

function play() {
  state.isPlaying = true;
  playIcon.innerHTML = '<rect x="6" y="4" width="4" height="16"/><rect x="14" y="4" width="4" height="16"/>';

  const tickRate = 50; // 50ms updates
  playInterval = setInterval(() => {
    let nextTime = state.currentTime + (tickRate / 1000) * state.playbackSpeed;
    if (nextTime >= state.totalDuration) {
      nextTime = 0.0;
      pause();
    }
    updateStageVisuals(nextTime);
  }, tickRate);
}

function pause() {
  state.isPlaying = false;
  playIcon.innerHTML = '<polygon points="5 3 19 12 5 21 5 3"/>';
  if (playInterval) {
    clearInterval(playInterval);
    playInterval = null;
  }
}

function seekTo(time) {
  updateStageVisuals(Math.max(0, Math.min(time, state.totalDuration)));
}

function formatTime(seconds) {
  const m = Math.floor(seconds / 60);
  const s = (seconds % 60).toFixed(1);
  return `${m.toString().padStart(2, "0")}:${s.padStart(4, "0")}`;
}

// Event Bindings
function bindEvents() {
  // Binary Input change
  binaryInput.addEventListener("input", () => {
    recomputeMath();
    updateStageVisuals(state.currentTime);
  });

  // Player controls
  btnPlayPause.addEventListener("click", togglePlay);
  btnRewind.addEventListener("click", () => {
    seekTo(0.0);
  });

  timelineScrubber.addEventListener("input", (e) => {
    seekTo(parseFloat(e.target.value));
  });

  // View Mode Switcher
  btnViewVideo.addEventListener("click", () => {
    btnViewVideo.classList.add("active");
    btnViewCanvas.classList.remove("active");
    manimVideoPlayer.style.display = "block";
    canvasBg.style.display = "none";
    animationStage.style.display = "none";
    subtitleBox.style.display = "none";
    pause();
  });

  btnViewCanvas.addEventListener("click", () => {
    btnViewCanvas.classList.add("active");
    btnViewVideo.classList.remove("active");
    manimVideoPlayer.style.display = "none";
    canvasBg.style.display = "block";
    animationStage.style.display = "flex";
    subtitleBox.style.display = "flex";
    manimVideoPlayer.pause();
  });

  // Apply Changes from Inspector
  btnUpdateStep.addEventListener("click", () => {
    const step = state.steps[state.activeStepIndex];
    if (step) {
      step.duration = parseFloat(inspDuration.value) || 3.0;
      step.narration = inspNarration.value;
      step.spoken = inspSpoken.value;
      subtitleText.textContent = `"${step.narration}"`;
      appendChatMessage("System", `Updated ${step.id} properties successfully.`);
    }
  });

  // AI Suggestion Chips
  document.querySelectorAll(".ai-chip").forEach(chip => {
    chip.addEventListener("click", () => {
      const action = chip.dataset.action;
      handleAiAction(action);
    });
  });

  // Chat send prompt
  btnSendPrompt.addEventListener("click", submitUserPrompt);
  copilotInput.addEventListener("keydown", (e) => {
    if (e.key === "Enter") submitUserPrompt();
  });

  // Render Modal
  btnRenderModal.addEventListener("click", () => {
    renderModal.classList.add("open");
  });
  btnCloseModal.addEventListener("click", () => {
    renderModal.classList.remove("open");
  });
  btnCancelRender.addEventListener("click", () => {
    renderModal.classList.remove("open");
  });

  // Start Render simulation
  btnStartRender.addEventListener("click", () => {
    renderProgressBox.style.display = "flex";
    btnStartRender.disabled = true;

    let pct = 0;
    const stages = [
      { p: 20, text: "Validating Lesson Schema & Math Proof..." },
      { p: 45, text: "Compiling Animation IR Graph..." },
      { p: 70, text: "Generating Python Manim Community Scene..." },
      { p: 90, text: "Synthesizing Sinhala Narration & Resolving Delays..." },
      { p: 100, text: "Export Completed: output/scene.py & final_lesson.mp4" }
    ];

    let stageIdx = 0;
    const renderTimer = setInterval(() => {
      if (stageIdx < stages.length) {
        progressFill.style.width = `${stages[stageIdx].p}%`;
        progressStatusText.textContent = stages[stageIdx].text;
        stageIdx++;
      } else {
        clearInterval(renderTimer);
        setTimeout(() => {
          renderModal.classList.remove("open");
          renderProgressBox.style.display = "none";
          btnStartRender.disabled = false;
          appendChatMessage("Renderer", "✓ Render Job Completed! Python script generated at output/scene.py.");
        }, 1000);
      }
    }, 600);
  });
}

function handleAiAction(action) {
  const step = state.steps[state.activeStepIndex];
  if (action === "simplify") {
    step.narration = "අපි 1 තියෙන තැන්වල අගයන් විතරක් අරගෙන එකතු කරමු.";
    inspNarration.value = step.narration;
    subtitleText.textContent = `"${step.narration}"`;
    appendChatMessage("Copilot", `Simplified explanation for ${step.id}.`);
  } else if (action === "sinhala") {
    step.narration = "ඔන්න දැන් අපි කලින් ආපු ස්ථානීය අගයන් ටික එකතු කරමු.";
    inspNarration.value = step.narration;
    subtitleText.textContent = `"${step.narration}"`;
    appendChatMessage("Copilot", `Naturalized Sinhala dialect for conversational classroom tone.`);
  } else if (action === "grade8") {
    step.narration = "8 ශ්‍රේණියේ විෂය නිර්දේශයට අනුව ද්විමය සංඛ්‍යා දශම බවට හැරවීම බලමු.";
    inspNarration.value = step.narration;
    subtitleText.textContent = `"${step.narration}"`;
    appendChatMessage("Copilot", `Calibrated terminology for Grade 8 curriculum.`);
  } else if (action === "shorten") {
    step.duration = Math.max(3.0, step.duration - 1.5);
    inspDuration.value = step.duration.toFixed(1);
    appendChatMessage("Copilot", `Reduced step duration to ${step.duration.toFixed(1)}s.`);
  } else if (action === "verify") {
    appendChatMessage("MathVerifier", `✓ Verified: Binary ${state.binary} is exactly equal to Decimal ${state.decimal}. Zero mathematical errors.`);
  }
}

function submitUserPrompt() {
  const query = copilotInput.value.trim();
  if (!query) return;

  appendChatMessage("You", query);
  copilotInput.value = "";

  setTimeout(() => {
    appendChatMessage("Copilot", `Understood: "${query}". Analyzed animation graph and synchronized timeline offsets.`);
  }, 400);
}

function appendChatMessage(sender, text) {
  const div = document.createElement("div");
  div.className = "chat-msg ai";
  div.innerHTML = `<span class="msg-sender">${sender}:</span><p>${text}</p>`;
  chatLogs.appendChild(div);
  chatLogs.scrollTop = chatLogs.scrollHeight;
}
