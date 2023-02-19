// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

export function meta(name: string): string {
	return (document.querySelector(`meta[name="${name}"]`) as HTMLMetaElement)?.content;
}
