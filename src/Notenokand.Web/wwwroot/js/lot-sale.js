(() => {
    const form = document.querySelector('[data-sale-form]');
    if (form) {
        const lines = form.querySelector('[data-sale-lines]');
        const template = document.querySelector('[data-sale-template]');
        const add = form.querySelector('[data-add-sale-line]');
        const money = value => value.toLocaleString('th-TH', {minimumFractionDigits:2, maximumFractionDigits:2});
        function refresh() {
            let kg = 0, amount = 0;
            const rows = [...lines.querySelectorAll('[data-sale-line]')];
            rows.forEach((row, index) => {
                row.querySelector('[data-line-number]').textContent = index + 1;
                row.querySelectorAll('[name],[id],[for],[data-valmsg-for]').forEach(el => {
                    ['name','id','for','data-valmsg-for'].forEach(attr => {
                        const value = el.getAttribute(attr);
                        if (value) el.setAttribute(attr, value.replace(/Items\[\d+\]/g, 'Items[' + index + ']').replace(/Items_\d+__/g, 'Items_' + index + '__'));
                    });
                });
                row.querySelector('[data-remove-sale-line]').disabled = rows.length === 1;
                const weight = row.querySelector('[data-sale-weight]');
                const selected = row.querySelector('[data-stock-select]').selectedOptions[0];
                weight.max = selected?.dataset.remaining || '99999999';
                const w = Math.max(0, Number(weight.value) || 0);
                const price = Math.max(0, Number(row.querySelector('[data-sale-price]').value) || 0);
                const total = Math.round((w * price + Number.EPSILON) * 100) / 100;
                kg += w; amount += total;
                row.querySelector('[data-line-amount]').textContent = money(total);
            });
            form.querySelector('[data-sale-total-kg]').textContent = kg.toLocaleString('th-TH', {minimumFractionDigits:3,maximumFractionDigits:3});
            form.querySelector('[data-sale-total]').textContent = money(amount);
            form.querySelector('[data-sale-average]').textContent = money(kg ? amount / kg : 0);
            add.disabled = rows.length >= 100;
        }
        add.addEventListener('click', () => {
            if (lines.children.length >= 100) return;
            const row = template.content.cloneNode(true);
            row.querySelectorAll('input').forEach(el => el.value = '');
            row.querySelectorAll('select').forEach(el => el.selectedIndex = 0);
            row.querySelectorAll('[data-valmsg-for]').forEach(el => el.textContent = '');
            lines.append(row); refresh();
        });
        lines.addEventListener('input', refresh);
        lines.addEventListener('change', refresh);
        lines.addEventListener('click', event => {
            const button = event.target.closest('[data-remove-sale-line]');
            if (button && lines.children.length > 1) { button.closest('[data-sale-line]').remove(); refresh(); }
        });
        form.addEventListener('submit', () => { form.querySelector('button[type=submit]').disabled = true; });
        refresh();
    }
    document.querySelectorAll('[data-cancel-sale]').forEach(cancelForm => {
        cancelForm.addEventListener('submit', async event => {
            event.preventDefault();
            const text = 'คืนสต็อกทุกล็อตและนำรายรับที่เชื่อมออกจากยอดสรุป ยืนยันว่ารายการขายนี้บันทึกผิดและต้องการย้อนรายการ?';
            const ok = window.Swal ? (await Swal.fire({title:'ยกเลิกการขาย?', text, icon:'warning', showCancelButton:true, confirmButtonText:'ยืนยันยกเลิกการขาย', cancelButtonText:'กลับ'})).isConfirmed : window.confirm(text);
            if (!ok) return;
            cancelForm.querySelector('button').disabled = true;
            HTMLFormElement.prototype.submit.call(cancelForm);
        });
    });
})();
