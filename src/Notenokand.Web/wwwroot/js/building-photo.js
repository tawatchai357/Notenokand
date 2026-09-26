(() => {
  const maxUploadBytes = 5 * 1024 * 1024;
  const maxSourceBytes = 25 * 1024 * 1024;
  const directTypes = new Set(['image/jpeg', 'image/png', 'image/webp']);
  const directExtensions = /\.(jpe?g|png|webp)$/i;

  function alertUser(title, text) {
    if (window.Swal) return Swal.fire({ icon: 'warning', title, text, confirmButtonColor: '#1565d8' });
    alert(text);
  }

  function loadImage(file) {
    return new Promise((resolve, reject) => {
      const url = URL.createObjectURL(file);
      const image = new Image();
      image.onload = () => { URL.revokeObjectURL(url); resolve(image); };
      image.onerror = () => { URL.revokeObjectURL(url); reject(new Error('decode')); };
      image.src = url;
    });
  }

  function canvasBlob(canvas, quality) {
    return new Promise(resolve => canvas.toBlob(resolve, 'image/jpeg', quality));
  }

  async function mobileReadyFile(file) {
    if (file.size > maxSourceBytes) throw new Error('รูปต้นฉบับต้องมีขนาดไม่เกิน 25 MB');
    const direct = (directTypes.has(file.type) || (!file.type && directExtensions.test(file.name))) && file.size <= maxUploadBytes;
    if (direct) return { file, converted: false };
    if (!(file.type.startsWith('image/') || /\.(heic|heif|jpe?g|png|webp)$/i.test(file.name)))
      throw new Error('กรุณาเลือกไฟล์รูปภาพ');

    let image;
    try { image = await loadImage(file); }
    catch {
      throw new Error('มือถือไม่สามารถแปลงรูปนี้ได้ หากเป็น iPhone ให้ตั้ง Camera > Formats > Most Compatible แล้วถ่ายใหม่ หรือบันทึกรูปเป็น JPG ก่อน');
    }
    const maxDimension = 1920;
    const scale = Math.min(1, maxDimension / Math.max(image.naturalWidth, image.naturalHeight));
    const canvas = document.createElement('canvas');
    canvas.width = Math.max(1, Math.round(image.naturalWidth * scale));
    canvas.height = Math.max(1, Math.round(image.naturalHeight * scale));
    const context = canvas.getContext('2d');
    context.fillStyle = '#ffffff';
    context.fillRect(0, 0, canvas.width, canvas.height);
    context.drawImage(image, 0, 0, canvas.width, canvas.height);
    let blob;
    for (const quality of [0.84, 0.72, 0.6]) {
      blob = await canvasBlob(canvas, quality);
      if (blob && blob.size <= maxUploadBytes) break;
    }
    if (!blob || blob.size > maxUploadBytes) throw new Error('ไม่สามารถลดขนาดรูปให้ต่ำกว่า 5 MB ได้ กรุณาเลือกรูปอื่น');
    const baseName = (file.name || 'building-photo').replace(/\.[^.]+$/, '').slice(0, 180);
    return { file: new File([blob], baseName + '.jpg', { type: 'image/jpeg', lastModified: Date.now() }), converted: true };
  }

  document.querySelectorAll('[data-photo-form]').forEach((form) => {
    const input = form.querySelector('[data-photo-input]');
    const preview = form.querySelector('[data-photo-preview]');
    const submit = form.querySelector('button[type="submit"]');
    if (!input || !preview) return;
    input.accept = 'image/jpeg,image/png,image/webp,image/heic,image/heif,.jpg,.jpeg,.png,.webp,.heic,.heif';
    const status = document.createElement('small');
    status.className = 'muted d-block mt-2';
    status.textContent = 'ระบบจะลดขนาดรูปจากมือถือให้อัตโนมัติ · ไฟล์ที่ส่งไม่เกิน 5 MB';
    input.closest('.building-photo-field')?.append(status);
    let objectUrl;
    let processing = false;

    form.addEventListener('submit', event => {
      if (!processing) return;
      event.preventDefault();
      alertUser('กำลังเตรียมรูป', 'กรุณารอให้ระบบลดขนาดรูปเสร็จก่อนบันทึก');
    });
    input.addEventListener('change', async () => {
      const selected = input.files?.[0];
      if (!selected) return;
      processing = true;
      if (submit) submit.disabled = true;
      status.textContent = 'กำลังตรวจและลดขนาดรูป...';
      try {
        const prepared = await mobileReadyFile(selected);
        if (prepared.file !== selected) {
          if (typeof DataTransfer === 'undefined') throw new Error('เบราว์เซอร์นี้ไม่รองรับการเตรียมรูปอัตโนมัติ กรุณาเลือกรูป JPG ที่มีขนาดไม่เกิน 5 MB');
          const transfer = new DataTransfer();
          transfer.items.add(prepared.file);
          input.files = transfer.files;
        }
        if (objectUrl) URL.revokeObjectURL(objectUrl);
        objectUrl = URL.createObjectURL(prepared.file);
        let image = preview.querySelector('img');
        if (!image) { image = document.createElement('img'); preview.prepend(image); }
        image.src = objectUrl;
        image.alt = 'ตัวอย่างรูปตึกที่เลือก';
        preview.classList.add('has-image');
        const title = preview.querySelector('strong');
        if (title) title.textContent = 'เลือกรูปแล้ว';
        const hint = preview.querySelector('small');
        if (hint) hint.textContent = prepared.file.name;
        const sizeText = (prepared.file.size / 1024 / 1024).toFixed(2);
        status.textContent = (prepared.converted ? 'ลดขนาดเรียบร้อย ' : 'ขนาด ') + sizeText + ' MB · พร้อมบันทึก';
      } catch (error) {
        input.value = '';
        status.textContent = error.message;
        await alertUser('เลือกรูปไม่ได้', error.message);
      } finally {
        processing = false;
        if (submit) submit.disabled = false;
      }
    });
    window.addEventListener('beforeunload', () => { if (objectUrl) URL.revokeObjectURL(objectUrl); });
  });
})();
