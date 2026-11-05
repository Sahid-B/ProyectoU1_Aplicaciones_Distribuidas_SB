const market = new signalR.HubConnectionBuilder().withUrl('/live', {
    transport: signalR.HttpTransportType.WebSockets, skipNegotiation: true
}).withAutomaticReconnect().build();
const notice = document.getElementById('live-notice');
const money = n => new Intl.NumberFormat('es-EC', {style:'currency',currency:'USD'}).format(n);
async function refreshTotals() {
    const total = document.querySelector('[data-sales-total]');
    if (!total) return;
    const values = await market.invoke('Totals');
    total.textContent = money(values.total);
    document.querySelector('[data-sales-count]').textContent = values.count;
}
market.on('Market', async e => {
    if (e.kind === 'stock') {
        document.querySelectorAll('[data-stock="' + e.id + '"]').forEach(el => el.textContent = e.stock);
        if (notice.dataset.admin === 'true' && e.stock <= 5) notice.textContent = 'Stock bajo: producto ' + e.id + ', quedan ' + e.stock;
    } else {
        if (e.kind === 'sale') notice.textContent = 'Nueva venta #' + e.id;
        await refreshTotals();
    }
});
market.on('Order', e => notice.textContent = 'Tu pedido #' + e.id + ': ' + e.status);
market.onreconnected(refreshTotals);
async function startMarket() {
    try { await market.start(); await refreshTotals(); }
    catch { if (market.state === signalR.HubConnectionState.Disconnected) setTimeout(startMarket, 4000); }
}
market.onclose(() => setTimeout(startMarket, 4000));
startMarket();

setInterval(() => {
    if (notice.dataset.admin !== 'true' && market.state === 'Connected')
        market.invoke('Heartbeat').catch(() => {});
}, 25000);
