/* =====================================================================
   market-chat.js - Chat interactivo WhatsApp / Messenger
   ===================================================================== */
(function () {
    'use strict';

    const el = id => document.getElementById(id);
    const chatWidget = el('chat-widget');
    if (!chatWidget) return;

    const me = chatWidget.dataset.me || '';
    const money = n => new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' }).format(n);

    let peer = null;
    let peerName = '';
    let call = null;
    const seen = new Set();
    const unread = {};
    let contacts = [];

    function status(text) {
        const s = el('chat-status');
        if (s) {
            s.textContent = text;
            s.style.display = text ? 'block' : 'none';
        }
    }

    let mode = null, typingTimer, lastTyping = 0;
    const records = new Map();
    function compose(action, m) {
        if (mode?.action === 'edit' || action === 'edit') el('body').value = action === 'edit' ? m.body : '';
        mode = action ? {action, id:m.id} : null;
        const bar = el('compose-mode');
        bar.hidden = !mode;
        bar.querySelector('span').textContent = mode ? (action === 'edit' ? 'Editando: ' : 'Respondiendo a: ') + (m.body || m.productName) : '';
        if (mode) el('body').focus();
    }
    el('compose-mode').querySelector('button').onclick = () => compose(null);
    function nowTime() {
        return new Date().toLocaleTimeString('es-EC', { hour: '2-digit', minute: '2-digit' });
    }

    /* Mostrar mensaje recibido o enviado */
    function showMessage(m) {
        const isForCurrentPeer = (m.sender === peer || m.recipient === peer);

        if (m.sender !== me && m.sender !== peer) {
            unread[m.sender] = (unread[m.sender] || 0) + 1;
            drawContacts();
            const contactObj = contacts.find(c => c.id === m.sender);
            status('Nuevo mensaje de ' + (contactObj?.name || m.sender));
            return;
        }

        if (!isForCurrentPeer || !peer) return;
        const previous = el('messages').querySelector('[data-message="'+m.id+'"]');
        records.set(m.id,m);
        seen.add(m.id);

        if (!isForCurrentPeer && peer) return;

        const row = document.createElement('li');
        row.dataset.message = m.id;
        if (m.replyTo && !m.deleted) {
            const quote = document.createElement('blockquote');
            quote.dataset.quote = m.replyTo;
            const source = records.get(m.replyTo);
            quote.textContent = source?.deleted ? 'Mensaje eliminado' : source?.body || source?.productName || 'Mensaje anterior';
            row.append(quote);
        }
        row.className = 'bubble'  + (m.sender === me ? ' mine' : '');

        const textSpan = document.createElement('div');
        textSpan.textContent = m.deleted ? 'Mensaje eliminado' : m.body;
        row.appendChild(textSpan);

        if (m.productId) {
            const card = document.createElement('div');
            card.className = 'bubble-product';
            card.textContent = '🛒 ' + (m.productName || 'Producto') + ' · ' + money(m.price || 0);
            row.appendChild(card);
        }

        const timeSpan = document.createElement('span');
        timeSpan.className = 'bubble-time';
        timeSpan.textContent = new Date(m.created).toLocaleTimeString('es-EC',{hour:'2-digit',minute:'2-digit'}) + (m.edited && !m.deleted ? ' · editado' : '');
        if (!m.deleted) {
            const tools = document.createElement('div'); tools.className = 'message-tools';
            const add = (label, action) => {
                const b = document.createElement('button'); b.type='button'; b.textContent=label;
                b.onclick = () => Promise.resolve(action()).catch(e=>status(e.message)); tools.append(b);
            };
            add('Responder',()=>compose('reply',m));
            for (const emoji of ['👍','❤️','😂']) {
                const count = [m.senderReaction,m.recipientReaction].filter(x=>x===emoji).length;
                add(emoji+(count ? ' '+count : ''),()=>market.invoke('React',m.id,emoji));
            }
            if (m.sender===me) {
                add('Editar',()=>compose('edit',m));
                add('Eliminar',()=>confirm('¿Eliminar este mensaje para ambos?') && market.invoke('Remove',m.id));
            }
            row.append(tools);
        }
        row.appendChild(timeSpan);

        const msgs = el('messages');
        if (msgs) {
            if (previous) previous.replaceWith(row); else msgs.appendChild(row);
            msgs.querySelectorAll('[data-quote="'+m.id+'"]').forEach(q=>q.textContent=m.deleted?'Mensaje eliminado':m.body||m.productName);
            if (previous) return;
            msgs.scrollTop = msgs.scrollHeight;
        }
    }

    /* Abrir chat con un contacto específico */
    async function openPeer(id, name) {
        compose(null); records.clear(); el('typing').textContent=''; peer = id;
        peerName = name;
        delete unread[id];
        seen.clear();

        // Actualizar header del peer
        const nameHdr = el('chat-peer-name-hdr');
        if (nameHdr) nameHdr.textContent = name;

        const avatarHdr = el('chat-peer-avatar-hdr');
        if (avatarHdr) avatarHdr.textContent = name.slice(0, 2).toUpperCase();

        const contact = contacts.find(x => x.id === id);
        const statusHdr = el('chat-peer-status-hdr');
        if (statusHdr) {
            statusHdr.innerHTML = contact?.online 
                ? '<span class="text-success fw-bold">● En línea</span>' 
                : '<span class="text-muted">Desconectado</span>';
        }

        // Habilitar botón de llamada
        const dialBtn = el('dial');
        if (dialBtn) dialBtn.hidden = false;

        // Ocultar pantalla de bienvenida y mostrar panel de conversación
        const noPeer = el('no-peer-msg');
        if (noPeer) noPeer.style.display = 'none';

        const convPanel = el('conv-panel');
        if (convPanel) convPanel.classList.add('open');

        const form = el('message-form');
        if (form) form.classList.remove('hidden-form');

        // Limpiar mensajes anteriores y cargar historial
        const msgs = el('messages');
        if (msgs) msgs.replaceChildren();

        status('Cargando conversación...');
        try {
            if (typeof market !== 'undefined' && market) {
                const history = await market.invoke('History', id);
                status('');
                if (peer === id && Array.isArray(history)) {
                    history.forEach(showMessage);
                    msgs.scrollTop = msgs.scrollHeight;
                }
            }
        } catch (e) {
            status(e.message || 'Error al cargar mensajes.');
        }

        drawContacts();
    }

    /* Renderizar lista de contactos con indicador de En Línea estilo WhatsApp */
    function drawContacts() {
        const searchInput = el('contact-search');
        const term = (searchInput ? searchInput.value : '').trim().toLocaleLowerCase();
        const matching = contacts.filter(c => c.name.toLocaleLowerCase().includes(term));

        const onlineNow = contacts.filter(c => c.online).length;
        const oc = el('online-count');
        if (oc) {
            oc.textContent = `${onlineNow} en línea`;
            oc.className = onlineNow > 0 ? 'badge-online' : 'badge bg-secondary text-white';
        }

        const threads = el('threads');
        if (!threads) return;
        threads.replaceChildren();

        if (!matching.length) {
            const p = document.createElement('div');
            p.style.cssText = 'text-align:center;color:#94a3b8;font-size:0.9rem;padding:24px 16px;';
            p.textContent = 'No se encontraron contactos.';
            threads.appendChild(p);
            return;
        }

        // Ordenar primero los que están conectados (online)
        matching.sort((a, b) => (b.online - a.online) || a.name.localeCompare(b.name));

        matching.forEach(c => {
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'person' + (peer === c.id ? ' selected' : '');

            const av = document.createElement('div');
            av.className = 'avatar' + (c.online ? ' avatar-online' : '');
            av.textContent = c.name.slice(0, 2).toUpperCase();

            const info = document.createElement('div');
            info.className = 'person-info';

            const nm = document.createElement('div');
            nm.className = 'person-name';
            nm.textContent = c.name;

            const st = document.createElement('div');
            st.className = 'person-status' + (c.online ? ' online' : '');
            st.textContent = c.online ? '● Conectado ahora' : 'Desconectado';

            info.append(nm, st);
            btn.append(av, info);

            const count = unread[c.id] || 0;
            if (count > 0) {
                const badge = document.createElement('div');
                badge.className = 'person-unread';
                badge.textContent = count > 9 ? '9+' : count;
                btn.appendChild(badge);
            }

            btn.onclick = () => openPeer(c.id, c.name).catch(e => status(e.message));
            threads.appendChild(btn);
        });
    }

    /* Obtener contactos desde el servidor */
    async function loadThreads() {
        try {
            const r = await fetch('/Chat/DirectoryList', { cache: 'no-store' });
            if (!r.ok) return;
            contacts = await r.json();
            drawContacts();

            if (peer) {
                const c = contacts.find(x => x.id === peer);
                const statusHdr = el('chat-peer-status-hdr');
                if (statusHdr) {
                    statusHdr.innerHTML = c?.online 
                        ? '<span class="text-success fw-bold">● En línea</span>' 
                        : '<span class="text-muted">Desconectado</span>';
                }
            }
        } catch (e) {
            // Silencioso
        }
    }

    /* Event Listeners */
    function init() {
        const contactSearch = el('contact-search');
        if (contactSearch) {
            contactSearch.addEventListener('input', drawContacts);
        }

        const msgForm = el('message-form');
        if (msgForm) {
            msgForm.addEventListener('submit', async e => {
                e.preventDefault();
                try {
                    if (!peer) throw Error('Selecciona un contacto primero.');
                    const bodyEl = el('body');
                    const productEl = el('product');
                    const text = bodyEl.value.trim();
                    if (!text && !productEl.value) return;

                    if (mode?.action === 'edit') await market.invoke('Edit',mode.id,text);
                    else if (mode) await market.invoke('Reply',peer,text,Number(productEl.value)||null,mode.id);
                    else await market.invoke('Send',peer,text,Number(productEl.value)||null);
                    compose(null);
                    bodyEl.value = '';
                    productEl.value = '';
                    const attachBar = el('product-attach-bar');
                    if (attachBar) attachBar.classList.remove('visible');
                    const toggleBtn = el('product-attach-toggle');
                    if (toggleBtn) toggleBtn.classList.remove('active');
                } catch (ex) {
                    status(ex.message);
                }
            });
        }

        const bodyEl = el('body');
        if (bodyEl) {
            bodyEl.addEventListener('input', () => {
                if (peer && Date.now()-lastTyping>1000 && market.state==='Connected') {
                    lastTyping=Date.now(); market.invoke('Typing',peer).catch(()=>{});
                }
            });
            bodyEl.addEventListener('keydown', e => {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    el('message-form')?.requestSubmit();
                }
            });
        }

        const attachToggle = el('product-attach-toggle');
        if (attachToggle) {
            attachToggle.addEventListener('click', () => {
                const bar = el('product-attach-bar');
                if (bar) bar.classList.toggle('visible');
                attachToggle.classList.toggle('active');
            });
        }

        const searchProduct = el('search-product');
        if (searchProduct) {
            searchProduct.addEventListener('input', async () => {
                const term = searchProduct.value;
                const r = await fetch('/Chat/Products?term=' + encodeURIComponent(term));
                if (!r.ok || term !== searchProduct.value) return;
                const sel = el('product');
                if (sel) {
                    sel.replaceChildren(new Option('Sin producto adjunto', ''));
                    for (const p of await r.json()) {
                        sel.add(new Option(p.nombre + ' · ' + money(p.precio), p.id));
                    }
                }
            });
        }

        // WebRTC Videollamada
        setupVideoCall();

        // Conectar con SignalR Hub
        if (typeof market !== 'undefined' && market) {
            market.on('Message', showMessage);
            market.on('Changed',m=>{ if (records.has(m.id)) showMessage(m); });
            market.on('Typing',from=>{
                if (from !== peer) return;
                clearTimeout(typingTimer); el('typing').textContent=peerName+' está escribiendo…';
                typingTimer=setTimeout(()=>el('typing').textContent='',2500);
            });
            market.on('ContactsChanged', loadThreads);
            market.on('Call', handleCall);
            market.onreconnected(() => {
                loadThreads();
                if (peer) openPeer(peer, peerName);
            });
            market.onreconnecting(endCall);
        }

        setInterval(loadThreads, 20000);
        loadThreads();
    }

    /* Videollamadas */
    function updateCallButtons() {
        const accept = el('accept'), hang_ = el('hang'), dial = el('dial');
        if (accept) accept.hidden = !call?.incoming;
        if (hang_) hang_.hidden = !call;
        if (dial) dial.disabled = !!call;
        const va = el('video-area');
        const videos = [el('local-video'),el('remote-video')];
        for (const v of videos) v.hidden = !(call && v.srcObject && v.videoWidth);
        if (va) va.classList.toggle('visible', videos.some(v=>!v.hidden));
    }

    function endCall() {
        const old = call;
        call = null;
        clearTimeout(old?.timeout);
        old?.pc?.close();
        old?.stream?.getTracks().forEach(t => t.stop());
        const lv = el('local-video'), rv = el('remote-video');
        if (lv) lv.srcObject = null;
        if (rv) rv.srcObject = null;
        updateCallButtons();
    }

    const signal = (c, kind, data = '') => market.invoke('Signal', c.peer, c.id, kind, data);

    async function hang() {
        const old = call;
        endCall();
        if (old) await signal(old, 'end').catch(() => {});
    }

    async function prepare(c) {
        if (!navigator.mediaDevices) throw Error('La cámara requiere HTTPS o localhost.');
        const iceServers = await (await fetch('/Chat/Ice')).json();
        const stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        if (call !== c) {
            stream.getTracks().forEach(t => t.stop());
            throw Error('Llamada finalizada');
        }
        c.stream = stream;
        el('local-video').srcObject = stream;
        const pc = c.pc = new RTCPeerConnection({ iceServers });
        stream.getTracks().forEach(t => pc.addTrack(t, stream));
        pc.ontrack = e => { const video=el('remote-video');
            video.srcObject=e.streams[0] || new MediaStream(c.pc.getReceivers().map(r=>r.track).filter(Boolean));
            video.play().catch(()=>{ video.controls=true; }); };
        pc.onicecandidate = e => {
            if (e.candidate && call === c) signal(c, 'ice', JSON.stringify(e.candidate)).catch(() => {});
        };
        pc.onconnectionstatechange = () => {
            if (pc.connectionState === 'connected') status('Videollamada conectada');
            if (pc.connectionState === 'failed' && call === c) {
                status('Falló la conexión de video.');
                hang();
            }
        };
    }

    function setupVideoCall() {
        for (const v of [el('local-video'),el('remote-video')]) {
            v.addEventListener('loadeddata',updateCallButtons);
            v.addEventListener('resize',updateCallButtons);
        }
        el('dial')?.addEventListener('click', async () => {
            if (!peer || call) return status('Abre una conversación para llamar.');
            call = { id: crypto.randomUUID(), peer, queue: [] };
            call.timeout = setTimeout(() => { status('Sin respuesta.'); hang(); }, 45000);
            updateCallButtons();
            try {
                await signal(call, 'invite');
                status('Esperando respuesta...');
            } catch (ex) {
                endCall();
                status(ex.message);
            }
        });

        el('accept')?.addEventListener('click', async () => {
            const c = call;
            c.incoming = false;
            updateCallButtons();
            try {
                await prepare(c);
                await signal(c, 'accept');
                status('Conectando videollamada...');
            } catch (ex) {
                await hang();
                status(ex.message);
            }
        });

        el('hang')?.addEventListener('click', hang);
    }

    async function handleCall(sender, id, kind, data) {
        try {
            if (kind === 'invite') {
                if (call) return signal({ peer: sender, id }, 'end');
                call = { peer: sender, id, incoming: true, queue: [] };
                updateCallButtons();
                const contact = contacts.find(c => c.id === sender);
                await openPeer(sender, contact?.name || sender);
                status('📞 Llamada entrante. Puedes aceptar o rechazar.');
                return;
            }
            const c = call;
            if (!c || c.id !== id || c.peer !== sender) return;
            if (kind === 'end') {
                endCall();
                return status('Llamada finalizada.');
            }
            if (kind === 'ice') {
                const candidate = JSON.parse(data);
                if (c.pc?.remoteDescription) await c.pc.addIceCandidate(candidate);
                else c.queue.push(candidate);
                return;
            }
            if (kind === 'accept' && !c.incoming && !c.pc) {
                clearTimeout(c.timeout);
                await prepare(c);
                await c.pc.setLocalDescription(await c.pc.createOffer());
                await signal(c, 'offer', JSON.stringify(c.pc.localDescription));
            } else if (c.pc && (kind === 'offer' || kind === 'answer')) {
                await c.pc.setRemoteDescription(JSON.parse(data));
                for (const candidate of c.queue.splice(0)) await c.pc.addIceCandidate(candidate);
                if (kind === 'offer') {
                    await c.pc.setLocalDescription(await c.pc.createAnswer());
                    await signal(c, 'answer', JSON.stringify(c.pc.localDescription));
                }
                status('Conectando audio y video…');
            }
        } catch (ex) {
            await hang();
            status(ex.message);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
