import { ScrollSpy } from 'bootstrap';
import { isVisible } from '../shared/utility';

export interface NavItem {
  name: string
  href?: string
}

const IN_THIS_ARTICLE_ID = 'in-this-article';
const ARTICLE_ID = 'article';

/**
 * Installs the aside which has "In this Article"
 */
export function initAside(): void {
    const inThisArticle = document.getElementById(IN_THIS_ARTICLE_ID);
    const articleElement = document.getElementById(ARTICLE_ID);

    if(!inThisArticle || !articleElement)
        return;

    const sections = Array.from(articleElement.querySelectorAll('h2, h3, h4, h5, h6'))
        .filter(e => isVisible(e) && e.id)
        .map(item => ({ name: item.textContent, href: '#' + item.id, type: item.tagName.toLowerCase() }));

    if (sections.length <= 0) {
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
        newHeader.setAttribute('class', `nav-link type-${section.type}`);
        newHeader.innerText = section.name;
        newHeader.href = section.href;

        inThisArticleNav.appendChild(newHeader);
        asideLinks.push(newHeader);
    }

    //Create scrollspy
    new ScrollSpy(articleElement, {
        target: inThisArticle,
        offset: 0
    });
}
