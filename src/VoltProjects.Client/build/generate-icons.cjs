const { resolve } = require('node:path');
const { existsSync, rmSync, mkdirSync, copyFileSync } = require('node:fs');
const { generateFonts } = require('@twbs/fantasticon');

const codepoints = require(resolve(__dirname, '../src/scss/components/icons/icons.json'));
const bootstrapIcons = resolve(__dirname, '../node_modules/bootstrap-icons/');
const outputDir = resolve(__dirname, '../src/scss/components/icons/font/');

//Pre-prep work
if (existsSync(outputDir))
    rmSync(outputDir, { recursive: true });
mkdirSync(outputDir);

const iconCopyPath = resolve(outputDir, 'icons');
mkdirSync(iconCopyPath);

for (const icon in codepoints) {
    const iconSvg = resolve(bootstrapIcons, `icons/${icon}.svg`);
    if (!existsSync(iconSvg)) {
        console.warn(`Icon ${iconSvg} does not exist!`);
        continue;
    }

    copyFileSync(iconSvg, resolve(iconCopyPath, `${icon}.svg`));
}

//Run fantasticon
generateFonts({
    inputDir: resolve(outputDir, 'icons/'),
    outputDir: outputDir,
    fontTypes: ['woff2', 'woff'],
    assetTypes: ['scss'],
    name: 'bootstrap-icons',
    codepoints,
    prefix: 'bi',
    selector: '.bi',
    fontsUrl: './components/icons/font',
    templates: {
        scss: resolve(__dirname, 'generate-icons-scss.hbs')
    },
}).then(results => console.log('Generated icons'));
