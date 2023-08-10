import Cookies from 'js-cookie';

const cookieThemeName = 'vp-theme';
const allowedThemes: string[] = ['dark', 'light'];

export function setTheme(theme: string): void {
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

export function getPreferredTheme(): string {
	const storedTheme = getStoredTheme();
	if (storedTheme) {
		return storedTheme;
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
