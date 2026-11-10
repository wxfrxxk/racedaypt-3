// Ask for confirmation before submitting any form marked with data-confirm (delete / cancel actions).
document.addEventListener('submit', function (event) {
    var form = event.target;
    var message = form.getAttribute && form.getAttribute('data-confirm');
    if (message && !window.confirm(message)) {
        event.preventDefault();
    }
});

// Prevent double submissions.
document.querySelectorAll('form').forEach(function (form) {
    form.addEventListener('submit', function () {
        if (form.getAttribute('data-confirm') && !form.dataset.confirmed) { return; }
        form.querySelectorAll('button[type=submit]').forEach(function (btn) {
            setTimeout(function () { btn.disabled = true; }, 0);
        });
    });
});
