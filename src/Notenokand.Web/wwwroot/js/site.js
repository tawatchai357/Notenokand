document.querySelectorAll('.password-toggle').forEach(button => button.addEventListener('click', () => {
  const input = button.previousElementSibling;
  input.type = input.type === 'password' ? 'text' : 'password';
  button.textContent = input.type === 'password' ? 'ดู' : 'ซ่อน';
}));
if (window.notenokandAlert && window.Swal) Swal.fire({ icon: 'success', title: 'เรียบร้อย', text: window.notenokandAlert, confirmButtonText: 'ตกลง', confirmButtonColor: '#1565d8' });