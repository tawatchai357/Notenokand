(() => {
  const form = document.querySelector('[data-finance-editor]');
  if (!form) return;

  const types = [...form.querySelectorAll('[data-transaction-type]')];
  const category = form.querySelector('[data-category-select]');
  const description = form.querySelector('[data-description-input]');
  const transactionDateLabel = form.querySelector('[data-transaction-date-label]');
  const totalAmountLabel = form.querySelector('[data-total-amount-label]');
  const counterpartyLabel = form.querySelector('[data-counterparty-label]');
  const counterpartyInput = form.querySelector('[data-counterparty-input]');
  const saleFields = [...form.querySelectorAll('[data-bird-nest-sale-field]')];
  const saleRequiredInputs = [...form.querySelectorAll('[data-sale-required]')];

  const selectedType = () => types.find((item) => item.checked)?.value;
  const updateConditionalFields = () => {
    const income = selectedType() === 'Income';
    if (description) description.placeholder = income ? 'เช่น ขายรังนกรอบเดือนนี้ หรือรายรับอื่น' : 'เช่น ค่าไฟฟ้าประจำเดือน';
    if (counterpartyLabel) counterpartyLabel.textContent = income ? 'ผู้ซื้อ/ผู้จ่ายเงิน' : 'ผู้รับเงิน';
    if (counterpartyInput) counterpartyInput.placeholder = income ? 'ชื่อผู้ซื้อหรือลูกค้า' : 'ชื่อร้านค้าหรือผู้รับเงิน';

    const selectedCategory = category?.selectedOptions[0];
    const birdNestSale = income && selectedCategory?.dataset.categoryName === 'ขายรังนก';
    if (transactionDateLabel) transactionDateLabel.textContent = birdNestSale ? 'วันที่ขาย' : (income ? 'วันที่รับเงิน' : 'วันที่จ่าย');
    if (totalAmountLabel) totalAmountLabel.textContent = birdNestSale ? 'ราคารวม (บาท)' : 'จำนวนเงิน (บาท)';
    saleFields.forEach((field) => { field.hidden = !birdNestSale; });
    saleRequiredInputs.forEach((input) => { input.required = birdNestSale; });
    if (counterpartyInput) counterpartyInput.required = birdNestSale;
  };

  const filterCategories = () => {
    const type = selectedType();
    [...category.options].forEach((option, index) => { if (index) option.hidden = option.dataset.type !== type; });
    if (category.selectedOptions[0]?.hidden) category.value = '';
    updateConditionalFields();
  };

  types.forEach((input) => input.addEventListener('change', filterCategories));
  category?.addEventListener('change', updateConditionalFields);
  filterCategories();

  const receipt = form.querySelector('[data-receipt-input]');
  const name = form.querySelector('[data-receipt-name]');
  receipt?.addEventListener('change', () => {
    const file = receipt.files?.[0];
    if (!file) return;
    if (file.size > 10 * 1024 * 1024) {
      receipt.value = '';
      Swal.fire({ icon: 'warning', title: 'ไฟล์ใหญ่เกินไป', text: 'ไฟล์ต้องมีขนาดไม่เกิน 10 MB', confirmButtonColor: '#1565d8' });
      return;
    }
    name.textContent = file.name;
  });

  document.querySelectorAll('[data-delete-receipt]').forEach((button) => button.addEventListener('click', async (event) => {
    event.preventDefault();
    const result = await Swal.fire({ icon: 'warning', title: 'ลบไฟล์แนบ?', showCancelButton: true, confirmButtonText: 'ลบ', cancelButtonText: 'ยกเลิก', confirmButtonColor: '#c93434' });
    if (result.isConfirmed) {
      button.formAction = button.getAttribute('formaction');
      button.formMethod = 'post';
      button.form.requestSubmit(button);
    }
  }));
})();