// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ===================================================
// HOTEL MANAGEMENT SYSTEM - ADMIN PANEL JAVASCRIPT
// ===================================================

// Wait for DOM to be ready
document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    initializeTooltips();
    
    // Initialize popovers
    initializePopovers();
    
    // Setup form validations
    setupFormValidation();
    
    // Setup delete confirmations
    setupDeleteConfirmations();
    
    // Setup table sorting
    setupTableSorting();
    
    // Setup filter forms
    setupFilterForms();
});

// ===================================================
// TOOLTIPS
// ===================================================
function initializeTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// ===================================================
// POPOVERS
// ===================================================
function initializePopovers() {
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}

// ===================================================
// FORM VALIDATION
// ===================================================
function setupFormValidation() {
    const forms = document.querySelectorAll('form[novalidate]');
    forms.forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
                showAlert('Vui lòng điền đầy đủ thông tin bắt buộc!', 'warning');
            }
            form.classList.add('was-validated');
        }, false);
    });
}

// ===================================================
// DELETE CONFIRMATIONS
// ===================================================
function setupDeleteConfirmations() {
    const deleteButtons = document.querySelectorAll('[data-action="delete"]');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const itemName = this.dataset.itemName || 'item này';
            if (confirm(`Bạn có chắc chắn muốn xóa ${itemName}? Hành động này không thể hoàn tác!`)) {
                window.location.href = this.href;
            }
        });
    });
}

// ===================================================
// TABLE SORTING
// ===================================================
function setupTableSorting() {
    const tables = document.querySelectorAll('.table-sortable');
    tables.forEach(table => {
        const headers = table.querySelectorAll('th[data-sortable="true"]');
        headers.forEach(header => {
            header.style.cursor = 'pointer';
            header.addEventListener('click', function() {
                const columnIndex = Array.from(headers).indexOf(this);
                const rows = Array.from(table.querySelectorAll('tbody tr'));
                const isAsc = this.classList.toggle('sort-asc');
                
                rows.sort((a, b) => {
                    const aVal = a.cells[columnIndex].textContent.trim();
                    const bVal = b.cells[columnIndex].textContent.trim();
                    
                    return isAsc ? 
                        aVal.localeCompare(bVal) : 
                        bVal.localeCompare(aVal);
                });
                
                const tbody = table.querySelector('tbody');
                rows.forEach(row => tbody.appendChild(row));
            });
        });
    });
}

// ===================================================
// FILTER FORMS
// ===================================================
function setupFilterForms() {
    const filterButtons = document.querySelectorAll('[data-action="filter"]');
    filterButtons.forEach(button => {
        button.addEventListener('click', function() {
            const form = this.closest('form');
            if (form) {
                form.submit();
            }
        });
    });
    
    // Auto-submit on filter change
    const filterInputs = document.querySelectorAll('.filter-auto-submit');
    filterInputs.forEach(input => {
        input.addEventListener('change', function() {
            this.closest('form').submit();
        });
    });
}

// ===================================================
// UTILITY FUNCTIONS
// ===================================================

// Show alert message
function showAlert(message, type = 'info') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.role = 'alert';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    const mainContent = document.querySelector('main') || document.body;
    mainContent.insertBefore(alertDiv, mainContent.firstChild);
    
    // Auto-hide after 5 seconds
    setTimeout(() => {
        alertDiv.remove();
    }, 5000);
}

// Format currency
function formatCurrency(value, currency = 'VNĐ') {
    return new Intl.NumberFormat('vi-VN').format(value) + ' ' + currency;
}

// Format date
function formatDate(dateString) {
    const options = { year: 'numeric', month: 'long', day: 'numeric' };
    return new Date(dateString).toLocaleDateString('vi-VN', options);
}

// Confirm action
function confirmAction(message = 'Xác nhận hành động này?') {
    return confirm(message);
}

// Copy to clipboard
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(() => {
        showAlert('Đã sao chép!', 'success');
    }).catch(err => {
        showAlert('Không thể sao chép!', 'danger');
    });
}

// ===================================================
// TABLE SEARCH/FILTER
// ===================================================
function setupTableSearch() {
    const searchInput = document.getElementById('tableSearch');
    if (searchInput) {
        searchInput.addEventListener('keyup', function() {
            const searchTerm = this.value.toLowerCase();
            const table = document.querySelector('table');
            const rows = table.querySelectorAll('tbody tr');
            
            rows.forEach(row => {
                const text = row.textContent.toLowerCase();
                row.style.display = text.includes(searchTerm) ? '' : 'none';
            });
        });
    }
}

// ===================================================
// PAGINATION
// ===================================================
function setupPagination() {
    const paginationLinks = document.querySelectorAll('.pagination a');
    paginationLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            if (this.classList.contains('disabled')) {
                e.preventDefault();
            }
        });
    });
}

// ===================================================
// EXPORT FUNCTIONS
// ===================================================

// Export table to CSV
function exportTableToCSV(filename = 'export.csv') {
    const table = document.querySelector('table');
    let csv = [];
    
    // Headers
    const headers = [];
    table.querySelectorAll('th').forEach(th => {
        headers.push(th.textContent.trim());
    });
    csv.push(headers.join(','));
    
    // Rows
    table.querySelectorAll('tbody tr').forEach(tr => {
        const row = [];
        tr.querySelectorAll('td').forEach(td => {
            row.push('"' + td.textContent.trim() + '"');
        });
        csv.push(row.join(','));
    });
    
    // Download
    const csvContent = csv.join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv' });
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = filename;
    link.click();
}

// Export table to PDF (requires external library)
function exportTableToPDF(filename = 'export.pdf') {
    showAlert('PDF export requires additional library', 'warning');
}

// ===================================================
// LOADING STATES
// ===================================================
function showLoading(element) {
    const spinner = document.createElement('div');
    spinner.className = 'spinner-border spinner-border-sm me-2';
    element.prepend(spinner);
    element.disabled = true;
}

function hideLoading(element) {
    const spinner = element.querySelector('.spinner-border');
    if (spinner) spinner.remove();
    element.disabled = false;
}

// ===================================================
// MODAL HELPERS
// ===================================================
function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        const bsModal = bootstrap.Modal.getInstance(modal);
        if (bsModal) bsModal.hide();
    }
}

// ===================================================
// INITIALIZE ON LOAD
// ===================================================
window.addEventListener('load', function() {
    setupTableSearch();
    setupPagination();
});
