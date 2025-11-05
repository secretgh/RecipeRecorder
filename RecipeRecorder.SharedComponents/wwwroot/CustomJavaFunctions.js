import 'https://cdn.jsdelivr.net/npm/@shoelace-style/shoelace@2.15.0/cdn/components/alert/alert.js';

// Always escape HTML for text arguments!
export function escapeHtml(html) {
    const div = document.createElement('div');
    div.textContent = html;
    return div.innerHTML;
}

// Custom function to emit toast notifications
export function notify(message, variant = 'primary', icon = 'info-circle', duration = 3000) {
    const alert = Object.assign(document.createElement('sl-alert'), {
        variant,
        closable: true,
        duration: duration,
        innerHTML: `<sl-icon name="${icon}" slot="icon"></sl-icon>${escapeHtml(message)}`
    });
    document.body.append(alert);
    return alert.toast();
}