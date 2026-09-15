(() => {
  document.querySelectorAll('[data-finance-delete]').forEach((form) => form.addEventListener('submit', async (event) => {
    event.preventDefault();
    const result = await Swal.fire({ icon: 'warning', title: 'ลบรายการนี้?', text: form.dataset.name || '', showCancelButton: true, confirmButtonText: 'ลบรายการ', cancelButtonText: 'ยกเลิก', confirmButtonColor: '#c93434' });
    if (result.isConfirmed) form.submit();
  }));
})();