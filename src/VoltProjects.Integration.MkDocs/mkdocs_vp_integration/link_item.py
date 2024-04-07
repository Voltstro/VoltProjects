from json import JSONEncoder

class LinkItem:
    """
    LinkItem, ported from VoltProjects
    """
    def __init__(self, href: str | None, title: str | None, items: list["LinkItem"] | None) -> None:
        self.Href = href
        self.Title = title
        self.Items = items
        pass

    Href: str | None
    Title: str | None
    Items: list["LinkItem"] | None

    class LinkItemJsonEncoder(JSONEncoder):
        """
        JsonEncoder for LinkItem
        """
        def default(self, o):
            return o.__dict__
