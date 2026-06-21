const API_BASE = document.querySelector('meta[name="api-base-url"]').content;

let presenceConnection;

async function getLatestToken() {
    const res = await fetch('/auth/GetCurrentToken');
    return await res.text();
}

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
        .withUrl(`${API_BASE}/hubs/presence`, {
            accessTokenFactory: async () => await getLatestToken()
        })
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
        // OnConnectedAsync re-fires automatically, re-seeding online users
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