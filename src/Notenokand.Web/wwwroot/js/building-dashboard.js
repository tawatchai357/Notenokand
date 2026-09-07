(() => {
  const cards = [...document.querySelectorAll('[data-building-card]')];
  const empty = document.querySelector('.no-filter-results');
  document.querySelectorAll('[data-building-filter]').forEach(button => button.addEventListener('click', () => {
    document.querySelectorAll('[data-building-filter]').forEach(item => item.classList.remove('active'));
    button.classList.add('active'); const filter = button.dataset.buildingFilter; let visible = 0;
    cards.forEach(card => { const show = filter === 'all' || card.dataset.status === filter; card.hidden = !show; if (show) visible++; });
    if (empty) empty.hidden = visible !== 0;
  }));
  document.querySelectorAll('[data-status-form]').forEach(form => form.addEventListener('submit', event => {
    event.preventDefault(); const activating = form.dataset.next === 'active';
    if (!window.Swal) return form.submit();
    Swal.fire({ icon: 'question', title: activating ? 'เปิดใช้งานตึกนี้?' : 'ปิดใช้งานตึกนี้?', text: activating ? `${form.dataset.name} จะกลับมาเลือกใช้ในรายการใหม่ได้` : 'ข้อมูลย้อนหลังจะยังคงอยู่ แต่ตึกจะไม่แสดงในตัวเลือกบันทึกรายการใหม่', showCancelButton: true, confirmButtonText: activating ? 'เปิดใช้งาน' : 'ปิดใช้งาน', cancelButtonText: 'ยกเลิก', confirmButtonColor: activating ? '#16845b' : '#c43b3b' }).then(result => { if (result.isConfirmed) form.submit(); });
  }));
})();