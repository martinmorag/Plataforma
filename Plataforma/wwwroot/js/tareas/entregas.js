const evaluationModalOverlay = document.getElementById('evaluationModalOverlay');
const evaluationModal = document.getElementById('evaluationModal'); // The inner modal content
const closeModalButton = document.getElementById('closeModalButton');
const cancelEvaluationBtn = document.getElementById('cancelEvaluationBtn');
const modalEntregaId = document.getElementById('modalEntregaId');
const modalStatus = document.getElementById('modalStatus');
const modalComments = document.getElementById('modalComments');
const evaluationMessage = document.getElementById('evaluationMessage');
const saveEvaluationBtn = document.getElementById('saveEvaluationBtn');

// Show modal function
function showModal() {
    evaluationModalOverlay.style.display = 'flex'; // Make it visible and centered
    // Optional: Add a class for animating the modal appearance
    evaluationModal.classList.add('is-open');
}

// Hide modal function
function hideModal() {
    evaluationModalOverlay.style.display = 'none';
    evaluationModal.classList.remove('is-open'); // Remove animation class
}

// Handle the 'Evaluar' button click
document.querySelectorAll('.evaluate-button').forEach(button => {
    button.addEventListener('click', function () {
        const entregaId = this.dataset.entregaId;
        const currentStatus = this.dataset.currentStatus;
        const currentComments = this.dataset.currentComments;

        modalEntregaId.value = entregaId;
        modalStatus.value = currentStatus;
        modalComments.value = currentComments === 'null' ? '' : currentComments;
        evaluationMessage.innerHTML = ''; // Clear previous messages

        showModal();
    });
});

// Handle modal close buttons
closeModalButton.addEventListener('click', hideModal);
cancelEvaluationBtn.addEventListener('click', hideModal);

// Close modal when clicking outside of it (optional, but good UX)
evaluationModalOverlay.addEventListener('click', function (event) {
    if (event.target === evaluationModalOverlay) {
        hideModal();
    }
});



function getStatusClass(status) {
    switch (status) {
        case "Aprobado":
            return "status-approved";
        case "EnRevision":
            return "status-in-review";
        case "EnProgreso":
            return "status-in-progress";
        case "Rehacer": // Assuming this is also a possible status from the backend or dropdown
            return "status-redo";
        case "Reprobado":
            return "status-rejected";
        case "Evaluado": // IMPORTANT: Add this if your backend will return "Evaluado"
            return "status-evaluated"; // Define this CSS class in your CSS if it doesn't exist
        default:
            return "status-unknown";
    }
}



// Handle the 'Guardar Evaluación' button click within the modal
saveEvaluationBtn.addEventListener('click', async function () {
    const entregaId = modalEntregaId.value;
    const status = modalStatus.value;
    const comments = modalComments.value;
    console.log("entrega", entregaId)
    console.log("status", status)
    console.log("comments", comments)
    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    evaluationMessage.innerHTML = `<div class="loading-spinner"><i class="fa-solid fa-spinner fa-spin-pulse"></i> Guardando...</div>`;

    try {
        const response = await fetch('/api/ApiProfesores/EvaluarEntrega', { // Still points to your API controller
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': antiForgeryToken
            },
            body: JSON.stringify({
                EntregaId: entregaId,
                Estado: status,
                ComentariosProfesor: comments
            })
        });

        const result = await response.json();

        if (response.ok) {
            evaluationMessage.innerHTML = `<div class="success-message">${result.message || 'Evaluación guardada.'}</div>`;
            // Update the status in the table row
            const row = document.querySelector(`button[data-entrega-id="${entregaId}"]`).closest('tr');
            if (row) {
                const statusBadge = row.querySelector('td:nth-child(2) span'); // Assuming status is the second <td> with a span
                if (statusBadge) {
                    statusBadge.textContent = result.newStatus;
                    // Update class for new status color
                    statusBadge.className = 'status-badge ' + getStatusClass(result.newStatus);
                }
                const commentsCell = row.querySelector('td:nth-child(4)');
                if (commentsCell) {
                    commentsCell.textContent = comments || 'Sin comentarios';
                }
            }
            // Close modal after a short delay to show success message
            setTimeout(hideModal, 1500);
        } else {
            evaluationMessage.innerHTML = `<div class="error-message">Error: ${result.message || 'No se pudo guardar la evaluación.'}</div>`;
        }
    } catch (error) {
        console.error('Error saving evaluation:', error);
        evaluationMessage.innerHTML = '<div class="error-message">Error de red al guardar la evaluación.</div>';
    }
});











document.querySelectorAll('.download-button').forEach(button => {
    button.addEventListener('click', async function (event) {
        event.preventDefault(); // Prevent the default link behavior
        console.log("triggereed")

        const entregaId = this.dataset.entregaId;
        const fileName = this.dataset.fileName; // Get the original file name
        const downloadUrl = `/api/ApiProfesores/DownloadSubmittedFile/${entregaId}`;

        // Optional: Provide visual feedback (e.g., spinning icon)
        const originalHtml = this.innerHTML;
        this.innerHTML = '<i class="fa-solid fa-spinner fa-spin-pulse"></i> Descargando...';
        this.disabled = true; // Disable button during download

        try {
            const response = await fetch(downloadUrl, {
                method: 'GET',
                // Add any necessary headers, e.g., authorization if using JWT/bearer tokens
                // 'Authorization': `Bearer ${yourAuthToken}`
            });

            if (!response.ok) {
                // Handle HTTP errors (e.g., 404 Not Found, 401 Unauthorized)
                const errorText = await response.text();
                console.error('Download failed:', response.status, errorText);
                alert(`Error al descargar el archivo: ${response.statusText || 'Error de red.'}`);
                return;
            }

            // Get the Content-Disposition header for a more accurate filename if available
            const contentDisposition = response.headers.get('Content-Disposition');
            let actualFileName = fileName; // Start with the filename from data attribute
            if (contentDisposition) {
                const fileNameMatch = contentDisposition.match(/filename\*?=(?:UTF-8'')?([^;]+)/i);
                if (fileNameMatch && fileNameMatch[1]) {
                    try {
                        actualFileName = decodeURIComponent(fileNameMatch[1].replace(/^"|"$/g, ''));
                    } catch (e) {
                        console.warn('Could not decode filename from Content-Disposition, using provided name.');
                        actualFileName = fileNameMatch[1].replace(/^"|"$/g, ''); // Fallback for decoding
                    }
                }
            }

            const blob = await response.blob(); // Get the response as a Blob

            // Create a temporary URL for the Blob
            const url = window.URL.createObjectURL(blob);

            // Create a temporary <a> element
            const a = document.createElement('a');
            a.style.display = 'none'; // Hide the link
            a.href = url;
            a.download = actualFileName; // Set the desired filename for the download

            document.body.appendChild(a); // Append to body (required for Firefox)
            a.click(); // Programmatically click the link to trigger download

            window.URL.revokeObjectURL(url); // Clean up the Blob URL

        } catch (error) {
            console.error('Network or unknown error during download:', error);
            alert('Error de red al intentar descargar el archivo.');
        } finally {
            // Restore button state
            this.innerHTML = originalHtml;
            this.disabled = false;
        }
    })
});