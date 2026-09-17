(() => {
    document.querySelectorAll('[data-complete-appointment]').forEach(form => {
        form.addEventListener('submit', async event => {
            event.preventDefault();
            const accepted = window.Swal
                ? (await Swal.fire({ title: 'ยืนยันว่าทำงานเสร็จแล้ว?', text: 'รายการนี้จะออกจากรายการแจ้งเตือน', icon: 'question', showCancelButton: true, confirmButtonText: 'เสร็จแล้ว', cancelButtonText: 'ยกเลิก' })).isConfirmed
                : window.confirm('ยืนยันว่าทำงานเสร็จแล้ว?');
            if (!accepted) return;
            form.querySelector('button[type="submit"]').disabled = true;
            HTMLFormElement.prototype.submit.call(form);
        });
    });
})();
