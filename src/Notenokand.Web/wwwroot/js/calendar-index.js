(() => {
  const dayButtons = [...document.querySelectorAll('[data-calendar-day]')];
  const cards = [...document.querySelectorAll('[data-appointment-card]')];
  const clearButton = document.querySelector('[data-clear-day]');
  const heading = document.querySelector('[data-appointment-heading]');
  const empty = document.querySelector('[data-no-appointment]');
  const createForDay = document.querySelector('[data-create-for-day]');

  const showDate = (date) => {
    let visible = 0;
    cards.forEach((card) => {
      const show = !date || card.dataset.date === date;
      card.hidden = !show;
      if (show) visible += 1;
    });
    dayButtons.forEach((button) => button.classList.toggle('selected', Boolean(date) && button.dataset.date === date));
    if (clearButton) clearButton.hidden = !date;
    if (heading) heading.textContent = date ? `นัดหมายวันที่ ${new Intl.DateTimeFormat('th-TH', { day: 'numeric', month: 'long', year: 'numeric' }).format(new Date(`${date}T12:00:00`))}` : 'ทั้งหมดในเดือนนี้';
    if (empty) empty.hidden = visible > 0;
    if (createForDay && date) createForDay.href = `${createForDay.pathname}?date=${encodeURIComponent(date)}`;
  };

  dayButtons.forEach((button) => button.addEventListener('click', () => showDate(button.dataset.date)));
  clearButton?.addEventListener('click', () => showDate(null));

  document.querySelectorAll('[data-appointment-delete]').forEach((form) => form.addEventListener('submit', async (event) => {
    event.preventDefault();
    const result = await Swal.fire({ icon: 'warning', title: 'ลบนัดหมายนี้?', text: form.dataset.name || '', showCancelButton: true, confirmButtonText: 'ลบนัดหมาย', cancelButtonText: 'ยกเลิก', confirmButtonColor: '#c93434' });
    if (result.isConfirmed) form.submit();
  }));
})();