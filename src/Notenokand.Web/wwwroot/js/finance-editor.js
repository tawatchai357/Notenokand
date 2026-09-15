(() => {
  const form = document.querySelector('[data-finance-editor]'); if (!form) return;
  const types = [...form.querySelectorAll('[data-transaction-type]')];
  const category = form.querySelector('[data-category-select]');
  const filterCategories = () => {
    const type = types.find((x) => x.checked)?.value;
    [...category.options].forEach((option, index) => { if (index) option.hidden = option.dataset.type !== type; });
    if (category.selectedOptions[0]?.hidden) category.value = '';
  };
  types.forEach((input) => input.addEventListener('change', filterCategories)); filterCategories();
  const receipt = form.querySelector('[data-receipt-input]'); const name = form.querySelector('[data-receipt-name]');
  receipt?.addEventListener('change', () => { const file = receipt.files?.[0]; if (!file) return; if (file.size > 10 * 1024 * 1024) { receipt.value = ''; Swal.fire({ icon: 'warning', title: 'ไฟล์ใหญ่เกินไป', text: 'ไฟล์ต้องมีขนาดไม่เกิน 10 MB', confirmButtonColor: '#1565d8' }); return; } name.textContent = file.name; });
  document.querySelectorAll('[data-delete-receipt]').forEach((button) => button.addEventListener('click', async (event) => { event.preventDefault(); const result = await Swal.fire({ icon: 'warning', title: 'ลบไฟล์แนบ?', showCancelButton: true, confirmButtonText: 'ลบ', cancelButtonText: 'ยกเลิก', confirmButtonColor: '#c93434' }); if (result.isConfirmed) { button.formAction = button.getAttribute('formaction'); button.formMethod = 'post'; button.form.requestSubmit(button); } }));
})();