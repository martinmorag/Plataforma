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
        case "En Revision":
            return "status-in-review";
        case "En Progreso":
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