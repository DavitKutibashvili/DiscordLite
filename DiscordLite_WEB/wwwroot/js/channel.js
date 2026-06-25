(function () {
    // ── Config ───────────────────────────────────────────────────────────────

    async function getLatestToken() {
        const res = await fetch('/auth/GetCurrentToken');
        return await res.text();
    }

    // ── State ─────────────────────────────────────────────────────────────────
    let connection;
    let currentChannelId = typeof SELECTED_CHANNEL_ID !== 'undefined' ? SELECTED_CHANNEL_ID : null;

    // Per-list append tracking (bottom of list)
    // Keyed by list element id
    const listState = {
        // [listId]: { lastSenderId, lastMsgTime }
    };

    // Pagination state — shared between desktop/mobile (same channel, same data)
    let currentPage = 1;
    let isLoadingMore = false;
    let hasMoreMessages = true;
    const PAGE_SIZE = 50;

    // ── DOM refs ──────────────────────────────────────────────────────────────
    const list = document.getElementById('messageList');
    const input = document.getElementById('messageInput');
    const sendBtn = document.getElementById('sendBtn');

    const mobileList = document.getElementById('mobileMessageList');
    const mobileInput = document.getElementById('mobileMessageInput');
    const mobileSendBtn = document.getElementById('mobileSendBtn');

    // ── Helpers ───────────────────────────────────────────────────────────────
    function formatTime(date) {
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    function formatDate(date) {
        const today = new Date();
        const yesterday = new Date();
        yesterday.setDate(today.getDate() - 1);
        if (date.toDateString() === today.toDateString()) return 'Today';
        if (date.toDateString() === yesterday.toDateString()) return 'Yesterday';
        return date.toLocaleDateString([], { month: 'long', day: 'numeric', year: 'numeric' });
    }

    function escHtml(str) {
        return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;').replace(/'/g, '&#039;');
    }

    function scrollToBottom(el, force = false) {
        if (!el) return;
        const threshold = 120;
        const nearBottom = el.scrollHeight - el.scrollTop - el.clientHeight < threshold;
        if (force || nearBottom) el.scrollTop = el.scrollHeight;
    }

    function getListState(targetList) {
        if (!targetList) return { lastSenderId: null, lastMsgTime: null };
        if (!listState[targetList.id]) {
            listState[targetList.id] = { lastSenderId: null, lastMsgTime: null };
        }
        return listState[targetList.id];
    }

    function resetListState(targetList) {
        if (!targetList) return;
        listState[targetList.id] = { lastSenderId: null, lastMsgTime: null };
    }

    // ── Build a message bubble (append to bottom) ─────────────────────────────
    function appendMessage(msg, targetList) {
        if (!targetList) return;

        const { id, senderId, senderDisplayName, content, sentAt } = msg;
        const date = new Date(sentAt);
        const isSent = senderId === CURRENT_USER;
        const side = isSent ? 'sent' : 'received';
        const state = getListState(targetList);

        // Date divider
        const dateStr = formatDate(date);
        const dividers = targetList.querySelectorAll('[data-date-label]');
        const lastDiv = dividers.length > 0 ? dividers[dividers.length - 1] : null;
        if (!lastDiv || lastDiv.dataset.dateLabel !== dateStr) {
            const div = document.createElement('div');
            div.className = 'date-divider';
            div.dataset.dateLabel = dateStr;
            div.textContent = dateStr;
            targetList.appendChild(div);
            state.lastSenderId = null;
        }

        const isConsecutive = state.lastSenderId === senderId &&
            state.lastMsgTime && (date - state.lastMsgTime) < 5 * 60 * 1000;

        const row = document.createElement('div');
        row.className = `message-row ${side}${isConsecutive ? ' consecutive' : ' show-sender'}`;
        row.dataset.msgId = id;
        row.dataset.sentAt = sentAt;

        row.innerHTML = `
            ${!isConsecutive ? `<div class="sender-label">${isSent ? 'You' : escHtml(senderDisplayName)}</div>` : ''}
            <div class="bubble">${escHtml(content)}</div>
            <div class="bubble-meta">${formatTime(date)}</div>
        `;

        targetList.appendChild(row);
        state.lastSenderId = senderId;
        state.lastMsgTime = date;

        scrollToBottom(targetList, true);
    }

    // ── Prepend a batch of messages to the top of a list ─────────────────────
    // msgs must be in chronological order (oldest first).
    function prependMessages(msgs, targetList) {
        if (!msgs.length || !targetList) return;

        const fragment = document.createDocumentFragment();
        let batchLastSenderId = null;
        let batchLastMsgTime = null;

        const existingTopDivider = targetList.querySelector('[data-date-label]');
        const existingTopDateStr = existingTopDivider ? existingTopDivider.dataset.dateLabel : null;

        const insertedDateLabels = new Set();

        msgs.forEach(({ id, senderId, senderDisplayName, content, sentAt }) => {
            const date = new Date(sentAt);
            const isSent = senderId === CURRENT_USER;
            const side = isSent ? 'sent' : 'received';
            const dateStr = formatDate(date);

            if (!insertedDateLabels.has(dateStr)) {
                insertedDateLabels.add(dateStr);
                const div = document.createElement('div');
                div.className = 'date-divider';
                div.dataset.dateLabel = dateStr;
                div.textContent = dateStr;
                fragment.appendChild(div);
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
                ${!isConsecutive ? `<div class="sender-label">${isSent ? 'You' : escHtml(senderDisplayName)}</div>` : ''}
                <div class="bubble">${escHtml(content)}</div>
                <div class="bubble-meta">${formatTime(date)}</div>
            `;

            fragment.appendChild(row);
            batchLastSenderId = senderId;
            batchLastMsgTime = date;
        });

        // Remove duplicate date divider at the top of the existing list
        if (existingTopDateStr && insertedDateLabels.has(existingTopDateStr)) {
            existingTopDivider.remove();
        }

        const firstExisting = targetList.querySelector('.message-row, .date-divider');
        if (firstExisting) {
            targetList.insertBefore(fragment, firstExisting);
        } else {
            targetList.appendChild(fragment);
        }
    }

    // ── Load history (initial, page 1) ────────────────────────────────────────
    async function loadHistory(channelId) {
        if (!channelId) return;

        // Reset pagination
        currentPage = 1;
        hasMoreMessages = true;
        isLoadingMore = false;

        // Reset per-list state and clear old messages
        [list, mobileList].forEach(el => {
            if (!el) return;
            resetListState(el);
            el.querySelectorAll('.message-row, .date-divider').forEach(n => n.remove());
        });

        try {
            const res = await fetch(
                `/Server/GetChannelMessages?channelId=${channelId}&page=1&pageSize=${PAGE_SIZE}`
            );
            if (!res.ok) return;
            const json = await res.json();
            if (json.success && Array.isArray(json.data)) {
                if (json.data.length < PAGE_SIZE) hasMoreMessages = false;

                const msgs = [...json.data].reverse(); // oldest-first

                // Reset state between rendering for each list independently
                msgs.forEach(msg => {
                    appendMessage(msg, list);
                });
                resetListState(mobileList);
                msgs.forEach(msg => {
                    appendMessage(msg, mobileList);
                });

                scrollToBottom(list, true);
                scrollToBottom(mobileList, true);
            }
        } catch (e) {
            console.warn('History load failed', e);
        }
    }

    // ── Load older messages (pages 2+) ────────────────────────────────────────
    async function loadMoreMessages() {
        if (isLoadingMore || !hasMoreMessages || !currentChannelId) return;
        isLoadingMore = true;

        // Show loader in both lists
        const loaders = [];
        [list, mobileList].forEach(el => {
            if (!el) return;
            const loader = document.createElement('div');
            loader.className = 'date-divider pagination-loader';
            loader.textContent = 'Loading…';
            const firstChild = el.querySelector('.message-row, .date-divider');
            if (firstChild) el.insertBefore(loader, firstChild);
            else el.appendChild(loader);
            loaders.push({ el, loader });
        });

        // Snapshot scroll heights before prepend
        const snapshots = [list, mobileList].map(el => el ? {
            el,
            scrollHeightBefore: el.scrollHeight,
            scrollTopBefore: el.scrollTop
        } : null).filter(Boolean);

        try {
            const nextPage = currentPage + 1;
            const res = await fetch(
                `/Server/GetChannelMessages?channelId=${currentChannelId}&page=${nextPage}&pageSize=${PAGE_SIZE}`
            );
            loaders.forEach(({ loader }) => loader.remove());

            if (!res.ok) { isLoadingMore = false; return; }
            const json = await res.json();

            if (json.success && Array.isArray(json.data) && json.data.length > 0) {
                currentPage = nextPage;
                if (json.data.length < PAGE_SIZE) hasMoreMessages = false;

                const msgs = [...json.data].reverse(); // oldest-first

                prependMessages(msgs, list);
                prependMessages(msgs, mobileList);

                // Restore scroll positions
                snapshots.forEach(({ el, scrollHeightBefore, scrollTopBefore }) => {
                    const addedHeight = el.scrollHeight - scrollHeightBefore;
                    el.scrollTop = scrollTopBefore + addedHeight;
                });
            } else {
                hasMoreMessages = false;
                // Show "beginning of channel" briefly in whichever list is visible
                [list, mobileList].forEach(el => {
                    if (!el) return;
                    const note = document.createElement('div');
                    note.className = 'date-divider';
                    note.textContent = 'Beginning of channel';
                    const firstChild = el.querySelector('.message-row, .date-divider');
                    if (firstChild) el.insertBefore(note, firstChild);
                    else el.appendChild(note);
                    setTimeout(() => note.remove(), 2000);
                });
            }
        } catch (e) {
            console.warn('Pagination load failed', e);
            loaders.forEach(({ loader }) => loader.remove());
        } finally {
            isLoadingMore = false;
        }
    }

    // ── Scroll listeners for pagination ───────────────────────────────────────
    [list, mobileList].forEach(el => {
        if (!el) return;
        el.addEventListener('scroll', () => {
            if (el.scrollTop < 80) {
                loadMoreMessages();
            }
        });
    });

    // ── SignalR connection ────────────────────────────────────────────────────
    async function startConnection() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(`${API_BASE}/hubs/channel`, {
                accessTokenFactory: async () => await getLatestToken()
            })
            .withAutomaticReconnect([0, 2000, 5000, 10000])
            .build();

        connection.on('ReceiveMessage', (msg) => {
            appendMessage(msg, list);
            appendMessage(msg, mobileList);
        });

        connection.onreconnected(async () => {
            if (currentChannelId) {
                await connection.invoke('JoinChannel', currentChannelId);
            }
        });

        try {
            await connection.start();
            if (currentChannelId) {
                await connection.invoke('JoinChannel', currentChannelId);
                await loadHistory(currentChannelId);
                enableInputs();
            }
        } catch (e) {
            console.error('Channel hub connection failed', e);
        }
    }

    // ── Channel switching ─────────────────────────────────────────────────────
    async function switchChannel(channelId) {
        if (currentChannelId === channelId) return;

        if (currentChannelId && connection?.state === signalR.HubConnectionState.Connected) {
            await connection.invoke('LeaveChannel', currentChannelId);
        }

        currentChannelId = channelId;

        if (connection?.state === signalR.HubConnectionState.Connected) {
            await connection.invoke('JoinChannel', channelId);
            await loadHistory(channelId);
            enableInputs();
        }
    }

    // ── Send ──────────────────────────────────────────────────────────────────
    async function sendMessage(inputEl, btnEl) {
        const text = inputEl.value.trim();
        if (!text || !currentChannelId || connection?.state !== signalR.HubConnectionState.Connected) return;

        inputEl.value = '';
        autoResize(inputEl);
        btnEl.disabled = true;

        try {
            await connection.invoke('SendMessage', currentChannelId, text);
        } catch (e) {
            console.error('Failed to send message', e);
        } finally {
            btnEl.disabled = inputEl.value.trim().length === 0;
            inputEl.focus();
        }
    }

    // ── Input helpers ─────────────────────────────────────────────────────────
    function autoResize(el) {
        el.style.height = 'auto';
        el.style.height = Math.min(el.scrollHeight, 140) + 'px';
    }

    function wireInput(inputEl, btnEl) {
        if (!inputEl || !btnEl) return;

        inputEl.addEventListener('input', () => {
            autoResize(inputEl);
            btnEl.disabled = inputEl.value.trim().length === 0;
        });

        inputEl.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                if (!btnEl.disabled) sendMessage(inputEl, btnEl);
            }
        });

        btnEl.addEventListener('click', () => sendMessage(inputEl, btnEl));
    }

    function enableInputs() {
        if (sendBtn) sendBtn.disabled = false;
        if (mobileSendBtn) mobileSendBtn.disabled = false;
    }

    // ── Mobile chat open/close hooks ──────────────────────────────────────────
    const _origOpen = window.openMobileChat;
    window.openMobileChat = async function (channelId, channelName) {
        if (_origOpen) _origOpen(channelId, channelName);
        await switchChannel(channelId);
    };

    const _origClose = window.closeMobileChat;
    window.closeMobileChat = async function () {
        if (_origClose) _origClose();
        if (currentChannelId && connection?.state === signalR.HubConnectionState.Connected) {
            await connection.invoke('LeaveChannel', currentChannelId);
        }
        currentChannelId = null;
    };

    // ── Boot ──────────────────────────────────────────────────────────────────
    wireInput(input, sendBtn);
    wireInput(mobileInput, mobileSendBtn);

    (async function init() {
        await startConnection();
    })();
})();