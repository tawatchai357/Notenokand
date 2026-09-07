(() => {
  const element = document.getElementById('building-map');
  if (!element || !window.L) return;
  const latitudeInput = document.querySelector('[data-latitude]');
  const longitudeInput = document.querySelector('[data-longitude]');
  const latitudeLabel = document.querySelector('[data-latitude-label]');
  const longitudeLabel = document.querySelector('[data-longitude-label]');
  const currentButton = document.querySelector('[data-current-location]');
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
  currentButton?.addEventListener('click', () => {
    if (!navigator.geolocation) return window.Swal?.fire({ icon: 'error', title: 'อุปกรณ์ไม่รองรับตำแหน่ง' });
    currentButton.disabled = true; currentButton.textContent = 'กำลังค้นหา...';
    navigator.geolocation.getCurrentPosition(
      position => { setLocation(position.coords.latitude, position.coords.longitude); currentButton.disabled = false; currentButton.textContent = '◎ ตำแหน่งปัจจุบัน'; },
      () => { currentButton.disabled = false; currentButton.textContent = '◎ ตำแหน่งปัจจุบัน'; window.Swal?.fire({ icon: 'warning', title: 'ไม่พบตำแหน่ง', text: 'กรุณาอนุญาตตำแหน่ง หรือแตะบนแผนที่เพื่อปักหมุด', confirmButtonColor: '#1565d8' }); },
      { enableHighAccuracy: true, timeout: 12000, maximumAge: 60000 }
    );
  });
  setTimeout(() => map.invalidateSize(), 150);
})();