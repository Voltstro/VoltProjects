from mkdocs_vp_integration.link_item import LinkItem

class TocHolder:
    def __init__(self, index: str, items: list[LinkItem]) -> None:
        self.toc_index = index
        self.toc_items = items

    toc_index: str
    toc_items: list[LinkItem]
