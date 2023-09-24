// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import React from 'jsx-dom';
import { isVisible } from './utility';

export interface NavItem {
  name: string
  href?: string
}

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

	//document.body.setAttribute('data-bs-spy', 'scroll');
	//document.body.setAttribute('data-bs-target', '#in-this-article');

	inThisArticle.appendChild(<h5 class='title'>In this Article</h5>);
	inThisArticle.appendChild(
		<ul class='nav'>
			{sections.map(item => {
				// https://github.com/twbs/bootstrap/pull/35566
				const target = item.href && item.href[0] === '#' ? '#' + CSS.escape(item.href.slice(1)) : item.href;
				return <a class='nav-link' data-bs-target={target} href={item.href}>{item.name}</a>;
			})}
		</ul>
	);
}