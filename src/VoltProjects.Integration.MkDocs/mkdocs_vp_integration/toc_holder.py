from mkdocs_vp_integration.link_item import LinkItem

class TocHolder:
    def __init__(self, index: str, item: LinkItem) -> None:
        self.toc_index = index
        self.toc_item = item

    toc_index: str
    toc_item: LinkItem
