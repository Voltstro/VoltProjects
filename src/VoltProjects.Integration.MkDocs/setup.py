from setuptools import setup, find_packages
from pathlib import Path

this_directory = Path(__file__).parent
long_description = (this_directory / "README.md").read_text()

setup(
    name = 'mkdocs_vp_integration',
    version = '1.1.0',
    description =' Integration for MkDocs and VoltProject',
    long_description = long_description,
    url = '',
    author = 'Voltstro',
    author_email = 'me@voltstro.dev',
    license = 'GPL-3.0',
    python_requires = '>=3.10',
    install_requires = [
        'mkdocs>=1.5.3'
    ],
    classifiers = [
        'Programming Language :: Python',
        'Programming Language :: Python :: 3 :: Only'
    ],
    packages = find_packages(),
    entry_points = {
        'mkdocs.plugins': [
            'vp_integration = mkdocs_vp_integration.plugin:VPIntegrationPlugin'
        ]
    }
)
