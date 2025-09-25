// Función para reproducir sonido de notificación
window.playNotificationSound = () => {
    try {
        const audioContext = new (window.AudioContext || window.webkitAudioContext)();
        const oscillator = audioContext.createOscillator();
        const gainNode = audioContext.createGain();

        oscillator.connect(gainNode);
        gainNode.connect(audioContext.destination);

        // Sonido más agradable - dos tonos
        oscillator.frequency.setValueAtTime(800, audioContext.currentTime);
        oscillator.frequency.setValueAtTime(600, audioContext.currentTime + 0.1);
        oscillator.type = 'sine';

        gainNode.gain.setValueAtTime(0.1, audioContext.currentTime);
        gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.2);

        oscillator.start(audioContext.currentTime);
        oscillator.stop(audioContext.currentTime + 0.2);
    } catch (error) {
        console.log('No se pudo reproducir el sonido de notificación:', error);
    }
};

// Función para cerrar dropdown cuando se hace click fuera
window.addClickOutsideListener = (elementClass) => {
    document.addEventListener('click', (event) => {
        const notificationBell = document.querySelector('.' + elementClass);
        if (notificationBell && !notificationBell.contains(event.target)) {
            // Llamar método Blazor para cerrar dropdown
            const blazorComponent = notificationBell.querySelector('[data-enhanced-nav]');
            if (blazorComponent && DotNet.invokeMethodAsync) {
                try {
                    DotNet.invokeMethodAsync('Crit.Client', 'CloseDropdown');
                } catch (error) {
                    // Intentar con la instancia del componente
                    console.log('Could not close dropdown via DotNet');
                }
            }
        }
    });
};

// Mejorar el comportamiento del navbar en móvil
document.addEventListener('DOMContentLoaded', function () {
    // Auto cerrar navbar en móvil después de click en link
    const navbarCollapse = document.getElementById('critNavbar');
    const navLinks = document.querySelectorAll('.navbar-nav .nav-link');

    if (navbarCollapse) {
        navLinks.forEach(link => {
            link.addEventListener('click', () => {
                // Solo cerrar en vista móvil
                if (window.innerWidth < 992) {
                    const bsCollapse = new bootstrap.Collapse(navbarCollapse, {
                        toggle: false
                    });
                    bsCollapse.hide();
                }
            });
        });
    }

    // Mejorar el toggle del navbar
    const navbarToggler = document.querySelector('.navbar-toggler');
    if (navbarToggler) {
        navbarToggler.addEventListener('click', function () {
            this.setAttribute('aria-expanded',
                this.getAttribute('aria-expanded') === 'true' ? 'false' : 'true');
        });
    }
});