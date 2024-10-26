from json import dumps
from os import path

from mkdocs_vp_integration.link_item import LinkItem
from mkdocs_vp_integration.vp_page import VPPage
from mkdocs_vp_integration.toc_holder import TocHolder

from mkdocs.config.defaults import MkDocsConfig
from mkdocs.plugins import BasePlugin, get_plugin_logger

from mkdocs.structure.pages import Page
from mkdocs.structure.files import Files
from mkdocs.structure.nav import Navigation

class VPIntegrationPlugin(BasePlugin):
    logger = get_plugin_logger(__name__)

    menu: list[LinkItem] = []
    tocs: list[TocHolder] = []
    pages: list[VPPage] = []

    def on_nav(self, nav: Navigation, *, config: MkDocsConfig, files: Files) -> Navigation | None:
        for item in nav.items:
            item_link_item = LinkItem(None, item.title, None)

            if item.is_page:
                # Homepages get skipped
                if item.is_homepage:
                    continue

                # Set href
                item_link_item.Href = item.url

            if item.is_link:
                #TODO: Handle links
                pass

            if item.is_section:
                has_menu_href = False

                #new_toc_item = LinkItem(None, None, [])
                new_toc_items = []

                for child_item in item.children:
                    # Write TOC
                    if child_item.is_page:
                        new_toc_items.append(LinkItem(child_item.url, child_item.title, None))

                        # First page will be this menu's URL
                        if not has_menu_href:
                            item_link_item.Href = child_item.url
                            has_menu_href = True

                new_toc_holder = TocHolder(str(len(self.tocs)), new_toc_items)
                self.tocs.append(new_toc_holder)

                # Menus should always have at least one link
                if not has_menu_href:
                    self.logger.warning(f"Failed to find menu item {item.title}'s first page link!")

            self.menu.append(item_link_item)


    def on_page_content(self, html: str, *, page: Page, config: MkDocsConfig, files: Files) -> str | None:
        toc_index = None

        # Need to find page's TOC
        for toc in self.tocs:
            toc_index = self.find_page_toc(page.url, toc, toc.toc_items)
            if toc_index is not None:
                break

        self.pages.append(VPPage(page.url, html, page.title, page.file.src_uri, toc_index))

        return html
        

    def on_post_build(self, *, config: MkDocsConfig) -> None:
        self.logger.info("Writing files...")

        # Write menu
        menu_obj = {'menu': self.menu}
        menu_json = dumps(menu_obj, indent=4, cls=LinkItem.LinkItemJsonEncoder)
        with open(path.join(config.site_dir, "menu.json"), "w") as menu_out:
            menu_out.write(menu_json)

        # Write TOCs            
        toc_obj = {'tocs': self.tocs}
        toc_json = dumps(toc_obj, indent=4, cls=LinkItem.LinkItemJsonEncoder)
        with open(path.join(config.site_dir, "tocs.json"), "w") as tocs_out:
            tocs_out.write(toc_json)

        # Write pages
        pages_obj = {'pages': self.pages}
        pages_json = dumps(pages_obj, indent=4, cls=VPPage.VPPageJsonEncoder)
        with open(path.join(config.site_dir, "pages.json"), "w") as pages_out:
            pages_out.write(pages_json)     


    def find_page_toc(self, page_url: str, toc_holder: TocHolder, toc_items: list[LinkItem]) -> str:
        for toc_item in toc_items:
            if toc_item.Href == page_url:
                return toc_holder.toc_index
        
            if toc_item.Items and len(toc_item.Items) > 0:
                result = self.find_page_toc(page_url, toc_holder, toc_item.Items)
                if result is not None:
                    return result

        return None
