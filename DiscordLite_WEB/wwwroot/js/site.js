//Makes user offline in case they log out
document.getElementById('logout-btn')?.addEventListener('click', async function (e) {
    e.preventDefault();
    const href = this.href;
    if (typeof presenceConnection !== 'undefined') {
        await presenceConnection.stop();
    }
    window.location.href = href;
});