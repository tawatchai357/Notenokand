(() => {
    const form = document.querySelector('[data-harvest-form]');
    if (!form) return;
    const lines = form.querySelector('[data-harvest-lines]');
    const template = document.querySelector('[data-harvest-template]');
    const add = form.querySelector('[data-add-line]');
    const total = form.querySelector('[data-harvest-total]');
    const refresh = () => {
        const rows = [...lines.children];
        let grams = 0;
        rows.forEach((row, index) => {
            row.querySelector('[data-line-number]').textContent = index + 1;
            row.querySelector('[data-remove-line]').disabled = rows.length === 1;
            row.querySelectorAll('[name], [id], [for], [data-valmsg-for]').forEach(el => {
                ['name','id','for','data-valmsg-for'].forEach(attr => {
                    if (el.hasAttribute(attr)) el.setAttribute(attr, el.getAttribute(attr).replace(/Items\[\d+\]/g, 'Items[' + index + ']').replace(/Items_\d+__/g, 'Items_' + index + '__'));
                });
            });
            const amount = Number(row.querySelector('[data-weight]').value);
            if (Number.isFinite(amount) && amount > 0) grams += Math.round(amount * 1000);
        });
        total.textContent = (grams / 1000).toLocaleString('th-TH', { minimumFractionDigits: 3, maximumFractionDigits: 3 });
        add.disabled = rows.length >= 100;
    };
    add.addEventListener('click', () => {
        if (lines.children.length >= 100) return;
        const row = template.content.firstElementChild.cloneNode(true);
        row.querySelectorAll('input,select').forEach(el => { el.value = ''; el.classList.remove('input-validation-error'); });
        row.querySelectorAll('[data-valmsg-for]').forEach(el => { el.textContent = ''; });
        lines.appendChild(row); refresh();
        row.querySelector('select').focus();
    });
    lines.addEventListener('click', event => {
        const button = event.target.closest('[data-remove-line]');
        if (!button || lines.children.length <= 1) return;
        button.closest('.harvest-line').remove(); refresh();
    });
    lines.addEventListener('input', refresh);
    form.addEventListener('submit', () => {
        refresh();
        form.querySelector('button[type=submit]').disabled = true;
    });
    refresh();
})();
