const API_BASE = document.querySelector('meta[name="api-base-url"]').content;
const token = document.querySelector('meta[name="access-token"]').content;

let presenceConnection;

function setPresence(userId, isOnline) {
    document.querySelectorAll(`[data-user-id="${userId}"]`).forEach(el => {
        if (isOnline) {
            el.classList.add('online');
        } else {
            el.classList.remove('online');
        }
    });
}

async function startPresenceConnection() {
    presenceConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${API_BASE}/hubs/presence?access_token=${encodeURIComponent(token)}`)
        .withAutomaticReconnect([0, 2000, 5000, 10000])
        .build();

    presenceConnection.on('ReceiveOnlineUsers', (userIds) => {
        userIds.forEach(id => setPresence(id, true));
    });

    presenceConnection.on('UserOnline', (userId) => {
        setPresence(userId, true);
    });

    presenceConnection.on('UserOffline', (userId) => {
        setPresence(userId, false);
    });

    presenceConnection.onreconnected(() => {
        // re-seed is handled automatically by OnConnectedAsync firing again
    });

    try {
        await presenceConnection.start();
    } catch (e) {
        console.error('Presence connection failed:', e);
    }
}

startPresenceConnection();

window.addEventListener('beforeunload', () => {
    presenceConnection.stop();
});