(() => {
  const element = document.getElementById('building-map');
  if (!element || !window.L) return;
  const latitudeInput = document.querySelector('[data-latitude]');
  const longitudeInput = document.querySelector('[data-longitude]');
  const latitudeLabel = document.querySelector('[data-latitude-label]');
  const longitudeLabel = document.querySelector('[data-longitude-label]');
  const currentButton = document.querySelector('[data-current-location]');
  const mapHelp = element.parentElement?.querySelector('.map-help');
  const initialLatitude = Number.parseFloat(latitudeInput.value);
  const initialLongitude = Number.parseFloat(longitudeInput.value);
  const hasInitialLocation = Number.isFinite(initialLatitude) && Number.isFinite(initialLongitude);
  const map = L.map(element, { scrollWheelZoom: false }).setView(hasInitialLocation ? [initialLatitude, initialLongitude] : [13.2, 101.0], hasInitialLocation ? 17 : 6);
  L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19, attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors' }).addTo(map);
  let marker;
  function setLocation(latitude, longitude, center = true) {
    const point = [latitude, longitude];
    if (!marker) {
      marker = L.marker(point, { draggable: true, title: 'ตำแหน่งตึกนก', alt: 'หมุดตำแหน่งตึกนก' }).addTo(map);
      marker.on('dragend', event => { const position = event.target.getLatLng(); setLocation(position.lat, position.lng, false); });
    } else marker.setLatLng(point);
    latitudeInput.value = latitude.toFixed(7); longitudeInput.value = longitude.toFixed(7);
    latitudeLabel.textContent = latitude.toFixed(7); longitudeLabel.textContent = longitude.toFixed(7);
    if (center) map.setView(point, Math.max(map.getZoom(), 17));
  }
  if (hasInitialLocation) setLocation(initialLatitude, initialLongitude, false);
  map.on('click', event => setLocation(event.latlng.lat, event.latlng.lng));
  const localHost = ['localhost', '127.0.0.1', '::1'].includes(window.location.hostname);
  const insecureMobileAccess = !window.isSecureContext && !localHost;
  if (insecureMobileAccess && mapHelp)
    mapHelp.textContent = 'ตำแหน่งปัจจุบันต้องเปิดผ่าน HTTPS · ระหว่างทดสอบผ่าน IP แบบ HTTP ให้แตะแผนที่เพื่อปักหมุดแทน';
  currentButton?.addEventListener('click', () => {
    if (insecureMobileAccess) {
      return window.Swal?.fire({ icon: 'info', title: 'ต้องใช้ HTTPS', text: 'เบราว์เซอร์มือถือไม่อนุญาตตำแหน่งปัจจุบันบน HTTP กรุณาแตะแผนที่เพื่อปักหมุดก่อน เมื่อเปิดระบบผ่าน HTTPS ปุ่มนี้จะใช้งานได้', confirmButtonColor: '#1565d8' });
    }
    if (!navigator.geolocation) return window.Swal?.fire({ icon: 'error', title: 'อุปกรณ์ไม่รองรับตำแหน่ง', text: 'กรุณาแตะแผนที่เพื่อปักหมุดแทน', confirmButtonColor: '#1565d8' });
    currentButton.disabled = true; currentButton.textContent = 'กำลังค้นหา...';
    navigator.geolocation.getCurrentPosition(
      position => { setLocation(position.coords.latitude, position.coords.longitude); currentButton.disabled = false; currentButton.textContent = '◎ ตำแหน่งปัจจุบัน'; },
      error => {
        currentButton.disabled = false;
        currentButton.textContent = '◎ ตำแหน่งปัจจุบัน';
        const message = error.code === error.PERMISSION_DENIED ? 'กรุณาอนุญาตสิทธิ์ตำแหน่งให้เว็บไซต์ในตั้งค่าเบราว์เซอร์'
          : error.code === error.TIMEOUT ? 'ค้นหาตำแหน่งไม่ทันเวลา กรุณาเปิด GPS แล้วลองใหม่'
          : 'อุปกรณ์ยังระบุตำแหน่งไม่ได้ กรุณาเปิด GPS หรือแตะแผนที่เพื่อปักหมุด';
        window.Swal?.fire({ icon: 'warning', title: 'ไม่พบตำแหน่ง', text: message, confirmButtonColor: '#1565d8' });
      },
      { enableHighAccuracy: true, timeout: 12000, maximumAge: 60000 }
    );
  });
  setTimeout(() => map.invalidateSize(), 150);
})();
