(function () {
    // ── Config ──────────────────────────────────────────────────────────────
    async function getLatestToken() {
        const res = await fetch('/auth/GetCurrentToken');
        return await res.text();
    }

    // ── State ────────────────────────────────────────────────────────────────
    let connection;
    let typingTimer;
    let isTyping = false;
    let lastSenderId = null;
    let lastMsgTime = null;

    // Pagination state
    let currentPage = 1;
    let isLoadingMore = false;
    let hasMoreMessages = true;
    const PAGE_SIZE = 50;

    // Track the oldest message rendered (for prepend consecutive logic)
    let firstSenderId = null;
    let firstMsgTime = null;

    // ── DOM refs ─────────────────────────────────────────────────────────────
    const list = document.getElementById('messageList');
    const input = document.getElementById('messageInput');
    const sendBtn = document.getElementById('sendBtn');
    const toast = document.getElementById('connToast');
    const typingEl = document.getElementById('typingIndicator');

    // ── Helpers ───────────────────────────────────────────────────────────────

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

    function escHtml(str) {
        return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;').replace(/'/g, '&#039;');
    }

    // ── Build a message bubble (append to bottom) ────────────────────────────
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
        row.dataset.sentAt = sentAt;

        row.innerHTML = `
            ${!isConsecutive ? `<div class="sender-label">${isSent ? 'You' : escHtml(OTHER_NAME)}</div>` : ''}
            <div class="bubble">${escHtml(content)}</div>
            <div class="bubble-meta">${formatTime(date)}</div>
        `;

        list.appendChild(row);

        // Update bottom-of-list tracking
        lastSenderId = senderId;
        lastMsgTime = date;

        // If this is the first message ever appended, also set top-of-list tracking
        if (firstSenderId === null) {
            firstSenderId = senderId;
            firstMsgTime = date;
        }

        scrollToBottom(true);
    }

    // ── Prepend a message batch to the top ───────────────────────────────────
    // msgs must be passed in chronological order (oldest first).
    // We insert them as a block before the current first child.
    function prependMessages(msgs) {
        if (!msgs.length) return;

        // Build a temporary fragment so we can insert all at once
        const fragment = document.createDocumentFragment();

        // We need to track consecutive runs inside this batch as well,
        // seeding from nothing (we don't know what came before in history).
        let batchLastSenderId = null;
        let batchLastMsgTime = null;

        // Also collect which date labels we're about to add so we don't duplicate
        // ones already present at the very top of the list.
        const existingTopDivider = list.querySelector('[data-date-label]');
        const existingTopDateStr = existingTopDivider ? existingTopDivider.dataset.dateLabel : null;

        // Track date labels we insert in this batch
        const insertedDateLabels = new Set();

        msgs.forEach(({ id, senderId, content, sentAt }) => {
            const date = new Date(sentAt);
            const isSent = senderId === CURRENT_USER;
            const side = isSent ? 'sent' : 'received';
            const dateStr = formatDate(date);

            // Date divider — only if not already inserted in this batch
            if (!insertedDateLabels.has(dateStr)) {
                insertedDateLabels.add(dateStr);
                const div = document.createElement('div');
                div.className = 'date-divider';
                div.dataset.dateLabel = dateStr;
                div.textContent = dateStr;
                fragment.appendChild(div);
                // A new date group resets consecutive tracking
                batchLastSenderId = null;
                batchLastMsgTime = null;
            }

            const isConsecutive = batchLastSenderId === senderId &&
                batchLastMsgTime && (date - batchLastMsgTime) < 5 * 60 * 1000;

            const row = document.createElement('div');
            row.className = `message-row ${side}${isConsecutive ? ' consecutive' : ' show-sender'}`;
            row.dataset.msgId = id;
            row.dataset.sentAt = sentAt;

            row.innerHTML = `
                ${!isConsecutive ? `<div class="sender-label">${isSent ? 'You' : escHtml(OTHER_NAME)}</div>` : ''}
                <div class="bubble">${escHtml(content)}</div>
                <div class="bubble-meta">${formatTime(date)}</div>
            `;

            fragment.appendChild(row);
            batchLastSenderId = senderId;
            batchLastMsgTime = date;
        });

        // Remove the duplicate top date divider if the batch ends on the same date
        // as the message list currently starts with
        if (existingTopDateStr && insertedDateLabels.has(existingTopDateStr)) {
            existingTopDivider.remove();
        }

        // Insert the fragment before the first non-system-msg child
        const firstMsg = list.querySelector('.message-row, .date-divider');
        if (firstMsg) {
            list.insertBefore(fragment, firstMsg);
        } else {
            list.appendChild(fragment);
        }

        // Update top-of-list tracking to the oldest message in this batch
        const oldest = msgs[0];
        firstSenderId = oldest.senderId;
        firstMsgTime = new Date(oldest.sentAt);
    }

    // ── Load history (initial, page 1) ────────────────────────────────────────
    async function loadHistory() {
        currentPage = 1;
        hasMoreMessages = true;
        isLoadingMore = false;
        lastSenderId = null;
        lastMsgTime = null;
        firstSenderId = null;
        firstMsgTime = null;

        try {
            const res = await fetch(
                `/Chat/GetMessages?chatId=${CHAT_ID}&page=1&pageSize=${PAGE_SIZE}`
            );
            if (!res.ok) return;
            const json = await res.json();
            if (json.success && Array.isArray(json.data)) {
                if (json.data.length < PAGE_SIZE) hasMoreMessages = false;
                json.data.reverse(); // API returns newest-first; reverse to oldest-first
                json.data.forEach(appendMessage);
                scrollToBottom(true);
            }
        } catch (e) {
            console.warn('History load failed', e);
        }
    }

    // ── Load older messages (pages 2+) ────────────────────────────────────────
    async function loadMoreMessages() {
        if (isLoadingMore || !hasMoreMessages) return;
        isLoadingMore = true;

        // Show a subtle loading indicator at the top
        let loader = document.getElementById('paginationLoader');
        if (!loader) {
            loader = document.createElement('div');
            loader.id = 'paginationLoader';
            loader.className = 'date-divider'; // reuse existing style
            loader.textContent = 'Loading…';
            const firstChild = list.querySelector('.message-row, .date-divider');
            if (firstChild) list.insertBefore(loader, firstChild);
            else list.appendChild(loader);
        }

        // Snapshot scroll position so we can restore it after prepend
        const scrollHeightBefore = list.scrollHeight;
        const scrollTopBefore = list.scrollTop;

        try {
            const nextPage = currentPage + 1;
            const res = await fetch(
                `/Chat/GetMessages?chatId=${CHAT_ID}&page=${nextPage}&pageSize=${PAGE_SIZE}`
            );
            if (!res.ok) { isLoadingMore = false; loader.remove(); return; }
            const json = await res.json();

            if (json.success && Array.isArray(json.data) && json.data.length > 0) {
                currentPage = nextPage;
                if (json.data.length < PAGE_SIZE) hasMoreMessages = false;

                // API returns newest-first; reverse to oldest-first for prepend
                const msgs = [...json.data].reverse();
                loader.remove();
                prependMessages(msgs);

                // Restore scroll position — shift by however much height was added
                const addedHeight = list.scrollHeight - scrollHeightBefore;
                list.scrollTop = scrollTopBefore + addedHeight;
            } else {
                hasMoreMessages = false;
                loader.textContent = 'Beginning of conversation';
                setTimeout(() => loader.remove(), 1500);
            }
        } catch (e) {
            console.warn('Pagination load failed', e);
            loader.remove();
        } finally {
            isLoadingMore = false;
        }
    }

    // ── Scroll listener for pagination ────────────────────────────────────────
    list.addEventListener('scroll', () => {
        if (list.scrollTop < 80) {
            loadMoreMessages();
        }
    });

    // ── SignalR connection ────────────────────────────────────────────────────
    async function startConnection() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(`${API_BASE}/hubs/dmchat?otherUserId=${OTHER_USER}`, {
                accessTokenFactory: async () => await getLatestToken()
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000])
            .build();

        connection.on('ReceiveMessage', (msg) => {
            appendMessage(msg);
        });

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

        connection.onreconnecting(() => showToast('Reconnecting…'));
        connection.onreconnected(() => showToast('Reconnected'));
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