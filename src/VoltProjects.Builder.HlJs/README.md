# VoltProjects.Builder.HlJs

This NodeJS project provides the build setup for the embedded version of [Highlight.js](https://highlightjs.org/) builder uses.

VoltProjects.Builder will embed `dist/index.js`, then load the resource internally. This project shouldn't need to re-built very often, only when updating Highlight.js.

## Getting Started

### Prerequisites

```
NodeJs 22
Corepack
```

### Building

1. Install packages using `pnpm install`
2. Build using `pnpm run build`
