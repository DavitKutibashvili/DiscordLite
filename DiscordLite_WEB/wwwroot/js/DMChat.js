(function () {
    // ── Config ──────────────────────────────────────────────────────────────
    const API_BASE = document.querySelector('meta[name="api-base-url"]')?.content ?? '';

    // ── State ────────────────────────────────────────────────────────────────
    let connection;
    let typingTimer;
    let isTyping = false;
    let lastSenderId = null;
    let lastMsgTime = null;

    // ── DOM refs ─────────────────────────────────────────────────────────────
    const list = document.getElementById('messageList');
    const input = document.getElementById('messageInput');
    const sendBtn = document.getElementById('sendBtn');
    const toast = document.getElementById('connToast');
    const typingEl = document.getElementById('typingIndicator');
    const statusDot = document.getElementById('otherStatusDot');

    // ── Helpers ───────────────────────────────────────────────────────────────
    function getToken() {
        return ACCESS_TOKEN || '';
    }

    function formatTime(date) {
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    function formatDate(date) {
        const today = new Date();
        const yesterday = new Date(); yesterday.setDate(today.getDate() - 1);
        if (date.toDateString() === today.toDateString()) return 'Today';
        if (date.toDateString() === yesterday.toDateString()) return 'Yesterday';
        return date.toLocaleDateString([], { month: 'long', day: 'numeric', year: 'numeric' });
    }

    function showToast(msg, isError = false) {
        toast.textContent = msg;
        toast.className = 'conn-toast show' + (isError ? ' error' : '');
        clearTimeout(toast._t);
        if (!isError) toast._t = setTimeout(() => toast.classList.remove('show'), 3000);
    }

    function scrollToBottom(force = false) {
        const threshold = 120;
        const nearBottom = list.scrollHeight - list.scrollTop - list.clientHeight < threshold;
        if (force || nearBottom) list.scrollTop = list.scrollHeight;
    }

    // ── Build a message bubble ────────────────────────────────────────────────
    function appendMessage({ id, senderId, content, sentAt }) {
        const date = new Date(sentAt);
        const isSent = senderId === CURRENT_USER;
        const side = isSent ? 'sent' : 'received';

        // date divider
        const dateStr = formatDate(date);
        const dividers = list.querySelectorAll('[data-date-label]');
        const lastDiv = dividers.length > 0 ? dividers[dividers.length - 1] : null;
        if (!lastDiv || lastDiv.dataset.dateLabel !== dateStr) {
            const div = document.createElement('div');
            div.className = 'date-divider';
            div.dataset.dateLabel = dateStr;
            div.textContent = dateStr;
            list.appendChild(div);
            lastSenderId = null;
        }

        const isConsecutive = lastSenderId === senderId &&
            lastMsgTime && (date - lastMsgTime) < 5 * 60 * 1000;

        const row = document.createElement('div');
        row.className = `message-row ${side}${isConsecutive ? ' consecutive' : ' show-sender'}`;
        row.dataset.msgId = id;

        row.innerHTML = `
        ${!isConsecutive ? `<div class="sender-label">${isSent ? 'You' : escHtml(OTHER_NAME)}</div>` : ''}
        <div class="bubble">${escHtml(content)}</div>
        <div class="bubble-meta">${formatTime(date)}</div>
        `;

        list.appendChild(row);

        lastSenderId = senderId;
        lastMsgTime = date;

        scrollToBottom(true);
    }

    function escHtml(str) {
        return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;').replace(/'/g, '&#039;');
    }

    // ── Load history ──────────────────────────────────────────────────────────
    async function loadHistory() {
        try {
            const res = await fetch(`${API_BASE}/api/DMChat/${CHAT_ID}/messages?page=1&pageSize=50`, {
                headers: { 'Authorization': 'Bearer ' + getToken() }
            });
            if (!res.ok) return;
            const json = await res.json();
            if (json.success && Array.isArray(json.data)) {
                json.data.reverse();
                json.data.forEach(appendMessage);
                scrollToBottom(true);
            }
        } catch (e) {
            console.warn('History load failed', e);
        }
    }

    // ── SignalR connection ────────────────────────────────────────────────────
    async function startConnection() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(`${API_BASE}/hubs/dmchat?access_token=${encodeURIComponent(getToken())}`)
            .withAutomaticReconnect([0, 2000, 5000, 10000])
            .build();

        // incoming message
        connection.on('ReceiveMessage', (msg) => {
            appendMessage(msg);
        });

        // typing indicators
        connection.on('UserTyping', (userId) => {
            if (userId !== CURRENT_USER) {
                typingEl.classList.add('visible');
                clearTimeout(typingEl._t);
                typingEl._t = setTimeout(() => typingEl.classList.remove('visible'), 3000);
            }
        });

        connection.on('UserStoppedTyping', (userId) => {
            if (userId !== CURRENT_USER) typingEl.classList.remove('visible');
        });

        // presence
        connection.on('UserOnline', (userId) => { if (userId === OTHER_USER) statusDot.classList.add('online'); });
        connection.on('UserOffline', (userId) => { if (userId === OTHER_USER) statusDot.classList.remove('online'); });

        connection.onreconnecting(() => showToast('Reconnecting…'));
        connection.onreconnected(() => {
            showToast('Reconnected');
        });
        connection.onclose(() => showToast('Disconnected', true));

        try {
            await connection.start();
            sendBtn.disabled = false;
        } catch (e) {
            showToast('Could not connect', true);
            console.error(e);
        }
    }

    // ── Send ──────────────────────────────────────────────────────────────────
    async function sendMessage() {
        const text = input.value.trim();
        if (!text || connection.state !== signalR.HubConnectionState.Connected) return;

        input.value = '';
        autoResize();
        sendBtn.disabled = true;

        try {
            await connection.invoke('SendMessage', CHAT_ID, text);
        } catch (e) {
            showToast('Failed to send message', true);
            console.error(e);
        } finally {
            sendBtn.disabled = false;
            input.focus();
        }
    }
    // ── Input events ──────────────────────────────────────────────────────────
    function autoResize() {
        input.style.height = 'auto';
        input.style.height = Math.min(input.scrollHeight, 140) + 'px';
    }

    input.addEventListener('input', () => {
        autoResize();
        sendBtn.disabled = input.value.trim().length === 0;

        if (connection?.state === signalR.HubConnectionState.Connected) {
            if (!isTyping) {
                isTyping = true;
                connection.invoke('StartTyping', CHAT_ID).catch(() => { });
            }
            clearTimeout(typingTimer);
            typingTimer = setTimeout(() => {
                isTyping = false;
                connection.invoke('StopTyping', CHAT_ID).catch(() => { });
            }, 2000);
        }
    });

    input.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            if (!sendBtn.disabled) sendMessage();
        }
    });

    sendBtn.addEventListener('click', sendMessage);

    // ── Boot ─────────────────────────────────────────────────────────────────
    (async function init() {
        await loadHistory();
        await startConnection();
    })();
})();