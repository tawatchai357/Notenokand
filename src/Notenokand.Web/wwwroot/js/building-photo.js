(() => {
  document.querySelectorAll('[data-photo-form]').forEach((form) => {
    const input = form.querySelector('[data-photo-input]');
    const preview = form.querySelector('[data-photo-preview]');
    if (!input || !preview) return;
    let objectUrl;
    input.addEventListener('change', () => {
      const file = input.files?.[0];
      if (!file) return;
      if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || file.size > 5 * 1024 * 1024) {
        input.value = '';
        const message = file.size > 5 * 1024 * 1024 ? 'รูปต้องมีขนาดไม่เกิน 5 MB' : 'รองรับเฉพาะรูป JPG, PNG หรือ WEBP';
        if (window.Swal) Swal.fire({ icon: 'warning', title: 'เลือกรูปไม่ได้', text: message, confirmButtonColor: '#1565d8' });
        else alert(message);
        return;
      }
      if (objectUrl) URL.revokeObjectURL(objectUrl);
      objectUrl = URL.createObjectURL(file);
      let image = preview.querySelector('img');
      if (!image) { image = document.createElement('img'); preview.prepend(image); }
      image.src = objectUrl; image.alt = 'ตัวอย่างรูปตึกที่เลือก';
      preview.classList.add('has-image');
      const title = preview.querySelector('strong'); if (title) title.textContent = 'เลือกรูปแล้ว';
      const hint = preview.querySelector('small'); if (hint) hint.textContent = file.name;
    });
    window.addEventListener('beforeunload', () => { if (objectUrl) URL.revokeObjectURL(objectUrl); });
  });
})();