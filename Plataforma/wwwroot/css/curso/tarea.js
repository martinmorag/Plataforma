// Video progress

const videoPlayerId = 'my-video';
const videoElement = document.getElementById(videoPlayerId);
const tareaId = videoElement.dataset.tareaId;
let initialTime = parseFloat(videoElement.dataset.initialTime || '0');
let videoCompleted = videoElement.dataset.completed === 'true';

// --- Video.js Player Initialization and Progress Tracking ---
function initializeVideoPlayer() {
    // Destroy any existing player instance to prevent multiple players on the same ID
    if (videojs.getPlayer(videoPlayerId)) {
        videojs.getPlayer(videoPlayerId).dispose();
    }

    const player = videojs(videoPlayerId);

    let lastSavedTime = initialTime;
    let lastKnownDuration = 0;
    let isSeeking = false; // Flag to manage seeking behavior
    let hasAttemptedSkip = false; // Flag to indicate if a forward skip was attempted

    // Event listener for when the video metadata is loaded
    player.on('loadedmetadata', function () {
        lastKnownDuration = player.duration();
        // Only set current time if not already completed and initial time is available
        if (!videoCompleted && initialTime > 0) {
            player.currentTime(initialTime);
            player.play(); // Auto-play from where they left off
        }
    });

    // Event listeners for preventing skipping forward
    player.on('seeking', function () {
        isSeeking = true;
    });

    player.on('seeked', function () {
        isSeeking = false;
        const currentTime = player.currentTime();
        // If current time is significantly ahead of last saved time, and it's a forward skip
        // Consider a small buffer (e.g., 2 seconds) to account for minor discrepancies
        if (currentTime > lastSavedTime + 2 && lastSavedTime > 0 && !videoCompleted) {
            player.currentTime(lastSavedTime); // Rewind to last saved position
            player.pause();
            alert('No puedes adelantar el video. Debes verlo secuencialmente.');
            hasAttemptedSkip = true; // Mark that a skip was attempted
        }
        // Reset last saved time if a skip occurred and was corrected
        lastSavedTime = player.currentTime();
    });

    // Event listener for time updates (fired frequently)
    player.on('timeupdate', function () {
        // Only save progress if not seeking and not completed
        if (!isSeeking && !videoCompleted) {
            const currentTime = player.currentTime();
            // Only save if current time has truly advanced beyond last saved time
            if (currentTime > lastSavedTime + 1) { // Save every 1 second of progress
                lastSavedTime = currentTime;
                sendVideoProgress(tareaId, currentTime, lastKnownDuration);
            }
        }
    });

    // Event listener for video completion
    player.on('ended', function () {
        if (!videoCompleted) { // Ensure it's marked complete only once
            sendVideoCompleted(tareaId, lastKnownDuration);
            videoCompleted = true; // Update client-side status
            alert('¡Video completado! Tarea marcada como completada.');
            // Optionally, update the UI to show the task as completed
            document.getElementById('current-tarea-details').classList.add('task-completed');
            // You might want to refresh the class details to update the task status in the sidebar
            // (requires emitting an event or direct manipulation of the sidebar)
            // Example: A global event for sidebar update
            // window.dispatchEvent(new CustomEvent('taskCompleted', { detail: { tareaId: tareaId } }));
        }
    });
}

// --- API Calls ---

// Function to send video progress to the backend
async function sendVideoProgress(tareaId, currentTime, videoDuration) {
    try {
        const response = await fetch('/api/Tarea/SaveVideoProgress', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({
                tareaId: tareaId,
                currentTime: currentTime,
                videoDuration: videoDuration
            })
        });
        if (!response.ok) {
            console.error('Failed to save video progress:', response.statusText);
            // Handle error (e.g., show a small non-intrusive message)
        }
    } catch (error) {
        console.error('Network error while saving video progress:', error);
    }
}

// Function to mark video task as completed
async function sendVideoCompleted(tareaId, videoDuration) {
    try {
        const response = await fetch('/api/Tarea/MarkVideoTaskCompleted', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({
                tareaId: tareaId,
                videoDuration: videoDuration
            })
        });
        if (!response.ok) {
            console.error('Failed to mark task completed:', response.statusText);
            // Handle error
        }
    } catch (error) {
        console.error('Network error while marking task completed:', error);
    }
}

// Initialize the player when the partial view is inserted into the DOM
document.addEventListener('DOMContentLoaded', initializeVideoPlayer); // For initial page load
// For AJAX loaded content, you might need to call this explicitly after innerHTML assignment
// For this specific setup, `initializeVideoPlayer()` will be called when the script is parsed,
// and since it disposes existing players, it should work for repeated loads.