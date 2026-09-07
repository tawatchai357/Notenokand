(() => {
  const monthNames = ['มกราคม','กุมภาพันธ์','มีนาคม','เมษายน','พฤษภาคม','มิถุนายน','กรกฎาคม','สิงหาคม','กันยายน','ตุลาคม','พฤศจิกายน','ธันวาคม'];
  const shortDays = ['จ','อ','พ','พฤ','ศ','ส','อา'];
  let activePicker = null;

  const pad = value => String(value).padStart(2, '0');
  const toIsoDate = date => `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
  const sameDay = (left, right) => left && right && left.getFullYear() === right.getFullYear() && left.getMonth() === right.getMonth() && left.getDate() === right.getDate();
  const parseValue = value => {
    const match = /^(\d{4})-(\d{2})-(\d{2})(?:T(\d{2}):(\d{2}))?/.exec(value || '');
    if (!match) return null;
    const date = new Date(Number(match[1]), Number(match[2]) - 1, Number(match[3]));
    return Number.isNaN(date.getTime()) ? null : { date, time: match[4] ? `${match[4]}:${match[5]}` : '09:00' };
  };
  const formatThai = (date, time, includeTime) => date ? `${date.getDate()} ${monthNames[date.getMonth()]} ${date.getFullYear() + 543}${includeTime ? ` เวลา ${time} น.` : ''}` : '';

  document.querySelectorAll('[data-thai-datepicker], [data-thai-datetimepicker]').forEach(display => {
    const target = document.getElementById(display.dataset.target);
    if (!target) return;
    const includeTime = display.hasAttribute('data-thai-datetimepicker');
    const parsed = parseValue(target.value);
    let selectedDate = parsed?.date ?? null;
    let selectedTime = parsed?.time ?? '09:00';
    let viewDate = selectedDate ? new Date(selectedDate) : new Date();
    const field = display.closest('.thai-date-field') ?? display.parentElement;
    const panel = document.createElement('div');
    panel.className = 'thai-calendar'; panel.hidden = true; panel.setAttribute('role', 'dialog'); panel.setAttribute('aria-label', 'ปฏิทินเลือกวันที่');
    field.appendChild(panel);

    const syncDisplay = () => { display.value = formatThai(selectedDate, selectedTime, includeTime); };
    const saveValue = () => {
      target.value = selectedDate ? `${toIsoDate(selectedDate)}${includeTime ? `T${selectedTime}` : ''}` : '';
      target.dispatchEvent(new Event('input', { bubbles: true })); target.dispatchEvent(new Event('change', { bubbles: true })); syncDisplay();
    };
    const close = () => { panel.hidden = true; display.setAttribute('aria-expanded', 'false'); if (activePicker === close) activePicker = null; };
    const open = () => { if (activePicker && activePicker !== close) activePicker(); activePicker = close; panel.hidden = false; display.setAttribute('aria-expanded', 'true'); render(); };
    const chooseToday = () => { selectedDate = new Date(); viewDate = new Date(selectedDate); saveValue(); if (!includeTime) close(); else render(); };

    const render = () => {
      const year = viewDate.getFullYear(), month = viewDate.getMonth();
      const firstOffset = (new Date(year, month, 1).getDay() + 6) % 7;
      const daysInMonth = new Date(year, month + 1, 0).getDate();
      const today = new Date();
      let days = '';
      for (let index = 0; index < firstOffset; index++) days += '<span class="thai-calendar-blank"></span>';
      for (let day = 1; day <= daysInMonth; day++) {
        const date = new Date(year, month, day);
        const classes = ['thai-calendar-day'];
        if (sameDay(date, today)) classes.push('today');
        if (sameDay(date, selectedDate)) classes.push('selected');
        days += `<button type="button" class="${classes.join(' ')}" data-day="${day}" aria-label="${day} ${monthNames[month]} ${year + 543}">${day}</button>`;
      }
      const timeControl = includeTime ? `<label class="thai-time-label">เวลา <input type="time" class="thai-time-input" value="${selectedTime}" /></label><button type="button" class="thai-calendar-confirm" data-confirm>ตกลง</button>` : '';
      panel.innerHTML = `<div class="thai-calendar-header"><button type="button" data-prev aria-label="เดือนก่อน">‹</button><strong>${monthNames[month]} ${year + 543}</strong><button type="button" data-next aria-label="เดือนถัดไป">›</button></div><div class="thai-calendar-weekdays">${shortDays.map(day => `<span>${day}</span>`).join('')}</div><div class="thai-calendar-days">${days}</div><div class="thai-calendar-footer"><button type="button" data-today>วันนี้</button><button type="button" data-clear>ล้าง</button>${timeControl}</div>`;
    };

    panel.addEventListener('click', event => {
      const button = event.target.closest('button'); if (!button) return;
      if (button.hasAttribute('data-prev')) { viewDate = new Date(viewDate.getFullYear(), viewDate.getMonth() - 1, 1); render(); return; }
      if (button.hasAttribute('data-next')) { viewDate = new Date(viewDate.getFullYear(), viewDate.getMonth() + 1, 1); render(); return; }
      if (button.hasAttribute('data-today')) { chooseToday(); return; }
      if (button.hasAttribute('data-clear')) { selectedDate = null; saveValue(); close(); return; }
      if (button.hasAttribute('data-confirm')) { if (selectedDate) { selectedTime = panel.querySelector('.thai-time-input')?.value || selectedTime; saveValue(); } close(); return; }
      if (button.dataset.day) { selectedDate = new Date(viewDate.getFullYear(), viewDate.getMonth(), Number(button.dataset.day)); saveValue(); if (includeTime) render(); else close(); }
    });
    panel.addEventListener('change', event => { if (event.target.matches('.thai-time-input')) selectedTime = event.target.value || '09:00'; });
    display.addEventListener('click', open); display.setAttribute('aria-haspopup', 'dialog'); display.setAttribute('aria-expanded', 'false');
    field.querySelector('[data-thai-date-trigger]')?.addEventListener('click', open);
    display.addEventListener('keydown', event => { if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); open(); } if (event.key === 'Escape') close(); });
    document.addEventListener('click', event => { if (!field.contains(event.target)) close(); });
    syncDisplay();
  });
})();