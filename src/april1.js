export function april1() {
    const isApril1 = new Date().toISOString().substring(5, 10) === '04-01';
    if (!isApril1) return;

    const emojis = ['🍕', '🍔', '🍎', '🍇', '🍩', '🍣', '🍪', '🍉', '🍗', '🥗'];
    const emojiCount = 20;
    for (let i = 0; i < emojiCount; i++) {
        const emoji = document.createElement('div');
        emoji.className = 'emoji';
        emoji.textContent = emojis[Math.floor(Math.random() * emojis.length)];
        emoji.style.left = Math.random() * 100 + 'vw';
        emoji.style.animationDuration = Math.random() * 3 + 3 + 's';
        emoji.style.animationDelay = Math.random() * 5 - 2 + 's';
        document.body.appendChild(emoji);
    }
}
