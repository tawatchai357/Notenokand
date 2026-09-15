(() => {
  const modal = document.getElementById('receiptPreviewModal');
  if (!modal) return;

  const frame = modal.querySelector('[data-receipt-frame]');
  const title = modal.querySelector('#receiptPreviewTitle');

  modal.addEventListener('show.bs.modal', (event) => {
    const trigger = event.relatedTarget;
    if (!(trigger instanceof HTMLElement)) return;
    frame.src = trigger.dataset.receiptUrl || 'about:blank';
    title.textContent = trigger.dataset.receiptName || 'ดูหลักฐาน';
  });

  modal.addEventListener('hidden.bs.modal', () => {
    frame.src = 'about:blank';
    title.textContent = 'ดูหลักฐาน';
  });
})();