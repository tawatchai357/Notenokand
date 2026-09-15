(() => {
  document.querySelectorAll('[data-category-toggle]').forEach((form) => form.addEventListener('submit', async (event) => {
    event.preventDefault(); const closing = form.dataset.active === 'true';
    const result = await Swal.fire({ icon: 'question', title: closing ? 'ปิดใช้หมวดหมู่นี้?' : 'เปิดใช้หมวดหมู่นี้?', text: form.dataset.name || '', showCancelButton: true, confirmButtonText: closing ? 'ปิดใช้' : 'เปิดใช้', cancelButtonText: 'ยกเลิก', confirmButtonColor: closing ? '#c93434' : '#16845f' });
    if (result.isConfirmed) form.submit();
  }));
})();