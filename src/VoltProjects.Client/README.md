# VoltProjects.Client

This project contains the source needed to build out VoltProject's client-side resources.

This project uses pnpm for package management, [Vite](https://vitejs.dev/) for building the entire project, [TypeScript](https://www.typescriptlang.org/) for typed JavaScript and [Bootstrap](https://getbootstrap.com/) with some custom [scss](https://sass-lang.com/documentation/syntax/) for frontend styling.

Client resources are served to the browser by `VoltProjects.Server`.

## Getting Started

### Prerequisites

```
NodeJs 22
Corepack
```

### Building

1. Install packages using `pnpm install`
2. Build using `pnpm run build`
   - Files will be outputted to `../VoltProjects.Server/wwwroot`.

### Development

We recommend using VSCode when working on this project. There is a provided configuration for VSCode which sets up ESLint to automatically lint files as you save.

For dev builds that include source maps, use `pnpm run build:dev`.
