import Cookies from 'js-cookie';

const cookieThemeName = 'vp-theme';
const allowedThemes: string[] = ['dark', 'light'];

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

    //Set button's active
    const darkBtn = document.getElementById('theme-dark-btn');
    const lightBtn = document.getElementById('theme-light-btn');

    if(theme === 'dark') {
        darkBtn.setAttribute('class', 'dropdown-item active');
        lightBtn.setAttribute('class', 'dropdown-item');
    } else {
        lightBtn.setAttribute('class', 'dropdown-item active');
        darkBtn.setAttribute('class', 'dropdown-item');
    }
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
