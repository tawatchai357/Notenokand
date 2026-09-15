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
    const width = Math.max(680, wrap.clientWidth);
    const height = 330;
    const ratio = Math.max(1, window.devicePixelRatio || 1);
    canvas.width = Math.round(width * ratio);
    canvas.height = Math.round(height * ratio);
    canvas.style.width = `${width}px`;
    canvas.style.height = `${height}px`;
    const context = canvas.getContext('2d');
    context.setTransform(ratio, 0, 0, ratio, 0, 0);
    context.clearRect(0, 0, width, height);

    const left = 64, right = 18, top = 18, bottom = 42;
    const chartWidth = width - left - right;
    const chartHeight = height - top - bottom;
    const maximum = Math.max(...income, ...expense, 0);
    const ceiling = maximum > 0 ? maximum * 1.12 : 1;
    context.font = '12px "IBM Plex Sans Thai", sans-serif';
    context.textBaseline = 'middle';

    for (let line = 0; line <= 4; line += 1) {
      const y = top + chartHeight - (chartHeight * line / 4);
      context.strokeStyle = '#e3ebf3';
      context.lineWidth = 1;
      context.beginPath(); context.moveTo(left, y); context.lineTo(width - right, y); context.stroke();
      context.fillStyle = '#71869a'; context.textAlign = 'right';
      context.fillText(compact.format(ceiling * line / 4), left - 9, y);
    }

    if (maximum === 0) {
      context.fillStyle = '#71869a'; context.textAlign = 'center'; context.font = '500 15px "IBM Plex Sans Thai", sans-serif';
      context.fillText('ยังไม่มีข้อมูลรายรับ–รายจ่ายในปีนี้', left + chartWidth / 2, top + chartHeight / 2);
    }

    const groupWidth = chartWidth / 12;
    const barWidth = Math.min(18, groupWidth * .28);
    labels.forEach((label, index) => {
      const center = left + groupWidth * (index + .5);
      const incomeHeight = maximum > 0 ? chartHeight * income[index] / ceiling : 0;
      const expenseHeight = maximum > 0 ? chartHeight * expense[index] / ceiling : 0;
      context.fillStyle = '#16845f';
      context.beginPath(); context.roundRect(center - barWidth - 2, top + chartHeight - incomeHeight, barWidth, incomeHeight, [4, 4, 0, 0]); context.fill();
      context.fillStyle = '#d34a4a';
      context.beginPath(); context.roundRect(center + 2, top + chartHeight - expenseHeight, barWidth, expenseHeight, [4, 4, 0, 0]); context.fill();
      context.fillStyle = '#536c82'; context.textAlign = 'center'; context.font = '12px "IBM Plex Sans Thai", sans-serif';
      context.fillText(label, center, height - 18);
    });
    geometry = { left, groupWidth, width };
  };

  canvas.addEventListener('pointermove', (event) => {
    if (!geometry) return;
    const rect = canvas.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const index = Math.floor((x - geometry.left) / geometry.groupWidth);
    if (index < 0 || index > 11) { tooltip.hidden = true; return; }
    tooltip.innerHTML = `<strong>${labels[index]}</strong><span class="income">รายรับ ${money.format(income[index])}</span><span class="expense">รายจ่าย ${money.format(expense[index])}</span>`;
    tooltip.style.left = `${geometry.left + geometry.groupWidth * (index + .5)}px`;
    tooltip.hidden = false;
  });
  canvas.addEventListener('pointerleave', () => { tooltip.hidden = true; });

  const observer = new ResizeObserver(draw);
  observer.observe(wrap);
  draw();
})();