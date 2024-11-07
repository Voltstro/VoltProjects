import Cookies from 'js-cookie';

const cookieThemeName = 'vp-theme';
const allowedThemes: string[] = ['dark', 'light'];

/**
 * Toggles current theme
 */
export function toggleTheme(): void {
    let currentTheme = document.documentElement.getAttribute('data-bs-theme');
    if(!currentTheme)
        currentTheme = getPreferredTheme();

    const theme = currentTheme === 'light' ? 'dark' : 'light';
    setTheme(theme);
}

/**
 * Changes current theme colour
 */
export function setTheme(theme: 'light' | 'dark'): void {
    //Shouldn't happen
    if(!allowedThemes.find(x => x == theme))
        theme = 'dark';

    //Set bootstrap theme
    document.documentElement.setAttribute('data-bs-theme', theme);
    setStoredTheme(theme);

    const themeBtnIcon = document.getElementById('theme-btn-icon');
    if(!themeBtnIcon) return;

    let themeBtnClasses = 'bi bi-brightness-high-fill';
    if(theme === 'dark') {
        themeBtnClasses = 'bi bi-moon-fill';
    }

    themeBtnIcon.setAttribute('class', themeBtnClasses);
}

/**
 * Gets user's preferred them.
 * If user has set a theme, it will return user's selection, otherwise browser's default
 */
export function getPreferredTheme(): 'light' | 'dark' {
    const storedTheme = getStoredTheme();
    if (storedTheme) {
        return storedTheme as 'light' | 'dark';
    }

    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

function setStoredTheme(theme: string): void {
    Cookies.set(cookieThemeName, theme, {
        sameSite: 'lax',
        expires: 160
    });
}

const getStoredTheme = (): string => Cookies.get(cookieThemeName);
