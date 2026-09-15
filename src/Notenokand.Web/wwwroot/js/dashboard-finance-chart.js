(() => {
  const canvas = document.querySelector('[data-annual-finance-chart]');
  if (!canvas) return;
  const labels = JSON.parse(canvas.dataset.labels || '[]');
  const income = JSON.parse(canvas.dataset.income || '[]').map(Number);
  const expense = JSON.parse(canvas.dataset.expense || '[]').map(Number);
  const wrap = canvas.parentElement;
  const tooltip = document.createElement('div');
  tooltip.className = 'annual-chart-tooltip';
  tooltip.hidden = true;
  wrap.appendChild(tooltip);
  let geometry = null;

  const money = new Intl.NumberFormat('th-TH', { style: 'currency', currency: 'THB', minimumFractionDigits: 2 });
  const compact = new Intl.NumberFormat('th-TH', { notation: 'compact', maximumFractionDigits: 1 });

  const draw = () => {
    const width = Math.max(300, wrap.clientWidth);
    const height = 540;
    const ratio = Math.max(1, window.devicePixelRatio || 1);
    canvas.width = Math.round(width * ratio);
    canvas.height = Math.round(height * ratio);
    canvas.style.width = `${width}px`;
    canvas.style.height = `${height}px`;
    const context = canvas.getContext('2d');
    context.setTransform(ratio, 0, 0, ratio, 0, 0);
    context.clearRect(0, 0, width, height);

    const left = width < 480 ? 48 : 58;
    const right = 16, top = 28, bottom = 34;
    const chartWidth = width - left - right;
    const chartHeight = height - top - bottom;
    const maximum = Math.max(...income, ...expense, 0);
    const ceiling = maximum > 0 ? maximum * 1.12 : 1;
    const groupHeight = chartHeight / 12;
    const barHeight = Math.min(11, groupHeight * .3);
    context.font = '12px "IBM Plex Sans Thai", sans-serif';
    context.textBaseline = 'middle';

    for (let line = 0; line <= 4; line += 1) {
      const x = left + chartWidth * line / 4;
      context.strokeStyle = '#e3ebf3';
      context.lineWidth = 1;
      context.beginPath(); context.moveTo(x, top); context.lineTo(x, height - bottom); context.stroke();
      context.fillStyle = '#71869a'; context.textAlign = line === 0 ? 'left' : (line === 4 ? 'right' : 'center');
      context.fillText(compact.format(ceiling * line / 4), x, height - 14);
    }

    if (maximum === 0) {
      context.fillStyle = '#71869a'; context.textAlign = 'center'; context.font = '500 14px "IBM Plex Sans Thai", sans-serif';
      context.fillText('ยังไม่มีข้อมูลรายรับ–รายจ่ายในปีนี้', left + chartWidth / 2, top + chartHeight / 2);
    }

    labels.forEach((label, index) => {
      const center = top + groupHeight * (index + .5);
      const incomeWidth = maximum > 0 ? chartWidth * income[index] / ceiling : 0;
      const expenseWidth = maximum > 0 ? chartWidth * expense[index] / ceiling : 0;
      context.fillStyle = '#536c82'; context.textAlign = 'right'; context.font = '12px "IBM Plex Sans Thai", sans-serif';
      context.fillText(label, left - 9, center);
      context.fillStyle = '#16845f';
      context.beginPath(); context.roundRect(left, center - barHeight - 2, incomeWidth, barHeight, [0, 4, 4, 0]); context.fill();
      context.fillStyle = '#d34a4a';
      context.beginPath(); context.roundRect(left, center + 2, expenseWidth, barHeight, [0, 4, 4, 0]); context.fill();
    });
    geometry = { top, groupHeight, height };
  };

  const showTooltip = (event) => {
    if (!geometry) return;
    const rect = canvas.getBoundingClientRect();
    const y = event.clientY - rect.top;
    const index = Math.floor((y - geometry.top) / geometry.groupHeight);
    if (index < 0 || index > 11) { tooltip.hidden = true; return; }
    tooltip.innerHTML = `<strong>${labels[index]}</strong><span class="income">รายรับ ${money.format(income[index])}</span><span class="expense">รายจ่าย ${money.format(expense[index])}</span>`;
    tooltip.style.left = `${Math.min(rect.width - 95, Math.max(95, event.clientX - rect.left))}px`;
    tooltip.style.top = `${Math.max(8, geometry.top + geometry.groupHeight * index - 18)}px`;
    tooltip.hidden = false;
  };
  canvas.addEventListener('pointermove', showTooltip);
  canvas.addEventListener('pointerdown', showTooltip);
  canvas.addEventListener('pointerleave', () => { tooltip.hidden = true; });

  const observer = new ResizeObserver(draw);
  observer.observe(wrap);
  draw();
})();