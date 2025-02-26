import { isVisible } from '../shared/utility';

export interface NavItem {
  name: string
  href?: string
}

/**
 * Renders out the aside
 */
export function renderAside(): void {
    const inThisArticle = document.getElementById('in-this-article');
    if(!inThisArticle)
        return;

    const sections = Array.from(document.querySelectorAll('article h2'))
        .filter(e => isVisible(e))
        .map(item => ({ name: item.textContent, href: '#' + item.id }));

    if (!inThisArticle || sections.length <= 0) {
        return;
    }

    //Create h5 'In this Article'
    const inThisArticleElement = document.createElement('h5');
    inThisArticleElement.setAttribute('class', 'title');
    inThisArticleElement.innerText = 'In this Article';

    inThisArticle.appendChild(inThisArticleElement);

    //Create anchor links
    const inThisArticleNav = document.createElement('ul');
    inThisArticleNav.setAttribute('class', 'nav');
    inThisArticle.appendChild(inThisArticleNav);

    const asideLinks: HTMLAnchorElement[] = [];
    for(const section of sections) {
        const newHeader = document.createElement('a');
        newHeader.setAttribute('class', 'nav-link');
        newHeader.innerText = section.name;
        newHeader.href = section.href;

        inThisArticleNav.appendChild(newHeader);
        asideLinks.push(newHeader);
    }
}
