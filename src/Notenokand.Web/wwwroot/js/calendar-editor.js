(() => {
  const form = document.querySelector('[data-appointment-editor]');
  if (!form) return;
  const types = [...form.querySelectorAll('[data-appointment-type]')];
  const title = form.querySelector('[data-appointment-title]');
  const location = form.querySelector('[data-appointment-location]');
  const update = () => {
    const type = types.find((item) => item.checked)?.value;
    if (type === 'Harvest') {
      title.placeholder = 'เช่น เก็บรังนกรอบเดือนนี้';
      location.placeholder = 'เช่น ชั้น 3 หรือโซนห้องนก';
    } else if (type === 'BirdNestSale') {
      title.placeholder = 'เช่น นัดขายรังนกล็อตเดือนนี้';
      location.placeholder = 'เช่น ลานประมูลสุราษฎร์ธานี หรือผู้ซื้อที่บ้าน';
    } else {
      title.placeholder = 'เช่น ตรวจเช็กเครื่องเสียงหรือระบบน้ำ';
      location.placeholder = 'เช่น ห้องควบคุม หรือชั้น 2';
    }
  };
  types.forEach((item) => item.addEventListener('change', update));
  update();
})();