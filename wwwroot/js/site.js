// Mostrar mensaje de error en la parte superior de la página
function mostrarError(mensaje) {
    const container = document.getElementById('alertContainer');

    // Evitar apilar alertas si el usuario envía el form varias veces
    container.innerHTML = '';

    const div = document.createElement('div');
    div.className = 'alert alert-danger alert-dismissible fade show';
    div.setAttribute('role', 'alert');
    div.innerHTML = `
        <i class="bi bi-exclamation-triangle-fill me-2"></i> ${mensaje}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;

    container.appendChild(div);
    div.scrollIntoView({ behavior: 'smooth', block: 'center' });
}

//DataTables JS
$(document).ready(function () {
    $('#miTabla').DataTable({
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json"
        }
    });
});

// SweetAlert2 para confirmación de eliminación
document.querySelectorAll('.form-eliminar').forEach(form => {
    form.addEventListener('submit', function (e) {
        e.preventDefault();
        const nombre = this.dataset.nombre;

        Swal.fire({
            title: '¿Eliminar producto?',
            text: `Se eliminará "${nombre}". Esta acción no se puede deshacer.`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar'
        }).then(result => {
            if (result.isConfirmed) {
                this.submit();
            }
        });
    });
});
