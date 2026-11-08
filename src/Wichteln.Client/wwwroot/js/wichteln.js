// Small browser helpers for the Blazor app.
window.wichteln = {
  getToken() { try { return localStorage.getItem('wichteln.token'); } catch { return null; } },
  setToken(t) { try { localStorage.setItem('wichteln.token', t); } catch { } },
  clearToken() { try { localStorage.removeItem('wichteln.token'); } catch { } },
  // Removes ?login=… from the address bar after it has been saved.
  cleanUrl() { history.replaceState(null, '', location.pathname); },
  // Slides the reel track so item `index` sits under the pointer. Resolves when it lands.
  spin(track, index, duration) {
    return new Promise(resolve => {
      const items = track.children;
      if (!items.length) return resolve();
      const reduce = matchMedia('(prefers-reduced-motion: reduce)').matches;
      const item = items[index];
      const offset = item.offsetLeft + item.offsetWidth / 2;
      track.style.transition = 'none';
      track.style.transform = `translateX(${-(items[0].offsetWidth / 2)}px)`;
      void track.offsetWidth;
      if (reduce) { track.style.transform = `translateX(${-offset}px)`; return resolve(); }
      // Tick the passing avatars for a little slot-machine feel.
      let raf;
      const tick = () => {
        const center = -new DOMMatrix(getComputedStyle(track).transform).m41;
        for (const el of items) {
          const mid = el.offsetLeft + el.offsetWidth / 2;
          el.classList.toggle('is-near', Math.abs(mid - center) < el.offsetWidth / 2);
        }
        raf = requestAnimationFrame(tick);
      };
      track.style.transition = `transform ${duration}ms cubic-bezier(.12,.8,.18,1)`;
      track.style.transform = `translateX(${-offset}px)`;
      raf = requestAnimationFrame(tick);
      setTimeout(() => { cancelAnimationFrame(raf); for (const el of items) el.classList.remove('is-near'); resolve(); }, duration + 50);
    });
  },
  burst(el) {
    // A small gold and red confetti burst from the revealed avatar.
    if (!el || matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const colors = ['var(--gold)', 'var(--red-500)', 'var(--green-500)'];
    for (let i = 0; i < 28; i++) {
      const s = document.createElement('span');
      s.className = 'wi-confetti';
      const a = Math.random() * Math.PI * 2, d = 80 + Math.random() * 90;
      s.style.setProperty('--dx', `${Math.cos(a) * d}px`);
      s.style.setProperty('--dy', `${Math.sin(a) * d - 40}px`);
      s.style.setProperty('--r', `${Math.random() * 360}deg`);
      s.style.background = colors[i % 3];
      el.appendChild(s);
      setTimeout(() => s.remove(), 1300);
    }
  }
};
